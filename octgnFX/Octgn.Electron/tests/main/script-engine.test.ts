import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  ScriptEngine,
  isValidFunctionName,
  isValidArgument,
  validateScriptCall,
  parseArgs,
} from '@main/scripting/script-engine';

// Mock electron BrowserWindow
vi.mock('electron', () => ({
  BrowserWindow: {
    getAllWindows: vi.fn(() => []),
  },
}));

describe('ScriptEngine', () => {
  describe('isValidFunctionName', () => {
    it('accepts valid Python identifiers', () => {
      expect(isValidFunctionName('OnGameStart')).toBe(true);
      expect(isValidFunctionName('myFunction')).toBe(true);
      expect(isValidFunctionName('_private')).toBe(true);
      expect(isValidFunctionName('func123')).toBe(true);
    });

    it('rejects dangerous built-in functions', () => {
      expect(isValidFunctionName('exec')).toBe(false);
      expect(isValidFunctionName('eval')).toBe(false);
      expect(isValidFunctionName('__import__')).toBe(false);
      expect(isValidFunctionName('compile')).toBe(false);
      expect(isValidFunctionName('open')).toBe(false);
      expect(isValidFunctionName('globals')).toBe(false);
      expect(isValidFunctionName('locals')).toBe(false);
    });

    it('rejects dunder names', () => {
      expect(isValidFunctionName('__builtins__')).toBe(false);
      expect(isValidFunctionName('__class__')).toBe(false);
      expect(isValidFunctionName('__dict__')).toBe(false);
    });

    it('rejects invalid identifiers', () => {
      expect(isValidFunctionName('')).toBe(false);
      expect(isValidFunctionName('123abc')).toBe(false);
      expect(isValidFunctionName('foo bar')).toBe(false);
      expect(isValidFunctionName('foo.bar')).toBe(false);
    });
  });

  describe('isValidArgument', () => {
    it('accepts numbers', () => {
      expect(isValidArgument('42')).toBe(true);
      expect(isValidArgument('-7')).toBe(true);
      expect(isValidArgument('3.14')).toBe(true);
    });

    it('accepts quoted strings', () => {
      expect(isValidArgument('"hello"')).toBe(true);
      expect(isValidArgument("'world'")).toBe(true);
    });

    it('accepts booleans and None', () => {
      expect(isValidArgument('True')).toBe(true);
      expect(isValidArgument('False')).toBe(true);
      expect(isValidArgument('None')).toBe(true);
    });

    it('accepts OCTGN constructors', () => {
      expect(isValidArgument('Player(1)')).toBe(true);
      expect(isValidArgument('Card(456)')).toBe(true);
      expect(isValidArgument('Counter(7)')).toBe(true);
      expect(isValidArgument('Pile(3)')).toBe(true);
    });

    it('accepts simple identifiers', () => {
      expect(isValidArgument('myVar')).toBe(true);
      expect(isValidArgument('table')).toBe(true);
    });

    it('rejects expressions with operators', () => {
      expect(isValidArgument('1 + 2')).toBe(false);
      expect(isValidArgument('Card(1) + Card(2)')).toBe(false);
      expect(isValidArgument('x = 5')).toBe(false);
    });

    it('rejects dangerous function names', () => {
      expect(isValidArgument('exec')).toBe(false);
      expect(isValidArgument('eval')).toBe(false);
    });

    it('accepts empty arguments', () => {
      expect(isValidArgument('')).toBe(true);
      expect(isValidArgument('  ')).toBe(true);
    });
  });

  describe('parseArgs', () => {
    it('parses comma-separated arguments', () => {
      expect(parseArgs('1, 2, 3')).toEqual(['1', '2', '3']);
    });

    it('respects quoted strings with commas', () => {
      expect(parseArgs('"hello, world", 42')).toEqual(['"hello, world"', '42']);
    });

    it('respects nested parentheses', () => {
      expect(parseArgs('Player(1), Card(2)')).toEqual(['Player(1)', 'Card(2)']);
    });

    it('handles empty args', () => {
      expect(parseArgs('')).toEqual([]);
    });

    it('handles single argument', () => {
      expect(parseArgs('42')).toEqual(['42']);
    });
  });

  describe('validateScriptCall', () => {
    it('validates safe calls', () => {
      expect(validateScriptCall('OnGameStart', '')).toEqual({ valid: true });
      expect(validateScriptCall('myFunc', '42, "hello"')).toEqual({ valid: true });
    });

    it('rejects dangerous function names', () => {
      const result = validateScriptCall('exec', '"malicious code"');
      expect(result.valid).toBe(false);
      expect(result.reason).toContain('dangerous');
    });

    it('rejects invalid arguments', () => {
      const result = validateScriptCall('myFunc', '1 + 2');
      expect(result.valid).toBe(false);
      expect(result.reason).toContain('Invalid argument');
    });
  });

  describe('ScriptEngine class', () => {
    let engine: ScriptEngine;

    beforeEach(() => {
      engine = new ScriptEngine();
    });

    it('handles valid remote calls', () => {
      const result = engine.handleRemoteCall(1, 'OnGameStart', '');
      expect(result.success).toBe(true);
    });

    it('rejects dangerous remote calls when sandboxed', () => {
      const result = engine.handleRemoteCall(1, 'exec', '"bad stuff"');
      expect(result.success).toBe(false);
      expect(result.error).toContain('security violation');
    });

    it('allows dangerous calls when sandboxing is disabled', () => {
      engine.setSandboxing(false);
      const result = engine.handleRemoteCall(1, 'exec', '"code"');
      expect(result.success).toBe(true);
    });

    it('logs calls', () => {
      engine.handleRemoteCall(1, 'OnGameStart', '');
      engine.handleRemoteCall(2, 'OnTurn', 'Player(1)');
      expect(engine.getCallLog()).toHaveLength(2);
      expect(engine.getCallLog()[0].function).toBe('OnGameStart');
      expect(engine.getCallLog()[1].function).toBe('OnTurn');
    });

    it('clears log', () => {
      engine.handleRemoteCall(1, 'OnGameStart', '');
      engine.clearLog();
      expect(engine.getCallLog()).toHaveLength(0);
    });

    it('trims log when exceeding max size', () => {
      for (let i = 0; i < 510; i++) {
        engine.handleRemoteCall(1, 'func', String(i));
      }
      expect(engine.getCallLog().length).toBeLessThanOrEqual(500);
    });

    it('reports sandboxing state', () => {
      expect(engine.isSandboxed).toBe(true);
      engine.setSandboxing(false);
      expect(engine.isSandboxed).toBe(false);
    });

    it('registers custom functions', () => {
      engine.registerFunction('customAction');
      const result = engine.handleRemoteCall(1, 'customAction', '');
      expect(result.success).toBe(true);
    });

    it('triggers events', () => {
      expect(() => engine.triggerEvent('OnGameStart', { player: 1 })).not.toThrow();
    });
  });
});
