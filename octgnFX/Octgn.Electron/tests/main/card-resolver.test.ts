import { describe, it, expect, vi } from 'vitest';
import { CardResolver, type CardResolverIO } from '@main/games/card-resolver';

// Mock logger only
vi.mock('@main/logger', () => ({
  log: vi.fn(),
  logError: vi.fn(),
}));

const DEFINITION_XML = `<?xml version="1.0" encoding="utf-8"?>
<game name="Test Card Game" id="game-001" version="1.0.0.0" description="A test game">
  <card width="63" height="88" back="back.png" />
  <player summary="Hand:{#Hand}">
    <group name="Hand" visibility="me" ordered="False" />
    <group name="Library" visibility="nobody" ordered="True" />
    <group name="Graveyard" visibility="all" ordered="False" />
    <group name="Exile" visibility="all" ordered="False" />
  </player>
</game>`;

const SET1_XML = `<?xml version="1.0" encoding="utf-8"?>
<set name="Core Set" id="set-001" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-aaa" name="Ezuri, Renegade Leader">
      <property name="Type" value="Creature" />
      <property name="Cost" value="1GG" />
    </card>
    <card id="card-bbb" name="Forest">
      <property name="Type" value="Land" />
    </card>
  </cards>
</set>`;

const SET2_XML = `<?xml version="1.0" encoding="utf-8"?>
<set name="Expansion" id="set-002" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-ccc" name="Lightning Bolt">
      <property name="Type" value="Instant" />
      <property name="Cost" value="R" />
    </card>
  </cards>
</set>`;

function createMockIO(overrides?: Partial<CardResolverIO>): CardResolverIO {
  return {
    findGameDir: vi.fn().mockResolvedValue('C:/games/game-001'),
    readdir: vi.fn().mockResolvedValue(['CoreSet', 'Expansion']),
    readFile: vi.fn().mockImplementation(async (path: string) => {
      if (path.includes('definition.xml')) return DEFINITION_XML;
      if (path.includes('CoreSet')) return SET1_XML;
      if (path.includes('Expansion')) return SET2_XML;
      throw new Error('File not found');
    }),
    ...overrides,
  };
}

describe('CardResolver', () => {
  describe('loadGame', () => {
    it('loads card definitions from multiple sets', async () => {
      const io = createMockIO();
      const resolver = new CardResolver(io);
      await resolver.loadGame('game-001');

      expect(resolver.resolve('card-aaa')).toEqual({
        id: 'card-aaa',
        name: 'Ezuri, Renegade Leader',
        setId: 'set-001',
        properties: { Type: 'Creature', Cost: '1GG' },
      });

      expect(resolver.resolve('card-ccc')).toEqual({
        id: 'card-ccc',
        name: 'Lightning Bolt',
        setId: 'set-002',
        properties: { Type: 'Instant', Cost: 'R' },
      });
    });

    it('does not reload an already-loaded game', async () => {
      const io = createMockIO();
      const resolver = new CardResolver(io);
      await resolver.loadGame('game-001');
      await resolver.loadGame('game-001');

      expect(io.findGameDir).toHaveBeenCalledTimes(1);
    });

    it('handles game not found locally', async () => {
      const io = createMockIO({
        findGameDir: vi.fn().mockResolvedValue(null),
      });
      const resolver = new CardResolver(io);
      await resolver.loadGame('missing-game');

      expect(resolver.resolve('anything')).toBeUndefined();
    });

    it('keeps first definition when duplicate card IDs exist across sets', async () => {
      const SET2_DUPE_XML = `<?xml version="1.0" encoding="utf-8"?>
<set name="Alternate Art" id="set-alt" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-aaa" name="Ezuri (Alt Art)">
      <property name="Type" value="Creature" />
    </card>
  </cards>
</set>`;

      const io = createMockIO({
        readdir: vi.fn().mockResolvedValue(['CoreSet', 'AltArt']),
        readFile: vi.fn().mockImplementation(async (path: string) => {
          if (path.includes('definition.xml')) return DEFINITION_XML;
          if (path.includes('CoreSet')) return SET1_XML;
          if (path.includes('AltArt')) return SET2_DUPE_XML;
          throw new Error('File not found');
        }),
      });
      const resolver = new CardResolver(io);
      await resolver.loadGame('game-001');

      // First set's definition should win (not overwritten by second set)
      const card = resolver.resolve('card-aaa');
      expect(card?.name).toBe('Ezuri, Renegade Leader');
      expect(card?.setId).toBe('set-001');
    });

    it('handles missing Sets directory', async () => {
      const io = createMockIO({
        readdir: vi.fn().mockRejectedValue(new Error('ENOENT')),
      });
      const resolver = new CardResolver(io);
      await resolver.loadGame('game-001');

      // Should still load the game definition
      expect(resolver.getGameDefinition('game-001')).toBeDefined();
      expect(resolver.resolve('anything')).toBeUndefined();
    });
  });

  describe('resolve', () => {
    it('returns undefined for unknown card GUID', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      expect(resolver.resolve('unknown-guid')).toBeUndefined();
    });

    it('resolves known card GUID to correct definition', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      const card = resolver.resolve('card-bbb');
      expect(card?.name).toBe('Forest');
      expect(card?.properties.Type).toBe('Land');
    });
  });

  describe('getGameDefinition', () => {
    it('returns loaded game definition', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      const def = resolver.getGameDefinition('game-001');
      expect(def).toBeDefined();
      expect(def!.name).toBe('Test Card Game');
      expect(def!.players[0].groups).toHaveLength(4);
    });

    it('returns undefined for unloaded game', () => {
      const resolver = new CardResolver(createMockIO());
      expect(resolver.getGameDefinition('not-loaded')).toBeUndefined();
    });
  });

  describe('resolveGroupName', () => {
    it('resolves group index to definition name', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      // Group index 1 (low byte) = first group = Hand
      expect(resolver.resolveGroupName('game-001', 1)).toBe('Hand');
      // Group index 2 = Library
      expect(resolver.resolveGroupName('game-001', 2)).toBe('Library');
      // Group index 3 = Graveyard
      expect(resolver.resolveGroupName('game-001', 3)).toBe('Graveyard');
      // Group index 4 = Exile
      expect(resolver.resolveGroupName('game-001', 4)).toBe('Exile');
    });

    it('handles high bytes in group ID (player encoding)', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      // 0x0101 = player 1, group index 1 = Hand
      expect(resolver.resolveGroupName('game-001', 0x0101)).toBe('Hand');
      // 0x0203 = player 2, group index 3 = Graveyard
      expect(resolver.resolveGroupName('game-001', 0x0203)).toBe('Graveyard');
    });

    it('returns undefined for out-of-range group index', async () => {
      const resolver = new CardResolver(createMockIO());
      await resolver.loadGame('game-001');

      // Index 5 is beyond the 4 groups defined
      expect(resolver.resolveGroupName('game-001', 5)).toBeUndefined();
      // Index 0 is invalid (groups are 1-based)
      expect(resolver.resolveGroupName('game-001', 0)).toBeUndefined();
    });

    it('returns undefined for unknown game', () => {
      const resolver = new CardResolver(createMockIO());
      expect(resolver.resolveGroupName('unknown', 1)).toBeUndefined();
    });
  });
});
