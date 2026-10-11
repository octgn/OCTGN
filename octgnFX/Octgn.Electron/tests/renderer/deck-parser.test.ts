import { describe, it, expect } from 'vitest';
import { parseO8dXml } from '@renderer/components/DeckLoader';

describe('parseO8dXml', () => {
  it('parses shared="True" attribute on sections', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<deck game="abc-123">
  <section name="Main Deck" shared="False">
    <card qty="2" id="card-1">Card One</card>
  </section>
  <section name="Shared Pile" shared="True">
    <card qty="1" id="card-2">Token</card>
  </section>
</deck>`;

    const deck = parseO8dXml(xml);

    expect(deck.sections).toHaveLength(2);
    expect(deck.sections[0].name).toBe('Main Deck');
    expect(deck.sections[0].shared).toBe(false);
    expect(deck.sections[1].name).toBe('Shared Pile');
    expect(deck.sections[1].shared).toBe(true);
  });

  it('defaults shared to false when attribute is missing', () => {
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<deck game="abc-123">
  <section name="Hand">
    <card qty="1" id="card-1">Card</card>
  </section>
</deck>`;

    const deck = parseO8dXml(xml);

    expect(deck.sections[0].shared).toBe(false);
  });

  it('supports legacy deck-level shared attribute', () => {
    // Old-style decks have shared="True" on the <deck> element,
    // which makes ALL sections shared
    const xml = `<?xml version="1.0" encoding="utf-8"?>
<deck game="abc-123" shared="True">
  <section name="Resources">
    <card qty="1" id="card-1">Resource</card>
  </section>
  <section name="Tokens">
    <card qty="1" id="card-2">Token</card>
  </section>
</deck>`;

    const deck = parseO8dXml(xml);

    expect(deck.sections[0].shared).toBe(true);
    expect(deck.sections[1].shared).toBe(true);
  });
});
