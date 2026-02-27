import { useState } from 'react';
import { Counter } from '../types/game';
import Button from './Button';

interface CounterPanelProps {
  counters: Counter[];
  playerId: string;
  onUpdateCounter?: (name: string, value: number) => void;
  className?: string;
}

export default function CounterPanel({
  counters,
  playerId,
  onUpdateCounter,
  className = '',
}: CounterPanelProps) {
  const [editingCounter, setEditingCounter] = useState<string | null>(null);
  const [editValue, setEditValue] = useState('');

  const handleIncrement = (counter: Counter, delta: number) => {
    const newValue = counter.value + delta;
    onUpdateCounter?.(counter.id, newValue);
  };

  const handleSetCounter = (counter: Counter) => {
    const value = parseInt(editValue, 10);
    if (!isNaN(value)) {
      onUpdateCounter?.(counter.id, value);
    }
    setEditingCounter(null);
    setEditValue('');
  };

  const startEditing = (counter: Counter) => {
    setEditingCounter(counter.id);
    setEditValue(counter.value.toString());
  };

  if (counters.length === 0) {
    return (
      <div className={`counter-panel ${className}`}>
        <h3 className="text-sm font-bold text-white mb-2 px-2">Counters</h3>
        <div className="text-gray-400 text-sm text-center py-4">
          No counters
        </div>
      </div>
    );
  }

  return (
    <div className={`counter-panel ${className}`}>
      <h3 className="text-sm font-bold text-white mb-2 px-2">Counters</h3>
      <div className="space-y-2">
        {counters.map((counter) => {
          const isEditing = editingCounter === counter.id;

          return (
            <div
              key={counter.id}
              className="bg-octgn-accent/30 rounded-lg p-2"
            >
              {/* Counter name */}
              <div className="flex items-center justify-between mb-1">
                <span className="text-xs font-medium text-gray-300">
                  {counter.name}
                </span>
                {counter.reset && (
                  <span className="text-xs text-gray-500">⟲</span>
                )}
              </div>

              {/* Counter value and controls */}
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-1">
                  <button
                    onClick={() => handleIncrement(counter, -1)}
                    className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white text-sm"
                  >
                    -
                  </button>

                  {isEditing ? (
                    <input
                      type="number"
                      value={editValue}
                      onChange={(e) => setEditValue(e.target.value)}
                      onBlur={() => handleSetCounter(counter)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') handleSetCounter(counter);
                        if (e.key === 'Escape') setEditingCounter(null);
                      }}
                      className="w-12 text-center bg-octgn-primary text-white text-lg font-bold rounded px-1 py-0.5"
                      autoFocus
                    />
                  ) : (
                    <button
                      onClick={() => startEditing(counter)}
                      className="min-w-[40px] text-center text-white text-lg font-bold hover:bg-octgn-primary rounded px-1 py-0.5"
                    >
                      {counter.value}
                    </button>
                  )}

                  <button
                    onClick={() => handleIncrement(counter, 1)}
                    className="w-6 h-6 rounded bg-octgn-primary hover:bg-octgn-accent text-white text-sm"
                  >
                    +
                  </button>
                </div>

                {/* Quick actions */}
                <div className="flex items-center space-x-1">
                  <button
                    onClick={() => handleIncrement(counter, 5)}
                    className="text-xs text-gray-400 hover:text-white"
                  >
                    +5
                  </button>
                  <button
                    onClick={() => handleIncrement(counter, 10)}
                    className="text-xs text-gray-400 hover:text-white"
                  >
                    +10
                  </button>
                  {counter.start !== counter.value && (
                    <button
                      onClick={() => onUpdateCounter?.(counter.id, counter.start)}
                      className="text-xs text-gray-400 hover:text-white"
                      title={`Reset to ${counter.start}`}
                    >
                      Reset
                    </button>
                  )}
                </div>
              </div>

              {/* Color indicator */}
              {counter.color && (
                <div
                  className="h-1 rounded-full mt-2"
                  style={{ backgroundColor: counter.color }}
                />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
