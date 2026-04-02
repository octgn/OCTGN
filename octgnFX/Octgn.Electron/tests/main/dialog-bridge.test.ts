import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { IPC_CHANNELS } from '../../src/shared/types';

// Store registered listeners for simulating IPC responses
let ipcMainOnHandlers: Record<string, ((...args: unknown[]) => void)[]> = {};

vi.mock('electron', () => ({
  ipcMain: {
    on: vi.fn((channel: string, handler: (...args: unknown[]) => void) => {
      if (!ipcMainOnHandlers[channel]) ipcMainOnHandlers[channel] = [];
      ipcMainOnHandlers[channel].push(handler);
    }),
    once: vi.fn(),
    removeListener: vi.fn(),
  },
}));

function simulateDialogResponse(requestId: string, result: unknown) {
  const listeners = ipcMainOnHandlers[IPC_CHANNELS.SCRIPT_DIALOG_RESPONSE] ?? [];
  for (const listener of listeners) {
    listener({}, { requestId, result });
  }
}

describe('createDialogBridge', () => {
  let mockWebContents: { send: ReturnType<typeof vi.fn>; isDestroyed: () => boolean };

  beforeEach(async () => {
    vi.clearAllMocks();
    ipcMainOnHandlers = {};
    mockWebContents = {
      send: vi.fn(),
      isDestroyed: () => false,
    };
    // Re-import to reset module state (listenerRegistered flag)
    vi.resetModules();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  async function getBridge(opts?: { timeoutMs?: number }) {
    const { createDialogBridge } = await import('@main/api/dialog-bridge');
    return createDialogBridge(mockWebContents as any, opts);
  }

  it('sends dialog request to webContents with correct IPC channel', async () => {
    const bridge = await getBridge();
    const promise = bridge('askInteger', { question: 'How many?', defaultAnswer: 5 });

    expect(mockWebContents.send).toHaveBeenCalledTimes(1);
    const [channel, payload] = mockWebContents.send.mock.calls[0];
    expect(channel).toBe(IPC_CHANNELS.SCRIPT_DIALOG_REQUEST);
    expect(payload).toMatchObject({
      type: 'askInteger',
      params: { question: 'How many?', defaultAnswer: 5 },
    });
    expect(payload).toHaveProperty('requestId');

    simulateDialogResponse(payload.requestId, 42);
    const result = await promise;
    expect(result).toBe(42);
  });

  it('resolves with null when user cancels dialog', async () => {
    const bridge = await getBridge();
    const promise = bridge('confirm', { message: 'Sure?' });

    const [, payload] = mockWebContents.send.mock.calls[0];
    simulateDialogResponse(payload.requestId, null);

    const result = await promise;
    expect(result).toBeNull();
  });

  it('generates unique request IDs for concurrent dialogs', async () => {
    const bridge = await getBridge();

    const p1 = bridge('askInteger', { question: 'First?' });
    const p2 = bridge('askString', { question: 'Second?' });

    const [, payload1] = mockWebContents.send.mock.calls[0];
    const [, payload2] = mockWebContents.send.mock.calls[1];

    expect(payload1.requestId).not.toBe(payload2.requestId);

    // Respond out of order
    simulateDialogResponse(payload2.requestId, 'hello');
    simulateDialogResponse(payload1.requestId, 99);

    expect(await p1).toBe(99);
    expect(await p2).toBe('hello');
  });

  it('rejects when webContents is destroyed', async () => {
    const destroyedWc = {
      send: vi.fn(),
      isDestroyed: () => true,
    };
    vi.resetModules();
    ipcMainOnHandlers = {};
    const { createDialogBridge } = await import('@main/api/dialog-bridge');
    const bridge = createDialogBridge(destroyedWc as any);

    await expect(bridge('confirm', { message: 'Sure?' })).rejects.toThrow('destroyed');
  });

  it('times out if no response after timeout period', async () => {
    vi.useFakeTimers();
    const bridge = await getBridge({ timeoutMs: 1000 });
    const promise = bridge('askInteger', { question: 'How many?' });

    vi.advanceTimersByTime(1001);

    await expect(promise).rejects.toThrow('timeout');
  });

  it('sends correct type for each dialog kind', async () => {
    const bridge = await getBridge();

    const types = ['confirm', 'askInteger', 'askString', 'askChoice', 'askMarker', 'askCard'];
    for (const type of types) {
      bridge(type, {});
      const lastCall = mockWebContents.send.mock.calls[mockWebContents.send.mock.calls.length - 1];
      expect(lastCall[1].type).toBe(type);
      simulateDialogResponse(lastCall[1].requestId, null);
    }
  });
});
