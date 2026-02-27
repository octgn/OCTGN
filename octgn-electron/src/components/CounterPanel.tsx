export default function CounterPanel({
  counters,
  playerId,
  onUpdateCounter,
}: {
  counters: Array<{
    id: string;
    name: string;
    value: number;
    ownerId?: string;
  }>;
  playerId: string;
  onUpdateCounter: (name: string, value: number) => void;
}) {
  const myCounters = counters.filter((c) => !c.ownerId || c.ownerId === playerId);
  const globalCounters = counters.filter((c) => !c.ownerId);

  return (
    <div className="space-y-3">
      <h3 className="text-sm font-semibold text-gray-400 uppercase tracking-wide">
        Counters
      </h3>

      {counters.length === 0 ? (
        <p className="text-gray-500 text-xs text-center py-2">No counters</p>
      ) : (
        <div className="space-y-2">
          {counters.map((counter) => (
            <div
              key={counter.id}
              className="flex items-center justify-between p-2 rounded-lg bg-octgn-dark/50 group"
            >
              <span className="text-sm text-gray-300 truncate flex-1">
                {counter.name}
              </span>
              
              <div className="flex items-center space-x-1">
                <button
                  onClick={() => onUpdateCounter(counter.name, counter.value - 1)}
                  className="w-6 h-6 rounded bg-octgn-accent/30 hover:bg-octgn-accent text-white text-sm font-bold transition-colors"
                >
                  -
                </button>
                
                <span className="w-10 text-center font-mono font-bold text-octgn-highlight">
                  {counter.value}
                </span>
                
                <button
                  onClick={() => onUpdateCounter(counter.name, counter.value + 1)}
                  className="w-6 h-6 rounded bg-octgn-accent/30 hover:bg-octgn-accent text-white text-sm font-bold transition-colors"
                >
                  +
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// Compact counter for toolbar
export function CounterBadge({
  name,
  value,
  onChange,
}: {
  name: string;
  value: number;
  onChange?: (delta: number) => void;
}) {
  return (
    <div className="flex items-center space-x-1 px-2 py-1 rounded-lg bg-octgn-accent/30">
      <span className="text-xs text-gray-400">{name}:</span>
      {onChange && (
        <button
          onClick={() => onChange(-1)}
          className="w-5 h-5 rounded hover:bg-octgn-accent text-white text-xs"
        >
          -
        </button>
      )}
      <span className="font-mono font-bold text-octgn-highlight min-w-[1.5rem] text-center">
        {value}
      </span>
      {onChange && (
        <button
          onClick={() => onChange(1)}
          className="w-5 h-5 rounded hover:bg-octgn-accent text-white text-xs"
        >
          +
        </button>
      )}
    </div>
  );
}

// Life counter specifically for card games
export function LifeCounter({
  life,
  onChange,
  maxLife,
}: {
  life: number;
  onChange: (delta: number) => void;
  maxLife?: number;
}) {
  const percentage = maxLife ? (life / maxLife) * 100 : 100;
  
  return (
    <div className="glass rounded-xl p-4 text-center">
      <p className="text-xs text-gray-500 uppercase tracking-wide mb-2">Life</p>
      
      <div className="relative mb-3">
        <span className={`text-4xl font-bold ${
          life <= 5 ? 'text-red-400' : life <= 10 ? 'text-yellow-400' : 'text-white'
        }`}>
          {life}
        </span>
        
        {maxLife && (
          <span className="text-sm text-gray-500 ml-1">/ {maxLife}</span>
        )}
      </div>
      
      {/* Life bar */}
      {maxLife && (
        <div className="h-2 bg-octgn-dark rounded-full overflow-hidden mb-3">
          <div
            className={`h-full transition-all duration-300 ${
              percentage > 50 ? 'bg-octgn-success' : percentage > 25 ? 'bg-octgn-warning' : 'bg-octgn-error'
            }`}
            style={{ width: `${percentage}%` }}
          />
        </div>
      )}
      
      {/* Controls */}
      <div className="grid grid-cols-4 gap-2">
        {[-5, -1, 1, 5].map((delta) => (
          <button
            key={delta}
            onClick={() => onChange(delta)}
            className={`
              py-2 rounded-lg font-bold transition-all
              ${delta < 0
                ? 'bg-red-500/20 text-red-400 hover:bg-red-500/30'
                : 'bg-green-500/20 text-green-400 hover:bg-green-500/30'
              }
            `}
          >
            {delta > 0 ? `+${delta}` : delta}
          </button>
        ))}
      </div>
    </div>
  );
}
