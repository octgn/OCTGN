import { describe, it, expect } from 'vitest';
import { parseSetXml } from '@main/games/set-parser';

describe('parseSetXml', () => {
  it('parses a simple set with value-attribute properties', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<set name="Core Set" id="set-001" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-aaa" name="Ezuri, Renegade Leader">
      <property name="Type" value="Creature" />
      <property name="Cost" value="1GG" />
      <property name="Power" value="2" />
    </card>
    <card id="card-bbb" name="Forest">
      <property name="Type" value="Land" />
    </card>
  </cards>
</set>`;

    const cards = parseSetXml(xml);
    expect(cards).toHaveLength(2);

    expect(cards[0]).toEqual({
      id: 'card-aaa',
      name: 'Ezuri, Renegade Leader',
      setId: 'set-001',
      properties: { Type: 'Creature', Cost: '1GG', Power: '2' },
    });

    expect(cards[1]).toEqual({
      id: 'card-bbb',
      name: 'Forest',
      setId: 'set-001',
      properties: { Type: 'Land' },
    });
  });

  it('parses rich text properties with <s> elements', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<set name="Test Set" id="set-002" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-ccc" name="Lightning Bolt">
      <property name="Type" value="Instant" />
      <property name="Cost"><s value="R">{R}</s></property>
    </card>
    <card id="card-ddd" name="Grizzly Bears">
      <property name="Type" value="Creature" />
      <property name="Cost"><s value="1">{1}</s><s value="G">{G}</s></property>
    </card>
  </cards>
</set>`;

    const cards = parseSetXml(xml);
    expect(cards).toHaveLength(2);

    expect(cards[0].properties.Cost).toBe('R');
    expect(cards[1].properties.Cost).toBe('1G');
  });

  it('handles missing cards gracefully', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<set name="Empty Set" id="set-003" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards />
</set>`;

    const cards = parseSetXml(xml);
    expect(cards).toEqual([]);
  });

  it('handles malformed XML gracefully', () => {
    const cards = parseSetXml('not xml at all <<<');
    expect(cards).toEqual([]);
  });

  it('handles missing set element', () => {
    const xml = `<?xml version="1.0"?><other />`;
    const cards = parseSetXml(xml);
    expect(cards).toEqual([]);
  });

  it('skips cards without an id', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<set name="Broken Set" id="set-004" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card name="No ID Card">
      <property name="Type" value="Thing" />
    </card>
    <card id="card-ok" name="Has ID">
      <property name="Type" value="Good" />
    </card>
  </cards>
</set>`;

    const cards = parseSetXml(xml);
    expect(cards).toHaveLength(1);
    expect(cards[0].id).toBe('card-ok');
  });

  it('handles cards with no properties', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<set name="Simple Set" id="set-005" gameId="game-001" gameVersion="1.0" version="0.1">
  <cards>
    <card id="card-bare" name="Bare Card" />
  </cards>
</set>`;

    const cards = parseSetXml(xml);
    expect(cards).toHaveLength(1);
    expect(cards[0].properties).toEqual({});
  });
});
