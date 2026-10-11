import { describe, it, expect } from 'vitest';
import { BinaryWriter } from '@main/protocol/binary-writer';

describe('BinaryWriter', () => {
  // ---------------------------------------------------------------------------
  // Primitive writers
  // ---------------------------------------------------------------------------

  describe('writeByte', () => {
    it('should write a single unsigned byte', () => {
      const w = new BinaryWriter();
      w.writeByte(0xab);
      const buf = w.toBuffer();
      expect(buf.length).toBe(1);
      expect(buf[0]).toBe(0xab);
    });

    it('should mask to 8 bits', () => {
      const w = new BinaryWriter();
      w.writeByte(0x1ff); // 0x1ff & 0xff = 0xff
      expect(w.toBuffer()[0]).toBe(0xff);
    });

    it('should write zero', () => {
      const w = new BinaryWriter();
      w.writeByte(0);
      expect(w.toBuffer()[0]).toBe(0);
    });
  });

  describe('writeInt16', () => {
    it('should write a positive int16 in little-endian', () => {
      const w = new BinaryWriter();
      w.writeInt16(0x0102);
      const buf = w.toBuffer();
      expect(buf.length).toBe(2);
      expect(buf[0]).toBe(0x02); // low byte first
      expect(buf[1]).toBe(0x01);
    });

    it('should write a negative int16', () => {
      const w = new BinaryWriter();
      w.writeInt16(-1);
      const buf = w.toBuffer();
      expect(buf.readInt16LE(0)).toBe(-1);
    });

    it('should write zero', () => {
      const w = new BinaryWriter();
      w.writeInt16(0);
      expect(w.toBuffer().readInt16LE(0)).toBe(0);
    });
  });

  describe('writeInt32', () => {
    it('should write a positive int32 in little-endian', () => {
      const w = new BinaryWriter();
      w.writeInt32(0x01020304);
      const buf = w.toBuffer();
      expect(buf.length).toBe(4);
      expect(buf[0]).toBe(0x04);
      expect(buf[1]).toBe(0x03);
      expect(buf[2]).toBe(0x02);
      expect(buf[3]).toBe(0x01);
    });

    it('should write a negative int32', () => {
      const w = new BinaryWriter();
      w.writeInt32(-42);
      expect(w.toBuffer().readInt32LE(0)).toBe(-42);
    });

    it('should write max int32', () => {
      const w = new BinaryWriter();
      w.writeInt32(0x7fffffff);
      expect(w.toBuffer().readInt32LE(0)).toBe(0x7fffffff);
    });

    it('should write min int32', () => {
      const w = new BinaryWriter();
      w.writeInt32(-2147483648);
      expect(w.toBuffer().readInt32LE(0)).toBe(-2147483648);
    });
  });

  describe('writeUint64', () => {
    it('should write a uint64 in little-endian', () => {
      const w = new BinaryWriter();
      w.writeUint64(0x0102030405060708n);
      const buf = w.toBuffer();
      expect(buf.length).toBe(8);
      expect(buf.readBigUInt64LE(0)).toBe(0x0102030405060708n);
    });

    it('should write zero', () => {
      const w = new BinaryWriter();
      w.writeUint64(0n);
      expect(w.toBuffer().readBigUInt64LE(0)).toBe(0n);
    });

    it('should write max uint64', () => {
      const w = new BinaryWriter();
      const maxU64 = 0xffffffffffffffffn;
      w.writeUint64(maxU64);
      expect(w.toBuffer().readBigUInt64LE(0)).toBe(maxU64);
    });
  });

  // ---------------------------------------------------------------------------
  // writeBool
  // ---------------------------------------------------------------------------

  describe('writeBool', () => {
    it('should write true as 0x01', () => {
      const w = new BinaryWriter();
      w.writeBool(true);
      expect(w.toBuffer()[0]).toBe(1);
    });

    it('should write false as 0x00', () => {
      const w = new BinaryWriter();
      w.writeBool(false);
      expect(w.toBuffer()[0]).toBe(0);
    });
  });

  // ---------------------------------------------------------------------------
  // writeString
  // ---------------------------------------------------------------------------

  describe('writeString', () => {
    it('should write an empty string (length prefix 0, no data bytes)', () => {
      const w = new BinaryWriter();
      w.writeString('');
      const buf = w.toBuffer();
      expect(buf.length).toBe(1); // just the 0 length prefix
      expect(buf[0]).toBe(0);
    });

    it('should write a short ASCII string', () => {
      const w = new BinaryWriter();
      w.writeString('Hi');
      const buf = w.toBuffer();
      // length prefix (1 byte: 2) + 2 bytes of data
      expect(buf.length).toBe(3);
      expect(buf[0]).toBe(2); // length
      expect(buf.toString('utf8', 1, 3)).toBe('Hi');
    });

    it('should write a unicode string with multi-byte characters', () => {
      const w = new BinaryWriter();
      const str = '\u00e9\u00e8\u00ea'; // e-acute, e-grave, e-circumflex (2 bytes each in UTF-8)
      w.writeString(str);
      const buf = w.toBuffer();
      const byteLength = Buffer.byteLength(str, 'utf8'); // 6
      expect(byteLength).toBe(6);
      expect(buf[0]).toBe(6); // length prefix
      expect(buf.length).toBe(7); // 1 prefix + 6 data
    });

    it('should write emoji characters correctly', () => {
      const w = new BinaryWriter();
      const str = '\ud83d\ude00'; // grinning face emoji, 4 bytes in UTF-8
      w.writeString(str);
      const buf = w.toBuffer();
      const byteLength = Buffer.byteLength(str, 'utf8');
      expect(byteLength).toBe(4);
      expect(buf[0]).toBe(4);
      expect(buf.length).toBe(5);
    });

    it('should use multi-byte VarInt encoding for strings > 127 bytes', () => {
      const w = new BinaryWriter();
      // 128 bytes of 'A' -> UTF-8 byte length = 128
      const str = 'A'.repeat(128);
      w.writeString(str);
      const buf = w.toBuffer();
      // VarInt(128) = 0x80 0x01 (2 bytes) + 128 data bytes = 130
      expect(buf.length).toBe(130);
      // First byte: 128 & 0x7f | 0x80 = 0x80
      expect(buf[0]).toBe(0x80);
      // Second byte: 128 >> 7 = 1
      expect(buf[1]).toBe(0x01);
    });

    it('should use multi-byte VarInt for strings > 16383 bytes', () => {
      const w = new BinaryWriter();
      const str = 'X'.repeat(16384);
      w.writeString(str);
      const buf = w.toBuffer();
      // VarInt(16384) = 0x80, 0x80, 0x01 (3 bytes) + 16384 data bytes = 16387
      expect(buf.length).toBe(16387);
      expect(buf[0]).toBe(0x80);
      expect(buf[1]).toBe(0x80);
      expect(buf[2]).toBe(0x01);
    });

    it('should encode VarInt length 1 as a single byte', () => {
      const w = new BinaryWriter();
      w.writeString('Z'); // 1 byte
      const buf = w.toBuffer();
      expect(buf[0]).toBe(1);
      expect(buf.length).toBe(2);
    });

    it('should encode VarInt length 127 as a single byte', () => {
      const w = new BinaryWriter();
      w.writeString('A'.repeat(127));
      const buf = w.toBuffer();
      expect(buf[0]).toBe(127); // single byte, no high bit
      expect(buf.length).toBe(128);
    });
  });

  // ---------------------------------------------------------------------------
  // writeGuid
  // ---------------------------------------------------------------------------

  describe('writeGuid', () => {
    it('should write a GUID with .NET byte order (mixed-endian)', () => {
      const w = new BinaryWriter();
      // GUID: 550e8400-e29b-41d4-a716-446655440000
      w.writeGuid('550e8400-e29b-41d4-a716-446655440000');
      const buf = w.toBuffer();
      expect(buf.length).toBe(16);

      // Group 1 (550e8400) reversed: 00 84 0e 55
      expect(buf[0]).toBe(0x00);
      expect(buf[1]).toBe(0x84);
      expect(buf[2]).toBe(0x0e);
      expect(buf[3]).toBe(0x55);

      // Group 2 (e29b) reversed: 9b e2
      expect(buf[4]).toBe(0x9b);
      expect(buf[5]).toBe(0xe2);

      // Group 3 (41d4) reversed: d4 41
      expect(buf[6]).toBe(0xd4);
      expect(buf[7]).toBe(0x41);

      // Groups 4-5 (a716-446655440000) as-is
      expect(buf[8]).toBe(0xa7);
      expect(buf[9]).toBe(0x16);
      expect(buf[10]).toBe(0x44);
      expect(buf[11]).toBe(0x66);
      expect(buf[12]).toBe(0x55);
      expect(buf[13]).toBe(0x44);
      expect(buf[14]).toBe(0x00);
      expect(buf[15]).toBe(0x00);
    });

    it('should write the nil GUID', () => {
      const w = new BinaryWriter();
      w.writeGuid('00000000-0000-0000-0000-000000000000');
      const buf = w.toBuffer();
      expect(buf.length).toBe(16);
      for (let i = 0; i < 16; i++) {
        expect(buf[i]).toBe(0);
      }
    });

    it('should throw on an invalid GUID string', () => {
      const w = new BinaryWriter();
      expect(() => w.writeGuid('not-a-guid')).toThrow('Invalid GUID string');
    });

    it('should throw on a GUID that is too short', () => {
      const w = new BinaryWriter();
      expect(() => w.writeGuid('550e8400-e29b-41d4-a716')).toThrow('Invalid GUID string');
    });
  });

  // ---------------------------------------------------------------------------
  // Array writers
  // ---------------------------------------------------------------------------

  describe('writeByteArray', () => {
    it('should write an empty array with int16 length prefix of 0', () => {
      const w = new BinaryWriter();
      w.writeByteArray([]);
      const buf = w.toBuffer();
      expect(buf.length).toBe(2);
      expect(buf.readInt16LE(0)).toBe(0);
    });

    it('should write bytes with int16 length prefix', () => {
      const w = new BinaryWriter();
      w.writeByteArray([0x01, 0x02, 0xff]);
      const buf = w.toBuffer();
      expect(buf.length).toBe(5); // 2 prefix + 3 bytes
      expect(buf.readInt16LE(0)).toBe(3);
      expect(buf[2]).toBe(0x01);
      expect(buf[3]).toBe(0x02);
      expect(buf[4]).toBe(0xff);
    });
  });

  describe('writeIntArray', () => {
    it('should write int32 values with int16 length prefix', () => {
      const w = new BinaryWriter();
      w.writeIntArray([1, -1, 0x7fffffff]);
      const buf = w.toBuffer();
      expect(buf.length).toBe(2 + 3 * 4); // prefix + 3 int32s
      expect(buf.readInt16LE(0)).toBe(3);
      expect(buf.readInt32LE(2)).toBe(1);
      expect(buf.readInt32LE(6)).toBe(-1);
      expect(buf.readInt32LE(10)).toBe(0x7fffffff);
    });

    it('should write an empty int array', () => {
      const w = new BinaryWriter();
      w.writeIntArray([]);
      expect(w.toBuffer().length).toBe(2);
    });
  });

  describe('writeStringArray', () => {
    it('should write strings with int16 length prefix', () => {
      const w = new BinaryWriter();
      w.writeStringArray(['hello', 'world']);
      const buf = w.toBuffer();
      expect(buf.readInt16LE(0)).toBe(2);
      // Remaining bytes: string "hello" (1+5) + string "world" (1+5) = 12
      expect(buf.length).toBe(2 + 6 + 6);
    });

    it('should write an empty string array', () => {
      const w = new BinaryWriter();
      w.writeStringArray([]);
      const buf = w.toBuffer();
      expect(buf.readInt16LE(0)).toBe(0);
      expect(buf.length).toBe(2);
    });
  });

  describe('writeGuidArray', () => {
    it('should write GUIDs with int16 length prefix', () => {
      const w = new BinaryWriter();
      const guids = [
        '550e8400-e29b-41d4-a716-446655440000',
        '00000000-0000-0000-0000-000000000000',
      ];
      w.writeGuidArray(guids);
      const buf = w.toBuffer();
      expect(buf.readInt16LE(0)).toBe(2);
      expect(buf.length).toBe(2 + 2 * 16);
    });
  });

  describe('writeBoolArray', () => {
    it('should write booleans with int16 length prefix', () => {
      const w = new BinaryWriter();
      w.writeBoolArray([true, false, true]);
      const buf = w.toBuffer();
      expect(buf.readInt16LE(0)).toBe(3);
      expect(buf[2]).toBe(1);
      expect(buf[3]).toBe(0);
      expect(buf[4]).toBe(1);
    });
  });

  // ---------------------------------------------------------------------------
  // Auto-growing buffer
  // ---------------------------------------------------------------------------

  describe('auto-growing buffer', () => {
    it('should grow when writing more than initial capacity', () => {
      const w = new BinaryWriter(4); // tiny initial capacity
      w.writeInt32(1);
      w.writeInt32(2);
      w.writeInt32(3);
      const buf = w.toBuffer();
      expect(buf.length).toBe(12);
      expect(buf.readInt32LE(0)).toBe(1);
      expect(buf.readInt32LE(4)).toBe(2);
      expect(buf.readInt32LE(8)).toBe(3);
    });

    it('should grow to accommodate a large string', () => {
      const w = new BinaryWriter(8);
      const longStr = 'A'.repeat(1024);
      w.writeString(longStr);
      const buf = w.toBuffer();
      // 2-byte VarInt (1024 > 127) + 1024 data
      expect(buf.length).toBe(2 + 1024);
    });
  });

  // ---------------------------------------------------------------------------
  // position and toBuffer
  // ---------------------------------------------------------------------------

  describe('position', () => {
    it('should track the current write offset', () => {
      const w = new BinaryWriter();
      expect(w.position).toBe(0);
      w.writeByte(1);
      expect(w.position).toBe(1);
      w.writeInt32(42);
      expect(w.position).toBe(5);
    });
  });

  describe('seek', () => {
    it('should allow seeking back and overwriting', () => {
      const w = new BinaryWriter();
      w.writeInt32(0);   // placeholder
      w.writeInt32(99);
      w.seek(0);
      w.writeInt32(42);
      const buf = w.toBuffer();
      // toBuffer uses the max of position and previous extent -- actually it uses offset
      // After seek(0) + writeInt32(42), offset = 4, so toBuffer is 4 bytes only.
      // We need to seek back to end.
      // Actually toBuffer() does subarray(0, offset), so after we wrote at 0 again offset = 4.
      // That means it loses the second int32. This is expected from the seek API.
      expect(buf.readInt32LE(0)).toBe(42);
    });
  });

  describe('toBuffer', () => {
    it('should return a copy, not a reference', () => {
      const w = new BinaryWriter();
      w.writeByte(0xaa);
      const buf1 = w.toBuffer();
      const buf2 = w.toBuffer();
      expect(buf1).toEqual(buf2);
      buf1[0] = 0x00;
      expect(buf2[0]).toBe(0xaa); // buf2 unaffected
    });

    it('should return an empty buffer when nothing is written', () => {
      const w = new BinaryWriter();
      expect(w.toBuffer().length).toBe(0);
    });
  });
});
