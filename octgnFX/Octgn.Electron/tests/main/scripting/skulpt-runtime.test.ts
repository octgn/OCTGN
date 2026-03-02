import { describe, it, expect, beforeEach } from 'vitest';
import { SkulptRuntime, ExecutionResult } from '@main/scripting/skulpt-runtime';

describe('SkulptRuntime', () => {
  let runtime: SkulptRuntime;

  beforeEach(() => {
    runtime = new SkulptRuntime();
  });

  describe('execute()', () => {
    it('executes simple Python arithmetic', async () => {
      const result = await runtime.execute('x = 1 + 2');
      expect(result.success).toBe(true);
      expect(result.error).toBeUndefined();
    });

    it('captures print output', async () => {
      const result = await runtime.execute('print("hello world")');
      expect(result.success).toBe(true);
      expect(result.output).toBe('hello world\n');
    });

    it('captures multiple print outputs', async () => {
      const result = await runtime.execute('print("a")\nprint("b")\nprint("c")');
      expect(result.success).toBe(true);
      expect(result.output).toBe('a\nb\nc\n');
    });

    it('handles syntax errors', async () => {
      const result = await runtime.execute('def foo(');
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
      expect(result.error).toContain('SyntaxError');
    });

    it('handles runtime errors', async () => {
      const result = await runtime.execute('x = 1 / 0');
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
      expect(result.error).toContain('ZeroDivisionError');
    });

    it('handles NameError for undefined variables', async () => {
      const result = await runtime.execute('print(undefined_var)');
      expect(result.success).toBe(false);
      expect(result.error).toContain('NameError');
    });

    it('uses custom filename in errors', async () => {
      const result = await runtime.execute('x = 1/0', 'myscript.py');
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
      expect(result.error).toContain('ZeroDivisionError');
    });

    it('supports Python 3 print function', async () => {
      const result = await runtime.execute('print(1, 2, 3)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('1 2 3\n');
    });

    it('preserves variables across calls within same module', async () => {
      await runtime.execute('x = 42');
      const result = await runtime.execute('print(x)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('42\n');
    });

    it('supports class definitions', async () => {
      const result = await runtime.execute(`
class Foo:
    def __init__(self, val):
        self.val = val
    def get(self):
        return self.val
f = Foo(10)
print(f.get())
`);
      expect(result.success).toBe(true);
      expect(result.output).toBe('10\n');
    });

    it('supports list comprehensions', async () => {
      const result = await runtime.execute('print([x * 2 for x in range(5)])');
      expect(result.success).toBe(true);
      expect(result.output).toBe('[0, 2, 4, 6, 8]\n');
    });
  });

  describe('executeFunction()', () => {
    it('calls a defined function with no args', async () => {
      await runtime.execute('def greet():\n    return "hello"');
      const result = await runtime.executeFunction('greet');
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe('hello');
    });

    it('calls a function with args and returns result', async () => {
      await runtime.execute('def add(a, b):\n    return a + b');
      const result = await runtime.executeFunction('add', 3, 4);
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe(7);
    });

    it('calls a function with string args', async () => {
      await runtime.execute('def greet(name):\n    return "Hello, " + name');
      const result = await runtime.executeFunction('greet', 'World');
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe('Hello, World');
    });

    it('returns None as null', async () => {
      await runtime.execute('def noop():\n    pass');
      const result = await runtime.executeFunction('noop');
      expect(result.success).toBe(true);
      expect(result.returnValue).toBeNull();
    });

    it('returns boolean values', async () => {
      await runtime.execute('def check(x):\n    return x > 0');
      const result = await runtime.executeFunction('check', 5);
      expect(result.success).toBe(true);
      expect(result.returnValue).toBe(true);
    });

    it('returns list as array', async () => {
      await runtime.execute('def make_list():\n    return [1, 2, 3]');
      const result = await runtime.executeFunction('make_list');
      expect(result.success).toBe(true);
      expect(result.returnValue).toEqual([1, 2, 3]);
    });

    it('returns dict as object', async () => {
      await runtime.execute('def make_dict():\n    return {"a": 1, "b": 2}');
      const result = await runtime.executeFunction('make_dict');
      expect(result.success).toBe(true);
      expect(result.returnValue).toEqual({ a: 1, b: 2 });
    });

    it('captures print output from function', async () => {
      await runtime.execute('def say_hi():\n    print("hi")\n    return 42');
      const result = await runtime.executeFunction('say_hi');
      expect(result.success).toBe(true);
      expect(result.output).toBe('hi\n');
      expect(result.returnValue).toBe(42);
    });

    it('returns error for undefined function', async () => {
      const result = await runtime.executeFunction('nonexistent');
      expect(result.success).toBe(false);
      expect(result.error).toBeDefined();
    });

    it('returns error when function throws', async () => {
      await runtime.execute('def bad():\n    raise ValueError("oops")');
      const result = await runtime.executeFunction('bad');
      expect(result.success).toBe(false);
      expect(result.error).toContain('ValueError');
      expect(result.error).toContain('oops');
    });
  });

  describe('injectGlobal()', () => {
    it('injects a number', async () => {
      runtime.injectGlobal('myNum', 42);
      const result = await runtime.execute('print(myNum)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('42\n');
    });

    it('injects a string', async () => {
      runtime.injectGlobal('myStr', 'hello');
      const result = await runtime.execute('print(myStr)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('hello\n');
    });

    it('injects a boolean', async () => {
      runtime.injectGlobal('flag', true);
      const result = await runtime.execute('print(flag)');
      expect(result.success).toBe(true);
      expect(result.output).toBe('True\n');
    });

    it('injects an object with methods as a proxy', async () => {
      const api = {
        getName: () => 'TestPlayer',
        add: (a: number, b: number) => a + b,
      };
      runtime.injectProxy('_api', api);
      const result = await runtime.execute('print(_api.getName())');
      expect(result.success).toBe(true);
      expect(result.output).toBe('TestPlayer\n');
    });

    it('proxy methods receive correct arguments', async () => {
      const api = {
        add: (a: number, b: number) => a + b,
      };
      runtime.injectProxy('_api', api);
      const result = await runtime.execute('print(_api.add(3, 4))');
      expect(result.success).toBe(true);
      expect(result.output).toBe('7\n');
    });

    it('proxy methods can return complex types', async () => {
      const api = {
        getData: () => [1, 2, 3],
      };
      runtime.injectProxy('_api', api);
      const result = await runtime.execute('print(_api.getData())');
      expect(result.success).toBe(true);
      expect(result.output).toBe('[1, 2, 3]\n');
    });

    it('proxy methods can return None (null/undefined)', async () => {
      const api = {
        getNull: () => null,
        getUndef: () => undefined,
      };
      runtime.injectProxy('_api', api);
      const r1 = await runtime.execute('print(_api.getNull())');
      expect(r1.output).toBe('None\n');
      const r2 = await runtime.execute('print(_api.getUndef())');
      expect(r2.output).toBe('None\n');
    });
  });

  describe('hasFunction()', () => {
    it('returns false for undefined function', () => {
      expect(runtime.hasFunction('nonexistent')).toBe(false);
    });

    it('returns true for defined function', async () => {
      await runtime.execute('def my_func():\n    pass');
      expect(runtime.hasFunction('my_func')).toBe(true);
    });

    it('returns false for non-function variables', async () => {
      await runtime.execute('x = 42');
      expect(runtime.hasFunction('x')).toBe(false);
    });
  });

  describe('reset()', () => {
    it('clears all state', async () => {
      await runtime.execute('x = 42');
      runtime.reset();
      const result = await runtime.execute('print(x)');
      expect(result.success).toBe(false);
      expect(result.error).toContain('NameError');
    });

    it('clears function definitions', async () => {
      await runtime.execute('def my_func():\n    pass');
      expect(runtime.hasFunction('my_func')).toBe(true);
      runtime.reset();
      expect(runtime.hasFunction('my_func')).toBe(false);
    });

    it('preserves injected globals after reset', () => {
      runtime.injectGlobal('persistent', 99);
      runtime.reset();
      // Injected globals should survive reset
      return runtime.execute('print(persistent)').then(result => {
        expect(result.success).toBe(true);
        expect(result.output).toBe('99\n');
      });
    });
  });
});
