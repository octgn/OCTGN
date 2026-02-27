import { useState, useEffect, useMemo, useCallback } from 'react';
import { useGameStore } from '../stores/gameStore';
import { parseDeck, serializeDeck, exportToText, saveDeckToFile, loadDeckFromFile } from '../utils';
import { Deck, DeckCard } from '../utils/deckParser';
import { Button, Modal, CardZoom } from '../components';

interface CardDatabaseCard {
  id: string;
  name: string;
  set: string;
  properties: Record<string, string>;
  imageUrl?: string;
}

export default function DeckEditorPage() {
  const { connected } = useGameStore();

  // Deck state
  const [deckName, setDeckName] = useState('Untitled Deck');
  const [deckSections, setDeckSections] = useState<Record<string, DeckCard[]>>({
    Main: [],
    Sideboard: [],
    Commander: [],
  });
  const [activeSection, setActiveSection] = useState('Main');
  const [notes, setNotes] = useState('');

  // Card database (demo)
  const [cardDatabase, setCardDatabase] = useState<CardDatabaseCard[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState<CardDatabaseCard[]>([]);

  // UI state
  const [selectedCard, setSelectedCard] = useState<DeckCard | null>(null);
  const [showCardDatabase, setShowCardDatabase] = useState(false);
  const [zoomCard, setZoomCard] = useState<CardDatabaseCard | null>(null);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Load demo card database
  useEffect(() => {
    const demoCards: CardDatabaseCard[] = [
      { id: 'bolt', name: 'Lightning Bolt', set: 'M12', properties: { type: 'Instant', cost: 'R', text: 'Deal 3 damage to any target.' } },
      { id: 'giantgrowth', name: 'Giant Growth', set: 'M10', properties: { type: 'Instant', cost: 'G', text: 'Target creature gets +3/+3 until end of turn.' } },
      { id: 'mountain', name: 'Mountain', set: 'UNH', properties: { type: 'Basic Land', subtype: 'Mountain' } },
      { id: 'forest', name: 'Forest', set: 'UNH', properties: { type: 'Basic Land', subtype: 'Forest' } },
      { id: 'serra', name: 'Serra Angel', set: 'M10', properties: { type: 'Creature', cost: '3WW', pt: '4/4', text: 'Flying, vigilance' } },
      { id: 'goblin', name: 'Goblin Guide', set: 'ZEN', properties: { type: 'Creature', cost: 'R', pt: '2/2', text: 'Haste. Whenever Goblin Guide attacks, defending player reveals the top card of their library.' } },
      { id: 'monolith', name: 'Basalt Monolith', set: 'M12', properties: { type: 'Artifact', cost: '3', text: 'Tap: Add CCC. 3: Untap Basalt Monolith.' } },
      { id: 'solring', name: 'Sol Ring', set: 'C13', properties: { type: 'Artifact', cost: '1', text: 'Tap: Add CC.' } },
    ];
    setCardDatabase(demoCards);
  }, []);

  // Search cards
  const filteredCards = useMemo(() => {
    if (!searchTerm.trim()) return cardDatabase.slice(0, 20);
    const term = searchTerm.toLowerCase();
    return cardDatabase.filter(
      (card) =>
        card.name.toLowerCase().includes(term) ||
        card.set.toLowerCase().includes(term) ||
        Object.values(card.properties).some((v) => v.toLowerCase().includes(term))
    );
  }, [cardDatabase, searchTerm]);

  // Calculate totals
  const sectionTotals = useMemo(() => {
    const totals: Record<string, number> = {};
    for (const [section, cards] of Object.entries(deckSections)) {
      totals[section] = cards.reduce((sum, c) => sum + c.quantity, 0);
    }
    return totals;
  }, [deckSections]);

  const totalCards = useMemo(
    () => Object.values(sectionTotals).reduce((sum, t) => sum + t, 0),
    [sectionTotals]
  );

  // Add card to deck
  const addCard = useCallback((dbCard: CardDatabaseCard, quantity = 1) => {
    setDeckSections((prev) => {
      const section = [...(prev[activeSection] || [])];
      const existingIndex = section.findIndex((c) => c.id === dbCard.id);

      if (existingIndex >= 0) {
        section[existingIndex] = {
          ...section[existingIndex],
          quantity: section[existingIndex].quantity + quantity,
        };
      } else {
        section.push({
          id: dbCard.id,
          name: dbCard.name,
          quantity,
          section: activeSection,
          properties: dbCard.properties,
        });
      }

      return { ...prev, [activeSection]: section };
    });
    setHasUnsavedChanges(true);
  }, [activeSection]);

  // Remove card from deck
  const removeCard = useCallback((cardId: string) => {
    setDeckSections((prev) => ({
      ...prev,
      [activeSection]: (prev[activeSection] || []).filter((c) => c.id !== cardId),
    }));
    setHasUnsavedChanges(true);
  }, [activeSection]);

  // Update card quantity
  const updateQuantity = useCallback((cardId: string, delta: number) => {
    setDeckSections((prev) => {
      const section = [...(prev[activeSection] || [])];
      const index = section.findIndex((c) => c.id === cardId);
      if (index >= 0) {
        const newQty = Math.max(0, section[index].quantity + delta);
        if (newQty === 0) {
          section.splice(index, 1);
        } else {
          section[index] = { ...section[index], quantity: newQty };
        }
      }
      return { ...prev, [activeSection]: section };
    });
    setHasUnsavedChanges(true);
  }, [activeSection]);

  // Save deck
  const handleSave = useCallback(async () => {
    const deck: Deck = {
      gameId: 'magic', // Would come from selected game
      name: deckName,
      sections: deckSections,
      notes,
    };

    const xml = serializeDeck(deck);
    await saveDeckToFile(xml);
    setHasUnsavedChanges(false);
  }, [deckName, deckSections, notes]);

  // Load deck
  const handleLoad = useCallback(async () => {
    const xml = await loadDeckFromFile();
    if (xml) {
      const deck = parseDeck(xml);
      setDeckName(deck.name);
      setDeckSections(deck.sections);
      setNotes(deck.notes || '');
      setHasUnsavedChanges(false);
    }
  }, []);

  // Export deck
  const handleExport = useCallback(() => {
    const deck: Deck = {
      gameId: 'magic',
      name: deckName,
      sections: deckSections,
      notes,
    };
    const text = exportToText(deck);
    const blob = new Blob([text], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${deckName}.txt`;
    a.click();
    URL.revokeObjectURL(url);
  }, [deckName, deckSections, notes]);

  // Clear deck
  const handleClear = useCallback(() => {
    if (hasUnsavedChanges && !confirm('Discard unsaved changes?')) return;
    setDeckName('Untitled Deck');
    setDeckSections({ Main: [], Sideboard: [], Commander: [] });
    setNotes('');
    setHasUnsavedChanges(false);
  }, [hasUnsavedChanges]);

  const sections = Object.keys(deckSections);

  return (
    <div className="h-full flex">
      {/* Left Panel - Card Database */}
      <div className="w-80 bg-octgn-primary border-r border-octgn-accent flex flex-col">
        <div className="p-4 border-b border-octgn-accent">
          <h2 className="font-bold text-white mb-2">Card Database</h2>
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search cards..."
            className="input w-full"
          />
        </div>

        <div className="flex-1 overflow-y-auto p-2">
          <div className="space-y-1">
            {filteredCards.map((card) => (
              <div
                key={card.id}
                className="flex items-center justify-between p-2 rounded hover:bg-octgn-accent/50 cursor-pointer group"
                onClick={() => addCard(card)}
                onContextMenu={(e) => {
                  e.preventDefault();
                  setZoomCard(card);
                }}
              >
                <div>
                  <p className="text-white text-sm">{card.name}</p>
                  <p className="text-gray-500 text-xs">{card.set}</p>
                </div>
                <div className="opacity-0 group-hover:opacity-100 flex space-x-1">
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      addCard(card, 4);
                    }}
                    className="px-2 py-1 bg-octgn-highlight rounded text-xs text-white"
                  >
                    +4
                  </button>
                </div>
              </div>
            ))}

            {filteredCards.length === 0 && (
              <div className="text-center text-gray-400 py-8">
                No cards found
              </div>
            )}
          </div>
        </div>

        <div className="p-4 border-t border-octgn-accent text-xs text-gray-500">
          Click to add • Right-click for details
        </div>
      </div>

      {/* Center Panel - Deck List */}
      <div className="flex-1 flex flex-col">
        {/* Toolbar */}
        <div className="bg-octgn-primary p-2 border-b border-octgn-accent flex items-center space-x-2">
          <input
            type="text"
            value={deckName}
            onChange={(e) => {
              setDeckName(e.target.value);
              setHasUnsavedChanges(true);
            }}
            className="input flex-1"
          />
          <Button onClick={handleSave} variant="primary">
            💾 Save
          </Button>
          <Button onClick={handleLoad} variant="secondary">
            📂 Load
          </Button>
          <Button onClick={handleExport} variant="secondary">
            📤 Export
          </Button>
          <Button onClick={handleClear} variant="danger">
            🗑️ Clear
          </Button>
        </div>

        {/* Section Tabs */}
        <div className="bg-octgn-primary/50 border-b border-octgn-accent">
          <div className="flex">
            {sections.map((section) => (
              <button
                key={section}
                onClick={() => setActiveSection(section)}
                className={`px-6 py-2 font-medium transition-colors ${
                  activeSection === section
                    ? 'text-octgn-highlight border-b-2 border-octgn-highlight'
                    : 'text-gray-400 hover:text-white'
                }`}
              >
                {section}
                <span className="ml-2 text-xs">
                  ({sectionTotals[section] || 0})
                </span>
              </button>
            ))}
          </div>
        </div>

        {/* Card List */}
        <div className="flex-1 overflow-y-auto p-4">
          {deckSections[activeSection]?.length === 0 ? (
            <div className="text-gray-400 text-center py-16">
              <p>No cards in this section</p>
              <p className="text-sm mt-2">Click cards from the database to add them</p>
            </div>
          ) : (
            <div className="space-y-1">
              {deckSections[activeSection]?.map((card) => (
                <div
                  key={card.id}
                  onClick={() => setSelectedCard(card)}
                  className={`flex items-center justify-between p-2 rounded cursor-pointer transition-colors ${
                    selectedCard?.id === card.id
                      ? 'bg-octgn-accent'
                      : 'hover:bg-octgn-accent/50'
                  }`}
                >
                  <div className="flex items-center space-x-2">
                    <span className="text-octgn-highlight font-mono w-8">
                      {card.quantity}x
                    </span>
                    <span className="text-white">{card.name}</span>
                    {card.properties?.cost && (
                      <span className="text-gray-400 text-xs">
                        {card.properties.cost}
                      </span>
                    )}
                  </div>
                  <div className="flex items-center space-x-1">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        updateQuantity(card.id, -1);
                      }}
                      className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white"
                    >
                      -
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        updateQuantity(card.id, 1);
                      }}
                      className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white"
                    >
                      +
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        removeCard(card.id);
                      }}
                      className="w-6 h-6 rounded bg-red-600 hover:bg-red-700 text-white"
                    >
                      ×
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Stats Bar */}
        <div className="bg-octgn-primary p-2 border-t border-octgn-accent flex items-center justify-between">
          <span className="text-gray-400">
            Total: {totalCards} cards
          </span>
          <div className="flex space-x-4 text-sm">
            {sections.map((section) => (
              <span key={section} className="text-gray-400">
                {section}: {sectionTotals[section] || 0}
              </span>
            ))}
          </div>
          {hasUnsavedChanges && (
            <span className="text-yellow-500 text-sm">● Unsaved</span>
          )}
        </div>
      </div>

      {/* Right Panel - Card Preview & Notes */}
      <div className="w-80 bg-octgn-primary border-l border-octgn-accent flex flex-col">
        {/* Card Preview */}
        <div className="p-4 border-b border-octgn-accent">
          <h3 className="font-bold text-white mb-2">Card Preview</h3>
          {selectedCard ? (
            <div>
              <div className="aspect-[3/4] bg-octgn-accent rounded-lg mb-4 flex items-center justify-center">
                <span className="text-gray-400 text-4xl">🃏</span>
              </div>
              <h4 className="text-lg font-bold text-white">{selectedCard.name}</h4>
              {selectedCard.properties && (
                <div className="mt-2 text-sm text-gray-400 space-y-1">
                  {Object.entries(selectedCard.properties).map(([key, value]) => (
                    <div key={key}>
                      <span className="text-gray-500">{key}: </span>
                      {value}
                    </div>
                  ))}
                </div>
              )}
            </div>
          ) : (
            <div className="text-gray-400 text-center py-8">
              Select a card to preview
            </div>
          )}
        </div>

        {/* Notes */}
        <div className="flex-1 p-4">
          <h3 className="font-bold text-white mb-2">Notes</h3>
          <textarea
            value={notes}
            onChange={(e) => {
              setNotes(e.target.value);
              setHasUnsavedChanges(true);
            }}
            className="input w-full h-48 resize-none"
            placeholder="Add notes about your deck..."
          />
        </div>
      </div>

      {/* Card Zoom Overlay */}
      {zoomCard && (
        <Modal
          isOpen={true}
          onClose={() => setZoomCard(null)}
          title={zoomCard.name}
        >
          <div className="space-y-4">
            <div className="aspect-[3/4] bg-octgn-accent rounded-lg flex items-center justify-center">
              <span className="text-gray-400 text-6xl">🃏</span>
            </div>
            <div className="text-sm text-gray-400 space-y-1">
              <p><span className="text-gray-500">Set:</span> {zoomCard.set}</p>
              {Object.entries(zoomCard.properties).map(([key, value]) => (
                <p key={key}>
                  <span className="text-gray-500">{key}:</span> {value}
                </p>
              ))}
            </div>
            <Button
              variant="primary"
              className="w-full"
              onClick={() => {
                addCard(zoomCard);
                setZoomCard(null);
              }}
            >
              Add to Deck
            </Button>
          </div>
        </Modal>
      )}
    </div>
  );
}
