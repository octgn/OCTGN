/**
 * EventDispatcher — maps game events to Python function calls.
 *
 * Handles version-aware event name mapping (e.g. OnGameStart → OnGameStarted in v3.1.0.2)
 * and dispatches events to the Python scope with correct parameters.
 */

import { PythonScope } from './python-scope';

export type GameEvent = string;

export interface DispatchResult {
  success: boolean;
  skipped?: boolean;
  returnValue?: unknown;
  error?: string;
}

/**
 * Event name mapping from internal/legacy names to version-specific Python function names.
 * v3.1.0.2 renamed most events to past tense.
 */
const EVENT_MAP_3102: Record<string, string> = {
  OnTableLoad: 'OnTableLoaded',
  OnGameStart: 'OnGameStarted',
  OnPlayerConnect: 'OnPlayerConnected',
  OnPlayerLeaveGame: 'OnPlayerQuit',
  OnLoadDeck: 'OnDeckLoaded',
  OnChangeCounter: 'OnCounterChanged',
  OnEndTurn: 'OnTurnPassed',
  OnTurn: 'OnTurnPassed',
  OnTargetCard: 'OnCardTargeted',
  OnTargetCardArrow: 'OnCardArrowTargeted',
  OnCardClick: 'OnCardClicked',
  OnCardDoubleClick: 'OnCardDoubleClicked',
  OnMoveCard: 'OnCardsMoved',
  OnMoveCards: 'OnCardsMoved',
  OnScriptedMoveCard: 'OnScriptedCardsMoved',
  OnScriptedMoveCards: 'OnScriptedCardsMoved',
  OnMarkerChanged: 'OnMarkerChanged', // Same name
  OnPlayerGlobalVariableChanged: 'OnPlayerGlobalVariableChanged',
  OnGlobalVariableChanged: 'OnGlobalVariableChanged',
  OnPhasePassed: 'OnPhasePassed',
  OnCounterChanged: 'OnCounterChanged',
  OnCardsMoved: 'OnCardsMoved',
  OnScriptedCardsMoved: 'OnScriptedCardsMoved',
  OnCardTargeted: 'OnCardTargeted',
  OnCardArrowTargeted: 'OnCardArrowTargeted',
  OnCardClicked: 'OnCardClicked',
  OnCardDoubleClicked: 'OnCardDoubleClicked',
  OnGameStarted: 'OnGameStarted',
  OnTableLoaded: 'OnTableLoaded',
  OnPlayerConnected: 'OnPlayerConnected',
  OnPlayerQuit: 'OnPlayerQuit',
  OnDeckLoaded: 'OnDeckLoaded',
  OnTurnPassed: 'OnTurnPassed',
  OnTurnPaused: 'OnTurnPaused',
  OnCardControllerChanged: 'OnCardControllerChanged',
  OnPhasePaused: 'OnPhasePaused',
};

/** Override events (v3.1.0.2 only) — returning True prevents default behavior. */
const OVERRIDE_EVENTS = new Set([
  'OverrideCardsMoved',
  'OverrideTurnPassed',
  'OverrideGameReset',
  'OverrideGameSoftReset',
  'OverridePhasePassed',
  'OverridePhaseClicked',
]);

export class EventDispatcher {
  private version: string;

  constructor(version: string) {
    this.version = version;
  }

  /**
   * Map an event name to the version-appropriate Python function name.
   */
  mapEventName(event: string): string {
    if (this.version === '3.1.0.2') {
      return EVENT_MAP_3102[event] ?? event;
    }
    // v3.1.0.0 and v3.1.0.1 use the original names
    return event;
  }

  /**
   * Check if an event is an override event (return value suppresses default).
   */
  isOverrideEvent(event: string): boolean {
    return OVERRIDE_EVENTS.has(event);
  }

  /**
   * Dispatch a game event to the Python scope.
   * Returns the dispatch result including any return value from override events.
   */
  async dispatch(
    scope: PythonScope,
    event: string,
    params: Record<string, unknown>,
  ): Promise<DispatchResult> {
    const functionName = this.mapEventName(event);

    // Check if the function exists in the scope
    if (!scope.hasFunction(functionName)) {
      return { success: true, skipped: true };
    }

    try {
      // Build argument list from params
      const args = this.buildArgs(functionName, params);
      const result = await scope.callFunction(functionName, ...args);

      if (!result.success) {
        return {
          success: false,
          error: result.error,
        };
      }

      return {
        success: true,
        returnValue: result.returnValue,
      };
    } catch (err: unknown) {
      return {
        success: false,
        error: err instanceof Error ? err.message : String(err),
      };
    }
  }

  /**
   * Build the argument list for a given event function.
   * For now, passes params as positional args based on known event signatures.
   */
  private buildArgs(functionName: string, params: Record<string, unknown>): unknown[] {
    // For override events, pass params as positional args
    if (this.isOverrideEvent(functionName)) {
      return this.extractOrderedArgs(functionName, params);
    }

    // For notification events, pass params as positional args
    return this.extractOrderedArgs(functionName, params);
  }

  /**
   * Extract arguments in the correct order for a function.
   * Uses known event signatures from GameEvents.xml.
   */
  private extractOrderedArgs(functionName: string, params: Record<string, unknown>): unknown[] {
    const signature = EVENT_SIGNATURES[functionName];
    if (!signature) {
      // Unknown event — pass all values in order
      return Object.values(params);
    }
    return signature.map(paramName => params[paramName]);
  }
}

/**
 * Known event parameter signatures (ordered parameter names).
 * Derived from GameEvents.xml for v3.1.0.2.
 */
const EVENT_SIGNATURES: Record<string, string[]> = {
  // Notification events
  OnTableLoaded: [],
  OnGameStarted: [],
  OnPlayerConnected: ['player'],
  OnPlayerQuit: ['player'],
  OnDeckLoaded: ['player', 'isLimited', 'groups'],
  OnCounterChanged: ['player', 'counter', 'value', 'scripted'],
  OnTurnPaused: ['player'],
  OnTurnPassed: ['player', 'turn', 'force'],
  OnCardTargeted: ['player', 'card', 'targeted', 'scripted'],
  OnCardArrowTargeted: ['player', 'fromCard', 'toCard', 'targeted', 'scripted'],
  OnPlayerGlobalVariableChanged: ['player', 'name', 'oldValue', 'value'],
  OnGlobalVariableChanged: ['name', 'oldValue', 'value'],
  OnCardClicked: ['card', 'mouseButton', 'keysDown'],
  OnCardDoubleClicked: ['card', 'mouseButton', 'keysDown'],
  OnMarkerChanged: ['card', 'marker', 'id', 'value', 'scripted'],
  OnCardControllerChanged: ['card', 'oldPlayer', 'player'],
  OnCardsMoved: ['player', 'cards', 'fromGroups', 'toGroups', 'indexs', 'xs', 'ys', 'highlights', 'markers', 'faceups', 'filters', 'alternates'],
  OnScriptedCardsMoved: ['player', 'cards', 'fromGroups', 'toGroups', 'indexs', 'xs', 'ys', 'highlights', 'markers', 'faceups', 'filters', 'alternates'],
  OnPhasePassed: ['name', 'id', 'force'],
  OnPhasePaused: ['player'],
  // Override events
  OverrideCardsMoved: ['cards', 'toGroups', 'indexs', 'xs', 'ys', 'faceups'],
  OverrideTurnPassed: ['player'],
  OverrideGameReset: [],
  OverrideGameSoftReset: [],
  OverridePhasePassed: ['name', 'id'],
  OverridePhaseClicked: ['name', 'id'],
  // Legacy event names (v3.1.0.0/3.1.0.1)
  OnTableLoad: [],
  OnGameStart: [],
  OnLoadDeck: ['player', 'groups'],
  OnChangeCounter: ['player', 'counter', 'oldValue'],
  OnEndTurn: ['player'],
  OnTurn: ['player', 'turnNumber'],
  OnTargetCard: ['player', 'card', 'isTargeted'],
  OnTargetCardArrow: ['player', 'fromCard', 'toCard', 'isTargeted'],
  OnCardClick: ['card', 'mouseButton', 'keysDown'],
  OnCardDoubleClick: ['card', 'mouseButton', 'keysDown'],
  OnMoveCard: ['player', 'card', 'fromGroup', 'toGroup', 'oldIndex', 'index', 'oldX', 'oldY', 'x', 'y', 'isScriptMove'],
  OnMoveCards: ['player', 'cards', 'fromGroups', 'toGroups', 'oldIndexs', 'indexs', 'oldX', 'oldY', 'x', 'y', 'highlights', 'markers', 'isScriptMove'],
  OnPlayerConnect: ['player'],
  OnPlayerLeaveGame: ['player'],
};
