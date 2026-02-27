import { describe, it, expect } from 'vitest';
import { BinaryReader } from '@main/protocol/binary-reader';

describe('BinaryReader', () => {
  // ---------------------------------------------------------------------------
  // Primitive readers
  // ---------------------------------------------------------------------------

  describe('readByte', () => {
    it('should read a single unsigned byte', () => {
      const r = new BinaryReader(Buffer.from([0xab]));
      expect(r.readByte()).toBe(0xab);
    });

    it('should read zero', () => {
      const r = new BinaryReader(Buffer.from([0x00]));
      expect(r.readByte()).toBe(0);
    });

    it('should read 0xff', () => {
      const r = new BinaryReader(Buffer.from([0xff]));
      expect(r.readByte()).toBe(255);
    });
  });

  describe('readInt16', () => {
    it('should read a positive int16 in little-endian', () => {
      const buf = Buffer.alloc(2);
      buf.writeInt16LE(0x0102, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt16()).toBe(0x0102);
    });

    it('should read a negative int16', () => {
      const buf = Buffer.alloc(2);
      buf.writeInt16LE(-1, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt16()).toBe(-1);
    });

    it('should read zero', () => {
      const buf = Buffer.alloc(2);
      const r = new BinaryReader(buf);
      expect(r.readInt16()).toBe(0);
    });
  });

  describe('readInt32', () => {
    it('should read a positive int32 in little-endian', () => {
      const buf = Buffer.alloc(4);
      buf.writeInt32LE(0x01020304, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt32()).toBe(0x01020304);
    });

    it('should read a negative int32', () => {
      const buf = Buffer.alloc(4);
      buf.writeInt32LE(-42, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt32()).toBe(-42);
    });

    it('should read max int32', () => {
      const buf = Buffer.alloc(4);
      buf.writeInt32LE(0x7fffffff, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt32()).toBe(0x7fffffff);
    });

    it('should read min int32', () => {
      const buf = Buffer.alloc(4);
      buf.writeInt32LE(-2147483648, 0);
      const r = new BinaryReader(buf);
      expect(r.readInt32()).toBe(-2147483648);
    });
  });

  describe('readUint64', () => {
    it('should read a uint64 in little-endian', () => {
      const buf = Buffer.alloc(8);
      buf.writeBigUInt64LE(0x0102030405060708n, 0);
      const r = new BinaryReader(buf);
      expect(r.readUint64()).toBe(0x0102030405060708n);
    });

    it('should read zero', () => {
      const buf = Buffer.alloc(8);
      const r = new BinaryReader(buf);
      expect(r.readUint64()).toBe(0n);
    });

    it('should read max uint64', () => {
      const buf = Buffer.alloc(8);
      buf.writeBigUInt64LE(0xffffffffffffffffn, 0);
      const r = new BinaryReader(buf);
      expect(r.readUint64()).toBe(0xffffffffffffffffn);
    });
  });

  // ---------------------------------------------------------------------------
  // readBool
  // ---------------------------------------------------------------------------

  describe('readBool', () => {
    it('should read 0x01 as true', () => {
      const r = new BinaryReader(Buffer.from([0x01]));
      expect(r.readBool()).toBe(true);
    });

    it('should read 0x00 as false', () => {
      const r = new BinaryReader(Buffer.from([0x00]));
      expect(r.readBool()).toBe(false);
    });

    it('should read any non-zero byte as true', () => {
      const r = new BinaryReader(Buffer.from([0xff]));
      expect(r.readBool()).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // readString
  // ---------------------------------------------------------------------------

  describe('readString', () => {
    it('should read an empty string', () => {
      const r = new BinaryReader(Buffer.from([0x00]));
      expect(r.readString()).toBe('');
    });

    it('should read a short ASCII string', () => {
      const data = Buffer.from([0x02, 0x48, 0x69]); // length=2, "Hi"
      const r = new BinaryReader(data);
      expect(r.readString()).toBe('Hi');
    });

    it('should read a unicode string with multi-byte UTF-8 characters', () => {
      const str = '\u00e9\u00e8\u00ea'; // 6 bytes in UTF-8
      const encoded = Buffer.from(str, 'utf8');
      const data = Buffer.concat([Buffer.from([encoded.length]), encoded]);
      const r = new BinaryReader(data);
      expect(r.readString()).toBe(str);
    });

    it('should read a string with multi-byte VarInt length prefix (128 bytes)', () => {
      const str = 'A'.repeat(128);
      const encoded = Buffer.from(str, 'utf8');
      // VarInt(128) = [0x80, 0x01]
      const prefix = Buffer.from([0x80, 0x01]);
      const data = Buffer.concat([prefix, encoded]);
      const r = new BinaryReader(data);
      expect(r.readString()).toBe(str);
    });

    it('should read a string with 3-byte VarInt length prefix (16384 bytes)', () => {
      const str = 'X'.repeat(16384);
      const encoded = Buffer.from(str, 'utf8');
      // VarInt(16384) = [0x80, 0x80, 0x01]
      const prefix = Buffer.from([0x80, 0x80, 0x01]);
      const data = Buffer.concat([prefix, encoded]);
      const r = new BinaryReader(data);
      expect(r.readString()).toBe(str);
    });

    it('should read emoji characters', () => {
      const str = '\ud83d\ude00';
      const encoded = Buffer.from(str, 'utf8');
      const data = Buffer.concat([Buffer.from([encoded.length]), encoded]);
      const r = new BinaryReader(data);
      expect(r.readString()).toBe(str);
    });
  });

  // ---------------------------------------------------------------------------
  // readGuid
  // ---------------------------------------------------------------------------

  describe('readGuid', () => {
    it('should read a GUID from .NET byte order', () => {
      // Write GUID bytes for "550e8400-e29b-41d4-a716-446655440000" in .NET order:
      // Group 1 reversed: 00 84 0e 55
      // Group 2 reversed: 9b e2
      // Group 3 reversed: d4 41
      // Groups 4-5 as-is: a7 16 44 66 55 44 00 00
      const bytes = Buffer.from([
        0x00, 0x84, 0x0e, 0x55, // group 1
        0x9b, 0xe2,             // group 2
        0xd4, 0x41,             // group 3
        0xa7, 0x16, 0x44, 0x66, 0x55, 0x44, 0x00, 0x00, // groups 4-5
      ]);
      const r = new BinaryReader(bytes);
      expect(r.readGuid()).toBe('550e8400-e29b-41d4-a716-446655440000');
    });

    it('should read the nil GUID', () => {
      const r = new BinaryReader(Buffer.alloc(16));
      expect(r.readGuid()).toBe('00000000-0000-0000-0000-000000000000');
    });

    it('should advance position by 16 bytes', () => {
      const r = new BinaryReader(Buffer.alloc(32));
      r.readGuid();
      expect(r.position).toBe(16);
    });
  });

  // ---------------------------------------------------------------------------
  // Array readers
  // ---------------------------------------------------------------------------

  describe('readByteArray', () => {
    it('should read an empty byte array', () => {
      const buf = Buffer.alloc(2);
      buf.writeInt16LE(0, 0);
      const r = new BinaryReader(buf);
      expect(r.readByteArray()).toEqual([]);
    });

    it('should read bytes with int16 length prefix', () => {
      const buf = Buffer.alloc(5);
      buf.writeInt16LE(3, 0);
      buf[2] = 0x01;
      buf[3] = 0x02;
      buf[4] = 0xff;
      const r = new BinaryReader(buf);
      expect(r.readByteArray()).toEqual([0x01, 0x02, 0xff]);
    });
  });

  describe('readIntArray', () => {
    it('should read int32 values with int16 length prefix', () => {
      const buf = Buffer.alloc(2 + 3 * 4);
      buf.writeInt16LE(3, 0);
      buf.writeInt32LE(1, 2);
      buf.writeInt32LE(-1, 6);
      buf.writeInt32LE(0x7fffffff, 10);
      const r = new BinaryReader(buf);
      expect(r.readIntArray()).toEqual([1, -1, 0x7fffffff]);
    });

    it('should read an empty int array', () => {
      const buf = Buffer.alloc(2);
      buf.writeInt16LE(0, 0);
      const r = new BinaryReader(buf);
      expect(r.readIntArray()).toEqual([]);
    });
  });

  describe('readStringArray', () => {
    it('should read strings with int16 length prefix', () => {
      // Build buffer: int16(2) + "hello" + "world"
      const parts: Buffer[] = [];
      const prefix = Buffer.alloc(2);
      prefix.writeInt16LE(2, 0);
      parts.push(prefix);
      // "hello" = VarInt(5) + 5 bytes
      parts.push(Buffer.from([5]));
      parts.push(Buffer.from('hello', 'utf8'));
      // "world" = VarInt(5) + 5 bytes
      parts.push(Buffer.from([5]));
      parts.push(Buffer.from('world', 'utf8'));
      const buf = Buffer.concat(parts);
      const r = new BinaryReader(buf);
      expect(r.readStringArray()).toEqual(['hello', 'world']);
    });

    it('should read an empty string array', () => {
      const buf = Buffer.alloc(2);
      buf.writeInt16LE(0, 0);
      const r = new BinaryReader(buf);
      expect(r.readStringArray()).toEqual([]);
    });
  });

  describe('readGuidArray', () => {
    it('should read GUIDs with int16 length prefix', () => {
      const buf = Buffer.alloc(2 + 2 * 16);
      buf.writeInt16LE(2, 0);
      // Two nil GUIDs (all zeros)
      const r = new BinaryReader(buf);
      const guids = r.readGuidArray();
      expect(guids).toHaveLength(2);
      expect(guids[0]).toBe('00000000-0000-0000-0000-000000000000');
      expect(guids[1]).toBe('00000000-0000-0000-0000-000000000000');
    });
  });

  describe('readBoolArray', () => {
    it('should read booleans with int16 length prefix', () => {
      const buf = Buffer.alloc(5);
      buf.writeInt16LE(3, 0);
      buf[2] = 1;
      buf[3] = 0;
      buf[4] = 1;
      const r = new BinaryReader(buf);
      expect(r.readBoolArray()).toEqual([true, false, true]);
    });
  });

  // ---------------------------------------------------------------------------
  // Error on reading past end of buffer
  // ---------------------------------------------------------------------------

  describe('boundary errors', () => {
    it('should throw RangeError when reading past end with readByte', () => {
      const r = new BinaryReader(Buffer.alloc(0));
      expect(() => r.readByte()).toThrow(RangeError);
    });

    it('should throw RangeError when reading past end with readInt32', () => {
      const r = new BinaryReader(Buffer.alloc(2)); // only 2 bytes, need 4
      expect(() => r.readInt32()).toThrow(RangeError);
    });

    it('should throw RangeError when reading past end with readUint64', () => {
      const r = new BinaryReader(Buffer.alloc(4)); // only 4 bytes, need 8
      expect(() => r.readUint64()).toThrow(RangeError);
    });

    it('should throw RangeError when reading past end with readGuid', () => {
      const r = new BinaryReader(Buffer.alloc(10)); // only 10, need 16
      expect(() => r.readGuid()).toThrow(RangeError);
    });

    it('should throw RangeError on readString with truncated data', () => {
      // Say length prefix = 100 but only 5 bytes of data available
      const buf = Buffer.alloc(6);
      buf[0] = 100; // VarInt length
      const r = new BinaryReader(buf);
      expect(() => r.readString()).toThrow(RangeError);
    });

    it('should include helpful error message', () => {
      const r = new BinaryReader(Buffer.alloc(1));
      r.readByte();
      expect(() => r.readByte()).toThrow(/offset 1/);
    });
  });

  // ---------------------------------------------------------------------------
  // position and remaining getters
  // ---------------------------------------------------------------------------

  describe('position', () => {
    it('should start at 0 by default', () => {
      const r = new BinaryReader(Buffer.alloc(10));
      expect(r.position).toBe(0);
    });

    it('should start at specified offset', () => {
      const r = new BinaryReader(Buffer.alloc(10), 5);
      expect(r.position).toBe(5);
    });

    it('should advance after reads', () => {
      const r = new BinaryReader(Buffer.alloc(10));
      r.readByte();
      expect(r.position).toBe(1);
      r.readInt32();
      expect(r.position).toBe(5);
    });
  });

  describe('remaining', () => {
    it('should return total length when nothing has been read', () => {
      const r = new BinaryReader(Buffer.alloc(10));
      expect(r.remaining).toBe(10);
    });

    it('should decrease after reads', () => {
      const r = new BinaryReader(Buffer.alloc(10));
      r.readByte();
      expect(r.remaining).toBe(9);
    });

    it('should be 0 at end', () => {
      const r = new BinaryReader(Buffer.alloc(1));
      r.readByte();
      expect(r.remaining).toBe(0);
    });
  });

  describe('isAtEnd', () => {
    it('should be false when data remains', () => {
      const r = new BinaryReader(Buffer.alloc(1));
      expect(r.isAtEnd).toBe(false);
    });

    it('should be true when all data is consumed', () => {
      const r = new BinaryReader(Buffer.alloc(1));
      r.readByte();
      expect(r.isAtEnd).toBe(true);
    });

    it('should be true for an empty buffer', () => {
      const r = new BinaryReader(Buffer.alloc(0));
      expect(r.isAtEnd).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // readBytes
  // ---------------------------------------------------------------------------

  describe('readBytes', () => {
    it('should read a specified number of raw bytes', () => {
      const r = new BinaryReader(Buffer.from([0x01, 0x02, 0x03, 0x04]));
      const result = r.readBytes(2);
      expect(result).toEqual(Buffer.from([0x01, 0x02]));
      expect(r.position).toBe(2);
    });

    it('should return a copy of the data', () => {
      const source = Buffer.from([0xaa, 0xbb]);
      const r = new BinaryReader(source);
      const result = r.readBytes(2);
      result[0] = 0x00;
      expect(source[0]).toBe(0xaa); // original unaffected
    });
  });

  // ---------------------------------------------------------------------------
  // readFloat
  // ---------------------------------------------------------------------------

  describe('readFloat', () => {
    it('should read a 32-bit float', () => {
      const buf = Buffer.alloc(4);
      buf.writeFloatLE(3.14, 0);
      const r = new BinaryReader(buf);
      expect(r.readFloat()).toBeCloseTo(3.14, 2);
    });
  });

  // ---------------------------------------------------------------------------
  // Sequential reads
  // ---------------------------------------------------------------------------

  describe('sequential reads', () => {
    it('should correctly read multiple types in sequence', () => {
      const buf = Buffer.alloc(1 + 4 + 2);
      buf.writeUInt8(0xff, 0);
      buf.writeInt32LE(12345, 1);
      buf.writeInt16LE(-100, 5);
      const r = new BinaryReader(buf);
      expect(r.readByte()).toBe(0xff);
      expect(r.readInt32()).toBe(12345);
      expect(r.readInt16()).toBe(-100);
      expect(r.isAtEnd).toBe(true);
    });
  });
});
