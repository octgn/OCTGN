import { describe, it, expect } from 'vitest';
import { serializeMessage, deserializeMessage, getMessageTypeName } from '@main/protocol/serializer';
import { MessageType, type ProtocolMessage } from '@main/protocol/types';

/**
 * Helper: serialize a message, then strip the 4-byte length prefix and
 * deserialize the result, returning the round-tripped ProtocolMessage.
 */
function roundtrip(msg: ProtocolMessage): ProtocolMessage {
  const buf = serializeMessage(msg);
  // The first 4 bytes are the length prefix; deserializeMessage expects
  // the payload starting at byte 4.
  const payload = buf.subarray(4);
  return deserializeMessage(payload);
}

describe('Serializer', () => {
  // ---------------------------------------------------------------------------
  // Hello (type 3) - complex message with strings, ulong, GUID, bool
  // ---------------------------------------------------------------------------

  describe('Hello message', () => {
    it('should serialize and deserialize a Hello message', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Hello,
        params: {
          nick: 'TestPlayer',
          userId: 'user-123',
          pkey: 123456789012345n,
          client: 'OCTGN.Electron',
          clientVer: '1.0.0',
          octgnVer: '3.4.424.0',
          gameId: '550e8400-e29b-41d4-a716-446655440000',
          gameVersion: '2.1.0',
          password: 'secret',
          spectator: false,
        },
      };

      const result = roundtrip(msg);
      expect(result.isMuted).toBe(0);
      expect(result.type).toBe(MessageType.Hello);
      expect(result.params.nick).toBe('TestPlayer');
      expect(result.params.userId).toBe('user-123');
      expect(result.params.pkey).toBe(123456789012345n);
      expect(result.params.client).toBe('OCTGN.Electron');
      expect(result.params.clientVer).toBe('1.0.0');
      expect(result.params.octgnVer).toBe('3.4.424.0');
      expect(result.params.gameId).toBe('550e8400-e29b-41d4-a716-446655440000');
      expect(result.params.gameVersion).toBe('2.1.0');
      expect(result.params.password).toBe('secret');
      expect(result.params.spectator).toBe(false);
    });

    it('should handle spectator=true', () => {
      const msg: ProtocolMessage = {
        isMuted: 1,
        type: MessageType.Hello,
        params: {
          nick: 'Spectator1',
          userId: 'spec-1',
          pkey: 0n,
          client: 'OCTGN',
          clientVer: '1.0.0',
          octgnVer: '3.4.0.0',
          gameId: '00000000-0000-0000-0000-000000000000',
          gameVersion: '1.0.0',
          password: '',
          spectator: true,
        },
      };

      const result = roundtrip(msg);
      expect(result.params.spectator).toBe(true);
      expect(result.isMuted).toBe(1);
    });
  });

  // ---------------------------------------------------------------------------
  // Welcome (type 5)
  // ---------------------------------------------------------------------------

  describe('Welcome message', () => {
    it('should serialize and deserialize a Welcome message', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Welcome,
        params: {
          id: 1,
          gameSessionId: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
          gameName: 'Test Game',
          waitForGameState: true,
        },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.Welcome);
      expect(result.params.id).toBe(1);
      expect(result.params.gameSessionId).toBe('a1b2c3d4-e5f6-7890-abcd-ef1234567890');
      expect(result.params.gameName).toBe('Test Game');
      expect(result.params.waitForGameState).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // Chat (type 22) and ChatReq (type 21)
  // ---------------------------------------------------------------------------

  describe('Chat messages', () => {
    it('should serialize and deserialize ChatReq', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.ChatReq,
        params: { text: 'Hello everyone!' },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.ChatReq);
      expect(result.params.text).toBe('Hello everyone!');
    });

    it('should serialize and deserialize Chat', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Chat,
        params: { player: 2, text: 'GG!' },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.Chat);
      expect(result.params.player).toBe(2);
      expect(result.params.text).toBe('GG!');
    });

    it('should handle unicode in chat text', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.ChatReq,
        params: { text: '\ud83d\ude00 \u4f60\u597d \u00fc\u00f6\u00e4' },
      };

      const result = roundtrip(msg);
      expect(result.params.text).toBe('\ud83d\ude00 \u4f60\u597d \u00fc\u00f6\u00e4');
    });

    it('should handle empty chat text', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.ChatReq,
        params: { text: '' },
      };

      const result = roundtrip(msg);
      expect(result.params.text).toBe('');
    });
  });

  // ---------------------------------------------------------------------------
  // LoadDeck (type 29) - arrays
  // ---------------------------------------------------------------------------

  describe('LoadDeck message', () => {
    it('should serialize and deserialize LoadDeck with arrays', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.LoadDeck,
        params: {
          id: [100, 101, 102],
          type: [
            '550e8400-e29b-41d4-a716-446655440000',
            'deadbeef-cafe-babe-dead-beefcafebabe',
            '00000000-0000-0000-0000-000000000000',
          ],
          group: [1, 2, 1],
          size: ['standard', 'standard', 'small'],
          sleeve: 'default-sleeve',
          limited: false,
        },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.LoadDeck);
      expect(result.params.id).toEqual([100, 101, 102]);
      expect(result.params.type).toEqual([
        '550e8400-e29b-41d4-a716-446655440000',
        'deadbeef-cafe-babe-dead-beefcafebabe',
        '00000000-0000-0000-0000-000000000000',
      ]);
      expect(result.params.group).toEqual([1, 2, 1]);
      expect(result.params.size).toEqual(['standard', 'standard', 'small']);
      expect(result.params.sleeve).toBe('default-sleeve');
      expect(result.params.limited).toBe(false);
    });

    it('should handle empty arrays in LoadDeck', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.LoadDeck,
        params: {
          id: [],
          type: [],
          group: [],
          size: [],
          sleeve: '',
          limited: true,
        },
      };

      const result = roundtrip(msg);
      expect(result.params.id).toEqual([]);
      expect(result.params.type).toEqual([]);
      expect(result.params.group).toEqual([]);
      expect(result.params.size).toEqual([]);
      expect(result.params.sleeve).toBe('');
      expect(result.params.limited).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // MoveCard (type 34) - arrays + bools
  // ---------------------------------------------------------------------------

  describe('MoveCard message', () => {
    it('should serialize and deserialize MoveCard with arrays and bools', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.MoveCard,
        params: {
          player: 1,
          id: [10, 20, 30],
          group: 5,
          idx: [0, 1, 2],
          faceUp: [true, false, true],
          isScriptMove: false,
        },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.MoveCard);
      expect(result.params.player).toBe(1);
      expect(result.params.id).toEqual([10, 20, 30]);
      expect(result.params.group).toBe(5);
      expect(result.params.idx).toEqual([0, 1, 2]);
      expect(result.params.faceUp).toEqual([true, false, true]);
      expect(result.params.isScriptMove).toBe(false);
    });

    it('should handle MoveCardReq', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.MoveCardReq,
        params: {
          id: [42],
          group: 3,
          idx: [0],
          faceUp: [true],
          isScriptMove: true,
        },
      };

      const result = roundtrip(msg);
      expect(result.params.id).toEqual([42]);
      expect(result.params.faceUp).toEqual([true]);
      expect(result.params.isScriptMove).toBe(true);
    });
  });

  // ---------------------------------------------------------------------------
  // Settings (type 6) - booleans
  // ---------------------------------------------------------------------------

  describe('Settings message', () => {
    it('should serialize and deserialize Settings with all booleans', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Settings,
        params: {
          twoSidedTable: true,
          allowSpectators: false,
          muteSpectators: true,
          allowCardList: false,
        },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.Settings);
      expect(result.params.twoSidedTable).toBe(true);
      expect(result.params.allowSpectators).toBe(false);
      expect(result.params.muteSpectators).toBe(true);
      expect(result.params.allowCardList).toBe(false);
    });

    it('should handle all true', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Settings,
        params: {
          twoSidedTable: true,
          allowSpectators: true,
          muteSpectators: true,
          allowCardList: true,
        },
      };

      const result = roundtrip(msg);
      expect(result.params.twoSidedTable).toBe(true);
      expect(result.params.allowSpectators).toBe(true);
      expect(result.params.muteSpectators).toBe(true);
      expect(result.params.allowCardList).toBe(true);
    });

    it('should handle all false', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Settings,
        params: {
          twoSidedTable: false,
          allowSpectators: false,
          muteSpectators: false,
          allowCardList: false,
        },
      };

      const result = roundtrip(msg);
      expect(result.params.twoSidedTable).toBe(false);
      expect(result.params.allowSpectators).toBe(false);
      expect(result.params.muteSpectators).toBe(false);
      expect(result.params.allowCardList).toBe(false);
    });
  });

  // ---------------------------------------------------------------------------
  // Ping (type 87) - no params
  // ---------------------------------------------------------------------------

  describe('Ping message', () => {
    it('should serialize and deserialize Ping (no parameters)', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Ping,
        params: {},
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.Ping);
      expect(result.params).toEqual({});
    });

    it('should handle non-zero isMuted', () => {
      const msg: ProtocolMessage = {
        isMuted: 42,
        type: MessageType.Ping,
        params: {},
      };

      const result = roundtrip(msg);
      expect(result.isMuted).toBe(42);
    });
  });

  // ---------------------------------------------------------------------------
  // Error (type 0) - string
  // ---------------------------------------------------------------------------

  describe('Error message', () => {
    it('should serialize and deserialize Error with a string', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Error,
        params: { msg: 'Something went wrong' },
      };

      const result = roundtrip(msg);
      expect(result.type).toBe(MessageType.Error);
      expect(result.params.msg).toBe('Something went wrong');
    });

    it('should handle empty error message', () => {
      const msg: ProtocolMessage = {
        isMuted: 0,
        type: MessageType.Error,
        params: { msg: '' },
      };

      const result = roundtrip(msg);
      expect(result.params.msg).toBe('');
    });
  });

  // ---------------------------------------------------------------------------
  // Additional message types for coverage
  // ---------------------------------------------------------------------------

  describe('additional message types', () => {
    it('should roundtrip Boot', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Boot,
        params: { player: 3, reason: 'AFK' },
      });
      expect(result.params.player).toBe(3);
      expect(result.params.reason).toBe('AFK');
    });

    it('should roundtrip Kick', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Kick,
        params: { reason: 'Cheating' },
      });
      expect(result.params.reason).toBe('Cheating');
    });

    it('should roundtrip HelloAgain', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.HelloAgain,
        params: {
          pid: 2,
          nick: 'Reconnect',
          userId: 'u-2',
          pkey: 42n,
          client: 'OCTGN',
          clientVer: '1.0',
          octgnVer: '3.4',
          gameId: 'deadbeef-cafe-babe-dead-beefcafebabe',
          gameVersion: '1.0',
          password: '',
        },
      });
      expect(result.params.pid).toBe(2);
      expect(result.params.nick).toBe('Reconnect');
      expect(result.params.pkey).toBe(42n);
      expect(result.params.gameId).toBe('deadbeef-cafe-babe-dead-beefcafebabe');
    });

    it('should roundtrip PlayerSettings', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.PlayerSettings,
        params: { playerId: 1, invertedTable: true, spectator: false },
      });
      expect(result.params.playerId).toBe(1);
      expect(result.params.invertedTable).toBe(true);
      expect(result.params.spectator).toBe(false);
    });

    it('should roundtrip NewPlayer', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.NewPlayer,
        params: {
          id: 5,
          nick: 'NewGuy',
          userId: 'ng-1',
          pkey: 100n,
          tableSide: true,
          spectator: false,
        },
      });
      expect(result.params.id).toBe(5);
      expect(result.params.nick).toBe('NewGuy');
      expect(result.params.pkey).toBe(100n);
    });

    it('should roundtrip Start (no params)', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Start,
        params: {},
      });
      expect(result.params).toEqual({});
    });

    it('should roundtrip Counter', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Counter,
        params: { player: 1, counter: 10, value: -5, isScriptChange: true },
      });
      expect(result.params.player).toBe(1);
      expect(result.params.counter).toBe(10);
      expect(result.params.value).toBe(-5);
      expect(result.params.isScriptChange).toBe(true);
    });

    it('should roundtrip Random', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Random,
        params: { result: 42 },
      });
      expect(result.params.result).toBe(42);
    });

    it('should roundtrip SetPhase (byte + byteArray + bool)', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.SetPhase,
        params: { phase: 2, players: [1, 2, 3], force: true },
      });
      expect(result.params.phase).toBe(2);
      expect(result.params.players).toEqual([1, 2, 3]);
      expect(result.params.force).toBe(true);
    });

    it('should roundtrip Shuffled (byte + int32 + intArray + shortArray)', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.Shuffled,
        params: { player: 1, group: 10, card: [100, 101, 102], pos: [0, 1, 2] },
      });
      expect(result.params.player).toBe(1);
      expect(result.params.group).toBe(10);
      expect(result.params.card).toEqual([100, 101, 102]);
      expect(result.params.pos).toEqual([0, 1, 2]);
    });

    it('should roundtrip GameState', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.GameState,
        params: { toPlayer: 1, state: '{"some":"json"}' },
      });
      expect(result.params.toPlayer).toBe(1);
      expect(result.params.state).toBe('{"some":"json"}');
    });

    it('should roundtrip SetCardProperty (int32 + byte + strings)', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.SetCardProperty,
        params: { id: 42, player: 1, name: 'Power', val: '5', valtype: 'int' },
      });
      expect(result.params.id).toBe(42);
      expect(result.params.name).toBe('Power');
      expect(result.params.val).toBe('5');
    });

    it('should roundtrip GrantPileViewPermission (complex)', () => {
      const result = roundtrip({
        isMuted: 0,
        type: MessageType.GrantPileViewPermission,
        params: {
          owner: 1,
          group: 5,
          requester: 2,
          granted: true,
          permanent: false,
          viewType: 'top',
          cardCount: 3,
        },
      });
      expect(result.params.owner).toBe(1);
      expect(result.params.granted).toBe(true);
      expect(result.params.permanent).toBe(false);
      expect(result.params.viewType).toBe('top');
    });
  });

  // ---------------------------------------------------------------------------
  // Wire format verification
  // ---------------------------------------------------------------------------

  describe('wire format', () => {
    it('should include 4-byte length prefix in serialized buffer', () => {
      const buf = serializeMessage({
        isMuted: 0,
        type: MessageType.Ping,
        params: {},
      });

      // Length prefix should equal total buffer length
      const lengthPrefix = buf.readInt32LE(0);
      expect(lengthPrefix).toBe(buf.length);
    });

    it('should have isMuted at offset 4 and type at offset 8', () => {
      const buf = serializeMessage({
        isMuted: 99,
        type: MessageType.Error,
        params: { msg: 'x' },
      });

      // offset 0-3: length prefix
      // offset 4-7: isMuted
      expect(buf.readInt32LE(4)).toBe(99);
      // offset 8: message type byte
      expect(buf[8]).toBe(MessageType.Error);
    });

    it('should have minimum size of 9 bytes (4 length + 4 isMuted + 1 type)', () => {
      const buf = serializeMessage({
        isMuted: 0,
        type: MessageType.Ping,
        params: {},
      });
      expect(buf.length).toBe(9);
    });
  });

  // ---------------------------------------------------------------------------
  // Registry completeness - all 111 message types
  // ---------------------------------------------------------------------------

  describe('registry completeness', () => {
    // Collect all numeric values from the MessageType enum
    const allMessageTypes = Object.values(MessageType)
      .filter((v): v is number => typeof v === 'number');

    it('should have exactly 111 message types in the enum', () => {
      expect(allMessageTypes.length).toBe(111);
    });

    it('should have a serializer registered for every message type', () => {
      for (const typeId of allMessageTypes) {
        // If no serializer exists, serializeMessage will throw.
        // We test that it does NOT throw for a minimally valid message.
        // For parameterless messages, params={} is fine.
        // For messages with params, we can't easily construct valid params
        // without knowing the schema, but we can at least verify the serializer
        // is registered by checking that it doesn't throw "No serializer registered".
        expect(() => {
          try {
            serializeMessage({ isMuted: 0, type: typeId, params: {} });
          } catch (e) {
            // Re-throw only if it's a "No serializer registered" error
            if (e instanceof Error && e.message.includes('No serializer registered')) {
              throw e;
            }
            // Other errors (e.g., from writing undefined as string) are expected
            // and mean the serializer IS registered but we gave it bad params.
          }
        }).not.toThrow();
      }
    });

    it('should have a deserializer registered for every message type', () => {
      for (const typeId of allMessageTypes) {
        // Build a minimal buffer: isMuted (4) + type (1) + lots of zeros for params
        const buf = Buffer.alloc(1024);
        buf.writeInt32LE(0, 0); // isMuted
        buf.writeUInt8(typeId, 4); // type

        expect(() => {
          try {
            deserializeMessage(buf);
          } catch (e) {
            // Re-throw only if it's a "No deserializer registered" error
            if (e instanceof Error && e.message.includes('No deserializer registered')) {
              throw e;
            }
            // Other errors (e.g., reading past buffer) are expected
          }
        }).not.toThrow();
      }
    });
  });

  // ---------------------------------------------------------------------------
  // Error handling
  // ---------------------------------------------------------------------------

  describe('error handling', () => {
    it('should throw when serializing an unregistered message type', () => {
      expect(() =>
        serializeMessage({
          isMuted: 0,
          type: 255 as MessageType,
          params: {},
        }),
      ).toThrow('No serializer registered');
    });

    it('should throw when deserializing an unregistered message type', () => {
      const buf = Buffer.alloc(5);
      buf.writeInt32LE(0, 0); // isMuted
      buf.writeUInt8(255, 4); // invalid type
      expect(() => deserializeMessage(buf)).toThrow('No deserializer registered');
    });
  });

  // ---------------------------------------------------------------------------
  // getMessageTypeName
  // ---------------------------------------------------------------------------

  describe('getMessageTypeName', () => {
    it('should return the name for known types', () => {
      expect(getMessageTypeName(MessageType.Error)).toBe('Error');
      expect(getMessageTypeName(MessageType.Hello)).toBe('Hello');
      expect(getMessageTypeName(MessageType.Ping)).toBe('Ping');
      expect(getMessageTypeName(MessageType.GroupProtection)).toBe('GroupProtection');
    });

    it('should return "Unknown" for unregistered types', () => {
      expect(getMessageTypeName(999 as MessageType)).toBe('Unknown');
    });
  });
});
