import { useState } from 'react';
import { useGameStore } from '../stores/gameStore';

interface DeckCard {
  id: string;
  name: string;
  quantity: number;
  section: string;
}

export default function DeckEditorPage() {
  const { connected } = useGameStore();
  const [deckName, setDeckName] = useState('Untitled Deck');
  const [cards, setCards] = useState<DeckCard[]>([
    { id: '1', name: 'Lightning Bolt', quantity: 4, section: 'Main' },
    { id: '2', name: 'Mountain', quantity: 20, section: 'Main' },
    { id: '3', name: 'Goblin Guide', quantity: 4, section: 'Main' },
  ]);
  const [selectedCard, setSelectedCard] = useState<DeckCard | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeSection, setActiveSection] = useState('Main');

  const sections = ['Main', 'Sideboard', 'Commander'];

  const totalCards = cards
    .filter((c) => c.section === activeSection)
    .reduce((sum, c) => sum + c.quantity, 0);

  const handleAddCard = () => {
    const newCard: DeckCard = {
      id: Date.now().toString(),
      name: `New Card ${cards.length + 1}`,
      quantity: 1,
      section: activeSection,
    };
    setCards([...cards, newCard]);
    setSelectedCard(newCard);
  };

  const handleRemoveCard = (cardId: string) => {
    setCards(cards.filter((c) => c.id !== cardId));
    if (selectedCard?.id === cardId) {
      setSelectedCard(null);
    }
  };

  const handleQuantityChange = (cardId: string, delta: number) => {
    setCards(
      cards.map((c) => {
        if (c.id === cardId) {
          return { ...c, quantity: Math.max(1, c.quantity + delta) };
        }
        return c;
      })
    );
  };

  const handleSaveDeck = () => {
    const deck = {
      name: deckName,
      cards,
      savedAt: new Date().toISOString(),
    };
    console.log('Saving deck:', deck);
    // TODO: Save to file system
    alert('Deck saved!');
  };

  const handleLoadDeck = () => {
    // TODO: Load from file system
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.o8d,.txt';
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        console.log('Loading file:', file.name);
      }
    };
    input.click();
  };

  const handleExportDeck = () => {
    const text = cards
      .map((c) => `${c.quantity}x ${c.name}`)
      .join('\n');
    const blob = new Blob([text], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${deckName}.txt`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const filteredCards = cards.filter(
    (c) =>
      c.section === activeSection &&
      c.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

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
          {/* Placeholder for card search results */}
          <div className="text-gray-400 text-center py-8">
            <p>Search for cards to add to your deck</p>
            <p className="text-sm mt-2">(Card database integration coming soon)</p>
          </div>
        </div>

        <div className="p-4 border-t border-octgn-accent">
          <button onClick={handleAddCard} className="btn btn-primary w-full">
            + Add Card
          </button>
        </div>
      </div>

      {/* Center Panel - Deck List */}
      <div className="flex-1 flex flex-col">
        {/* Toolbar */}
        <div className="bg-octgn-primary p-2 border-b border-octgn-accent flex items-center space-x-2">
          <input
            type="text"
            value={deckName}
            onChange={(e) => setDeckName(e.target.value)}
            className="input flex-1"
          />
          <button onClick={handleSaveDeck} className="btn btn-primary">
            💾 Save
          </button>
          <button onClick={handleLoadDeck} className="btn btn-secondary">
            📂 Load
          </button>
          <button onClick={handleExportDeck} className="btn btn-secondary">
            📤 Export
          </button>
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
                  (
                  {cards
                    .filter((c) => c.section === section)
                    .reduce((sum, c) => sum + c.quantity, 0)}
                  )
                </span>
              </button>
            ))}
          </div>
        </div>

        {/* Card List */}
        <div className="flex-1 overflow-y-auto p-4">
          {filteredCards.length === 0 ? (
            <div className="text-gray-400 text-center py-16">
              <p>No cards in this section</p>
              <p className="text-sm mt-2">Add cards from the database or use the + Add Card button</p>
            </div>
          ) : (
            <div className="space-y-1">
              {filteredCards.map((card) => (
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
                  </div>
                  <div className="flex items-center space-x-1">
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleQuantityChange(card.id, -1);
                      }}
                      className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white"
                    >
                      -
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleQuantityChange(card.id, 1);
                      }}
                      className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white"
                    >
                      +
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        handleRemoveCard(card.id);
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
            <span className="text-gray-400">
              Main:{' '}
              {cards
                .filter((c) => c.section === 'Main')
                .reduce((sum, c) => sum + c.quantity, 0)}
            </span>
            <span className="text-gray-400">
              Side:{' '}
              {cards
                .filter((c) => c.section === 'Sideboard')
                .reduce((sum, c) => sum + c.quantity, 0)}
            </span>
          </div>
        </div>
      </div>

      {/* Right Panel - Card Preview */}
      <div className="w-80 bg-octgn-primary border-l border-octgn-accent">
        <div className="p-4 border-b border-octgn-accent">
          <h2 className="font-bold text-white">Card Preview</h2>
        </div>

        {selectedCard ? (
          <div className="p-4">
            {/* Card Image Placeholder */}
            <div className="aspect-[3/4] bg-octgn-accent rounded-lg mb-4 flex items-center justify-center">
              <span className="text-gray-400 text-4xl">🃏</span>
            </div>

            <h3 className="text-lg font-bold text-white mb-2">
              {selectedCard.name}
            </h3>

            <div className="space-y-2 text-sm text-gray-400">
              <p>Quantity: {selectedCard.quantity}</p>
              <p>Section: {selectedCard.section}</p>
            </div>

            <div className="mt-4 space-y-2">
              <button className="btn btn-secondary w-full text-sm">
                View Full Card
              </button>
              <button className="btn btn-secondary w-full text-sm">
                Find in Database
              </button>
            </div>
          </div>
        ) : (
          <div className="p-4 text-center text-gray-400">
            <p>Select a card to preview</p>
          </div>
        )}
      </div>
    </div>
  );
}
