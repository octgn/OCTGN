/**
 * Binary Protocol Implementation for OCTGN
 *
 * This implements the binary wire protocol used by OCTGN for
 * communication between clients and servers.
 *
 * Protocol format:
 * - 4 bytes: message length
 * - 4 bytes: muted/sequence number
 * - 1 byte: method ID
 * - Variable: method arguments
 */

export interface ProtocolMessage {
  methodId: number;
  methodName: string;
  muted: number;
  data: any;
}

// Method IDs (matching the existing protocol)
export const METHODS = {
  // Client -> Server
  Error: 0,
  Boot: 1,
  Hello: 3,
  HelloAgain: 4,
  Settings: 6,
  PlayerSettings: 7,
  Leave: 9,
  Start: 10,
  ResetReq: 11,
  NextTurn: 13,
  StopTurnReq: 14,
  SetPhaseReq: 16,
  StopPhaseReq: 18,
  SetActivePlayer: 19,
  ClearActivePlayer: 20,
  ChatReq: 21,
  PrintReq: 23,
  RandomReq: 25,
  CounterReq: 27,
  LoadDeck: 29,
  CreateCard: 30,
  CreateCardAt: 31,
  CreateAliasDeprecated: 32,
  MoveCardReq: 33,
  MoveCardAtReq: 35,
  PeekReq: 37,
  UntargetReq: 39,
  TargetReq: 41,
  TargetArrowReq: 43,
  Highlight: 45,
  TurnReq: 46,
  RotateReq: 48,
  ShuffleDeprecated: 50,
  Shuffled: 51,
  UnaliasGrpDeprecated: 52,
  UnaliasDeprecated: 53,
  AddMarkerReq: 54,
  RemoveMarkerReq: 56,
  TransferMarkerReq: 58,
  PassToReq: 60,
  TakeFromReq: 62,
  DontTakeReq: 64,
  FreezeCardsVisibility: 66,
  GroupVisReq: 67,
  GroupVisAddReq: 69,
  GroupVisRemoveReq: 71,
  LookAtReq: 73,
  LookAtTopReq: 75,
  LookAtBottomReq: 77,
  StartLimitedReq: 79,
  CancelLimitedReq: 81,
  CardSwitchTo: 83,
  PlayerSetGlobalVariable: 84,
  SetGlobalVariable: 85,
  Ping: 87,
  IsTableBackgroundFlipped: 88,
  PlaySound: 89,
  Ready: 90,
  RemoteCall: 92,
  GameStateReq: 93,
  GameState: 94,
  DeleteCard: 95,
  AddPacksReq: 97,
  AnchorCard: 99,
  SetCardProperty: 100,
  ResetCardProperties: 101,
  Filter: 102,
  SetBoard: 103,
  RemoveBoard: 104,
  SetPlayerColor: 105,
  RequestPileViewPermission: 106,
  GrantPileViewPermission: 107,
  Shake: 108,
  GroupProtectionReq: 109,
} as const;

export class BinaryProtocol {
  /**
   * Parse a binary message into a ProtocolMessage
   */
  parse(data: Buffer): ProtocolMessage {
    const reader = new BufferReader(data);

    const muted = reader.readInt32();
    const methodId = reader.readByte();

    const methodName = this.getMethodName(methodId);
    const args = this.parseMethodArgs(reader, methodId);

    return {
      methodId,
      methodName,
      muted,
      data: args,
    };
  }

  /**
   * Create a binary message from method name and arguments
   */
  createMessage(methodName: string, args: any = {}): Buffer {
    const methodId = METHODS[methodName as keyof typeof METHODS];
    if (methodId === undefined) {
      throw new Error(`Unknown method: ${methodName}`);
    }

    const writer = new BufferWriter();
    writer.writeInt32(0); // muted/sequence - will be set by caller
    writer.writeByte(methodId);

    this.writeMethodArgs(writer, methodId, args);

    return writer.toBuffer();
  }

  /**
   * Get method name from ID
   */
  private getMethodName(id: number): string {
    for (const [name, methodId] of Object.entries(METHODS)) {
      if (methodId === id) return name;
    }
    return `Unknown_${id}`;
  }

  /**
   * Parse method-specific arguments
   */
  private parseMethodArgs(reader: BufferReader, methodId: number): any {
    switch (methodId) {
      case METHODS.Hello:
        return {
          nick: reader.readString(),
          userId: reader.readString(),
          pkey: reader.readUInt64(),
          client: reader.readString(),
          version: reader.readVersion(),
          gameVersion: reader.readVersion(),
          gameId: reader.readGuid(),
          scriptVersion: reader.readVersion(),
          password: reader.readString(),
          spectator: reader.readBoolean(),
        };

      case METHODS.ChatReq:
        return { text: reader.readString() };

      case METHODS.MoveCardReq:
        return {
          cardIds: reader.readInt32Array(),
          toGroup: reader.readInt32(),
          toIndex: reader.readInt32Array(),
          faceUp: reader.readBooleanArray(),
          isScriptMove: reader.readBoolean(),
        };

      case METHODS.LoadDeck:
        return {
          ids: reader.readInt32Array(),
          types: reader.readGuidArray(),
          groups: reader.readInt32Array(),
          sizes: reader.readStringArray(),
          sleeve: reader.readString(),
          limited: reader.readBoolean(),
        };

      case METHODS.Ping:
        return {};

      default:
        return {};
    }
  }

  /**
   * Write method-specific arguments
   */
  private writeMethodArgs(writer: BufferWriter, methodId: number, args: any): void {
    switch (methodId) {
      case METHODS.Welcome:
        writer.writeByte(args.id);
        writer.writeGuid(args.gameSessionId);
        writer.writeString(args.gameName);
        writer.writeBoolean(args.waitForGameState);
        break;

      case METHODS.NewPlayer:
        writer.writeByte(args.id);
        writer.writeString(args.nick);
        writer.writeString(args.userId);
        writer.writeUInt64(args.pkey);
        writer.writeBoolean(args.tableSide);
        writer.writeBoolean(args.spectator);
        break;

      case METHODS.Chat:
        writer.writeByte(args.player);
        writer.writeString(args.text);
        break;

      case METHODS.MoveCard:
        writer.writeByte(args.player);
        writer.writeInt32Array(args.cardIds);
        writer.writeInt32(args.toGroup);
        writer.writeInt32Array(args.toIndex);
        writer.writeBooleanArray(args.faceUp);
        writer.writeBoolean(args.isScriptMove);
        break;

      case METHODS.Error:
        writer.writeString(args.msg);
        break;

      case METHODS.Ping:
        // No args
        break;
    }
  }
}

/**
 * Buffer reader helper class
 */
class BufferReader {
  private offset = 0;

  constructor(private buffer: Buffer) {}

  readByte(): number {
    return this.buffer.readUInt8(this.offset++);
  }

  readInt32(): number {
    const val = this.buffer.readInt32LE(this.offset);
    this.offset += 4;
    return val;
  }

  readUInt64(): bigint {
    const val = this.buffer.readBigUInt64LE(this.offset);
    this.offset += 8;
    return val;
  }

  readBoolean(): boolean {
    return this.readByte() !== 0;
  }

  readString(): string {
    const length = this.buffer.readUInt8(this.offset++);
    if (length === 255) {
      // Null string
      return '';
    }
    const str = this.buffer.toString('utf8', this.offset, this.offset + length);
    this.offset += length;
    return str;
  }

  readGuid(): string {
    const bytes = this.buffer.slice(this.offset, this.offset + 16);
    this.offset += 16;
    // Format as GUID
    const hex = bytes.toString('hex');
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
  }

  readVersion(): string {
    return this.readString();
  }

  readInt32Array(): number[] {
    const length = this.buffer.readInt16LE(this.offset);
    this.offset += 2;
    const arr: number[] = [];
    for (let i = 0; i < length; i++) {
      arr.push(this.readInt32());
    }
    return arr;
  }

  readGuidArray(): string[] {
    const length = this.buffer.readInt16LE(this.offset);
    this.offset += 2;
    const arr: string[] = [];
    for (let i = 0; i < length; i++) {
      arr.push(this.readGuid());
    }
    return arr;
  }

  readStringArray(): string[] {
    const length = this.buffer.readInt16LE(this.offset);
    this.offset += 2;
    const arr: string[] = [];
    for (let i = 0; i < length; i++) {
      arr.push(this.readString());
    }
    return arr;
  }

  readBooleanArray(): boolean[] {
    const length = this.buffer.readInt16LE(this.offset);
    this.offset += 2;
    const arr: boolean[] = [];
    for (let i = 0; i < length; i++) {
      arr.push(this.readBoolean());
    }
    return arr;
  }
}

/**
 * Buffer writer helper class
 */
class BufferWriter {
  private buffers: Buffer[] = [];

  writeByte(value: number): void {
    const buf = Buffer.alloc(1);
    buf.writeUInt8(value, 0);
    this.buffers.push(buf);
  }

  writeInt32(value: number): void {
    const buf = Buffer.alloc(4);
    buf.writeInt32LE(value, 0);
    this.buffers.push(buf);
  }

  writeUInt64(value: bigint | number): void {
    const buf = Buffer.alloc(8);
    buf.writeBigUInt64LE(BigInt(value), 0);
    this.buffers.push(buf);
  }

  writeBoolean(value: boolean): void {
    this.writeByte(value ? 1 : 0);
  }

  writeString(value: string): void {
    if (value === null || value === undefined) {
      this.writeByte(255);
      return;
    }
    const strBuf = Buffer.from(value, 'utf8');
    if (strBuf.length > 254) {
      throw new Error('String too long');
    }
    this.writeByte(strBuf.length);
    this.buffers.push(strBuf);
  }

  writeGuid(value: string): void {
    // Parse GUID and write as 16 bytes
    const hex = value.replace(/-/g, '');
    const buf = Buffer.from(hex, 'hex');
    this.buffers.push(buf);
  }

  writeInt32Array(values: number[]): void {
    const lenBuf = Buffer.alloc(2);
    lenBuf.writeInt16LE(values.length, 0);
    this.buffers.push(lenBuf);
    for (const val of values) {
      this.writeInt32(val);
    }
  }

  writeBooleanArray(values: boolean[]): void {
    const lenBuf = Buffer.alloc(2);
    lenBuf.writeInt16LE(values.length, 0);
    this.buffers.push(lenBuf);
    for (const val of values) {
      this.writeBoolean(val);
    }
  }

  toBuffer(): Buffer {
    return Buffer.concat(this.buffers);
  }
}
