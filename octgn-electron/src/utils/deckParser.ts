/**
 * OCTGN Deck File Format Parser
 * 
 * .o8d files are XML-based deck files used by OCTGN
 * 
 * Format:
 * <deck game="game-id">
 *   <section name="Main">
 *     <card qty="4" id="card-id">Card Name</card>
 *   </section>
 * </deck>
 */

export interface DeckCard {
  id: string;
  name: string;
  quantity: number;
  section: string;
  properties?: Record<string, string>;
}

export interface Deck {
  gameId: string;
  name: string;
  sections: Record<string, DeckCard[]>;
  notes?: string;
  sleeveId?: string;
  createdAt?: Date;
  modifiedAt?: Date;
}

export function parseDeck(xml: string): Deck {
  const parser = new DOMParser();
  const doc = parser.parseFromString(xml, 'text/xml');

  const deckElement = doc.querySelector('deck');
  if (!deckElement) {
    throw new Error('Invalid deck file: missing deck element');
  }

  const gameId = deckElement.getAttribute('game') || '';
  const name = deckElement.getAttribute('name') || 'Untitled Deck';
  const notes = deckElement.querySelector('notes')?.textContent || '';
  const sleeveId = deckElement.getAttribute('sleeve') || undefined;

  const sections: Record<string, DeckCard[]> = {};

  deckElement.querySelectorAll('section').forEach((sectionElement) => {
    const sectionName = sectionElement.getAttribute('name') || 'Main';
    const cards: DeckCard[] = [];

    sectionElement.querySelectorAll('card').forEach((cardElement) => {
      const card: DeckCard = {
        id: cardElement.getAttribute('id') || '',
        name: cardElement.textContent?.trim() || '',
        quantity: parseInt(cardElement.getAttribute('qty') || '1', 10),
        section: sectionName,
      };

      // Parse any additional properties
      cardElement.getAttributeNames().forEach((attr) => {
        if (!['qty', 'id'].includes(attr)) {
          card.properties = card.properties || {};
          card.properties[attr] = cardElement.getAttribute(attr) || '';
        }
      });

      cards.push(card);
    });

    sections[sectionName] = cards;
  });

  return {
    gameId,
    name,
    sections,
    notes,
    sleeveId,
  };
}

export function serializeDeck(deck: Deck): string {
  let xml = '<?xml version="1.0" encoding="utf-8"?>\n';
  xml += `<deck game="${deck.gameId}"`;

  if (deck.name) {
    xml += ` name="${escapeXml(deck.name)}"`;
  }
  if (deck.sleeveId) {
    xml += ` sleeve="${deck.sleeveId}"`;
  }
  xml += '>\n';

  // Add sections
  for (const [sectionName, cards] of Object.entries(deck.sections)) {
    xml += `  <section name="${escapeXml(sectionName)}">\n`;
    for (const card of cards) {
      xml += `    <card qty="${card.quantity}" id="${escapeXml(card.id)}">${escapeXml(card.name)}</card>\n`;
    }
    xml += '  </section>\n';
  }

  // Add notes
  if (deck.notes) {
    xml += `  <notes>${escapeXml(deck.notes)}</notes>\n`;
  }

  xml += '</deck>';

  return xml;
}

function escapeXml(str: string): string {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

/**
 * Parse a deck from a file path (Electron only)
 */
export async function loadDeckFromFile(path: string): Promise<Deck> {
  if (!window.electronAPI?.readFile) {
    throw new Error('File loading requires Electron');
  }

  const result = await window.electronAPI.readFile(path);
  if (!result.success) {
    throw new Error(result.error);
  }

  return parseDeck(result.data);
}

/**
 * Save a deck to a file (Electron only)
 */
export async function saveDeckToFile(deck: Deck, path: string): Promise<void> {
  if (!window.electronAPI?.writeFile) {
    throw new Error('File saving requires Electron');
  }

  const xml = serializeDeck(deck);
  const result = await window.electronAPI.writeFile(path, xml);

  if (!result.success) {
    throw new Error(result.error);
  }
}

/**
 * Export deck to plain text format
 */
export function exportToText(deck: Deck): string {
  let text = `# ${deck.name}\n`;
  text += `# Game: ${deck.gameId}\n\n`;

  for (const [sectionName, cards] of Object.entries(deck.sections)) {
    const total = cards.reduce((sum, c) => sum + c.quantity, 0);
    text += `## ${sectionName} (${total} cards)\n`;

    for (const card of cards) {
      text += `${card.quantity}x ${card.name}\n`;
    }

    text += '\n';
  }

  if (deck.notes) {
    text += `## Notes\n${deck.notes}\n`;
  }

  return text;
}

/**
 * Export deck to Magic Workstation format
 */
export function exportToMWS(deck: Deck): string {
  let text = '// Main Deck\n';

  const main = deck.sections['Main'] || [];
  const side = deck.sections['Sideboard'] || [];

  for (const card of main) {
    for (let i = 0; i < card.quantity; i++) {
      text += `1 ${card.name}\n`;
    }
  }

  if (side.length > 0) {
    text += '\n// Sideboard\n';
    for (const card of side) {
      for (let i = 0; i < card.quantity; i++) {
        text += `SB: 1 ${card.name}\n`;
      }
    }
  }

  return text;
}
