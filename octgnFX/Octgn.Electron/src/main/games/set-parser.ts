import { XMLParser } from 'fast-xml-parser';

export interface CardDefinition {
  id: string;
  name: string;
  setId: string;
  properties: Record<string, string>;
}

const parser = new XMLParser({
  ignoreAttributes: false,
  attributeNamePrefix: '@_',
  isArray: (tagName) => ['card', 'property', 's'].includes(tagName.toLowerCase()),
});

function attr(obj: Record<string, unknown>, name: string, fallback = ''): string {
  return (obj[`@_${name}`] as string | undefined) ?? fallback;
}

/**
 * Extract property value — handles both simple `value` attribute and
 * rich-text `<s value="...">` children by concatenating their values.
 */
function extractPropertyValue(prop: Record<string, unknown>): string {
  // Simple: <property name="Type" value="Creature" />
  const simple = attr(prop, 'value');
  if (simple) return simple;

  // Rich text: <property name="Cost"><s value="1">{1}</s><s value="G">{G}</s></property>
  const sElements = prop['s'];
  if (sElements && Array.isArray(sElements)) {
    return sElements.map((s) => attr(s as Record<string, unknown>, 'value')).join('');
  }

  return '';
}

/**
 * Parse a set.xml string and return all card definitions.
 */
export function parseSetXml(xml: string): CardDefinition[] {
  try {
    const doc = parser.parse(xml) as Record<string, unknown>;
    const set = doc['set'] as Record<string, unknown> | undefined;
    if (!set) return [];

    const setId = attr(set, 'id');
    const cardsContainer = set['cards'] as Record<string, unknown> | undefined;
    if (!cardsContainer) return [];

    const rawCards = cardsContainer['card'];
    if (!rawCards) return [];

    const cards = Array.isArray(rawCards) ? rawCards : [rawCards];
    const results: CardDefinition[] = [];

    for (const c of cards) {
      const card = c as Record<string, unknown>;
      const id = attr(card, 'id');
      const name = attr(card, 'name');
      if (!id) continue;

      const properties: Record<string, string> = {};
      const rawProps = card['property'];
      if (rawProps) {
        const props = Array.isArray(rawProps) ? rawProps : [rawProps];
        for (const p of props) {
          const prop = p as Record<string, unknown>;
          const propName = attr(prop, 'name');
          if (propName) {
            properties[propName] = extractPropertyValue(prop);
          }
        }
      }

      results.push({ id, name, setId, properties });
    }

    return results;
  } catch {
    return [];
  }
}
