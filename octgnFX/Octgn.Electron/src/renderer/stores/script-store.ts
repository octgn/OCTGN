import { create } from 'zustand';

export interface ScriptEvent {
  id: string;
  type: string;
  function?: string;
  args?: string;
  event?: string;
  playerId?: number;
  params?: Record<string, unknown>;
  timestamp: number;
  error?: string;
}

interface ScriptStore {
  /** Script events received from the engine */
  events: ScriptEvent[];
  /** Whether the script engine is active */
  isActive: boolean;
  /** Maximum events to retain */
  maxEvents: number;

  /** Subscribe to script events from main process */
  subscribe: () => () => void;
  /** Execute a script function */
  execute: (functionName: string, args?: string) => Promise<void>;
  /** Clear event log */
  clearEvents: () => void;
}

let eventCounter = 0;

export const useScriptStore = create<ScriptStore>((set, get) => ({
  events: [],
  isActive: false,
  maxEvents: 200,

  subscribe: () => {
    set({ isActive: true });

    const unsubscribe = window.octgn.onScriptEvent((data: unknown) => {
      const event = data as Record<string, unknown>;
      const scriptEvent: ScriptEvent = {
        id: String(++eventCounter),
        type: event.type as string,
        function: event.function as string | undefined,
        args: event.args as string | undefined,
        event: event.event as string | undefined,
        playerId: event.playerId as number | undefined,
        params: event.params as Record<string, unknown> | undefined,
        timestamp: (event.timestamp as number) ?? Date.now(),
        error: event.error as string | undefined,
      };

      const { events, maxEvents } = get();
      const updated = [...events, scriptEvent];
      set({
        events: updated.length > maxEvents ? updated.slice(-maxEvents) : updated,
      });
    });

    return () => {
      unsubscribe();
      set({ isActive: false });
    };
  },

  execute: async (functionName: string, args?: string) => {
    await window.octgn.executeScript(functionName, args);
  },

  clearEvents: () => set({ events: [] }),
}));
