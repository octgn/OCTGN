/**
 * ActionExecutor — executes card/group actions defined in game definitions
 * by calling the corresponding Python functions in the PythonScope.
 */

import { PythonScope } from './python-scope';
import type { CardAction, GroupAction } from '../../shared/types';

export interface ActionResult {
  success: boolean;
  error?: string;
}

export class ActionExecutor {
  private scope: PythonScope;

  constructor(scope: PythonScope) {
    this.scope = scope;
  }

  /**
   * Execute a card action's execute function with a Card argument.
   */
  async executeCardAction(action: CardAction, cardId: number): Promise<ActionResult> {
    const funcName = action.execute;
    if (!funcName || !this.scope.hasFunction(funcName)) {
      return { success: false, error: `Function '${funcName}' not found` };
    }

    const expr = `${funcName}(Card(${cardId}))`;
    const result = await this.scope.getRuntime().execute(expr);
    return { success: result.success, error: result.error };
  }

  /**
   * Execute a batch card action with a list of Card arguments.
   * Falls back to single execute per card if no batchExecute defined.
   */
  async executeBatchCardAction(action: CardAction, cardIds: number[]): Promise<ActionResult> {
    if (action.batchExecute && this.scope.hasFunction(action.batchExecute)) {
      const cardList = cardIds.map(id => `Card(${id})`).join(', ');
      const expr = `${action.batchExecute}([${cardList}])`;
      const result = await this.scope.getRuntime().execute(expr);
      return { success: result.success, error: result.error };
    }

    // Fallback: call single execute for each card
    for (const cardId of cardIds) {
      const result = await this.executeCardAction(action, cardId);
      if (!result.success) return result;
    }
    return { success: true };
  }

  /**
   * Execute a group action's execute function with a Group argument.
   * Builds a proper Pile(id, 'name', Player(pid)) constructor for the group.
   */
  async executeGroupAction(action: GroupAction, groupId: number): Promise<ActionResult> {
    const funcName = action.execute;
    if (!funcName || !this.scope.hasFunction(funcName)) {
      return { success: false, error: `Function '${funcName}' not found` };
    }

    const isTable = groupId === 0x01000000;
    let groupExpr: string;
    if (isTable) {
      groupExpr = 'table';
    } else {
      // Build Pile constructor: extract player ID and group name via _api
      const playerId = (groupId >> 16) & 0xFF;
      groupExpr = `Pile(${groupId}, _api.GroupGetName(${groupId}), Player(${playerId}))`;
    }

    const expr = `${funcName}(${groupExpr})`;
    const result = await this.scope.getRuntime().execute(expr);
    return { success: result.success, error: result.error };
  }

  /**
   * Evaluate a showIf function for a card action.
   * Returns true if the action should be shown (default if no showIf).
   */
  async evaluateShowIf(action: CardAction | GroupAction, cardOrGroupId: number): Promise<boolean> {
    if (!action.showIf) return true;
    if (!this.scope.hasFunction(action.showIf)) return true;

    try {
      const expr = `${action.showIf}(Card(${cardOrGroupId}))`;
      const result = await this.scope.getRuntime().execute(`_showif_result = ${expr}`);
      if (!result.success) return true;

      const check = await this.scope.getRuntime().execute('print(_showif_result)');
      return check.output?.trim() === 'True';
    } catch {
      return true;
    }
  }

  /**
   * Evaluate a getName function for a card action.
   * Returns the default action name if no getName or on error.
   */
  async evaluateGetName(action: CardAction | GroupAction, cardOrGroupId: number): Promise<string> {
    if (!action.getName) return action.name;
    if (!this.scope.hasFunction(action.getName)) return action.name;

    try {
      const expr = `${action.getName}(Card(${cardOrGroupId}))`;
      const result = await this.scope.getRuntime().execute(`_getname_result = ${expr}`);
      if (!result.success) return action.name;

      const check = await this.scope.getRuntime().execute('print(_getname_result)');
      const name = check.output?.trim();
      return name || action.name;
    } catch {
      return action.name;
    }
  }
}
