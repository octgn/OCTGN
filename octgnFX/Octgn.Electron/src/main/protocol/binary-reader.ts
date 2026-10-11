/**
 * OCTGN Game Binary Protocol - Binary Reader
 *
 * Deserializes primitive and composite types from a byte buffer
 * using the .NET BinaryReader-compatible wire format:
 *   - All integers are little-endian.
 *   - Strings use .NET 7-bit variable-length integer length prefix followed
 *     by UTF-8 encoded bytes.
 *   - GUIDs use .NET System.Guid byte ordering (mixed-endian).
 *   - Arrays use a 2-byte (int16) length prefix.
 */

/**
 * A binary reader that deserializes data from a Buffer using the
 * .NET BinaryReader-compatible encoding used by the OCTGN server.
 */
export class BinaryReader {
  private readonly buffer: Buffer;
  private offset: number;

  /**
   * Create a new BinaryReader over an existing buffer.
   * @param buffer - The source buffer to read from.
   * @param offset - Optional starting offset (default 0).
   */
  constructor(buffer: Buffer, offset: number = 0) {
    this.buffer = buffer;
    this.offset = offset;
  }

  // ---------------------------------------------------------------------------
  // Internal helpers
  // ---------------------------------------------------------------------------

  /** Ensure at least `bytes` bytes remain to be read. */
  private ensureAvailable(bytes: number): void {
    if (this.offset + bytes > this.buffer.length) {
      throw new RangeError(
        `BinaryReader: attempted to read ${bytes} byte(s) at offset ${this.offset}, ` +
        `but buffer length is ${this.buffer.length}`
      );
    }
  }

  // ---------------------------------------------------------------------------
  // Primitive readers
  // ---------------------------------------------------------------------------

  /** Read a single unsigned byte (uint8). */
  readByte(): number {
    this.ensureAvailable(1);
    const value = this.buffer.readUInt8(this.offset);
    this.offset += 1;
    return value;
  }

  /** Read a signed 16-bit integer (little-endian). */
  readInt16(): number {
    this.ensureAvailable(2);
    const value = this.buffer.readInt16LE(this.offset);
    this.offset += 2;
    return value;
  }

  /** Read an unsigned 16-bit integer (little-endian). */
  readUint16(): number {
    this.ensureAvailable(2);
    const value = this.buffer.readUInt16LE(this.offset);
    this.offset += 2;
    return value;
  }

  /** Read a signed 32-bit integer (little-endian). */
  readInt32(): number {
    this.ensureAvailable(4);
    const value = this.buffer.readInt32LE(this.offset);
    this.offset += 4;
    return value;
  }

  /** Read an unsigned 64-bit integer (little-endian) as a BigInt. */
  readUint64(): bigint {
    this.ensureAvailable(8);
    const value = this.buffer.readBigUInt64LE(this.offset);
    this.offset += 8;
    return value;
  }

  /** Read a 32-bit IEEE 754 float (little-endian). */
  readFloat(): number {
    this.ensureAvailable(4);
    const value = this.buffer.readFloatLE(this.offset);
    this.offset += 4;
    return value;
  }

  /** Read a boolean from a single byte (0x00 = false, anything else = true). */
  readBool(): boolean {
    return this.readByte() !== 0;
  }

  // ---------------------------------------------------------------------------
  // .NET 7-bit encoded string
  // ---------------------------------------------------------------------------

  /**
   * Read a .NET 7-bit variable-length encoded integer.
   *
   * Each byte contributes 7 payload bits. If the high bit (0x80) is set,
   * more bytes follow. The final byte has the high bit clear.
   */
  private read7BitEncodedInt(): number {
    let result = 0;
    let shift = 0;
    let byte: number;
    do {
      if (shift >= 35) {
        throw new Error('BinaryReader: malformed 7-bit encoded integer (too many bytes)');
      }
      byte = this.readByte();
      result |= (byte & 0x7f) << shift;
      shift += 7;
    } while ((byte & 0x80) !== 0);
    return result >>> 0; // ensure unsigned
  }

  /**
   * Read a string using .NET BinaryReader encoding:
   * 7-bit variable-length integer byte count followed by UTF-8 bytes.
   */
  readString(): string {
    const length = this.read7BitEncodedInt();
    if (length === 0) return '';
    this.ensureAvailable(length);
    const value = this.buffer.toString('utf8', this.offset, this.offset + length);
    this.offset += length;
    return value;
  }

  // ---------------------------------------------------------------------------
  // GUID (.NET System.Guid byte order)
  // ---------------------------------------------------------------------------

  /**
   * Read 16 raw bytes as a GUID/UUID string using .NET System.Guid ordering.
   *
   * .NET Guid byte layout (mixed-endian):
   *   Bytes 0-3:  first group  reversed (LE int32)
   *   Bytes 4-5:  second group reversed (LE int16)
   *   Bytes 6-7:  third group  reversed (LE int16)
   *   Bytes 8-15: fourth+fifth groups as-is (big-endian)
   *
   * @returns UUID string in standard format (e.g. "550e8400-e29b-41d4-a716-446655440000").
   */
  readGuid(): string {
    this.ensureAvailable(16);
    const b = this.buffer;
    const o = this.offset;

    // Group 1 (4 bytes, reversed)
    const g1 = (
      this.hex(b[o + 3]) + this.hex(b[o + 2]) +
      this.hex(b[o + 1]) + this.hex(b[o + 0])
    );

    // Group 2 (2 bytes, reversed)
    const g2 = this.hex(b[o + 5]) + this.hex(b[o + 4]);

    // Group 3 (2 bytes, reversed)
    const g3 = this.hex(b[o + 7]) + this.hex(b[o + 6]);

    // Group 4 (2 bytes, as-is)
    const g4 = this.hex(b[o + 8]) + this.hex(b[o + 9]);

    // Group 5 (6 bytes, as-is)
    let g5 = '';
    for (let i = 10; i < 16; i++) {
      g5 += this.hex(b[o + i]);
    }

    this.offset += 16;
    return `${g1}-${g2}-${g3}-${g4}-${g5}`;
  }

  /** Format a byte as a two-character lowercase hex string. */
  private hex(byte: number): string {
    return byte.toString(16).padStart(2, '0');
  }

  // ---------------------------------------------------------------------------
  // Raw bytes
  // ---------------------------------------------------------------------------

  /** Read a specified number of raw bytes as a new Buffer. */
  readBytes(count: number): Buffer {
    this.ensureAvailable(count);
    const result = Buffer.from(this.buffer.subarray(this.offset, this.offset + count));
    this.offset += count;
    return result;
  }

  // ---------------------------------------------------------------------------
  // Array readers (all use int16 length prefix)
  // ---------------------------------------------------------------------------

  /** Read an array of bytes with int16 length prefix. */
  readByteArray(): number[] {
    const length = this.readInt16();
    const result: number[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readByte();
    return result;
  }

  /** Read an array of int16 values with int16 length prefix. */
  readShortArray(): number[] {
    const length = this.readInt16();
    const result: number[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readInt16();
    return result;
  }

  /** Read an array of int32 values with int16 length prefix. */
  readIntArray(): number[] {
    const length = this.readInt16();
    const result: number[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readInt32();
    return result;
  }

  /** Read an array of uint64 values with int16 length prefix. */
  readUint64Array(): bigint[] {
    const length = this.readInt16();
    const result: bigint[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readUint64();
    return result;
  }

  /** Read an array of booleans with int16 length prefix. */
  readBoolArray(): boolean[] {
    const length = this.readInt16();
    const result: boolean[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readBool();
    return result;
  }

  /** Read an array of strings with int16 length prefix. */
  readStringArray(): string[] {
    const length = this.readInt16();
    const result: string[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readString();
    return result;
  }

  /** Read an array of GUID strings with int16 length prefix. */
  readGuidArray(): string[] {
    const length = this.readInt16();
    const result: string[] = new Array(length);
    for (let i = 0; i < length; i++) result[i] = this.readGuid();
    return result;
  }

  // ---------------------------------------------------------------------------
  // Position and state
  // ---------------------------------------------------------------------------

  /** Get the current read position. */
  get position(): number {
    return this.offset;
  }

  /** Get the number of bytes remaining to be read. */
  get remaining(): number {
    return this.buffer.length - this.offset;
  }

  /** Check whether all bytes have been consumed. */
  get isAtEnd(): boolean {
    return this.offset >= this.buffer.length;
  }
}
