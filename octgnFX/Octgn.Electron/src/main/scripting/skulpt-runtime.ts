/**
 * SkulptRuntime — low-level wrapper around the Skulpt Python interpreter.
 *
 * Provides execute(), executeFunction(), injectGlobal(), injectProxy(),
 * hasFunction(), and reset() for running Python code in-process.
 */

// Skulpt sets itself as a global `Sk` when loaded. We need `globalThis.window`
// to exist for Skulpt's UMD wrapper to work in Node/Electron.
if (typeof globalThis.window === 'undefined') {
  (globalThis as Record<string, unknown>).window = globalThis;
}

// eslint-disable-next-line @typescript-eslint/no-require-imports
require('skulpt/dist/skulpt.js');
// eslint-disable-next-line @typescript-eslint/no-require-imports
require('skulpt/dist/skulpt-stdlib.js');

declare const Sk: SkulptGlobal;

/** Minimal type declarations for the Skulpt API we use. */
interface SkulptGlobal {
  configure(opts: Record<string, unknown>): void;
  python3: Record<string, unknown>;
  builtinFiles: { files: Record<string, string> };
  builtins: Record<string, unknown>;
  ffi: {
    remapToPy(val: unknown): SkulptPyObject;
    remapToJs(val: SkulptPyObject): unknown;
  };
  misceval: {
    asyncToPromise(fn: () => unknown): Promise<SkulptModule>;
    callsimOrSuspendArray(fn: SkulptPyObject, args: SkulptPyObject[]): unknown;
    promiseToSuspension(p: Promise<unknown>): unknown;
    loadname(name: string, globals: Record<string, unknown>): SkulptPyObject;
  };
  importMainWithBody(name: string, dumpJS: boolean, body: string): SkulptModule;
  builtin: {
    func: new (fn: (...args: SkulptPyObject[]) => SkulptPyObject | unknown) => SkulptPyObject;
    none: { none$: SkulptPyObject };
    checkNone(val: SkulptPyObject): boolean;
  };
  abstr: {
    buildNativeClass(name: string, spec: NativeClassSpec): new () => SkulptPyObject;
  };
}

interface NativeClassSpec {
  constructor: () => void;
  slots: Record<string, unknown>;
}

interface SkulptPyObject {
  $d?: Record<string, SkulptPyObject>;
  $isSuspension?: boolean;
  tp$call?: (args: SkulptPyObject[], kw?: SkulptPyObject[]) => SkulptPyObject;
  ob$type?: { prototype?: { sk$klass?: boolean } };
  v?: unknown;
}

interface SkulptModule {
  $d: Record<string, SkulptPyObject>;
}

export interface ExecutionResult {
  success: boolean;
  output?: string;
  error?: string;
  returnValue?: unknown;
}

export class SkulptRuntime {
  /** The current module scope (persists between execute() calls). */
  private module: SkulptModule | null = null;
  /** Injected builtins that survive reset(). */
  private injectedBuiltins: Map<string, unknown> = new Map();
  /** Module-scope names copied to builtins (cleared on reset). */
  private moduleScopeNames: Set<string> = new Set();
  /** Track whether Skulpt is configured. */
  private configured = false;
  /** Counter for unique module names. */
  private execCounter = 0;

  constructor() {
    this.configureSkulpt();
  }

  private configureSkulpt(): void {
    if (this.configured) return;
    Sk.configure({
      __future__: Sk.python3,
      output: () => {}, // overridden per-call
      read: (x: string) => {
        if (Sk.builtinFiles?.files?.[x as string]) return Sk.builtinFiles.files[x as string];
        throw new Error(`File not found: ${x}`);
      },
    });
    this.configured = true;
  }

  /**
   * Execute Python source code. Variables persist across calls.
   */
  async execute(source: string, filename?: string): Promise<ExecutionResult> {
    let output = '';
    Sk.configure({
      __future__: Sk.python3,
      output: (text: string) => {
        output += text;
      },
      read: (x: string) => {
        if (Sk.builtinFiles?.files?.[x as string]) return Sk.builtinFiles.files[x as string];
        throw new Error(`File not found: ${x}`);
      },
    });

    // Restore injected builtins
    for (const [name, value] of this.injectedBuiltins) {
      Sk.builtins[name] = value;
    }

    try {
      // Each importMainWithBody creates a fresh scope, so we use a unique name
      // to avoid module cache conflicts, and copy results into our persistent module.
      // Skulpt uses the name as a module path — dots cause ImportError.
      // Wrap in angle brackets and strip dots to avoid issues.
      const rawName = filename ?? `script_${this.execCounter++}`;
      const modName = `<${rawName.replace(/\./g, '_')}>`;
      const mod = await Sk.misceval.asyncToPromise(() => {
        return Sk.importMainWithBody(modName, false, source);
      });

      // Merge new definitions into our persistent module
      if (!this.module) {
        this.module = mod;
      } else {
        for (const key of Object.keys(mod.$d)) {
          this.module.$d[key] = mod.$d[key];
        }
      }

      // Copy module scope to builtins so subsequent imports can access them
      for (const key of Object.keys(this.module.$d)) {
        if (key.startsWith('__')) continue;
        Sk.builtins[key] = this.module.$d[key];
        this.moduleScopeNames.add(key);
      }

      return {
        success: true,
        output: output || undefined,
      };
    } catch (err: unknown) {
      const errorStr = err instanceof Error ? err.message : String(err);
      return {
        success: false,
        output: output || undefined,
        error: errorStr,
      };
    }
  }

  /**
   * Call a previously defined Python function by name.
   */
  async executeFunction(name: string, ...args: unknown[]): Promise<ExecutionResult> {
    if (!this.module || !this.module.$d[name]) {
      return {
        success: false,
        error: `Function '${name}' is not defined`,
      };
    }

    const fn = this.module.$d[name];

    let output = '';
    Sk.configure({
      __future__: Sk.python3,
      output: (text: string) => {
        output += text;
      },
      read: (x: string) => {
        if (Sk.builtinFiles?.files?.[x as string]) return Sk.builtinFiles.files[x as string];
        throw new Error(`File not found: ${x}`);
      },
    });

    // Restore injected builtins
    for (const [name, value] of this.injectedBuiltins) {
      Sk.builtins[name] = value;
    }

    try {
      const pyArgs = args.map(a => Sk.ffi.remapToPy(a));
      const pyResult = Sk.misceval.callsimOrSuspendArray(fn, pyArgs) as SkulptPyObject;

      // Handle potential suspension (wrap in asyncToPromise)
      const resolved = await Sk.misceval.asyncToPromise(() => pyResult);

      const jsResult = Sk.ffi.remapToJs(resolved as SkulptPyObject);

      return {
        success: true,
        output: output || undefined,
        returnValue: jsResult === undefined ? null : jsResult,
      };
    } catch (err: unknown) {
      const errorStr = err instanceof Error ? err.message : String(err);
      return {
        success: false,
        output: output || undefined,
        error: errorStr,
      };
    }
  }

  /**
   * Inject a simple JS value (number, string, boolean) as a Python global (builtin).
   */
  injectGlobal(name: string, value: unknown): void {
    const pyVal = Sk.ffi.remapToPy(value);
    Sk.builtins[name] = pyVal;
    this.injectedBuiltins.set(name, pyVal);
  }

  /**
   * Inject a JS object as a Python proxy. Attribute access on the proxy calls
   * the corresponding method on the JS object.
   * This is how `_api` is exposed to Python scripts.
   */
  injectProxy(name: string, obj: Record<string, (...args: unknown[]) => unknown>): void {
    const ProxyClass = Sk.abstr.buildNativeClass(name + 'Proxy', {
      constructor: function () {},
      slots: {
        tp$getattr(this: void, pyName: SkulptPyObject) {
          const attrName = String(pyName);
          const method = obj[attrName];
          if (typeof method === 'function') {
            return new Sk.builtin.func((...pyArgs: SkulptPyObject[]) => {
              const jsArgs = pyArgs.map(a => Sk.ffi.remapToJs(a));
              const result = method(...jsArgs);
              if (result === null || result === undefined) {
                return Sk.builtin.none.none$;
              }
              return Sk.ffi.remapToPy(result);
            });
          }
          return undefined;
        },
      },
    });

    const instance = new ProxyClass();
    Sk.builtins[name] = instance;
    this.injectedBuiltins.set(name, instance);
  }

  /**
   * Check whether a function with the given name is defined in the current scope.
   */
  hasFunction(name: string): boolean {
    if (!this.module || !this.module.$d[name]) return false;
    const val = this.module.$d[name];
    // Check if it's callable (has tp$call)
    return typeof val.tp$call === 'function';
  }

  /**
   * Clear all Python state (variables, functions, classes).
   * Injected globals (via injectGlobal/injectProxy) are preserved.
   */
  reset(): void {
    // Remove module-scope names from builtins
    for (const name of this.moduleScopeNames) {
      if (!this.injectedBuiltins.has(name)) {
        delete Sk.builtins[name];
      }
    }
    this.moduleScopeNames.clear();
    this.module = null;
    // Re-apply injected builtins
    for (const [name, value] of this.injectedBuiltins) {
      Sk.builtins[name] = value;
    }
  }
}
