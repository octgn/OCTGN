import { useState, useEffect, useMemo, useCallback } from 'react';
import { parseDeck, serializeDeck, exportToText } from '../utils/deckParser';
import { Deck, DeckCard } from '../utils/deckParser';
import { Button, Modal, Badge } from '../components';
import { useInstalledGames, GamePackage } from '../services/GameFeedService';

interface CardDatabaseCard {
  id: string;
  name: string;
  set: string;
  properties: Record<string, string>;
  imageUrl?: string;
}

export default function DeckEditorPage() {
  const { games: installedGames } = useInstalledGames();
  
  // Selected game
  const [selectedGameId, setSelectedGameId] = useState<string>('');
  const [selectedGame, setSelectedGame] = useState<GamePackage | null>(null);

  // Deck state
  const [deckName, setDeckName] = useState('Untitled Deck');
  const [deckSections, setDeckSections] = useState<Record<string, DeckCard[]>>({
    Main: [],
    Sideboard: [],
    Commander: [],
  });
  const [activeSection, setActiveSection] = useState('Main');
  const [notes, setNotes] = useState('');

  // Card database
  const [cardDatabase, setCardDatabase] = useState<CardDatabaseCard[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [databaseLoading, setDatabaseLoading] = useState(false);

  // UI state
  const [selectedCard, setSelectedCard] = useState<DeckCard | null>(null);
  const [zoomCard, setZoomCard] = useState<CardDatabaseCard | null>(null);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(false);

  // Recent decks
  const [recentDecks, setRecentDecks] = useState<Array<{ name: string; path: string }>>([]);

  // Load installed games and select first
  useEffect(() => {
    if (installedGames.length > 0 && !selectedGameId) {
      setSelectedGameId(installedGames[0].id);
      setSelectedGame(installedGames[0]);
    }
  }, [installedGames, selectedGameId]);

  // Update selected game
  useEffect(() => {
    const game = installedGames.find((g) => g.id === selectedGameId);
    setSelectedGame(game || null);
  }, [selectedGameId, installedGames]);

  // Load card database for selected game
  useEffect(() => {
    if (!selectedGame?.installPath) {
      setCardDatabase([]);
      return;
    }

    const loadDatabase = async () => {
      setDatabaseLoading(true);
      try {
        // In a real implementation, we'd load cards from the game package
        // For now, use demo cards
        const demoCards: CardDatabaseCard[] = [
          { id: 'card1', name: 'Lightning Bolt', set: 'M12', properties: { type: 'Instant', cost: 'R', text: 'Deal 3 damage to any target.' } },
          { id: 'card2', name: 'Giant Growth', set: 'M10', properties: { type: 'Instant', cost: 'G', text: 'Target creature gets +3/+3 until end of turn.' } },
          { id: 'card3', name: 'Mountain', set: 'UNH', properties: { type: 'Basic Land', subtype: 'Mountain' } },
          { id: 'card4', name: 'Forest', set: 'UNH', properties: { type: 'Basic Land', subtype: 'Forest' } },
          { id: 'card5', name: 'Serra Angel', set: 'M10', properties: { type: 'Creature', cost: '3WW', pt: '4/4', text: 'Flying, vigilance' } },
          { id: 'card6', name: 'Goblin Guide', set: 'ZEN', properties: { type: 'Creature', cost: 'R', pt: '2/2', text: 'Haste' } },
          { id: 'card7', name: 'Sol Ring', set: 'C13', properties: { type: 'Artifact', cost: '1', text: 'Tap: Add CC.' } },
          { id: 'card8', name: 'Counterspell', set: 'IMA', properties: { type: 'Instant', cost: 'UU', text: 'Counter target spell.' } },
        ];
        setCardDatabase(demoCards);
      } finally {
        setDatabaseLoading(false);
      }
    };

    loadDatabase();
  }, [selectedGame]);

  // Load recent decks on mount
  useEffect(() => {
    const loadRecentDecks = async () => {
      if (window.electronAPI?.listDecks) {
        const result = await window.electronAPI.listDecks();
        if (result.success && result.decks) {
          setRecentDecks(result.decks);
        }
      }
    };
    loadRecentDecks();
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
    if (!selectedGame) {
      alert('Please select a game first');
      return;
    }

    const deck: Deck = {
      gameId: selectedGame.id,
      name: deckName,
      sections: deckSections,
      notes,
    };

    setSaving(true);
    try {
      const xml = serializeDeck(deck);

      if (window.electronAPI?.saveDeck) {
        const result = await window.electronAPI.saveDeck(deckName, xml);
        if (result.success) {
          setHasUnsavedChanges(false);
          // Refresh recent decks
          if (window.electronAPI?.listDecks) {
            const listResult = await window.electronAPI.listDecks();
            if (listResult.success && listResult.decks) {
              setRecentDecks(listResult.decks);
            }
          }
        } else {
          alert(`Failed to save: ${result.error}`);
        }
      } else {
        // Browser fallback - download file
        const blob = new Blob([xml], { type: 'application/xml' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${deckName}.o8d`;
        a.click();
        URL.revokeObjectURL(url);
        setHasUnsavedChanges(false);
      }
    } finally {
      setSaving(false);
    }
  }, [selectedGame, deckName, deckSections, notes]);

  // Load deck
  const handleLoad = useCallback(async (deckPath?: string) => {
    setLoading(true);
    try {
      let xml: string | null = null;

      if (deckPath && window.electronAPI?.loadDeck) {
        const result = await window.electronAPI.loadDeck(deckPath);
        if (result.success && result.data) {
          xml = result.data;
        }
      } else if (window.electronAPI?.openFileDialog) {
        const result = await window.electronAPI.openFileDialog({
          title: 'Open Deck',
          filters: [{ name: 'OCTGN Decks', extensions: ['o8d'] }],
          properties: ['openFile'],
        });

        if (!result.canceled && result.filePaths.length > 0) {
          const fileResult = await window.electronAPI.readFile(result.filePaths[0]);
          if (fileResult.success && fileResult.data) {
            xml = fileResult.data;
          }
        }
      }

      if (xml) {
        const deck = parseDeck(xml);
        setDeckName(deck.name);
        setDeckSections(deck.sections);
        setNotes(deck.notes || '');
        setHasUnsavedChanges(false);
        
        // Update selected game if different
        if (deck.gameId && deck.gameId !== selectedGameId) {
          setSelectedGameId(deck.gameId);
        }
      }
    } finally {
      setLoading(false);
    }
  }, [selectedGameId]);

  // Export deck to text
  const handleExport = useCallback(() => {
    if (!selectedGame) return;

    const deck: Deck = {
      gameId: selectedGame.id,
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
  }, [selectedGame, deckName, deckSections, notes]);

  // Clear deck
  const handleClear = useCallback(() => {
    if (hasUnsavedChanges && !confirm('Discard unsaved changes?')) return;
    setDeckName('Untitled Deck');
    setDeckSections({ Main: [], Sideboard: [], Commander: [] });
    setNotes('');
    setHasUnsavedChanges(false);
  }, [hasUnsavedChanges]);

  // New deck
  const handleNew = useCallback(() => {
    handleClear();
  }, [handleClear]);

  const sections = Object.keys(deckSections);

  return (
    <div className="h-full flex">
      {/* Left Panel - Card Database */}
      <div className="w-80 bg-octgn-primary border-r border-octgn-accent flex flex-col">
        {/* Game Selector */}
        <div className="p-4 border-b border-octgn-accent">
          <label className="block text-sm text-gray-400 mb-2">Game</label>
          <select
            value={selectedGameId}
            onChange={(e) => setSelectedGameId(e.target.value)}
            className="input w-full"
          >
            <option value="">Select a game...</option>
            {installedGames.map((game) => (
              <option key={game.id} value={game.id}>
                {game.name}
              </option>
            ))}
          </select>
        </div>

        {/* Search */}
        <div className="p-4 border-b border-octgn-accent">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search cards..."
            className="input w-full"
          />
        </div>

        {/* Card List */}
        <div className="flex-1 overflow-y-auto p-2">
          {databaseLoading ? (
            <div className="text-center py-8 text-gray-400">Loading cards...</div>
          ) : !selectedGame ? (
            <div className="text-center py-8 text-gray-400">
              Select a game to load cards
            </div>
          ) : filteredCards.length === 0 ? (
            <div className="text-center py-8 text-gray-400">No cards found</div>
          ) : (
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
            </div>
          )}
        </div>

        {/* Stats */}
        <div className="p-4 border-t border-octgn-accent text-xs text-gray-500">
          {cardDatabase.length} cards in database
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
            placeholder="Deck Name"
          />
          <Button onClick={handleNew} variant="ghost" size="sm">
            📄 New
          </Button>
          <Button onClick={handleSave} variant="primary" size="sm" loading={saving}>
            💾 Save
          </Button>
          <Button onClick={() => handleLoad()} variant="secondary" size="sm">
            📂 Open
          </Button>
          <Button onClick={handleExport} variant="secondary" size="sm">
            📤 Export
          </Button>
          <Button onClick={handleClear} variant="ghost" size="sm">
            🗑️ Clear
          </Button>
        </div>

        {/* Recent Decks */}
        {recentDecks.length > 0 && (
          <div className="bg-octgn-accent/20 p-2 border-b border-octgn-accent flex items-center space-x-2 text-sm">
            <span className="text-gray-400">Recent:</span>
            {recentDecks.slice(0, 5).map((deck) => (
              <button
                key={deck.path}
                onClick={() => handleLoad(deck.path)}
                className="text-octgn-highlight hover:underline"
              >
                {deck.name.replace('.o8d', '')}
              </button>
            ))}
          </div>
        )}

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
          {loading ? (
            <div className="text-center py-16 text-gray-400">Loading deck...</div>
          ) : deckSections[activeSection]?.length === 0 ? (
            <div className="text-gray-400 text-center py-16">
              <p className="mb-2">No cards in this section</p>
              <p className="text-sm">Click cards from the database to add them</p>
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
          <span className="text-gray-400">Total: {totalCards} cards</span>
          <div className="flex space-x-4 text-sm">
            {sections.map((section) => (
              <span key={section} className="text-gray-400">
                {section}: {sectionTotals[section] || 0}
              </span>
            ))}
          </div>
          {hasUnsavedChanges && (
            <Badge variant="warning">Unsaved</Badge>
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

      {/* Card Zoom Modal */}
      {zoomCard && (
        <Modal
          isOpen={true}
          onClose={() => setZoomCard(null)}
          title={zoomCard.name}
          size="sm"
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
