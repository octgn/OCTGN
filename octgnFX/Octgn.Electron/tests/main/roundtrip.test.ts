import { describe, it, expect } from 'vitest';
import { BinaryWriter } from '@main/protocol/binary-writer';
import { BinaryReader } from '@main/protocol/binary-reader';

describe('BinaryWriter -> BinaryReader roundtrip', () => {
  // ---------------------------------------------------------------------------
  // Primitive roundtrips
  // ---------------------------------------------------------------------------

  describe('primitive types', () => {
    it('should roundtrip a byte', () => {
      const w = new BinaryWriter();
      w.writeByte(0xab);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readByte()).toBe(0xab);
      expect(r.isAtEnd).toBe(true);
    });

    it('should roundtrip int16', () => {
      for (const val of [0, 1, -1, 32767, -32768]) {
        const w = new BinaryWriter();
        w.writeInt16(val);
        const r = new BinaryReader(w.toBuffer());
        expect(r.readInt16()).toBe(val);
      }
    });

    it('should roundtrip int32', () => {
      for (const val of [0, 1, -1, 2147483647, -2147483648, 42, -42]) {
        const w = new BinaryWriter();
        w.writeInt32(val);
        const r = new BinaryReader(w.toBuffer());
        expect(r.readInt32()).toBe(val);
      }
    });

    it('should roundtrip uint64', () => {
      for (const val of [0n, 1n, 0xffffffffffffffffn, 12345678901234n]) {
        const w = new BinaryWriter();
        w.writeUint64(val);
        const r = new BinaryReader(w.toBuffer());
        expect(r.readUint64()).toBe(val);
      }
    });

    it('should roundtrip float', () => {
      const w = new BinaryWriter();
      w.writeFloat(3.14);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readFloat()).toBeCloseTo(3.14, 2);
    });

    it('should roundtrip bool true', () => {
      const w = new BinaryWriter();
      w.writeBool(true);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readBool()).toBe(true);
    });

    it('should roundtrip bool false', () => {
      const w = new BinaryWriter();
      w.writeBool(false);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readBool()).toBe(false);
    });
  });

  // ---------------------------------------------------------------------------
  // String roundtrips
  // ---------------------------------------------------------------------------

  describe('string roundtrips', () => {
    it('should roundtrip an empty string', () => {
      const w = new BinaryWriter();
      w.writeString('');
      const r = new BinaryReader(w.toBuffer());
      expect(r.readString()).toBe('');
    });

    it('should roundtrip a short ASCII string', () => {
      const w = new BinaryWriter();
      w.writeString('Hello, OCTGN!');
      const r = new BinaryReader(w.toBuffer());
      expect(r.readString()).toBe('Hello, OCTGN!');
    });

    it('should roundtrip unicode strings', () => {
      const testStrings = [
        '\u00e9\u00e8\u00ea',           // accented characters
        '\u4f60\u597d',                   // Chinese "Hello"
        '\ud83d\ude00\ud83d\ude01\ud83d\ude02', // emojis
        '\u00fc\u00f6\u00e4\u00df',       // German umlauts
        '\u0410\u0411\u0412',             // Cyrillic
      ];
      for (const str of testStrings) {
        const w = new BinaryWriter();
        w.writeString(str);
        const r = new BinaryReader(w.toBuffer());
        expect(r.readString()).toBe(str);
      }
    });

    it('should roundtrip a string with length > 127 (multi-byte VarInt)', () => {
      const str = 'A'.repeat(200);
      const w = new BinaryWriter();
      w.writeString(str);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readString()).toBe(str);
    });

    it('should roundtrip a string with length > 16383 (3-byte VarInt)', () => {
      const str = 'B'.repeat(20000);
      const w = new BinaryWriter();
      w.writeString(str);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readString()).toBe(str);
    });

    it('should roundtrip a string at the 127-byte boundary', () => {
      const str127 = 'C'.repeat(127);
      const str128 = 'C'.repeat(128);
      const w = new BinaryWriter();
      w.writeString(str127);
      w.writeString(str128);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readString()).toBe(str127);
      expect(r.readString()).toBe(str128);
    });
  });

  // ---------------------------------------------------------------------------
  // GUID roundtrips
  // ---------------------------------------------------------------------------

  describe('GUID roundtrips', () => {
    it('should roundtrip a standard GUID', () => {
      const guid = '550e8400-e29b-41d4-a716-446655440000';
      const w = new BinaryWriter();
      w.writeGuid(guid);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readGuid()).toBe(guid);
    });

    it('should roundtrip the nil GUID', () => {
      const guid = '00000000-0000-0000-0000-000000000000';
      const w = new BinaryWriter();
      w.writeGuid(guid);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readGuid()).toBe(guid);
    });

    it('should roundtrip a max GUID', () => {
      const guid = 'ffffffff-ffff-ffff-ffff-ffffffffffff';
      const w = new BinaryWriter();
      w.writeGuid(guid);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readGuid()).toBe(guid);
    });

    it('should roundtrip multiple GUIDs', () => {
      const guids = [
        'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
        '12345678-9abc-def0-1234-56789abcdef0',
        'deadbeef-cafe-babe-dead-beefcafebabe',
      ];
      const w = new BinaryWriter();
      for (const g of guids) w.writeGuid(g);
      const r = new BinaryReader(w.toBuffer());
      for (const g of guids) expect(r.readGuid()).toBe(g);
    });
  });

  // ---------------------------------------------------------------------------
  // Array roundtrips
  // ---------------------------------------------------------------------------

  describe('array roundtrips', () => {
    it('should roundtrip a byte array', () => {
      const values = [0, 1, 127, 255];
      const w = new BinaryWriter();
      w.writeByteArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readByteArray()).toEqual(values);
    });

    it('should roundtrip an int array', () => {
      const values = [0, -1, 42, 2147483647, -2147483648];
      const w = new BinaryWriter();
      w.writeIntArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readIntArray()).toEqual(values);
    });

    it('should roundtrip a string array', () => {
      const values = ['hello', '', 'world', '\u00e9\u00e8', 'A'.repeat(200)];
      const w = new BinaryWriter();
      w.writeStringArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readStringArray()).toEqual(values);
    });

    it('should roundtrip a GUID array', () => {
      const values = [
        '550e8400-e29b-41d4-a716-446655440000',
        '00000000-0000-0000-0000-000000000000',
        'deadbeef-cafe-babe-dead-beefcafebabe',
      ];
      const w = new BinaryWriter();
      w.writeGuidArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readGuidArray()).toEqual(values);
    });

    it('should roundtrip a bool array', () => {
      const values = [true, false, true, false, false, true];
      const w = new BinaryWriter();
      w.writeBoolArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readBoolArray()).toEqual(values);
    });

    it('should roundtrip a short array', () => {
      const values = [0, -1, 32767, -32768, 100];
      const w = new BinaryWriter();
      w.writeShortArray(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readShortArray()).toEqual(values);
    });

    it('should roundtrip a uint64 array', () => {
      const values = [0n, 1n, 0xffffffffffffffffn, 999999n];
      const w = new BinaryWriter();
      w.writeUint64Array(values);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readUint64Array()).toEqual(values);
    });

    it('should roundtrip empty arrays', () => {
      const w = new BinaryWriter();
      w.writeByteArray([]);
      w.writeIntArray([]);
      w.writeStringArray([]);
      w.writeGuidArray([]);
      w.writeBoolArray([]);
      const r = new BinaryReader(w.toBuffer());
      expect(r.readByteArray()).toEqual([]);
      expect(r.readIntArray()).toEqual([]);
      expect(r.readStringArray()).toEqual([]);
      expect(r.readGuidArray()).toEqual([]);
      expect(r.readBoolArray()).toEqual([]);
    });
  });

  // ---------------------------------------------------------------------------
  // Multiple values in sequence
  // ---------------------------------------------------------------------------

  describe('sequential multi-type roundtrip', () => {
    it('should write and read multiple types in order', () => {
      const w = new BinaryWriter();
      w.writeByte(42);
      w.writeInt16(-100);
      w.writeInt32(123456);
      w.writeUint64(99999999999n);
      w.writeBool(true);
      w.writeBool(false);
      w.writeString('test string');
      w.writeGuid('a1b2c3d4-e5f6-7890-abcd-ef1234567890');
      w.writeIntArray([1, 2, 3]);
      w.writeStringArray(['foo', 'bar']);

      const r = new BinaryReader(w.toBuffer());
      expect(r.readByte()).toBe(42);
      expect(r.readInt16()).toBe(-100);
      expect(r.readInt32()).toBe(123456);
      expect(r.readUint64()).toBe(99999999999n);
      expect(r.readBool()).toBe(true);
      expect(r.readBool()).toBe(false);
      expect(r.readString()).toBe('test string');
      expect(r.readGuid()).toBe('a1b2c3d4-e5f6-7890-abcd-ef1234567890');
      expect(r.readIntArray()).toEqual([1, 2, 3]);
      expect(r.readStringArray()).toEqual(['foo', 'bar']);
      expect(r.isAtEnd).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // Byte-for-byte verification
  // ---------------------------------------------------------------------------

  describe('byte-for-byte buffer match', () => {
    it('should produce identical buffers for identical writes', () => {
      const w1 = new BinaryWriter();
      const w2 = new BinaryWriter();

      w1.writeInt32(42);
      w1.writeString('hello');
      w1.writeGuid('deadbeef-cafe-babe-dead-beefcafebabe');

      w2.writeInt32(42);
      w2.writeString('hello');
      w2.writeGuid('deadbeef-cafe-babe-dead-beefcafebabe');

      expect(w1.toBuffer().equals(w2.toBuffer())).toBe(true);
    });

    it('should produce different buffers for different values', () => {
      const w1 = new BinaryWriter();
      const w2 = new BinaryWriter();

      w1.writeInt32(42);
      w2.writeInt32(43);

      expect(w1.toBuffer().equals(w2.toBuffer())).toBe(false);
    });
  });
});
