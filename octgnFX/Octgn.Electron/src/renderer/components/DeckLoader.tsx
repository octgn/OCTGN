import React, { useState, useCallback } from 'react';
import Button from './Button';
import { useGameStore } from '../stores/game-store';
import { useToastStore } from '../stores/toast-store';
import type { Deck, DeckSection, DeckCard } from '../../shared/types';

interface DeckLoaderProps {
  onDeckLoaded?: (deck: Deck) => void;
  className?: string;
}

export function parseO8dXml(xmlContent: string): Deck {
  const parser = new DOMParser();
  const doc = parser.parseFromString(xmlContent, 'application/xml');

  const parseError = doc.querySelector('parsererror');
  if (parseError) {
    throw new Error('Invalid deck file: XML parsing failed');
  }

  const deckElement = doc.querySelector('deck');
  if (!deckElement) {
    throw new Error('Invalid deck file: missing <deck> element');
  }

  const gameId = deckElement.getAttribute('game') || '';

  const sectionElements = deckElement.querySelectorAll('section');
  const sections: DeckSection[] = [];

  sectionElements.forEach((sectionEl) => {
    const sectionName = sectionEl.getAttribute('name') || 'Unknown';
    const cards: DeckCard[] = [];

    const cardElements = sectionEl.querySelectorAll('card');
    cardElements.forEach((cardEl) => {
      const id = cardEl.getAttribute('id') || '';
      const quantity = parseInt(cardEl.getAttribute('qty') || '1', 10);
      const name = cardEl.textContent?.trim() || 'Unknown Card';

      cards.push({
        id,
        name,
        quantity,
        properties: {},
      });
    });

    sections.push({ name: sectionName, cards });
  });

  const notesElement = deckElement.querySelector('notes');
  const notes = notesElement?.textContent?.trim() || undefined;

  const sleeveElement = deckElement.querySelector('sleeve');
  const sleeveUrl = sleeveElement?.textContent?.trim() || undefined;

  return {
    gameId,
    sections,
    notes,
    sleeveUrl,
  };
}

const DeckLoader: React.FC<DeckLoaderProps> = ({ onDeckLoaded, className }) => {
  const [isLoading, setIsLoading] = useState(false);
  const loadDeck = useGameStore((s) => s.loadDeck);
  const addToast = useToastStore((s) => s.addToast);

  const handleLoadDeck = useCallback(async () => {
    setIsLoading(true);

    try {
      const result = await window.octgn.openFileDialog([
        { name: 'OCTGN Deck Files', extensions: ['o8d'] },
        { name: 'All Files', extensions: ['*'] },
      ]);

      if (!result) {
        setIsLoading(false);
        return;
      }

      const deck = parseO8dXml(result.content);

      const totalCards = deck.sections.reduce(
        (sum, section) => sum + section.cards.reduce((a, c) => a + c.quantity, 0),
        0
      );

      if (totalCards === 0) {
        addToast('Deck file is empty - no cards found', 'warning');
        setIsLoading(false);
        return;
      }

      await loadDeck(deck);

      const fileName = result.filePath.split(/[/\\]/).pop() || 'deck';
      addToast(
        `Loaded "${fileName}" - ${totalCards} card${totalCards !== 1 ? 's' : ''} in ${deck.sections.length} section${deck.sections.length !== 1 ? 's' : ''}`,
        'success'
      );

      onDeckLoaded?.(deck);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load deck file';
      addToast(message, 'error');
    } finally {
      setIsLoading(false);
    }
  }, [loadDeck, onDeckLoaded, addToast]);

  return (
    <Button
      variant="ghost"
      size="sm"
      loading={isLoading}
      onClick={handleLoadDeck}
      className={className}
      icon={
        <svg
          className="w-3.5 h-3.5"
          viewBox="0 0 16 16"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M2 13V3a1 1 0 011-1h4l2 2h4a1 1 0 011 1v8a1 1 0 01-1 1H3a1 1 0 01-1-1z" />
          <path d="M8 7v4M6 9h4" />
        </svg>
      }
    >
      Load Deck
    </Button>
  );
};

export default DeckLoader;
