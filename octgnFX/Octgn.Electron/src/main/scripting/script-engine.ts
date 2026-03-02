import { BrowserWindow } from 'electron';
import { IPC_CHANNELS } from '../../shared/types';
import type { GameDefinition } from '../../shared/types';
import { SkulptRuntime } from './skulpt-runtime';
import { ScriptApi, ScriptApiDeps } from './script-api';
import { PythonScope, PythonScopeIO } from './python-scope';
import { EventDispatcher, DispatchResult } from './event-dispatcher';

/**
 * Game event types that scripts can trigger.
 * These map to the OCTGN IronPython script event system.
 */
export const SCRIPT_EVENTS = [
  'OnTableLoad',
  'OnGameStart',
  'OnGameStarted',
  'OnLoadDeck',
  'OnDeckLoaded',
  'OnCardClick',
  'OnCardDoubleClick',
  'OnMoveCard',
  'OnMoveCards',
  'OnCardTargeted',
  'OnCardArrowTargeted',
  'OnMarkerChanged',
  'OnPlayerConnect',
  'OnPlayerLeaveGame',
  'OnTurn',
  'OnEndTurn',
  'OnChangeCounter',
  'OnCounterChanged',
  'OnPhasePassed',
  'OnScriptedCardsMoved',
  'OverrideCardsMoved',
  'OverrideTurnPassed',
] as const;

export type ScriptEvent = (typeof SCRIPT_EVENTS)[number];

export interface ScriptCall {
  /** The script function name to execute */
  function: string;
  /** Serialized arguments string */
  args: string;
  /** Player ID that initiated the call */
  playerId: number;
  /** Timestamp of the call */
  timestamp: number;
}

export interface ScriptResult {
  success: boolean;
  output?: string;
  error?: string;
  returnValue?: unknown;
}

/**
 * Security validator for script function calls.
 * Mirrors the .NET Engine.ExecuteFunctionSecureNoFormat validation.
 */
const DANGEROUS_FUNCTIONS = new Set([
  'exec', 'eval', 'compile', '__import__', 'open', 'file',
  'input', 'raw_input', 'reload', 'execfile', 'apply',
  'getattr', 'setattr', 'delattr', 'hasattr',
  'globals', 'locals', 'vars', 'dir', 'exit', 'quit',
]);

const VALID_IDENTIFIER_RE = /^[a-zA-Z_][a-zA-Z0-9_]*$/;
const DUNDER_RE = /^__.*__$/;
const EXPRESSION_OPERATORS = /[+\-*/%&|^~<>=;]/;

function isValidFunctionName(name: string): boolean {
  if (!VALID_IDENTIFIER_RE.test(name)) return false;
  if (DANGEROUS_FUNCTIONS.has(name)) return false;
  if (DUNDER_RE.test(name)) return false;
  return true;
}

function isValidArgument(arg: string): boolean {
  const trimmed = arg.trim();
  if (!trimmed) return true;

  // Numbers
  if (/^-?\d+(\.\d+)?$/.test(trimmed)) return true;

  // Quoted strings
  if (/^"[^"]*"$/.test(trimmed) || /^'[^']*'$/.test(trimmed)) return true;

  // Boolean/None
  if (['True', 'False', 'None', 'true', 'false'].includes(trimmed)) return true;

  // OCTGN constructors: Player(123), Card(456)
  if (/^(Player|Card|Counter|Pile)\(\d+\)$/.test(trimmed)) return true;

  // Simple identifiers (variable names)
  if (VALID_IDENTIFIER_RE.test(trimmed) && !DANGEROUS_FUNCTIONS.has(trimmed) && !DUNDER_RE.test(trimmed)) {
    return true;
  }

  // Reject anything with expression operators
  if (EXPRESSION_OPERATORS.test(trimmed)) return false;

  return false;
}

function validateScriptCall(functionName: string, args: string): { valid: boolean; reason?: string } {
  if (!isValidFunctionName(functionName)) {
    return { valid: false, reason: `Invalid or dangerous function name: ${functionName}` };
  }

  if (args) {
    // Parse comma-separated arguments respecting nesting
    const parsedArgs = parseArgs(args);
    for (const arg of parsedArgs) {
      if (!isValidArgument(arg)) {
        return { valid: false, reason: `Invalid argument: ${arg}` };
      }
    }
  }

  return { valid: true };
}

function parseArgs(args: string): string[] {
  const result: string[] = [];
  let current = '';
  let depth = 0;
  let inString = false;
  let stringChar = '';

  for (let i = 0; i < args.length; i++) {
    const ch = args[i];

    if (inString) {
      current += ch;
      if (ch === stringChar && args[i - 1] !== '\\') {
        inString = false;
      }
      continue;
    }

    if (ch === '"' || ch === "'") {
      inString = true;
      stringChar = ch;
      current += ch;
    } else if (ch === '(' || ch === '[') {
      depth++;
      current += ch;
    } else if (ch === ')' || ch === ']') {
      depth--;
      current += ch;
    } else if (ch === ',' && depth === 0) {
      result.push(current.trim());
      current = '';
    } else {
      current += ch;
    }
  }

  if (current.trim()) {
    result.push(current.trim());
  }

  return result;
}

/**
 * Queued execution item — ensures sequential Python execution.
 */
interface QueueItem {
  execute: () => Promise<ScriptResult>;
  resolve: (result: ScriptResult) => void;
}

/**
 * ScriptEngine bridges the OCTGN Python scripting system to the Electron client.
 *
 * Orchestrates Python execution via Skulpt: initializes a versioned Python scope,
 * loads game scripts, dispatches events, and handles RemoteCall messages.
 *
 * All Python execution is serialized through an execution queue to prevent
 * concurrent access to the shared Python scope.
 */
export class ScriptEngine {
  private callLog: ScriptCall[] = [];
  private maxLogSize = 500;
  private sandboxingEnabled = true;
  private registeredFunctions: Set<string> = new Set();

  // Python execution components
  private scope: PythonScope | null = null;
  private eventDispatcher: EventDispatcher | null = null;
  private _initialized = false;

  // Execution queue for sequential Python execution
  private queue: QueueItem[] = [];
  private processing = false;

  constructor() {
    // Register known safe game event functions
    for (const event of SCRIPT_EVENTS) {
      this.registeredFunctions.add(event);
    }
  }

  /**
   * Initialize the scripting engine with a game definition and dependencies.
   * Creates the Python scope, loads the versioned API, and prepares for script execution.
   */
  async initialize(
    gameDef: GameDefinition,
    deps: ScriptApiDeps,
    io?: PythonScopeIO,
  ): Promise<ScriptResult> {
    const version = gameDef.scriptVersion ?? '3.1.0.2';
    const runtime = new SkulptRuntime();
    const api = new ScriptApi(deps);
    const scope = new PythonScope(runtime, api, version, io);
    const dispatcher = new EventDispatcher(version);

    const result = await scope.initialize();

    if (result.success) {
      this.scope = scope;
      this.eventDispatcher = dispatcher;
      this._initialized = true;
    }

    return {
      success: result.success,
      error: result.error,
    };
  }

  /**
   * Check if the engine has been initialized with a Python scope.
   */
  isInitialized(): boolean {
    return this._initialized;
  }

  /**
   * Load a game Python script into the scope.
   */
  async loadGameScript(source: string, filename?: string): Promise<ScriptResult> {
    if (!this._initialized || !this.scope) {
      return { success: false, error: 'ScriptEngine not initialized. Call initialize() first.' };
    }

    const result = await this.scope.loadGameScript(source, filename);
    return {
      success: result.success,
      error: result.error,
    };
  }

  /**
   * Handle an incoming RemoteCall message from the game server.
   * Validates the call, then executes the Python function if the engine is initialized.
   */
  handleRemoteCall(playerId: number, functionName: string, args: string): ScriptResult {
    const call: ScriptCall = {
      function: functionName,
      args,
      playerId,
      timestamp: Date.now(),
    };

    // Validate if sandboxing is enabled
    if (this.sandboxingEnabled) {
      const validation = validateScriptCall(functionName, args);
      if (!validation.valid) {
        const result: ScriptResult = {
          success: false,
          error: `Script security violation: ${validation.reason}`,
        };
        this.logCall(call);
        this.broadcastScriptEvent('error', { call, result });
        return result;
      }
    }

    this.logCall(call);

    // If initialized, execute the Python function
    if (this._initialized && this.scope) {
      const callExpr = args ? `${functionName}(${args})` : `${functionName}()`;
      try {
        // Execute synchronously by running the expression
        // RemoteCall is inherently synchronous in the protocol
        const scope = this.scope;
        scope.getRuntime().execute(callExpr);
      } catch {
        // Log error but don't fail — the WPF client also catches and logs
      }
    }

    // Broadcast to renderer for UI handling
    this.broadcastScriptEvent('remoteCall', {
      function: functionName,
      args,
      playerId,
    });

    return { success: true };
  }

  /**
   * Trigger a game event, dispatching it to the Python scope via EventDispatcher.
   * Returns the dispatch result, including any return value from override events.
   */
  async triggerEvent(event: string, params: Record<string, unknown> = {}): Promise<DispatchResult | null> {
    if (!this._initialized || !this.scope || !this.eventDispatcher) {
      // Not initialized — just broadcast to renderer
      this.broadcastScriptEvent('gameEvent', {
        event,
        params,
        timestamp: Date.now(),
      });
      return null;
    }

    // Queue the event dispatch for sequential execution
    return new Promise<DispatchResult | null>((resolve) => {
      this.queue.push({
        execute: async () => {
          const result = await this.eventDispatcher!.dispatch(this.scope!, event, params);
          return {
            success: result.success,
            error: result.error,
            returnValue: result.returnValue,
          };
        },
        resolve: (scriptResult) => {
          this.broadcastScriptEvent('gameEvent', {
            event,
            params,
            timestamp: Date.now(),
          });
          resolve({
            success: scriptResult.success,
            error: scriptResult.error,
            returnValue: scriptResult.returnValue,
            skipped: false,
          });
        },
      });

      this.processQueue();
    });
  }

  /**
   * Register a custom function name as safe for execution.
   */
  registerFunction(name: string): void {
    this.registeredFunctions.add(name);
  }

  /**
   * Get the script execution log (for debug/game log UI).
   */
  getCallLog(): readonly ScriptCall[] {
    return this.callLog;
  }

  /**
   * Clear the script execution log.
   */
  clearLog(): void {
    this.callLog = [];
  }

  /**
   * Enable or disable sandboxing.
   */
  setSandboxing(enabled: boolean): void {
    this.sandboxingEnabled = enabled;
  }

  get isSandboxed(): boolean {
    return this.sandboxingEnabled;
  }

  /**
   * Get the underlying PythonScope (for advanced use / testing).
   */
  getScope(): PythonScope | null {
    return this.scope;
  }

  private async processQueue(): Promise<void> {
    if (this.processing) return;
    this.processing = true;

    while (this.queue.length > 0) {
      const item = this.queue.shift()!;
      try {
        const result = await item.execute();
        item.resolve(result);
      } catch (err: unknown) {
        item.resolve({
          success: false,
          error: err instanceof Error ? err.message : String(err),
        });
      }
    }

    this.processing = false;
  }

  private logCall(call: ScriptCall): void {
    this.callLog.push(call);
    if (this.callLog.length > this.maxLogSize) {
      this.callLog = this.callLog.slice(-this.maxLogSize);
    }
  }

  private broadcastScriptEvent(type: string, data: Record<string, unknown>): void {
    const windows = BrowserWindow.getAllWindows();
    for (const win of windows) {
      win.webContents.send(IPC_CHANNELS.SCRIPT_EVENT, { type, ...data });
    }
  }
}

// Exported validation utilities for testing
export { isValidFunctionName, isValidArgument, validateScriptCall, parseArgs };
