/**
 * OCTGN Game Binary Protocol - Message Types and Shared Type Definitions
 *
 * This module defines the message type enumeration and TypeScript interfaces
 * used throughout the OCTGN game binary protocol implementation.
 *
 * Wire format:
 *   [4 bytes: total length (LE, includes these 4 bytes)]
 *   [4 bytes: IsMuted (int32 LE)]
 *   [1 byte: message type ID]
 *   [parameters...]
 */

/** All 111 message types in the OCTGN game binary protocol (IDs 0-110). */
export enum MessageType {
  Error = 0,
  Boot = 1,
  Kick = 2,
  Hello = 3,
  HelloAgain = 4,
  Welcome = 5,
  Settings = 6,
  PlayerSettings = 7,
  NewPlayer = 8,
  Leave = 9,
  Start = 10,
  ResetReq = 11,
  Reset = 12,
  NextTurn = 13,
  StopTurnReq = 14,
  StopTurn = 15,
  SetPhaseReq = 16,
  SetPhase = 17,
  StopPhaseReq = 18,
  SetActivePlayer = 19,
  ClearActivePlayer = 20,
  ChatReq = 21,
  Chat = 22,
  PrintReq = 23,
  Print = 24,
  RandomReq = 25,
  Random = 26,
  CounterReq = 27,
  Counter = 28,
  LoadDeck = 29,
  CreateCard = 30,
  CreateCardAt = 31,
  CreateAliasDeprecated = 32,
  MoveCardReq = 33,
  MoveCard = 34,
  MoveCardAtReq = 35,
  MoveCardAt = 36,
  PeekReq = 37,
  Peek = 38,
  UntargetReq = 39,
  Untarget = 40,
  TargetReq = 41,
  Target = 42,
  TargetArrowReq = 43,
  TargetArrow = 44,
  Highlight = 45,
  TurnReq = 46,
  Turn = 47,
  RotateReq = 48,
  Rotate = 49,
  ShuffleDeprecated = 50,
  Shuffled = 51,
  UnaliasGrpDeprecated = 52,
  UnaliasDeprecated = 53,
  AddMarkerReq = 54,
  AddMarker = 55,
  RemoveMarkerReq = 56,
  RemoveMarker = 57,
  TransferMarkerReq = 58,
  TransferMarker = 59,
  PassToReq = 60,
  PassTo = 61,
  TakeFromReq = 62,
  TakeFrom = 63,
  DontTakeReq = 64,
  DontTake = 65,
  FreezeCardsVisibility = 66,
  GroupVisReq = 67,
  GroupVis = 68,
  GroupVisAddReq = 69,
  GroupVisAdd = 70,
  GroupVisRemoveReq = 71,
  GroupVisRemove = 72,
  LookAtReq = 73,
  LookAt = 74,
  LookAtTopReq = 75,
  LookAtTop = 76,
  LookAtBottomReq = 77,
  LookAtBottom = 78,
  StartLimitedReq = 79,
  StartLimited = 80,
  CancelLimitedReq = 81,
  CancelLimited = 82,
  CardSwitchTo = 83,
  PlayerSetGlobalVariable = 84,
  SetGlobalVariable = 85,
  SwitchWithAlternate = 86,
  Ping = 87,
  IsTableBackgroundFlipped = 88,
  PlaySound = 89,
  Ready = 90,
  PlayerState = 91,
  RemoteCall = 92,
  GameStateReq = 93,
  GameState = 94,
  DeleteCard = 95,
  PlayerDisconnect = 96,
  AddPacksReq = 97,
  AddPacks = 98,
  AnchorCard = 99,
  SetCardProperty = 100,
  ResetCardProperties = 101,
  Filter = 102,
  SetBoard = 103,
  RemoveBoard = 104,
  SetPlayerColor = 105,
  RequestPileViewPermission = 106,
  GrantPileViewPermission = 107,
  Shake = 108,
  GroupProtectionReq = 109,
  GroupProtection = 110,
}

/**
 * A parsed protocol message envelope containing the muted flag,
 * message type, and the deserialized payload parameters.
 */
export interface ProtocolMessage {
  /** The IsMuted state at the time the message was sent. */
  isMuted: number;
  /** The message type identifier. */
  type: MessageType;
  /** The deserialized message parameters (varies by message type). */
  params: Record<string, unknown>;
}
