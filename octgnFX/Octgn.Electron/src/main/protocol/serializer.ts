/**
 * OCTGN Game Binary Protocol - Message Serializer / Deserializer
 *
 * Provides serialize and deserialize functions for all 111 message types.
 * Each message type has a pair of functions registered in lookup maps
 * keyed by {@link MessageType}.
 *
 * Wire format (after the 4-byte length prefix has been stripped):
 *   [4 bytes: IsMuted (int32 LE)]
 *   [1 byte: message type ID]
 *   [parameters...]
 */

import { BinaryReader } from './binary-reader';
import { BinaryWriter } from './binary-writer';
import { MessageType, type ProtocolMessage } from './types';

// ---------------------------------------------------------------------------
// Public types
// ---------------------------------------------------------------------------

/** A function that writes message parameters to a BinaryWriter. */
type SerializeFn = (writer: BinaryWriter, params: Record<string, unknown>) => void;

/** A function that reads message parameters from a BinaryReader. */
type DeserializeFn = (reader: BinaryReader) => Record<string, unknown>;

// ---------------------------------------------------------------------------
// Registries
// ---------------------------------------------------------------------------

const serializers = new Map<MessageType, SerializeFn>();
const deserializers = new Map<MessageType, DeserializeFn>();

function register(
  type: MessageType,
  serialize: SerializeFn,
  deserialize: DeserializeFn,
): void {
  serializers.set(type, serialize);
  deserializers.set(type, deserialize);
}

// ---------------------------------------------------------------------------
// Message definitions (all 111)
// ---------------------------------------------------------------------------

// 0 - Error
register(
  MessageType.Error,
  (w, p) => { w.writeString(p.msg as string); },
  (r) => ({ msg: r.readString() }),
);

// 1 - Boot
register(
  MessageType.Boot,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.reason as string); },
  (r) => ({ player: r.readByte(), reason: r.readString() }),
);

// 2 - Kick
register(
  MessageType.Kick,
  (w, p) => { w.writeString(p.reason as string); },
  (r) => ({ reason: r.readString() }),
);

// 3 - Hello
register(
  MessageType.Hello,
  (w, p) => {
    w.writeString(p.nick as string);
    w.writeString(p.userId as string);
    w.writeUint64(p.pkey as bigint);
    w.writeString(p.client as string);
    w.writeString(p.clientVer as string);
    w.writeString(p.octgnVer as string);
    w.writeGuid(p.gameId as string);
    w.writeString(p.gameVersion as string);
    w.writeString(p.password as string);
    w.writeBool(p.spectator as boolean);
  },
  (r) => ({
    nick: r.readString(),
    userId: r.readString(),
    pkey: r.readUint64(),
    client: r.readString(),
    clientVer: r.readString(),
    octgnVer: r.readString(),
    gameId: r.readGuid(),
    gameVersion: r.readString(),
    password: r.readString(),
    spectator: r.readBool(),
  }),
);

// 4 - HelloAgain
register(
  MessageType.HelloAgain,
  (w, p) => {
    w.writeByte(p.pid as number);
    w.writeString(p.nick as string);
    w.writeString(p.userId as string);
    w.writeUint64(p.pkey as bigint);
    w.writeString(p.client as string);
    w.writeString(p.clientVer as string);
    w.writeString(p.octgnVer as string);
    w.writeGuid(p.gameId as string);
    w.writeString(p.gameVersion as string);
    w.writeString(p.password as string);
  },
  (r) => ({
    pid: r.readByte(),
    nick: r.readString(),
    userId: r.readString(),
    pkey: r.readUint64(),
    client: r.readString(),
    clientVer: r.readString(),
    octgnVer: r.readString(),
    gameId: r.readGuid(),
    gameVersion: r.readString(),
    password: r.readString(),
  }),
);

// 5 - Welcome
register(
  MessageType.Welcome,
  (w, p) => {
    w.writeByte(p.id as number);
    w.writeGuid(p.gameSessionId as string);
    w.writeString(p.gameName as string);
    w.writeBool(p.waitForGameState as boolean);
  },
  (r) => ({
    id: r.readByte(),
    gameSessionId: r.readGuid(),
    gameName: r.readString(),
    waitForGameState: r.readBool(),
  }),
);

// 6 - Settings
register(
  MessageType.Settings,
  (w, p) => {
    w.writeBool(p.twoSidedTable as boolean);
    w.writeBool(p.allowSpectators as boolean);
    w.writeBool(p.muteSpectators as boolean);
    w.writeBool(p.allowCardList as boolean);
  },
  (r) => ({
    twoSidedTable: r.readBool(),
    allowSpectators: r.readBool(),
    muteSpectators: r.readBool(),
    allowCardList: r.readBool(),
  }),
);

// 7 - PlayerSettings
register(
  MessageType.PlayerSettings,
  (w, p) => {
    w.writeByte(p.playerId as number);
    w.writeBool(p.invertedTable as boolean);
    w.writeBool(p.spectator as boolean);
  },
  (r) => ({
    playerId: r.readByte(),
    invertedTable: r.readBool(),
    spectator: r.readBool(),
  }),
);

// 8 - NewPlayer
register(
  MessageType.NewPlayer,
  (w, p) => {
    w.writeByte(p.id as number);
    w.writeString(p.nick as string);
    w.writeString(p.userId as string);
    w.writeUint64(p.pkey as bigint);
    w.writeBool(p.tableSide as boolean);
    w.writeBool(p.spectator as boolean);
  },
  (r) => ({
    id: r.readByte(),
    nick: r.readString(),
    userId: r.readString(),
    pkey: r.readUint64(),
    tableSide: r.readBool(),
    spectator: r.readBool(),
  }),
);

// 9 - Leave
register(
  MessageType.Leave,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 10 - Start
register(
  MessageType.Start,
  () => {},
  () => ({}),
);

// 11 - ResetReq
register(
  MessageType.ResetReq,
  (w, p) => { w.writeBool(p.isSoft as boolean); },
  (r) => ({ isSoft: r.readBool() }),
);

// 12 - Reset
register(
  MessageType.Reset,
  (w, p) => { w.writeByte(p.player as number); w.writeBool(p.isSoft as boolean); },
  (r) => ({ player: r.readByte(), isSoft: r.readBool() }),
);

// 13 - NextTurn
register(
  MessageType.NextTurn,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeBool(p.setActive as boolean);
    w.writeBool(p.force as boolean);
  },
  (r) => ({ player: r.readByte(), setActive: r.readBool(), force: r.readBool() }),
);

// 14 - StopTurnReq
register(
  MessageType.StopTurnReq,
  (w, p) => { w.writeInt32(p.turnNumber as number); w.writeBool(p.stop as boolean); },
  (r) => ({ turnNumber: r.readInt32(), stop: r.readBool() }),
);

// 15 - StopTurn
register(
  MessageType.StopTurn,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 16 - SetPhaseReq
register(
  MessageType.SetPhaseReq,
  (w, p) => { w.writeByte(p.nextPhase as number); w.writeBool(p.force as boolean); },
  (r) => ({ nextPhase: r.readByte(), force: r.readBool() }),
);

// 17 - SetPhase
register(
  MessageType.SetPhase,
  (w, p) => {
    w.writeByte(p.phase as number);
    w.writeByteArray(p.players as number[]);
    w.writeBool(p.force as boolean);
  },
  (r) => ({ phase: r.readByte(), players: r.readByteArray(), force: r.readBool() }),
);

// 18 - StopPhaseReq
register(
  MessageType.StopPhaseReq,
  (w, p) => { w.writeByte(p.phase as number); w.writeBool(p.stop as boolean); },
  (r) => ({ phase: r.readByte(), stop: r.readBool() }),
);

// 19 - SetActivePlayer
register(
  MessageType.SetActivePlayer,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 20 - ClearActivePlayer
register(
  MessageType.ClearActivePlayer,
  () => {},
  () => ({}),
);

// 21 - ChatReq
register(
  MessageType.ChatReq,
  (w, p) => { w.writeString(p.text as string); },
  (r) => ({ text: r.readString() }),
);

// 22 - Chat
register(
  MessageType.Chat,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.text as string); },
  (r) => ({ player: r.readByte(), text: r.readString() }),
);

// 23 - PrintReq
register(
  MessageType.PrintReq,
  (w, p) => { w.writeString(p.text as string); },
  (r) => ({ text: r.readString() }),
);

// 24 - Print
register(
  MessageType.Print,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.text as string); },
  (r) => ({ player: r.readByte(), text: r.readString() }),
);

// 25 - RandomReq
register(
  MessageType.RandomReq,
  (w, p) => { w.writeInt32(p.min as number); w.writeInt32(p.max as number); },
  (r) => ({ min: r.readInt32(), max: r.readInt32() }),
);

// 26 - Random
register(
  MessageType.Random,
  (w, p) => { w.writeInt32(p.result as number); },
  (r) => ({ result: r.readInt32() }),
);

// 27 - CounterReq
register(
  MessageType.CounterReq,
  (w, p) => {
    w.writeInt32(p.counter as number);
    w.writeInt32(p.value as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({ counter: r.readInt32(), value: r.readInt32(), isScriptChange: r.readBool() }),
);

// 28 - Counter
register(
  MessageType.Counter,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.counter as number);
    w.writeInt32(p.value as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    player: r.readByte(),
    counter: r.readInt32(),
    value: r.readInt32(),
    isScriptChange: r.readBool(),
  }),
);

// 29 - LoadDeck
register(
  MessageType.LoadDeck,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeGuidArray(p.type as string[]);
    w.writeIntArray(p.group as number[]);
    w.writeStringArray(p.size as string[]);
    w.writeString(p.sleeve as string);
    w.writeBool(p.limited as boolean);
  },
  (r) => ({
    id: r.readIntArray(),
    type: r.readGuidArray(),
    group: r.readIntArray(),
    size: r.readStringArray(),
    sleeve: r.readString(),
    limited: r.readBool(),
  }),
);

// 30 - CreateCard
register(
  MessageType.CreateCard,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeGuidArray(p.type as string[]);
    w.writeStringArray(p.size as string[]);
    w.writeInt32(p.group as number);
  },
  (r) => ({
    id: r.readIntArray(),
    type: r.readGuidArray(),
    size: r.readStringArray(),
    group: r.readInt32(),
  }),
);

// 31 - CreateCardAt
register(
  MessageType.CreateCardAt,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeGuidArray(p.modelId as string[]);
    w.writeIntArray(p.x as number[]);
    w.writeIntArray(p.y as number[]);
    w.writeBool(p.faceUp as boolean);
    w.writeBool(p.persist as boolean);
  },
  (r) => ({
    id: r.readIntArray(),
    modelId: r.readGuidArray(),
    x: r.readIntArray(),
    y: r.readIntArray(),
    faceUp: r.readBool(),
    persist: r.readBool(),
  }),
);

// 32 - CreateAliasDeprecated
register(
  MessageType.CreateAliasDeprecated,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeUint64Array(p.type as bigint[]);
  },
  (r) => ({ id: r.readIntArray(), type: r.readUint64Array() }),
);

// 33 - MoveCardReq
register(
  MessageType.MoveCardReq,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeInt32(p.group as number);
    w.writeIntArray(p.idx as number[]);
    w.writeBoolArray(p.faceUp as boolean[]);
    w.writeBool(p.isScriptMove as boolean);
  },
  (r) => ({
    id: r.readIntArray(),
    group: r.readInt32(),
    idx: r.readIntArray(),
    faceUp: r.readBoolArray(),
    isScriptMove: r.readBool(),
  }),
);

// 34 - MoveCard
register(
  MessageType.MoveCard,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeIntArray(p.id as number[]);
    w.writeInt32(p.group as number);
    w.writeIntArray(p.idx as number[]);
    w.writeBoolArray(p.faceUp as boolean[]);
    w.writeBool(p.isScriptMove as boolean);
  },
  (r) => ({
    player: r.readByte(),
    id: r.readIntArray(),
    group: r.readInt32(),
    idx: r.readIntArray(),
    faceUp: r.readBoolArray(),
    isScriptMove: r.readBool(),
  }),
);

// 35 - MoveCardAtReq
register(
  MessageType.MoveCardAtReq,
  (w, p) => {
    w.writeIntArray(p.id as number[]);
    w.writeIntArray(p.x as number[]);
    w.writeIntArray(p.y as number[]);
    w.writeIntArray(p.idx as number[]);
    w.writeBool(p.isScriptMove as boolean);
    w.writeBoolArray(p.faceUp as boolean[]);
  },
  (r) => ({
    id: r.readIntArray(),
    x: r.readIntArray(),
    y: r.readIntArray(),
    idx: r.readIntArray(),
    isScriptMove: r.readBool(),
    faceUp: r.readBoolArray(),
  }),
);

// 36 - MoveCardAt
register(
  MessageType.MoveCardAt,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeIntArray(p.id as number[]);
    w.writeIntArray(p.x as number[]);
    w.writeIntArray(p.y as number[]);
    w.writeIntArray(p.idx as number[]);
    w.writeBoolArray(p.faceUp as boolean[]);
    w.writeBool(p.isScriptMove as boolean);
  },
  (r) => ({
    player: r.readByte(),
    id: r.readIntArray(),
    x: r.readIntArray(),
    y: r.readIntArray(),
    idx: r.readIntArray(),
    faceUp: r.readBoolArray(),
    isScriptMove: r.readBool(),
  }),
);

// 37 - PeekReq
register(
  MessageType.PeekReq,
  (w, p) => { w.writeInt32(p.card as number); },
  (r) => ({ card: r.readInt32() }),
);

// 38 - Peek
register(
  MessageType.Peek,
  (w, p) => { w.writeByte(p.player as number); w.writeInt32(p.card as number); },
  (r) => ({ player: r.readByte(), card: r.readInt32() }),
);

// 39 - UntargetReq
register(
  MessageType.UntargetReq,
  (w, p) => { w.writeInt32(p.card as number); w.writeBool(p.isScriptChange as boolean); },
  (r) => ({ card: r.readInt32(), isScriptChange: r.readBool() }),
);

// 40 - Untarget
register(
  MessageType.Untarget,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({ player: r.readByte(), card: r.readInt32(), isScriptChange: r.readBool() }),
);

// 41 - TargetReq
register(
  MessageType.TargetReq,
  (w, p) => { w.writeInt32(p.card as number); w.writeBool(p.isScriptChange as boolean); },
  (r) => ({ card: r.readInt32(), isScriptChange: r.readBool() }),
);

// 42 - Target
register(
  MessageType.Target,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({ player: r.readByte(), card: r.readInt32(), isScriptChange: r.readBool() }),
);

// 43 - TargetArrowReq
register(
  MessageType.TargetArrowReq,
  (w, p) => {
    w.writeInt32(p.card as number);
    w.writeInt32(p.otherCard as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({ card: r.readInt32(), otherCard: r.readInt32(), isScriptChange: r.readBool() }),
);

// 44 - TargetArrow
register(
  MessageType.TargetArrow,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeInt32(p.otherCard as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    player: r.readByte(),
    card: r.readInt32(),
    otherCard: r.readInt32(),
    isScriptChange: r.readBool(),
  }),
);

// 45 - Highlight
register(
  MessageType.Highlight,
  (w, p) => { w.writeInt32(p.card as number); w.writeString(p.color as string); },
  (r) => ({ card: r.readInt32(), color: r.readString() }),
);

// 46 - TurnReq
register(
  MessageType.TurnReq,
  (w, p) => { w.writeInt32(p.card as number); w.writeBool(p.up as boolean); },
  (r) => ({ card: r.readInt32(), up: r.readBool() }),
);

// 47 - Turn
register(
  MessageType.Turn,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeBool(p.up as boolean);
  },
  (r) => ({ player: r.readByte(), card: r.readInt32(), up: r.readBool() }),
);

// 48 - RotateReq
register(
  MessageType.RotateReq,
  (w, p) => { w.writeInt32(p.card as number); w.writeByte(p.rot as number); },
  (r) => ({ card: r.readInt32(), rot: r.readByte() }),
);

// 49 - Rotate
register(
  MessageType.Rotate,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeByte(p.rot as number);
  },
  (r) => ({ player: r.readByte(), card: r.readInt32(), rot: r.readByte() }),
);

// 50 - ShuffleDeprecated
register(
  MessageType.ShuffleDeprecated,
  (w, p) => { w.writeInt32(p.group as number); w.writeIntArray(p.card as number[]); },
  (r) => ({ group: r.readInt32(), card: r.readIntArray() }),
);

// 51 - Shuffled
register(
  MessageType.Shuffled,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.group as number);
    w.writeIntArray(p.card as number[]);
    w.writeShortArray(p.pos as number[]);
  },
  (r) => ({
    player: r.readByte(),
    group: r.readInt32(),
    card: r.readIntArray(),
    pos: r.readShortArray(),
  }),
);

// 52 - UnaliasGrpDeprecated
register(
  MessageType.UnaliasGrpDeprecated,
  (w, p) => { w.writeInt32(p.group as number); },
  (r) => ({ group: r.readInt32() }),
);

// 53 - UnaliasDeprecated
register(
  MessageType.UnaliasDeprecated,
  (w, p) => {
    w.writeIntArray(p.card as number[]);
    w.writeUint64Array(p.type as bigint[]);
  },
  (r) => ({ card: r.readIntArray(), type: r.readUint64Array() }),
);

// 54 - AddMarkerReq
register(
  MessageType.AddMarkerReq,
  (w, p) => {
    w.writeInt32(p.card as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    card: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 55 - AddMarker
register(
  MessageType.AddMarker,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    player: r.readByte(),
    card: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 56 - RemoveMarkerReq
register(
  MessageType.RemoveMarkerReq,
  (w, p) => {
    w.writeInt32(p.card as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    card: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 57 - RemoveMarker
register(
  MessageType.RemoveMarker,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    player: r.readByte(),
    card: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 58 - TransferMarkerReq
register(
  MessageType.TransferMarkerReq,
  (w, p) => {
    w.writeInt32(p.from as number);
    w.writeInt32(p.to as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    from: r.readInt32(),
    to: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 59 - TransferMarker
register(
  MessageType.TransferMarker,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.from as number);
    w.writeInt32(p.to as number);
    w.writeString(p.id as string);
    w.writeString(p.name as string);
    w.writeUint16(p.count as number);
    w.writeUint16(p.origCount as number);
    w.writeBool(p.isScriptChange as boolean);
  },
  (r) => ({
    player: r.readByte(),
    from: r.readInt32(),
    to: r.readInt32(),
    id: r.readString(),
    name: r.readString(),
    count: r.readUint16(),
    origCount: r.readUint16(),
    isScriptChange: r.readBool(),
  }),
);

// 60 - PassToReq
register(
  MessageType.PassToReq,
  (w, p) => {
    w.writeInt32(p.id as number);
    w.writeByte(p.to as number);
    w.writeBool(p.requested as boolean);
  },
  (r) => ({ id: r.readInt32(), to: r.readByte(), requested: r.readBool() }),
);

// 61 - PassTo
register(
  MessageType.PassTo,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.id as number);
    w.writeByte(p.to as number);
    w.writeBool(p.requested as boolean);
  },
  (r) => ({
    player: r.readByte(),
    id: r.readInt32(),
    to: r.readByte(),
    requested: r.readBool(),
  }),
);

// 62 - TakeFromReq
register(
  MessageType.TakeFromReq,
  (w, p) => { w.writeInt32(p.id as number); w.writeByte(p.from as number); },
  (r) => ({ id: r.readInt32(), from: r.readByte() }),
);

// 63 - TakeFrom
register(
  MessageType.TakeFrom,
  (w, p) => { w.writeInt32(p.id as number); w.writeByte(p.to as number); },
  (r) => ({ id: r.readInt32(), to: r.readByte() }),
);

// 64 - DontTakeReq
register(
  MessageType.DontTakeReq,
  (w, p) => { w.writeInt32(p.id as number); w.writeByte(p.to as number); },
  (r) => ({ id: r.readInt32(), to: r.readByte() }),
);

// 65 - DontTake
register(
  MessageType.DontTake,
  (w, p) => { w.writeInt32(p.id as number); },
  (r) => ({ id: r.readInt32() }),
);

// 66 - FreezeCardsVisibility
register(
  MessageType.FreezeCardsVisibility,
  (w, p) => { w.writeInt32(p.group as number); },
  (r) => ({ group: r.readInt32() }),
);

// 67 - GroupVisReq
register(
  MessageType.GroupVisReq,
  (w, p) => {
    w.writeInt32(p.group as number);
    w.writeBool(p.defined as boolean);
    w.writeBool(p.visible as boolean);
  },
  (r) => ({ group: r.readInt32(), defined: r.readBool(), visible: r.readBool() }),
);

// 68 - GroupVis
register(
  MessageType.GroupVis,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.group as number);
    w.writeBool(p.defined as boolean);
    w.writeBool(p.visible as boolean);
  },
  (r) => ({
    player: r.readByte(),
    group: r.readInt32(),
    defined: r.readBool(),
    visible: r.readBool(),
  }),
);

// 69 - GroupVisAddReq
register(
  MessageType.GroupVisAddReq,
  (w, p) => { w.writeInt32(p.group as number); w.writeByte(p.who as number); },
  (r) => ({ group: r.readInt32(), who: r.readByte() }),
);

// 70 - GroupVisAdd
register(
  MessageType.GroupVisAdd,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.group as number);
    w.writeByte(p.who as number);
  },
  (r) => ({ player: r.readByte(), group: r.readInt32(), who: r.readByte() }),
);

// 71 - GroupVisRemoveReq
register(
  MessageType.GroupVisRemoveReq,
  (w, p) => { w.writeInt32(p.group as number); w.writeByte(p.who as number); },
  (r) => ({ group: r.readInt32(), who: r.readByte() }),
);

// 72 - GroupVisRemove
register(
  MessageType.GroupVisRemove,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.group as number);
    w.writeByte(p.who as number);
  },
  (r) => ({ player: r.readByte(), group: r.readInt32(), who: r.readByte() }),
);

// 73 - LookAtReq
register(
  MessageType.LookAtReq,
  (w, p) => {
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({ uid: r.readInt32(), group: r.readInt32(), look: r.readBool() }),
);

// 74 - LookAt
register(
  MessageType.LookAt,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({
    player: r.readByte(),
    uid: r.readInt32(),
    group: r.readInt32(),
    look: r.readBool(),
  }),
);

// 75 - LookAtTopReq
register(
  MessageType.LookAtTopReq,
  (w, p) => {
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeInt32(p.count as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({
    uid: r.readInt32(),
    group: r.readInt32(),
    count: r.readInt32(),
    look: r.readBool(),
  }),
);

// 76 - LookAtTop
register(
  MessageType.LookAtTop,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeInt32(p.count as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({
    player: r.readByte(),
    uid: r.readInt32(),
    group: r.readInt32(),
    count: r.readInt32(),
    look: r.readBool(),
  }),
);

// 77 - LookAtBottomReq
register(
  MessageType.LookAtBottomReq,
  (w, p) => {
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeInt32(p.count as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({
    uid: r.readInt32(),
    group: r.readInt32(),
    count: r.readInt32(),
    look: r.readBool(),
  }),
);

// 78 - LookAtBottom
register(
  MessageType.LookAtBottom,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.uid as number);
    w.writeInt32(p.group as number);
    w.writeInt32(p.count as number);
    w.writeBool(p.look as boolean);
  },
  (r) => ({
    player: r.readByte(),
    uid: r.readInt32(),
    group: r.readInt32(),
    count: r.readInt32(),
    look: r.readBool(),
  }),
);

// 79 - StartLimitedReq
register(
  MessageType.StartLimitedReq,
  (w, p) => { w.writeGuidArray(p.packs as string[]); },
  (r) => ({ packs: r.readGuidArray() }),
);

// 80 - StartLimited
register(
  MessageType.StartLimited,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeGuidArray(p.packs as string[]);
  },
  (r) => ({ player: r.readByte(), packs: r.readGuidArray() }),
);

// 81 - CancelLimitedReq
register(
  MessageType.CancelLimitedReq,
  () => {},
  () => ({}),
);

// 82 - CancelLimited
register(
  MessageType.CancelLimited,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 83 - CardSwitchTo
register(
  MessageType.CardSwitchTo,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.card as number);
    w.writeString(p.alternate as string);
  },
  (r) => ({ player: r.readByte(), card: r.readInt32(), alternate: r.readString() }),
);

// 84 - PlayerSetGlobalVariable
register(
  MessageType.PlayerSetGlobalVariable,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeString(p.name as string);
    w.writeString(p.oldval as string);
    w.writeString(p.val as string);
  },
  (r) => ({
    player: r.readByte(),
    name: r.readString(),
    oldval: r.readString(),
    val: r.readString(),
  }),
);

// 85 - SetGlobalVariable
register(
  MessageType.SetGlobalVariable,
  (w, p) => {
    w.writeString(p.name as string);
    w.writeString(p.oldval as string);
    w.writeString(p.val as string);
  },
  (r) => ({ name: r.readString(), oldval: r.readString(), val: r.readString() }),
);

// 86 - SwitchWithAlternate (deprecated/unused - no parameters observed in C# source)
register(
  MessageType.SwitchWithAlternate,
  () => {},
  () => ({}),
);

// 87 - Ping
register(
  MessageType.Ping,
  () => {},
  () => ({}),
);

// 88 - IsTableBackgroundFlipped
register(
  MessageType.IsTableBackgroundFlipped,
  (w, p) => { w.writeBool(p.isFlipped as boolean); },
  (r) => ({ isFlipped: r.readBool() }),
);

// 89 - PlaySound
register(
  MessageType.PlaySound,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.name as string); },
  (r) => ({ player: r.readByte(), name: r.readString() }),
);

// 90 - Ready
register(
  MessageType.Ready,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 91 - PlayerState
register(
  MessageType.PlayerState,
  (w, p) => { w.writeByte(p.player as number); w.writeByte(p.state as number); },
  (r) => ({ player: r.readByte(), state: r.readByte() }),
);

// 92 - RemoteCall
register(
  MessageType.RemoteCall,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeString(p.function as string);
    w.writeString(p.args as string);
  },
  (r) => ({ player: r.readByte(), function: r.readString(), args: r.readString() }),
);

// 93 - GameStateReq
register(
  MessageType.GameStateReq,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 94 - GameState
register(
  MessageType.GameState,
  (w, p) => { w.writeByte(p.toPlayer as number); w.writeString(p.state as string); },
  (r) => ({ toPlayer: r.readByte(), state: r.readString() }),
);

// 95 - DeleteCard
register(
  MessageType.DeleteCard,
  (w, p) => { w.writeInt32(p.card as number); w.writeByte(p.player as number); },
  (r) => ({ card: r.readInt32(), player: r.readByte() }),
);

// 96 - PlayerDisconnect
register(
  MessageType.PlayerDisconnect,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 97 - AddPacksReq
register(
  MessageType.AddPacksReq,
  (w, p) => {
    w.writeGuidArray(p.packs as string[]);
    w.writeBool(p.selfOnly as boolean);
  },
  (r) => ({ packs: r.readGuidArray(), selfOnly: r.readBool() }),
);

// 98 - AddPacks
register(
  MessageType.AddPacks,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeGuidArray(p.packs as string[]);
    w.writeBool(p.selfOnly as boolean);
  },
  (r) => ({ player: r.readByte(), packs: r.readGuidArray(), selfOnly: r.readBool() }),
);

// 99 - AnchorCard
register(
  MessageType.AnchorCard,
  (w, p) => {
    w.writeInt32(p.id as number);
    w.writeByte(p.player as number);
    w.writeBool(p.anchor as boolean);
  },
  (r) => ({ id: r.readInt32(), player: r.readByte(), anchor: r.readBool() }),
);

// 100 - SetCardProperty
register(
  MessageType.SetCardProperty,
  (w, p) => {
    w.writeInt32(p.id as number);
    w.writeByte(p.player as number);
    w.writeString(p.name as string);
    w.writeString(p.val as string);
    w.writeString(p.valtype as string);
  },
  (r) => ({
    id: r.readInt32(),
    player: r.readByte(),
    name: r.readString(),
    val: r.readString(),
    valtype: r.readString(),
  }),
);

// 101 - ResetCardProperties
register(
  MessageType.ResetCardProperties,
  (w, p) => { w.writeInt32(p.id as number); w.writeByte(p.player as number); },
  (r) => ({ id: r.readInt32(), player: r.readByte() }),
);

// 102 - Filter
register(
  MessageType.Filter,
  (w, p) => { w.writeInt32(p.card as number); w.writeString(p.color as string); },
  (r) => ({ card: r.readInt32(), color: r.readString() }),
);

// 103 - SetBoard
register(
  MessageType.SetBoard,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.name as string); },
  (r) => ({ player: r.readByte(), name: r.readString() }),
);

// 104 - RemoveBoard
register(
  MessageType.RemoveBoard,
  (w, p) => { w.writeByte(p.player as number); },
  (r) => ({ player: r.readByte() }),
);

// 105 - SetPlayerColor
register(
  MessageType.SetPlayerColor,
  (w, p) => { w.writeByte(p.player as number); w.writeString(p.color as string); },
  (r) => ({ player: r.readByte(), color: r.readString() }),
);

// 106 - RequestPileViewPermission
register(
  MessageType.RequestPileViewPermission,
  (w, p) => {
    w.writeByte(p.requester as number);
    w.writeInt32(p.group as number);
    w.writeByte(p.targetPlayer as number);
    w.writeString(p.viewType as string);
    w.writeInt32(p.cardCount as number);
  },
  (r) => ({
    requester: r.readByte(),
    group: r.readInt32(),
    targetPlayer: r.readByte(),
    viewType: r.readString(),
    cardCount: r.readInt32(),
  }),
);

// 107 - GrantPileViewPermission
register(
  MessageType.GrantPileViewPermission,
  (w, p) => {
    w.writeByte(p.owner as number);
    w.writeInt32(p.group as number);
    w.writeByte(p.requester as number);
    w.writeBool(p.granted as boolean);
    w.writeBool(p.permanent as boolean);
    w.writeString(p.viewType as string);
    w.writeInt32(p.cardCount as number);
  },
  (r) => ({
    owner: r.readByte(),
    group: r.readInt32(),
    requester: r.readByte(),
    granted: r.readBool(),
    permanent: r.readBool(),
    viewType: r.readString(),
    cardCount: r.readInt32(),
  }),
);

// 108 - Shake
register(
  MessageType.Shake,
  (w, p) => { w.writeByte(p.player as number); w.writeInt32(p.card as number); },
  (r) => ({ player: r.readByte(), card: r.readInt32() }),
);

// 109 - GroupProtectionReq
register(
  MessageType.GroupProtectionReq,
  (w, p) => { w.writeInt32(p.group as number); w.writeString(p.state as string); },
  (r) => ({ group: r.readInt32(), state: r.readString() }),
);

// 110 - GroupProtection
register(
  MessageType.GroupProtection,
  (w, p) => {
    w.writeByte(p.player as number);
    w.writeInt32(p.group as number);
    w.writeString(p.state as string);
  },
  (r) => ({ player: r.readByte(), group: r.readInt32(), state: r.readString() }),
);

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

/**
 * Serialize a protocol message into a framed binary buffer ready for
 * transmission over TCP.
 *
 * The returned buffer includes the 4-byte length prefix, the IsMuted
 * int32, the message type byte, and all serialized parameters.
 *
 * @param message - The protocol message to serialize.
 * @returns A Buffer containing the complete framed message.
 * @throws If no serializer is registered for the given message type.
 */
export function serializeMessage(message: ProtocolMessage): Buffer {
  const serializeFn = serializers.get(message.type);
  if (!serializeFn) {
    throw new Error(`No serializer registered for message type ${message.type} (${MessageType[message.type]})`);
  }

  const writer = new BinaryWriter();

  // Reserve 4 bytes for the length prefix (will be written at the end).
  writer.writeInt32(0);

  // IsMuted field.
  writer.writeInt32(message.isMuted);

  // Message type ID.
  writer.writeByte(message.type);

  // Message-specific parameters.
  serializeFn(writer, message.params);

  // Go back and write the total length (includes the 4 length-prefix bytes).
  const buf = writer.toBuffer();
  buf.writeInt32LE(buf.length, 0);

  return buf;
}

/**
 * Deserialize a protocol message from a binary buffer.
 *
 * The buffer should contain the payload AFTER the 4-byte length prefix
 * has been stripped (i.e., starting at the IsMuted int32).
 *
 * @param data - Buffer containing the message payload (without length prefix).
 * @returns The deserialized protocol message.
 * @throws If no deserializer is registered for the message type byte.
 */
export function deserializeMessage(data: Buffer): ProtocolMessage {
  const reader = new BinaryReader(data);

  const isMuted = reader.readInt32();
  const typeId = reader.readByte();
  const type = typeId as MessageType;

  const deserializeFn = deserializers.get(type);
  if (!deserializeFn) {
    throw new Error(`No deserializer registered for message type ${typeId} (${MessageType[typeId] ?? 'unknown'})`);
  }

  const params = deserializeFn(reader);

  return { isMuted, type, params };
}

/**
 * Get the human-readable name of a message type.
 * @param type - The message type ID.
 * @returns The name string, or "Unknown" if not a valid MessageType.
 */
export function getMessageTypeName(type: MessageType): string {
  return MessageType[type] ?? 'Unknown';
}
