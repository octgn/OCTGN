/**
 * Dialog bridge — sends dialog requests from the main process to the renderer
 * via IPC and returns a Promise that resolves when the renderer responds.
 *
 * Used by GameService to wire up ScriptApiDeps.requestDialog.
 */
import { ipcMain, type WebContents } from 'electron';
import { IPC_CHANNELS } from '../../shared/types';

let requestIdCounter = 0;

export interface DialogBridgeOptions {
  timeoutMs?: number;
}

/** Pending dialog requests keyed by requestId. */
const pendingRequests = new Map<string, (result: unknown) => void>();

/** Global listener registered once for all dialog responses. */
let listenerRegistered = false;

function ensureResponseListener(): void {
  if (listenerRegistered) return;
  listenerRegistered = true;

  ipcMain.on(IPC_CHANNELS.SCRIPT_DIALOG_RESPONSE, (_event: unknown, response: { requestId: string; result: unknown }) => {
    const resolver = pendingRequests.get(response.requestId);
    if (resolver) {
      pendingRequests.delete(response.requestId);
      resolver(response.result);
    }
  });
}

/**
 * Creates a requestDialog callback bound to a specific WebContents (game window).
 * Each call sends a SCRIPT_DIALOG_REQUEST IPC event and waits for the
 * corresponding SCRIPT_DIALOG_RESPONSE with a matching requestId.
 */
export function createDialogBridge(
  webContents: WebContents,
  options?: DialogBridgeOptions,
): (type: string, params: Record<string, unknown>) => Promise<unknown> {
  const timeoutMs = options?.timeoutMs ?? 30000;

  ensureResponseListener();

  return (type: string, params: Record<string, unknown>): Promise<unknown> => {
    if (webContents.isDestroyed()) {
      return Promise.reject(new Error('WebContents destroyed — cannot show dialog'));
    }

    const requestId = `dialog-${++requestIdCounter}-${Date.now()}`;

    return new Promise((resolve, reject) => {
      let timer: ReturnType<typeof setTimeout> | null = null;

      pendingRequests.set(requestId, (result: unknown) => {
        if (timer) clearTimeout(timer);
        resolve(result);
      });

      timer = setTimeout(() => {
        pendingRequests.delete(requestId);
        reject(new Error(`Dialog request timeout after ${timeoutMs}ms`));
      }, timeoutMs);

      webContents.send(IPC_CHANNELS.SCRIPT_DIALOG_REQUEST, {
        requestId,
        type,
        params,
      });
    });
  };
}
