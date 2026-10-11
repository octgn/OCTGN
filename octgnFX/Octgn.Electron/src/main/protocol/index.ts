/**
 * OCTGN Game Binary Protocol
 *
 * This module provides a complete TypeScript implementation of the OCTGN
 * game server binary protocol, including:
 *
 * - {@link MessageType} enum with all 111 message type identifiers
 * - {@link BinaryWriter} for serializing primitives and composites
 * - {@link BinaryReader} for deserializing primitives and composites
 * - {@link serializeMessage} / {@link deserializeMessage} for full message
 *   round-trip serialization
 * - {@link GameConnection} for TCP connection management with framing,
 *   keepalive, and automatic reconnection
 *
 * @example
 * ```ts
 * import { GameConnection, MessageType } from './protocol';
 *
 * const conn = new GameConnection({ host: '127.0.0.1', port: 36963 });
 * conn.on('Chat', (msg) => {
 *   console.log(`Player ${msg.params.player}: ${msg.params.text}`);
 * });
 * await conn.connect();
 * conn.sendMessage(MessageType.ChatReq, 0, { text: 'Hello!' });
 * ```
 */

export { MessageType } from './types';
export type { ProtocolMessage } from './types';

export { BinaryWriter } from './binary-writer';
export { BinaryReader } from './binary-reader';

export {
  serializeMessage,
  deserializeMessage,
  getMessageTypeName,
} from './serializer';

export { GameConnection } from './connection';
export type { GameConnectionOptions, GameConnectionEvents } from './connection';
