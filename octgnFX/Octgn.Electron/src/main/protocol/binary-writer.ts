/**
 * OCTGN Game Binary Protocol - Binary Writer
 *
 * Serializes primitive and composite types into a growable byte buffer
 * using the .NET BinaryWriter-compatible wire format:
 *   - All integers are little-endian.
 *   - Strings use .NET 7-bit variable-length integer length prefix followed
 *     by UTF-8 encoded bytes.
 *   - GUIDs use .NET System.Guid byte ordering (mixed-endian).
 *   - Arrays use a 2-byte (int16) length prefix.
 */

/** Default initial capacity for the internal buffer (bytes). */
const DEFAULT_CAPACITY = 512;

/**
 * A binary writer that serializes data into an expanding Buffer using
 * the .NET BinaryWriter-compatible encoding expected by the OCTGN server.
 */
export class BinaryWriter {
  private buffer: Buffer;
  private offset: number;

  /**
   * Create a new BinaryWriter.
   * @param initialCapacity - Starting buffer size in bytes (default 512).
   */
  constructor(initialCapacity: number = DEFAULT_CAPACITY) {
    this.buffer = Buffer.alloc(initialCapacity);
    this.offset = 0;
  }

  // ---------------------------------------------------------------------------
  // Internal helpers
  // ---------------------------------------------------------------------------

  /** Ensure at least `bytes` additional bytes can be written. */
  private ensureCapacity(bytes: number): void {
    const required = this.offset + bytes;
    if (required <= this.buffer.length) return;
    let newSize = this.buffer.length * 2;
    while (newSize < required) newSize *= 2;
    const newBuf = Buffer.alloc(newSize);
    this.buffer.copy(newBuf, 0, 0, this.offset);
    this.buffer = newBuf;
  }

  // ---------------------------------------------------------------------------
  // Primitive writers
  // ---------------------------------------------------------------------------

  /** Write a single unsigned byte (uint8). */
  writeByte(value: number): void {
    this.ensureCapacity(1);
    this.buffer.writeUInt8(value & 0xff, this.offset);
    this.offset += 1;
  }

  /** Write a signed 16-bit integer (little-endian). */
  writeInt16(value: number): void {
    this.ensureCapacity(2);
    this.buffer.writeInt16LE(value, this.offset);
    this.offset += 2;
  }

  /** Write an unsigned 16-bit integer (little-endian). */
  writeUint16(value: number): void {
    this.ensureCapacity(2);
    this.buffer.writeUInt16LE(value, this.offset);
    this.offset += 2;
  }

  /** Write a signed 32-bit integer (little-endian). */
  writeInt32(value: number): void {
    this.ensureCapacity(4);
    this.buffer.writeInt32LE(value, this.offset);
    this.offset += 4;
  }

  /** Write an unsigned 64-bit integer (little-endian) from a BigInt. */
  writeUint64(value: bigint): void {
    this.ensureCapacity(8);
    this.buffer.writeBigUInt64LE(value, this.offset);
    this.offset += 8;
  }

  /** Write a 32-bit IEEE 754 float (little-endian). */
  writeFloat(value: number): void {
    this.ensureCapacity(4);
    this.buffer.writeFloatLE(value, this.offset);
    this.offset += 4;
  }

  /** Write a boolean as a single byte (0x00 = false, 0x01 = true). */
  writeBool(value: boolean): void {
    this.writeByte(value ? 1 : 0);
  }

  // ---------------------------------------------------------------------------
  // .NET 7-bit encoded string
  // ---------------------------------------------------------------------------

  /**
   * Write a .NET 7-bit variable-length encoded integer.
   *
   * The encoding writes 7 payload bits per byte. If more bytes follow,
   * the high bit (0x80) is set. The final byte has the high bit clear.
   */
  private write7BitEncodedInt(value: number): void {
    let v = value >>> 0; // ensure unsigned 32-bit
    while (v > 0x7f) {
      this.writeByte((v & 0x7f) | 0x80);
      v >>>= 7;
    }
    this.writeByte(v);
  }

  /**
   * Write a string using .NET BinaryWriter encoding:
   * 7-bit variable-length integer byte count followed by UTF-8 bytes.
   */
  writeString(value: string): void {
    const encoded = Buffer.from(value, 'utf8');
    this.write7BitEncodedInt(encoded.length);
    this.ensureCapacity(encoded.length);
    encoded.copy(this.buffer, this.offset);
    this.offset += encoded.length;
  }

  // ---------------------------------------------------------------------------
  // GUID (.NET System.Guid byte order)
  // ---------------------------------------------------------------------------

  /**
   * Write a GUID/UUID string as 16 raw bytes using .NET System.Guid ordering.
   *
   * .NET Guid byte layout (mixed-endian):
   *   Bytes 0-3:  first group  (4 hex chars) reversed (LE int32)
   *   Bytes 4-5:  second group (2 hex chars) reversed (LE int16)
   *   Bytes 6-7:  third group  (2 hex chars) reversed (LE int16)
   *   Bytes 8-15: fourth+fifth groups as-is (big-endian)
   *
   * @param value - UUID string in standard format (e.g. "550e8400-e29b-41d4-a716-446655440000").
   */
  writeGuid(value: string): void {
    const hex = value.replace(/-/g, '');
    if (hex.length !== 32) {
      throw new Error(`Invalid GUID string: "${value}"`);
    }

    this.ensureCapacity(16);

    // Group 1: bytes 0-3 (4 bytes) - reversed
    this.buffer[this.offset + 0] = parseInt(hex.substring(6, 8), 16);
    this.buffer[this.offset + 1] = parseInt(hex.substring(4, 6), 16);
    this.buffer[this.offset + 2] = parseInt(hex.substring(2, 4), 16);
    this.buffer[this.offset + 3] = parseInt(hex.substring(0, 2), 16);

    // Group 2: bytes 4-5 (2 bytes) - reversed
    this.buffer[this.offset + 4] = parseInt(hex.substring(10, 12), 16);
    this.buffer[this.offset + 5] = parseInt(hex.substring(8, 10), 16);

    // Group 3: bytes 6-7 (2 bytes) - reversed
    this.buffer[this.offset + 6] = parseInt(hex.substring(14, 16), 16);
    this.buffer[this.offset + 7] = parseInt(hex.substring(12, 14), 16);

    // Groups 4-5: bytes 8-15 (8 bytes) - as-is
    for (let i = 0; i < 8; i++) {
      this.buffer[this.offset + 8 + i] = parseInt(hex.substring(16 + i * 2, 18 + i * 2), 16);
    }

    this.offset += 16;
  }

  // ---------------------------------------------------------------------------
  // Raw bytes
  // ---------------------------------------------------------------------------

  /** Write raw bytes from a Buffer or Uint8Array. */
  writeBytes(data: Buffer | Uint8Array): void {
    this.ensureCapacity(data.length);
    if (Buffer.isBuffer(data)) {
      data.copy(this.buffer, this.offset);
    } else {
      Buffer.from(data).copy(this.buffer, this.offset);
    }
    this.offset += data.length;
  }

  // ---------------------------------------------------------------------------
  // Array writers (all use int16 length prefix)
  // ---------------------------------------------------------------------------

  /** Write an array of bytes with int16 length prefix. */
  writeByteArray(values: number[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeByte(v);
  }

  /** Write an array of int16 values with int16 length prefix. */
  writeShortArray(values: number[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeInt16(v);
  }

  /** Write an array of int32 values with int16 length prefix. */
  writeIntArray(values: number[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeInt32(v);
  }

  /** Write an array of uint64 values with int16 length prefix. */
  writeUint64Array(values: bigint[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeUint64(v);
  }

  /** Write an array of booleans with int16 length prefix. */
  writeBoolArray(values: boolean[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeBool(v);
  }

  /** Write an array of strings with int16 length prefix. */
  writeStringArray(values: string[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeString(v);
  }

  /** Write an array of GUID strings with int16 length prefix. */
  writeGuidArray(values: string[]): void {
    this.writeInt16(values.length);
    for (const v of values) this.writeGuid(v);
  }

  // ---------------------------------------------------------------------------
  // Seeking and output
  // ---------------------------------------------------------------------------

  /** Get the current write position (number of bytes written so far). */
  get position(): number {
    return this.offset;
  }

  /** Seek to an absolute position in the buffer. */
  seek(position: number): void {
    this.offset = position;
  }

  /**
   * Return a new Buffer containing exactly the bytes written so far.
   * The returned buffer is a copy and can be sent on the wire as-is.
   */
  toBuffer(): Buffer {
    return Buffer.from(this.buffer.subarray(0, this.offset));
  }
}
