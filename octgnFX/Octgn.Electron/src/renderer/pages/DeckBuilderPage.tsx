import React, { useState, useCallback } from 'react';
import { clsx } from 'clsx';
import Button from '../components/Button';
import DeckLoader from '../components/DeckLoader';
import { useAppStore } from '../stores/app-store';
import type { Deck, DeckCard, DeckSection } from '../../shared/types';

interface SearchResult {
  id: string;
  name: string;
  imageUrl: string;
  properties: Record<string, string>;
}

const DeckBuilderPage: React.FC = () => {
  const navigate = useAppStore((s) => s.navigate);

  // Local deck state
  const [deckName, setDeckName] = useState('Untitled Deck');
  const [sections, setSections] = useState<DeckSection[]>([
    { name: 'Main', cards: [] },
    { name: 'Sideboard', cards: [] },
  ]);
  const [activeSection, setActiveSection] = useState(0);

  // Search state
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);

  // Filters
  const [filterType, setFilterType] = useState('');

  const handleSearch = useCallback(() => {
    if (!searchQuery.trim()) return;
    setIsSearching(true);
    // Simulated search - actual implementation will use IPC
    setTimeout(() => {
      setSearchResults([]);
      setIsSearching(false);
    }, 500);
  }, [searchQuery]);

  const addCardToDeck = useCallback(
    (result: SearchResult) => {
      setSections((prev) => {
        const updated = [...prev];
        const section = { ...updated[activeSection] };
        const existing = section.cards.find((c) => c.id === result.id);
        if (existing) {
          section.cards = section.cards.map((c) =>
            c.id === result.id ? { ...c, quantity: c.quantity + 1 } : c
          );
        } else {
          section.cards = [
            ...section.cards,
            {
              id: result.id,
              name: result.name,
              quantity: 1,
              properties: result.properties,
            },
          ];
        }
        updated[activeSection] = section;
        return updated;
      });
    },
    [activeSection]
  );

  const removeCardFromDeck = useCallback(
    (sectionIndex: number, cardId: string) => {
      setSections((prev) => {
        const updated = [...prev];
        const section = { ...updated[sectionIndex] };
        const card = section.cards.find((c) => c.id === cardId);
        if (card && card.quantity > 1) {
          section.cards = section.cards.map((c) =>
            c.id === cardId ? { ...c, quantity: c.quantity - 1 } : c
          );
        } else {
          section.cards = section.cards.filter((c) => c.id !== cardId);
        }
        updated[sectionIndex] = section;
        return updated;
      });
    },
    []
  );

  const totalCards = sections.reduce(
    (sum, s) => sum + s.cards.reduce((a, c) => a + c.quantity, 0),
    0
  );

  const handleDeckLoaded = useCallback((deck: Deck) => {
    if (deck.sections.length > 0) {
      setSections(deck.sections);
      setActiveSection(0);
      setDeckName(deck.notes || 'Loaded Deck');
    }
  }, []);

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="flex items-center gap-3 px-5 py-3 border-b border-octgn-border/30 bg-octgn-surface/40 backdrop-blur-sm">
        <Button variant="ghost" size="sm" onClick={() => navigate('lobby')}>
          <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M10 2L4 8l6 6" />
          </svg>
          Back
        </Button>
        <div className="w-px h-5 bg-octgn-border/40" />
        <h2 className="font-display text-base font-semibold tracking-wide text-octgn-text">
          Deck Builder
        </h2>
        <div className="flex-1" />
        <DeckLoader onDeckLoaded={handleDeckLoaded} />
        <Button variant="primary" size="sm">
          Save Deck
        </Button>
        <Button variant="accent" size="sm">
          Export
        </Button>
      </div>

      {/* Main content - 3 panels */}
      <div className="flex-1 flex min-h-0">
        {/* Left: Search + Filters */}
        <div className="w-64 shrink-0 flex flex-col border-r border-octgn-border/30 bg-octgn-surface/30">
          <div className="p-3 space-y-3">
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Search cards..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="flex-1 h-8 px-3 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-colors"
              />
              <Button variant="primary" size="sm" onClick={handleSearch} loading={isSearching}>
                <svg className="w-3.5 h-3.5" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                  <circle cx="6.5" cy="6.5" r="4.5" />
                  <path d="M10 10l4 4" />
                </svg>
              </Button>
            </div>

            {/* Filters */}
            <div className="space-y-2">
              <label className="text-[10px] text-octgn-text-dim uppercase tracking-wider font-semibold">
                Type Filter
              </label>
              <select
                value={filterType}
                onChange={(e) => setFilterType(e.target.value)}
                className="w-full h-8 px-2 text-xs rounded-lg bg-octgn-surface border border-octgn-border/50 text-octgn-text outline-none focus:border-octgn-primary/50 transition-colors"
              >
                <option value="">All Types</option>
                <option value="creature">Creature</option>
                <option value="spell">Spell</option>
                <option value="artifact">Artifact</option>
                <option value="enchantment">Enchantment</option>
              </select>
            </div>
          </div>

          {/* Search results info */}
          <div className="px-3 py-1.5 border-t border-octgn-border/20">
            <span className="text-[10px] text-octgn-text-dim uppercase tracking-wider">
              {searchResults.length} results
            </span>
          </div>
        </div>

        {/* Center: Search results grid */}
        <div className="flex-1 overflow-y-auto p-4 min-w-0">
          {searchResults.length === 0 && (
            <div className="flex flex-col items-center justify-center h-full text-octgn-text-dim/60">
              <svg className="w-16 h-16 mb-4" viewBox="0 0 64 64" fill="none" stroke="currentColor" strokeWidth="1.5">
                <rect x="8" y="4" width="28" height="38" rx="3" />
                <rect x="28" y="22" width="28" height="38" rx="3" />
                <path d="M12 14h20M12 20h14" strokeOpacity="0.3" />
              </svg>
              <p className="text-sm font-medium">Search for cards to build your deck</p>
              <p className="text-xs mt-1">Use the search panel on the left</p>
            </div>
          )}

          <div className="grid grid-cols-[repeat(auto-fill,minmax(120px,1fr))] gap-3">
            {searchResults.map((result) => (
              <button
                key={result.id}
                onClick={() => addCardToDeck(result)}
                className="group relative rounded-lg overflow-hidden bg-octgn-surface border border-octgn-border/30 card-glow aspect-[5/7]"
              >
                {result.imageUrl ? (
                  <img src={result.imageUrl} alt={result.name} className="w-full h-full object-cover" />
                ) : (
                  <div className="flex items-center justify-center h-full p-2">
                    <span className="text-xs text-octgn-text text-center">{result.name}</span>
                  </div>
                )}
                {/* Hover overlay */}
                <div className="absolute inset-0 bg-octgn-primary/0 group-hover:bg-octgn-primary/10 transition-colors flex items-center justify-center">
                  <span className="opacity-0 group-hover:opacity-100 transition-opacity text-white text-2xl font-bold drop-shadow-lg">
                    +
                  </span>
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Right: Current deck */}
        <div className="w-72 shrink-0 flex flex-col border-l border-octgn-border/30 bg-octgn-surface/30">
          {/* Deck name */}
          <div className="p-3 border-b border-octgn-border/20">
            <input
              type="text"
              value={deckName}
              onChange={(e) => setDeckName(e.target.value)}
              className="w-full text-sm font-semibold text-octgn-text bg-transparent outline-none border-b border-transparent focus:border-octgn-primary/50 transition-colors pb-0.5"
            />
            <p className="text-[10px] text-octgn-text-dim mt-1">
              {totalCards} card{totalCards !== 1 ? 's' : ''} total
            </p>
          </div>

          {/* Section tabs */}
          <div className="flex border-b border-octgn-border/20">
            {sections.map((section, idx) => (
              <button
                key={section.name}
                onClick={() => setActiveSection(idx)}
                className={clsx(
                  'flex-1 px-3 py-2 text-xs font-medium transition-colors',
                  idx === activeSection
                    ? 'text-octgn-primary border-b-2 border-octgn-primary'
                    : 'text-octgn-text-dim hover:text-octgn-text'
                )}
              >
                {section.name}
                <span className="ml-1 text-octgn-text-dim">
                  ({section.cards.reduce((a, c) => a + c.quantity, 0)})
                </span>
              </button>
            ))}
          </div>

          {/* Cards in section */}
          <div className="flex-1 overflow-y-auto">
            {sections[activeSection].cards.length === 0 && (
              <div className="flex items-center justify-center h-32 text-xs text-octgn-text-dim">
                No cards in this section
              </div>
            )}
            {sections[activeSection].cards.map((card: DeckCard) => (
              <div
                key={card.id}
                className="flex items-center gap-2 px-3 py-1.5 hover:bg-white/3 group transition-colors"
              >
                <span className="font-mono text-xs text-octgn-text-muted w-5 text-right">
                  {card.quantity}x
                </span>
                <span className="flex-1 text-xs text-octgn-text truncate">{card.name}</span>
                <div className="flex gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button
                    onClick={() => removeCardFromDeck(activeSection, card.id)}
                    className="w-5 h-5 flex items-center justify-center rounded text-octgn-text-dim hover:text-octgn-danger hover:bg-octgn-danger/10 transition-colors text-xs"
                  >
                    -
                  </button>
                  <button
                    onClick={() =>
                      addCardToDeck({
                        id: card.id,
                        name: card.name,
                        imageUrl: '',
                        properties: card.properties,
                      })
                    }
                    className="w-5 h-5 flex items-center justify-center rounded text-octgn-text-dim hover:text-octgn-success hover:bg-octgn-success/10 transition-colors text-xs"
                  >
                    +
                  </button>
                </div>
              </div>
            ))}
          </div>

          {/* Deck stats */}
          <div className="p-3 border-t border-octgn-border/20 bg-octgn-surface/40">
            <div className="grid grid-cols-2 gap-2 text-[10px] text-octgn-text-dim">
              <div>
                <span className="block text-octgn-text font-mono text-sm">{totalCards}</span>
                Total Cards
              </div>
              <div>
                <span className="block text-octgn-text font-mono text-sm">
                  {sections.reduce((a, s) => a + s.cards.length, 0)}
                </span>
                Unique Cards
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default DeckBuilderPage;
