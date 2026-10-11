import React, { useState, useRef, useEffect, useCallback } from 'react';

export interface DialogRequest {
  requestId: string;
  type: string;
  params: Record<string, unknown>;
}

interface ScriptDialogProps {
  request: DialogRequest;
  onRespond: (requestId: string, result: unknown) => void;
}

// ── Dialog type icons ──
const DIALOG_ICONS: Record<string, React.ReactNode> = {
  confirm: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="10" cy="10" r="8" />
      <path d="M10 6v5M10 13.5v.5" />
    </svg>
  ),
  askInteger: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <rect x="3" y="3" width="14" height="14" rx="2" />
      <path d="M7 10h6M10 7v6" />
    </svg>
  ),
  askString: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M4 7h12M4 13h8" />
      <rect x="3" y="3" width="14" height="14" rx="2" />
    </svg>
  ),
  askChoice: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <path d="M6 7h8M6 10h8M6 13h5" />
      <rect x="3" y="3" width="14" height="14" rx="2" />
    </svg>
  ),
  askMarker: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <circle cx="10" cy="8" r="5" />
      <path d="M10 13v4" />
    </svg>
  ),
  askCard: (
    <svg className="w-5 h-5" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
      <rect x="4" y="2" width="12" height="16" rx="2" />
      <circle cx="10" cy="10" r="3" />
    </svg>
  ),
};

const DIALOG_LABELS: Record<string, string> = {
  confirm: 'Confirm',
  askInteger: 'Input',
  askString: 'Input',
  askChoice: 'Choose',
  askMarker: 'Marker',
  askCard: 'Card Search',
};

const ScriptDialog: React.FC<ScriptDialogProps> = ({ request, onRespond }) => {
  const { requestId, type, params } = request;

  const respond = useCallback((result: unknown) => {
    onRespond(requestId, result);
  }, [onRespond, requestId]);

  return (
    <div
      data-testid="dialog-overlay"
      className="fixed inset-0 z-50 flex items-center justify-center"
      style={{
        background: 'radial-gradient(ellipse at center, rgba(59,130,246,0.06) 0%, rgba(0,0,0,0.75) 100%)',
        animation: 'dialog-overlay-in 150ms ease-out both',
      }}
    >
      <div
        className="relative min-w-[340px] max-w-[480px] rounded-xl overflow-hidden"
        style={{
          background: 'linear-gradient(170deg, rgba(17,24,39,0.97) 0%, rgba(10,14,23,0.98) 100%)',
          border: '1px solid rgba(59,130,246,0.2)',
          boxShadow: '0 0 40px rgba(59,130,246,0.12), 0 25px 60px rgba(0,0,0,0.5), inset 0 1px 0 rgba(255,255,255,0.04)',
          animation: 'dialog-panel-in 200ms cubic-bezier(0.23,1,0.32,1) both',
        }}
      >
        {/* Top accent line */}
        <div
          className="h-[2px] w-full"
          style={{ background: 'linear-gradient(90deg, transparent, rgba(59,130,246,0.6), rgba(139,92,246,0.4), transparent)' }}
        />

        {/* Header */}
        <div className="flex items-center gap-2.5 px-5 pt-4 pb-0">
          <span className="text-octgn-primary/70">
            {DIALOG_ICONS[type] ?? DIALOG_ICONS.confirm}
          </span>
          <span className="font-display text-[11px] font-bold tracking-[0.2em] uppercase text-octgn-text-dim">
            {DIALOG_LABELS[type] ?? 'Dialog'}
          </span>
        </div>

        {/* Content */}
        <div className="px-5 pt-3 pb-5">
          {type === 'confirm' && <ConfirmContent params={params} respond={respond} />}
          {type === 'askInteger' && <IntegerContent params={params} respond={respond} />}
          {type === 'askString' && <StringContent params={params} respond={respond} />}
          {type === 'askChoice' && <ChoiceContent params={params} respond={respond} />}
          {type === 'askMarker' && <MarkerContent params={params} respond={respond} />}
          {type === 'askCard' && <CardContent params={params} respond={respond} />}
        </div>
      </div>

      {/* Inline keyframes */}
      <style>{`
        @keyframes dialog-overlay-in {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        @keyframes dialog-panel-in {
          from { opacity: 0; transform: translateY(8px) scale(0.97); }
          to { opacity: 1; transform: translateY(0) scale(1); }
        }
        @keyframes dialog-shake {
          0%, 100% { transform: translateX(0); }
          20% { transform: translateX(-4px); }
          40% { transform: translateX(4px); }
          60% { transform: translateX(-3px); }
          80% { transform: translateX(2px); }
        }
      `}</style>
    </div>
  );
};

// ── Shared button styles ──
const btnBase = 'px-4 py-1.5 text-sm font-medium rounded-lg tracking-wide transition-all duration-150 outline-none focus-visible:ring-2 focus-visible:ring-octgn-primary/50 focus-visible:ring-offset-1 focus-visible:ring-offset-octgn-bg';
const btnCancel = `${btnBase} bg-white/[0.04] border border-octgn-border/40 text-octgn-text-muted hover:bg-white/[0.08] hover:text-octgn-text hover:border-octgn-border/60 active:scale-[0.97]`;
const btnPrimary = `${btnBase} bg-octgn-primary/90 border border-octgn-primary/40 text-white hover:bg-octgn-primary hover:shadow-[0_0_16px_rgba(59,130,246,0.35)] hover:border-octgn-primary/60 active:scale-[0.97]`;
const btnDanger = `${btnBase} bg-octgn-danger/20 border border-octgn-danger/30 text-octgn-danger hover:bg-octgn-danger/30 hover:border-octgn-danger/50 active:scale-[0.97]`;

// ── Confirm ──
function ConfirmContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') respond(false);
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [respond]);

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-5">{String(params.message ?? '')}</p>
      <div className="flex justify-end gap-2.5">
        <button onClick={() => respond(false)} className={btnCancel}>
          No
        </button>
        <button onClick={() => respond(true)} className={btnPrimary} autoFocus>
          Yes
        </button>
      </div>
    </div>
  );
}

// ── AskInteger ──
function IntegerContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  const defaultVal = typeof params.defaultAnswer === 'number' ? params.defaultAnswer : 0;
  const [value, setValue] = useState(String(defaultVal));
  const [shaking, setShaking] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.select();
  }, []);

  const submit = () => {
    const num = parseInt(value, 10);
    if (isNaN(num) || num < 0) {
      setShaking(true);
      setValue(String(Math.max(0, num || 0)));
      setTimeout(() => setShaking(false), 400);
      respond(Math.max(0, num || 0));
    } else {
      respond(num);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') submit();
    if (e.key === 'Escape') respond(null);
  };

  const isInvalid = (() => {
    const n = parseInt(value, 10);
    return value !== '' && (isNaN(n) || n < 0);
  })();

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-3">{String(params.question ?? '')}</p>
      <div
        style={shaking ? { animation: 'dialog-shake 0.35s ease-out' } : undefined}
      >
        <input
          ref={inputRef}
          type="number"
          min="0"
          value={value}
          onChange={(e) => setValue(e.target.value)}
          onKeyDown={handleKeyDown}
          className={`w-full px-3 py-2 text-sm rounded-lg border text-octgn-text font-mono
            transition-all duration-150 outline-none
            ${isInvalid
              ? 'bg-octgn-danger/10 border-octgn-danger/50 shadow-[0_0_12px_rgba(239,68,68,0.15)]'
              : 'bg-white/[0.03] border-octgn-border/40 focus:border-octgn-primary/60 focus:shadow-[0_0_12px_rgba(59,130,246,0.12)]'
            }`}
          autoFocus
        />
      </div>
      {isInvalid && (
        <p className="text-octgn-danger text-[11px] mt-1.5 tracking-wide">Positive numbers only</p>
      )}
      <div className="flex justify-end gap-2.5 mt-4">
        <button onClick={() => respond(null)} className={btnCancel}>
          Cancel
        </button>
        <button onClick={submit} className={btnPrimary}>
          OK
        </button>
      </div>
    </div>
  );
}

// ── AskString ──
function StringContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  const defaultVal = typeof params.defaultAnswer === 'string' ? params.defaultAnswer : '';
  const [value, setValue] = useState(defaultVal);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    inputRef.current?.select();
  }, []);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') respond(value);
    if (e.key === 'Escape') respond(null);
  };

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-3">{String(params.question ?? '')}</p>
      <input
        ref={inputRef}
        type="text"
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={handleKeyDown}
        className="w-full px-3 py-2 text-sm rounded-lg bg-white/[0.03] border border-octgn-border/40 text-octgn-text
          transition-all duration-150 outline-none
          focus:border-octgn-primary/60 focus:shadow-[0_0_12px_rgba(59,130,246,0.12)]"
        autoFocus
      />
      <div className="flex justify-end gap-2.5 mt-4">
        <button onClick={() => respond(null)} className={btnCancel}>
          Cancel
        </button>
        <button onClick={() => respond(value)} className={btnPrimary}>
          OK
        </button>
      </div>
    </div>
  );
}

// ── AskChoice ──
function ChoiceContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  const choices = Array.isArray(params.choices) ? params.choices as string[] : [];
  const colors = Array.isArray(params.colors) ? params.colors as string[] : [];
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') respond(-1);
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [respond]);

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-3">{String(params.question ?? '')}</p>
      <div className="flex flex-col gap-0.5 max-h-[280px] overflow-y-auto rounded-lg border border-octgn-border/20 bg-white/[0.01] p-1">
        {choices.map((choice, i) => {
          const color = colors[i] || null;
          const isHovered = hoveredIndex === i;
          return (
            <button
              key={i}
              onClick={() => respond(i)}
              onMouseEnter={() => setHoveredIndex(i)}
              onMouseLeave={() => setHoveredIndex(null)}
              className="relative text-left px-3 py-2 text-sm rounded-md text-octgn-text transition-all duration-100
                hover:bg-octgn-primary/10"
              style={{
                borderLeft: color ? `3px solid ${color}` : '3px solid transparent',
                boxShadow: isHovered && color ? `inset 0 0 20px ${color}18` : undefined,
              }}
            >
              {choice}
            </button>
          );
        })}
      </div>
      <div className="flex justify-end gap-2.5 mt-4">
        <button onClick={() => respond(-1)} className={btnCancel}>
          Cancel
        </button>
      </div>
    </div>
  );
}

// ── AskMarker ──
function MarkerContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') respond(null);
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [respond]);

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-2">Select a marker</p>
      <div className="flex items-center gap-2 px-3 py-3 rounded-lg bg-white/[0.02] border border-octgn-border/20 mb-4">
        <svg className="w-4 h-4 text-octgn-text-dim" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.2">
          <circle cx="8" cy="8" r="6" strokeDasharray="3 2" />
        </svg>
        <span className="text-xs text-octgn-text-dim italic">Marker picker coming soon</span>
      </div>
      <div className="flex justify-end gap-2.5">
        <button onClick={() => respond(null)} className={btnCancel}>
          Cancel
        </button>
      </div>
    </div>
  );
}

// ── AskCard ──
function CardContent({ params, respond }: { params: Record<string, unknown>; respond: (r: unknown) => void }) {
  const title = typeof params.title === 'string' && params.title ? params.title : 'Select a Card';

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') respond(null);
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [respond]);

  return (
    <div>
      <p className="text-octgn-text text-sm leading-relaxed mb-2">{title}</p>
      <div className="flex items-center gap-2 px-3 py-3 rounded-lg bg-white/[0.02] border border-octgn-border/20 mb-4">
        <svg className="w-4 h-4 text-octgn-text-dim" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.2">
          <rect x="3" y="1" width="10" height="14" rx="1.5" strokeDasharray="3 2" />
        </svg>
        <span className="text-xs text-octgn-text-dim italic">Card search coming soon</span>
      </div>
      <div className="flex justify-end gap-2.5">
        <button onClick={() => respond(null)} className={btnCancel}>
          Cancel
        </button>
      </div>
    </div>
  );
}

export default ScriptDialog;
