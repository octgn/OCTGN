/**
 * PythonScope — manages the Python runtime scope, loads versioned API files,
 * injects globals (me, table, players, etc.), and runs game scripts.
 */

import * as fs from 'fs';
import * as path from 'path';
import { SkulptRuntime, ExecutionResult } from './skulpt-runtime';
import { ScriptApi } from './script-api';

/** Interface for filesystem I/O (for dependency injection in tests). */
export interface PythonScopeIO {
  readFile(filePath: string): string;
  fileExists(filePath: string): boolean;
}

const defaultIO: PythonScopeIO = {
  readFile: (p: string) => fs.readFileSync(p, 'utf-8'),
  fileExists: (p: string) => fs.existsSync(p),
};

export class PythonScope {
  private runtime: SkulptRuntime;
  private scriptApi: ScriptApi;
  private io: PythonScopeIO;
  private initialized = false;
  private scriptVersion: string;

  constructor(
    runtime: SkulptRuntime,
    scriptApi: ScriptApi,
    scriptVersion: string = '3.1.0.2',
    io: PythonScopeIO = defaultIO,
  ) {
    this.runtime = runtime;
    this.scriptApi = scriptApi;
    this.scriptVersion = scriptVersion;
    this.io = io;
  }

  /**
   * Initialize the Python scope: inject _api proxy, load versioned API file,
   * which creates Card/Player/Group/Table classes and the me/table/players globals.
   */
  async initialize(): Promise<ExecutionResult> {
    // Inject the _api proxy into the Skulpt runtime as a builtin
    this.runtime.injectProxy('_api', this.buildApiMethodMap());

    // Load the versioned Python API file
    const apiSource = this.loadVersionedApi();
    const result = await this.runtime.execute(apiSource, `octgn_api_${this.scriptVersion}`);

    if (result.success) {
      this.initialized = true;
    }
    return result;
  }

  /**
   * Load and execute a game's Python script into the scope.
   */
  async loadGameScript(source: string, filename?: string): Promise<ExecutionResult> {
    if (!this.initialized) {
      return { success: false, error: 'PythonScope not initialized. Call initialize() first.' };
    }
    return this.runtime.execute(source, filename);
  }

  /**
   * Call a Python function by name with arguments.
   * Arguments can be raw JS values or will be converted to Python types.
   */
  async callFunction(name: string, ...args: unknown[]): Promise<ExecutionResult> {
    if (!this.initialized) {
      return { success: false, error: 'PythonScope not initialized. Call initialize() first.' };
    }
    return this.runtime.executeFunction(name, ...args);
  }

  /**
   * Check if a function exists in the current scope.
   */
  hasFunction(name: string): boolean {
    return this.runtime.hasFunction(name);
  }

  /**
   * Reset the scope (clears all Python state except _api).
   */
  reset(): void {
    this.runtime.reset();
    this.initialized = false;
  }

  /**
   * Get the underlying runtime (for advanced use).
   */
  getRuntime(): SkulptRuntime {
    return this.runtime;
  }

  /**
   * Build a flat method map from the ScriptApi for the _api proxy.
   * Each key is a method name, each value is a function.
   */
  private buildApiMethodMap(): Record<string, (...args: unknown[]) => unknown> {
    const api = this.scriptApi;
    /* eslint-disable @typescript-eslint/no-explicit-any */
    const map: Record<string, (...args: any[]) => any> = {};

    // Enumerate all own methods of ScriptApi (excluding constructor and privates)
    const proto = Object.getPrototypeOf(api);
    const methodNames = Object.getOwnPropertyNames(proto).filter(
      name => name !== 'constructor' && typeof (api as any)[name] === 'function' && !name.startsWith('_')
    );

    for (const name of methodNames) {
      map[name] = (...args: unknown[]) => (api as any)[name](...args);
    }

    // Also add isMuted
    map['isMuted'] = () => api.isMuted();

    // PlayerCounters/PlayerPiles return objects with Key/Value — convert to tuples for Python
    const origPlayerCounters = map['PlayerCounters'];
    map['PlayerCounters'] = (id: number) => {
      const result = origPlayerCounters(id) as Array<{ Key: number; Value: string }>;
      return result.map(r => [r.Key, r.Value]);
    };

    const origPlayerPiles = map['PlayerPiles'];
    map['PlayerPiles'] = (id: number) => {
      const result = origPlayerPiles(id) as Array<{ Key: number; Value: string }>;
      return result.map(r => [r.Key, r.Value]);
    };

    // GetCurrentPhase returns {Item1, Item2} — convert to tuple
    const origGetCurrentPhase = map['GetCurrentPhase'];
    map['GetCurrentPhase'] = () => {
      const result = origGetCurrentPhase() as { Item1: string; Item2: number };
      return [result.Item1, result.Item2];
    };

    // Web_Read/Web_Post return {Item1, Item2} — convert to tuple
    const origWebRead = map['Web_Read'];
    map['Web_Read'] = (url: string, timeout: number) => {
      const result = origWebRead(url, timeout) as { Item1: string; Item2: number };
      return [result.Item1, result.Item2];
    };

    const origWebPost = map['Web_Post'];
    map['Web_Post'] = (url: string, data: string, timeout: number) => {
      const result = origWebPost(url, data, timeout) as { Item1: string; Item2: number };
      return [result.Item1, result.Item2];
    };

    // CardGetMarkers returns {Item1, Item2}[] — convert to tuples
    const origCardGetMarkers = map['CardGetMarkers'];
    map['CardGetMarkers'] = (cardId: number) => {
      const result = origCardGetMarkers(cardId) as Array<{ Item1: string; Item2: string }>;
      return result.map(r => [r.Item1, r.Item2]);
    };

    // CardSize returns {Name} — just return the name string
    const origCardSize = map['CardSize'];
    map['CardSize'] = (cardId: number) => {
      const result = origCardSize(cardId) as { Name: string };
      return result.Name;
    };

    /* eslint-enable @typescript-eslint/no-explicit-any */
    return map;
  }

  /**
   * Load the versioned Python API source code.
   */
  private loadVersionedApi(): string {
    // First try loading from the bundled versions directory
    const versionsDir = path.join(__dirname, 'versions');
    const apiFile = path.join(versionsDir, `${this.scriptVersion}.py`);

    if (this.io.fileExists(apiFile)) {
      return this.io.readFile(apiFile);
    }

    // Fallback: try common version
    const fallback = path.join(versionsDir, '3.1.0.2.py');
    if (this.io.fileExists(fallback)) {
      return this.io.readFile(fallback);
    }

    throw new Error(`Python API version ${this.scriptVersion} not found at ${apiFile}`);
  }
}
