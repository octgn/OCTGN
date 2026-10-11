import '@testing-library/jest-dom';
import { beforeAll, afterAll } from 'vitest';

// Suppress console noise during tests (stderr still shown for real errors)
const originalConsole = { log: console.log, warn: console.warn, debug: console.debug, info: console.info };
beforeAll(() => {
  console.log = () => {};
  console.warn = () => {};
  console.debug = () => {};
  console.info = () => {};
});
afterAll(() => {
  Object.assign(console, originalConsole);
});

// Polyfill ResizeObserver for jsdom environment
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class ResizeObserver {
    private callback: ResizeObserverCallback;
    constructor(callback: ResizeObserverCallback) {
      this.callback = callback;
    }
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}
