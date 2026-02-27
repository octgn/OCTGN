import { BrowserWindow } from 'electron';
import { IPC_CHANNELS } from '../../shared/types';

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
 * ScriptEngine bridges the OCTGN Python scripting system to the Electron client.
 *
 * In the .NET client, scripts run via IronPython in-process. In the Electron client,
 * script execution happens server-side — the client receives RemoteCall messages
 * and dispatches the resulting game events to the renderer.
 *
 * This engine:
 * 1. Validates incoming RemoteCall function names and arguments (security)
 * 2. Maintains a log of script executions for debugging
 * 3. Forwards script events to the renderer for UI updates
 * 4. Provides an API for the renderer to request script execution
 */
export class ScriptEngine {
  private callLog: ScriptCall[] = [];
  private maxLogSize = 500;
  private sandboxingEnabled = true;
  private registeredFunctions: Set<string> = new Set();

  constructor() {
    // Register known safe game event functions
    for (const event of SCRIPT_EVENTS) {
      this.registeredFunctions.add(event);
    }
  }

  /**
   * Handle an incoming RemoteCall message from the game server.
   * Validates the call, logs it, and broadcasts to the renderer.
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

    // Broadcast to renderer for UI handling
    this.broadcastScriptEvent('remoteCall', {
      function: functionName,
      args,
      playerId,
    });

    return { success: true };
  }

  /**
   * Handle a script event triggered by a game action.
   * Called when game actions occur that should trigger script callbacks.
   */
  triggerEvent(event: ScriptEvent, params: Record<string, unknown> = {}): void {
    this.broadcastScriptEvent('gameEvent', {
      event,
      params,
      timestamp: Date.now(),
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
