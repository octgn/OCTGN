/**
 * ScriptApi — the `_api` bridge between Python scripts and TypeScript game state.
 *
 * Python code calls `_api.MethodName(args)` which maps to methods here.
 * All external dependencies (game state, protocol, dialogs) are injected.
 */

import type {
  GameState,
  Card,
  Group,
  Player,
  GameDefinition,
} from '../../shared/types';

const TABLE_ID = 0x01000000;
const OCTGN_VERSION = '3.4.426.0';

/** Rotation enum values matching the WPF constants (Rot0=0, Rot90=1, etc.) */
const ROTATION_TO_DEGREES: Record<number, number> = { 0: 0, 1: 90, 2: 180, 3: 270 };
const DEGREES_TO_ROTATION: Record<number, number> = { 0: 0, 90: 1, 180: 2, 270: 3 };

export interface ScriptApiDeps {
  getGameState: () => GameState;
  getLocalPlayerId: () => number;
  getGameDefinition: () => GameDefinition | undefined;
  sendProtocolMessage: (type: string, params: Record<string, unknown>) => void;
  addChatMessage: (message: string, isSystem: boolean) => void;
  /** Synchronous dialog callback (legacy). */
  requestDialogSync?: (type: string, params: Record<string, unknown>) => unknown;
  /** Async dialog callback. Returns a Promise that Skulpt's proxy layer automatically
   *  converts to a suspension via promiseToSuspension, pausing Python execution
   *  until the dialog resolves. Preferred over requestDialogSync when available. */
  requestDialog?: (type: string, params: Record<string, unknown>) => Promise<unknown>;
}

export class ScriptApi {
  private deps: ScriptApiDeps;
  private muted = false;
  private pendingRandomResolve: ((value: number) => void) | null = null;

  constructor(deps: ScriptApiDeps) {
    this.deps = deps;
  }

  // ── Player API ──

  LocalPlayerId(): number {
    return this.deps.getLocalPlayerId();
  }

  SharedPlayerId(): number {
    const state = this.deps.getGameState();
    const global = state.players.find(p => p.id === 0);
    return global ? 0 : -1;
  }

  AllPlayers(): number[] {
    return this.deps.getGameState().players
      .filter(p => !p.isSpectator)
      .map(p => p.id);
  }

  PlayerName(id: number): string {
    return this.findPlayer(id)?.name ?? '';
  }

  PlayerColor(id: number): string {
    return this.findPlayer(id)?.color ?? '#000000';
  }

  PlayerCounters(id: number): Array<{ Key: number; Value: string }> {
    const player = this.findPlayer(id);
    if (!player) return [];
    return player.counters.map(c => ({ Key: c.id, Value: c.name }));
  }

  PlayerPiles(id: number): Array<{ Key: number; Value: string }> {
    const player = this.findPlayer(id);
    if (!player) return [];
    return player.groups.map(g => ({ Key: parseInt(g.id, 10), Value: g.name }));
  }

  PlayerHasInvertedTable(id: number): boolean {
    return this.findPlayer(id)?.invertedTable ?? false;
  }

  IsSubscriber(_id: number): boolean {
    return false; // Not implemented in Electron client
  }

  SetPlayerColor(id: number, color: string): void {
    this.deps.sendProtocolMessage('PlayerSettings', { player: id, color });
  }

  // ── Card API ──

  CardName(cardId: number): string {
    return this.findCard(cardId)?.name ?? '';
  }

  CardModel(cardId: number): string {
    return this.findCard(cardId)?.definitionId ?? '';
  }

  CardSet(_cardId: number): string {
    return ''; // Set name not stored on Card objects
  }

  CardSetId(_cardId: number): string {
    return ''; // Set ID not stored on Card objects
  }

  CardOwner(cardId: number): number {
    const card = this.findCard(cardId);
    return card ? parseInt(card.ownerId, 10) : -1;
  }

  CardController(cardId: number): number {
    return this.CardOwner(cardId); // Simplified; controller = owner
  }

  CardGetFaceUp(cardId: number): boolean {
    return this.findCard(cardId)?.faceUp ?? false;
  }

  CardSetFaceUp(cardId: number, up: boolean): void {
    this.deps.sendProtocolMessage('TurnReq', { card: cardId, up });
  }

  CardGetOrientation(cardId: number): number {
    const card = this.findCard(cardId);
    if (!card) return 0;
    return DEGREES_TO_ROTATION[card.rotation] ?? 0;
  }

  CardSetOrientation(cardId: number, rot: number): void {
    const degrees = ROTATION_TO_DEGREES[rot] ?? 0;
    this.deps.sendProtocolMessage('RotateReq', { card: cardId, rot: degrees });
  }

  CardPosition(cardId: number): [number, number] {
    const card = this.findCard(cardId);
    return card ? [card.position.x, card.position.y] : [0, 0];
  }

  CardGroup(cardId: number): number {
    const card = this.findCard(cardId);
    if (!card) return TABLE_ID;
    if (card.groupId === 'table') return TABLE_ID;
    return parseInt(card.groupId, 10);
  }

  CardMoveTo(cardId: number, groupId: number, index: number | null): void {
    if (groupId === TABLE_ID) {
      const card = this.findCard(cardId);
      const x = card?.position.x ?? 0;
      const y = card?.position.y ?? 0;
      this.deps.sendProtocolMessage('MoveCardAtReq', {
        id: [cardId], x: [x], y: [y], idx: [index ?? 0], faceUp: [true],
      });
    } else {
      this.deps.sendProtocolMessage('MoveCardReq', {
        id: [cardId], group: groupId, idx: [index ?? 0], faceUp: [true],
      });
    }
  }

  CardMoveToTable(cardId: number, x: number, y: number, forceFaceDown: boolean): void {
    this.deps.sendProtocolMessage('MoveCardAtReq', {
      id: [cardId], x: [x], y: [y], idx: [0], faceUp: [!forceFaceDown],
    });
  }

  CardProperty(cardId: number, key: string): string {
    const card = this.findCard(cardId);
    if (!card) return '';
    return card.properties[key] ?? '';
  }

  CardSetProperty(cardId: number, key: string, value: string): void {
    // Properties are typically set via protocol, but can update locally
    const card = this.findCard(cardId);
    if (card) {
      card.properties[key] = value;
    }
  }

  CardHasProperty(cardId: number, prop: string, _alt?: string): boolean {
    const card = this.findCard(cardId);
    return card ? prop in card.properties : false;
  }

  CardGetHighlight(cardId: number): string | null {
    return this.findCard(cardId)?.highlighted ?? null;
  }

  CardSetHighlight(cardId: number, color: string | null): void {
    this.deps.sendProtocolMessage('Highlight', { card: cardId, color: color ?? '' });
  }

  CardGetFilter(cardId: number): string | null {
    return null; // Filter not implemented yet
  }

  CardSetFilter(_cardId: number, _value: string | null): void {
    // Filter not implemented yet
  }

  CardGetMarkers(cardId: number): Array<{ Item1: string; Item2: string }> {
    const card = this.findCard(cardId);
    if (!card) return [];
    return card.markers.map(m => ({ Item1: m.name, Item2: m.id }));
  }

  CardGetIndex(cardId: number): number {
    const card = this.findCard(cardId);
    if (!card) return 0;
    const group = this.findGroupContaining(cardId);
    if (!group) return 0;
    return group.cards.findIndex(c => c.id === String(cardId));
  }

  CardSetIndex(cardId: number, index: number, _notify: boolean): void {
    // Index setting requires reorder protocol message
  }

  CardIsSelected(_cardId: number): boolean {
    return false; // Selection is a UI concept
  }

  CardSelect(_cardId: number, _selection: boolean): void {
    // Selection is a UI concept
  }

  CardPeek(cardId: number): void {
    this.deps.sendProtocolMessage('PeekReq', { card: cardId });
  }

  CardPeekers(cardId: number): number[] {
    const card = this.findCard(cardId);
    return card ? card.peekingPlayers.map(id => parseInt(id, 10)) : [];
  }

  CardTarget(cardId: number, active: boolean): void {
    this.deps.sendProtocolMessage('TargetReq', { card: cardId, active });
  }

  CardTargetArrow(cardId: number, targetCardId: number, active: boolean): void {
    this.deps.sendProtocolMessage('TargetArrowReq', {
      card: cardId, target: targetCardId, active,
    });
  }

  CardTargeted(cardId: number): number {
    const card = this.findCard(cardId);
    if (!card || !card.targetedBy) return -1;
    return parseInt(card.targetedBy, 10);
  }

  CardAnchored(_cardId: number): boolean {
    return false; // Anchor not implemented
  }

  CardSetAnchored(_cardId: number, _anchored: boolean): void {
    // Anchor not implemented
  }

  CardDelete(cardId: number): void {
    this.deps.sendProtocolMessage('DeleteCard', { card: cardId });
  }

  CardResetProperties(cardId: number): void {
    const card = this.findCard(cardId);
    if (card) {
      card.properties = {};
    }
  }

  CardAlternate(_cardId: number): string {
    return ''; // Alternate not implemented
  }

  CardSwitchTo(_cardId: number, _alt: string): void {
    // Alternate not implemented
  }

  CardAlternates(_cardId: number): string[] {
    return [];
  }

  RealHeight(cardId: number): number {
    return this.findCard(cardId)?.size.height ?? 88;
  }

  RealWidth(cardId: number): number {
    return this.findCard(cardId)?.size.width ?? 63;
  }

  CardSize(_cardId: number): { Name: string } {
    return { Name: 'default' };
  }

  SetController(cardId: number, playerId: number): void {
    this.deps.sendProtocolMessage('PassToReq', { card: cardId, player: playerId });
  }

  // ── Group API ──

  GroupGetName(groupId: number): string {
    if (groupId === TABLE_ID) return 'Table';
    const group = this.findGroup(groupId);
    return group?.name ?? 'Unknown';
  }

  GroupCount(groupId: number): number {
    if (groupId === TABLE_ID) {
      return this.deps.getGameState().table.cards.length;
    }
    const group = this.findGroup(groupId);
    return group?.cards.length ?? 0;
  }

  GroupCards(groupId: number): number[] {
    if (groupId === TABLE_ID) {
      return this.deps.getGameState().table.cards.map(c => parseInt(c.id, 10));
    }
    const group = this.findGroup(groupId);
    return group?.cards.map(c => parseInt(c.id, 10)) ?? [];
  }

  GroupCard(groupId: number, index: number): number {
    const cards = this.GroupCards(groupId);
    return cards[index] ?? -1;
  }

  GroupShuffle(groupId: number): void {
    this.deps.sendProtocolMessage('ShuffleDeprecated', { group: groupId, card: [] });
  }

  GroupGetVisibility(groupId: number): string {
    const group = this.findGroup(groupId);
    if (!group) return 'undefined';
    const map: Record<number, string> = { 0: 'undefined', 1: 'nobody', 2: 'owner', 3: 'everybody' };
    return map[group.visibility] ?? 'undefined';
  }

  GroupSetVisibility(groupId: number, value: string): void {
    this.deps.sendProtocolMessage('GroupVisReq', { group: groupId, value });
  }

  GroupViewers(groupId: number): number[] {
    return []; // Viewers not tracked in current state model
  }

  GroupAddViewer(groupId: number, playerId: number): void {
    this.deps.sendProtocolMessage('GroupVisAddReq', { group: groupId, player: playerId });
  }

  GroupRemoveViewer(groupId: number, playerId: number): void {
    this.deps.sendProtocolMessage('GroupVisRemoveReq', { group: groupId, player: playerId });
  }

  GroupController(groupId: number): number {
    const group = this.findGroup(groupId);
    return group?.controller ?? -1;
  }

  GroupSetController(groupId: number, playerId: number): void {
    this.deps.sendProtocolMessage('GroupControllerReq', { group: groupId, player: playerId });
  }

  GroupGetCollapsed(_groupId: number): boolean {
    return false;
  }

  GroupSetCollapsed(_groupId: number, _value: boolean): void {
    // UI-only
  }

  PileGetViewState(_groupId: number): string {
    return 'collapsed';
  }

  PileSetViewState(_groupId: number, _value: string): void {
    // UI-only
  }

  PileGetProtectionState(_groupId: number): string {
    return 'none';
  }

  PileSetProtectionState(_groupId: number, _value: string): void {
    // UI-only
  }

  GroupLookAt(_groupId: number, _value: number, _isTop: boolean): void {
    // UI-only
  }

  Create(model: string, groupId: number, quantity: number): number[] {
    // Creating cards requires server coordination
    this.deps.sendProtocolMessage('CreateCard', { model, group: groupId, quantity });
    return []; // IDs assigned by server
  }

  CreateOnTable(model: string, x: number, y: number, persist: boolean, quantity: number, faceDown: boolean): number[] {
    this.deps.sendProtocolMessage('CreateCardAt', {
      model, x, y, persist, quantity, faceDown,
    });
    return [];
  }

  // ── Counter API ──

  CounterGet(counterId: number): number {
    for (const player of this.deps.getGameState().players) {
      const counter = player.counters.find(c => c.id === counterId);
      if (counter) return counter.value;
    }
    return 0;
  }

  CounterSet(counterId: number, value: number): void {
    this.deps.sendProtocolMessage('CounterReq', {
      counter: counterId,
      value,
      isScriptChange: true,
    });
  }

  // ── Marker API ──

  MarkerGetCount(cardId: number, markerId: string, _name: string): number {
    const card = this.findCard(cardId);
    if (!card) return 0;
    const marker = card.markers.find(m => m.id === markerId);
    return marker?.count ?? 0;
  }

  MarkerSetCount(cardId: number, count: number, markerId: string, name: string): void {
    const currentCount = this.MarkerGetCount(cardId, markerId, name);
    if (count > currentCount) {
      this.deps.sendProtocolMessage('AddMarkerReq', {
        card: cardId,
        id: markerId,
        name,
        count: count - currentCount,
        origCount: currentCount,
      });
    } else if (count < currentCount) {
      this.deps.sendProtocolMessage('RemoveMarkerReq', {
        card: cardId,
        id: markerId,
        name,
        count: currentCount - count,
        origCount: currentCount,
      });
    }
  }

  // ── Variable API ──

  GetGlobalVariable(name: string): string {
    return this.deps.getGameState().globalVariables?.[name] ?? '';
  }

  SetGlobalVariable(name: string, value: string): void {
    const state = this.deps.getGameState();
    const oldval = state.globalVariables?.[name] ?? '';
    if (!state.globalVariables) {
      state.globalVariables = {};
    }
    state.globalVariables[name] = value;
    this.deps.sendProtocolMessage('SetGlobalVariable', { name, oldval, val: value });
  }

  PlayerGetGlobalVariable(playerId: number, name: string): string {
    const player = this.findPlayer(playerId);
    return player?.globalVariables[name] ?? '';
  }

  PlayerSetGlobalVariable(playerId: number, name: string, value: string): void {
    const player = this.findPlayer(playerId);
    const oldval = player?.globalVariables[name] ?? '';
    if (player) {
      player.globalVariables[name] = value;
    }
    this.deps.sendProtocolMessage('PlayerSetGlobalVariable', {
      player: playerId, name, oldval, val: value,
    });
  }

  // ── Notification API ──

  Notify(message: string): void {
    // WPF: sends PrintReq to server (broadcast to all) + prints locally
    this.deps.sendProtocolMessage('PrintReq', { text: message });
    this.deps.addChatMessage(message, true);
  }

  NotifyBar(_color: string, message: string): void {
    // WPF: local-only colored notification (no protocol message)
    this.deps.addChatMessage(message, true);
  }

  Whisper(message: string): void {
    // WPF: local-only private message (no protocol message)
    this.deps.addChatMessage(message, true);
  }

  Mute(muted: boolean): void {
    this.muted = muted;
  }

  isMuted(): boolean {
    return this.muted;
  }

  // ── Game Flow API ──

  TurnNumber(): number {
    return this.deps.getGameState().turnNumber;
  }

  GetActivePlayer(): number | null {
    const ap = this.deps.getGameState().activePlayer;
    return ap >= 0 ? ap : null;
  }

  SetActivePlayer(playerId?: number): void {
    if (playerId !== undefined) {
      this.deps.sendProtocolMessage('SetActivePlayer', { player: playerId });
    } else {
      this.deps.sendProtocolMessage('SetActivePlayer', {});
    }
  }

  NextTurn(playerIdOrForce?: number | boolean, force?: boolean): void {
    if (typeof playerIdOrForce === 'boolean') {
      this.deps.sendProtocolMessage('NextTurn', { force: playerIdOrForce });
    } else if (typeof playerIdOrForce === 'number') {
      this.deps.sendProtocolMessage('NextTurn', {
        player: playerIdOrForce,
        force: force ?? false,
      });
    } else {
      this.deps.sendProtocolMessage('NextTurn', { force: false });
    }
  }

  GetCurrentPhase(): { Item1: string; Item2: number } {
    const state = this.deps.getGameState();
    const def = this.deps.getGameDefinition();
    const phaseIdx = state.phase;
    const phaseName = def?.phases?.[phaseIdx]?.name ?? '';
    return { Item1: phaseName, Item2: phaseIdx };
  }

  SetPhase(id: number, force: boolean): void {
    this.deps.sendProtocolMessage('SetPhase', { phase: id, force });
  }

  GetStop(_id: number): boolean {
    return false;
  }

  SetStop(id: number, stop: boolean): void {
    this.deps.sendProtocolMessage('SetStop', { phase: id, stop });
  }

  IsTwoSided(): boolean {
    return this.deps.getGameState().useTwoSidedTable ?? false;
  }

  // ── Misc API ──

  Random(min: number, max: number): number | Promise<number> {
    if (!this.deps.sendProtocolMessage) {
      // Fallback to local random when no protocol connection
      return Math.floor(Math.random() * (max - min + 1)) + min;
    }
    // Send RandomReq to server and wait for Random response
    this.deps.sendProtocolMessage('RandomReq', { min, max });
    return new Promise<number>((resolve) => {
      this.pendingRandomResolve = resolve;
    });
  }

  /** Called when the server responds with a Random result. */
  handleRandomResult(result: number): void {
    if (this.pendingRandomResolve) {
      this.pendingRandomResolve(result);
      this.pendingRandomResolve = null;
    }
  }

  RandomArray(min: number, max: number, count: number): number[] {
    // WPF: RandomArray uses local random, NOT server-synchronized
    return Array.from({ length: count }, () =>
      Math.floor(Math.random() * (max - min + 1)) + min
    );
  }

  OCTGN_Version(): string {
    return OCTGN_VERSION;
  }

  GameDef_Version(): string {
    return this.deps.getGameDefinition()?.version ?? '0.0.0';
  }

  GetGameName(): string {
    return this.deps.getGameState().gameName;
  }

  CardProperties(): string[] {
    // TODO: extract from game definition when property definitions are available
    return [];
  }

  GroupCtor(groupId: number): string {
    if (groupId === TABLE_ID) return 'Table()';
    const group = this.findGroup(groupId);
    if (!group) return `Pile(${groupId}, '', Player(0))`;
    const playerId = (groupId >> 16) & 0xFF;
    return `Pile(${groupId}, '${group.name.replace(/'/g, "\\'")}', Player(${playerId}))`;
  }

  RemoteCall(playerId: number, functionName: string, args: string): void {
    this.deps.sendProtocolMessage('RemoteCall', {
      player: playerId,
      function: functionName,
      args,
    });
  }

  Update(): void {
    // Force UI render — no-op in Electron (React handles rendering)
  }

  ForceDisconnect(): void {
    this.deps.sendProtocolMessage('Leave', {});
  }

  ResetGame(): void {
    this.deps.sendProtocolMessage('ResetReq', {});
  }

  SoftResetGame(): void {
    this.deps.sendProtocolMessage('SoftResetReq', {});
  }

  ClearSelection(): void {
    // UI-only
  }

  Open_URL(url: string): void {
    // Will be handled by shell.openExternal in Phase 8
  }

  // ── Table API ──

  GetBoard(): string {
    return this.deps.getGameState().table.board?.name ?? '';
  }

  SetBoard(name: string): void {
    this.deps.sendProtocolMessage('SetBoard', { name });
  }

  GetBoardList(): string[] {
    const def = this.deps.getGameDefinition();
    return def?.boards?.map(b => b.name) ?? [];
  }

  IsTableBackgroundFlipped(): boolean {
    return false; // Not tracked in current state
  }

  SetTableBackgroundFlipped(_value: boolean): void {
    // Not implemented
  }

  TableResetScreen(): void {
    // UI-only
  }

  TableRefitScreen(): void {
    // UI-only
  }

  // ── Settings API ──

  GetSetting(name: string, defaultValue: string): string {
    // TODO: implement per-game settings storage
    return defaultValue;
  }

  SaveSetting(_name: string, _value: string): void {
    // TODO: implement per-game settings storage
  }

  // ── Dialog APIs ──
  // When requestDialog (async) is available, methods return Promises.
  // Skulpt's proxy layer automatically converts Promise returns to suspensions
  // via promiseToSuspension, pausing Python execution until the dialog resolves.
  // Falls back to requestDialogSync (legacy) or default values.

  Confirm(message: string): boolean | Promise<boolean> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('confirm', { message })
        .then(result => !!result)
        .catch(() => true);
    }
    if (!this.deps.requestDialogSync) return true;
    try {
      return !!this.deps.requestDialogSync('confirm', { message });
    } catch {
      return true;
    }
  }

  AskInteger(question: string, defaultAnswer: number): number | Promise<number | null> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('askInteger', { question, defaultAnswer })
        .then(result => result === null ? null : (typeof result === 'number' ? result : defaultAnswer))
        .catch(() => defaultAnswer);
    }
    if (!this.deps.requestDialogSync) return defaultAnswer;
    try {
      const result = this.deps.requestDialogSync('askInteger', { question, defaultAnswer });
      return typeof result === 'number' ? result : defaultAnswer;
    } catch {
      return defaultAnswer;
    }
  }

  AskString(question: string, defaultAnswer: string): string | Promise<string | null> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('askString', { question, defaultAnswer })
        .then(result => result === null ? null : (typeof result === 'string' ? result : defaultAnswer))
        .catch(() => defaultAnswer);
    }
    if (!this.deps.requestDialogSync) return defaultAnswer;
    try {
      const result = this.deps.requestDialogSync('askString', { question, defaultAnswer });
      return typeof result === 'string' ? result : defaultAnswer;
    } catch {
      return defaultAnswer;
    }
  }

  AskChoice(question: string, choices: string[], colors: string[], buttons: string[]): number | Promise<number> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('askChoice', { question, choices, colors, buttons })
        .then(result => typeof result === 'number' ? result : -1)
        .catch(() => -1);
    }
    if (!this.deps.requestDialogSync) return -1;
    try {
      const result = this.deps.requestDialogSync('askChoice', { question, choices, colors, buttons });
      return typeof result === 'number' ? result : -1;
    } catch {
      return -1;
    }
  }

  AskMarker(): unknown | Promise<unknown> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('askMarker', {})
        .then(result => result ?? null)
        .catch(() => null);
    }
    if (!this.deps.requestDialogSync) return null;
    try {
      return this.deps.requestDialogSync('askMarker', {}) ?? null;
    } catch {
      return null;
    }
  }

  AskCard(dict: Record<string, string[]>, operator: string | null, title: string): unknown | Promise<unknown> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('askCard', { properties: dict, operator, title })
        .then(result => result ?? null)
        .catch(() => null);
    }
    if (!this.deps.requestDialogSync) return null;
    try {
      return this.deps.requestDialogSync('askCard', { properties: dict, operator, title }) ?? null;
    } catch {
      return null;
    }
  }

  QueryCard(dict: Record<string, string[]>, exact: boolean): string[] | Promise<string[]> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('queryCard', { properties: dict, exact })
        .then(result => Array.isArray(result) ? result : [])
        .catch(() => []);
    }
    if (!this.deps.requestDialogSync) return [];
    try {
      const result = this.deps.requestDialogSync('queryCard', { properties: dict, exact });
      return Array.isArray(result) ? result : [];
    } catch {
      return [];
    }
  }

  SelectMultiCard(): unknown | Promise<unknown> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('selectMultiCard', {})
        .then(result => result ?? null)
        .catch(() => null);
    }
    if (!this.deps.requestDialogSync) return null;
    try {
      return this.deps.requestDialogSync('selectMultiCard', {}) ?? null;
    } catch {
      return null;
    }
  }

  Focus(cardIds: number[]): void {
    this.deps.sendProtocolMessage('ScriptFocus', { cards: cardIds });
  }

  ClearFocus(): void {
    this.deps.sendProtocolMessage('ScriptClearFocus', {});
  }

  GetFocusedCards(): null { return null; }

  PlaySound(name: string): void {
    this.deps.sendProtocolMessage('ScriptPlaySound', { name });
  }

  Web_Read(url: string, timeout: number): { Item1: string; Item2: number } | Promise<{ Item1: string; Item2: number }> {
    const defaultResult = { Item1: '', Item2: 0 };
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('webRead', { url, timeout })
        .then(result => {
          if (result && typeof result === 'object' && 'Item1' in result) {
            return result as { Item1: string; Item2: number };
          }
          return defaultResult;
        })
        .catch(() => defaultResult);
    }
    if (!this.deps.requestDialogSync) return defaultResult;
    try {
      const result = this.deps.requestDialogSync('webRead', { url, timeout });
      if (result && typeof result === 'object' && 'Item1' in result) {
        return result as { Item1: string; Item2: number };
      }
      return defaultResult;
    } catch {
      return defaultResult;
    }
  }

  Web_Post(url: string, data: string, timeout: number): { Item1: string; Item2: number } | Promise<{ Item1: string; Item2: number }> {
    const defaultResult = { Item1: '', Item2: 0 };
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('webPost', { url, data, timeout })
        .then(result => {
          if (result && typeof result === 'object' && 'Item1' in result) {
            return result as { Item1: string; Item2: number };
          }
          return defaultResult;
        })
        .catch(() => defaultResult);
    }
    if (!this.deps.requestDialogSync) return defaultResult;
    try {
      const result = this.deps.requestDialogSync('webPost', { url, data, timeout });
      if (result && typeof result === 'object' && 'Item1' in result) {
        return result as { Item1: string; Item2: number };
      }
      return defaultResult;
    } catch {
      return defaultResult;
    }
  }

  RndArray(min: number, max: number, count: number): number[] {
    const result: number[] = [];
    for (let i = 0; i < count; i++) {
      result.push(Math.floor(Math.random() * (max - min + 1)) + min);
    }
    return result;
  }

  ChooseCardPackage(): null { return null; }
  GenerateCardsFromPackage(_model: unknown): string[] { return []; }

  SaveFileDlg(): unknown | Promise<unknown> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('saveFile', {}).then(r => r ?? null).catch(() => null);
    }
    if (!this.deps.requestDialogSync) return null;
    try {
      return this.deps.requestDialogSync('saveFile', {}) ?? null;
    } catch {
      return null;
    }
  }

  OpenFileDlg(): unknown | Promise<unknown> {
    if (this.deps.requestDialog) {
      return this.deps.requestDialog('openFile', {}).then(r => r ?? null).catch(() => null);
    }
    if (!this.deps.requestDialogSync) return null;
    try {
      return this.deps.requestDialogSync('openFile', {}) ?? null;
    } catch {
      return null;
    }
  }

  ShowWinForm(_form: unknown): void { }

  // ── Internal helpers ──

  private findPlayer(id: number): Player | undefined {
    return this.deps.getGameState().players.find(p => p.id === id);
  }

  private findCard(cardId: number): Card | undefined {
    const state = this.deps.getGameState();
    const cardIdStr = String(cardId);

    // Check table
    const tableCard = state.table.cards.find(c => c.id === cardIdStr);
    if (tableCard) return tableCard;

    // Check all player groups
    for (const player of state.players) {
      for (const group of player.groups) {
        const card = group.cards.find(c => c.id === cardIdStr);
        if (card) return card;
      }
    }
    return undefined;
  }

  private findGroup(groupId: number): Group | undefined {
    const groupIdStr = String(groupId);
    for (const player of this.deps.getGameState().players) {
      const group = player.groups.find(g => g.id === groupIdStr);
      if (group) return group;
    }
    return undefined;
  }

  private findGroupContaining(cardId: number): { cards: Card[] } | undefined {
    const state = this.deps.getGameState();
    const cardIdStr = String(cardId);

    if (state.table.cards.some(c => c.id === cardIdStr)) {
      return state.table;
    }

    for (const player of state.players) {
      for (const group of player.groups) {
        if (group.cards.some(c => c.id === cardIdStr)) {
          return group;
        }
      }
    }
    return undefined;
  }
}
