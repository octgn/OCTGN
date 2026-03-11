import React, { useState, useEffect, useRef, useMemo, useCallback } from 'react';
import { clsx } from 'clsx';
import GlassPanel from './GlassPanel';
import { useGameStore } from '../stores/game-store';
import type { ChatMessage } from '../../shared/types';

interface GameLogProps {
  className?: string;
}

type ActionFilter = 'all' | 'chat' | 'system' | 'action';

const filterLabels: Record<ActionFilter, string> = {
  all: 'All',
  chat: 'Chat',
  system: 'System',
  action: 'Actions',
};

function classifyMessage(msg: ChatMessage): ActionFilter {
  if (msg.isSystem) return 'system';
  if (msg.message.startsWith('/') || msg.message.startsWith('[Action]')) return 'action';
  return 'chat';
}

function getMessageColor(msg: ChatMessage): string {
  if (msg.isSystem) return 'text-octgn-warning';
  if (msg.message.startsWith('/') || msg.message.startsWith('[Action]')) return 'text-octgn-accent';
  if (msg.color) return '';
  return 'text-octgn-text';
}

function formatTimestamp(ts: number): string {
  const d = new Date(ts);
  const h = d.getHours().toString().padStart(2, '0');
  const m = d.getMinutes().toString().padStart(2, '0');
  const s = d.getSeconds().toString().padStart(2, '0');
  return `${h}:${m}:${s}`;
}

const GameLog: React.FC<GameLogProps> = ({ className }) => {
  const [isOpen, setIsOpen] = useState(true);
  const [searchText, setSearchText] = useState('');
  const [activeFilter, setActiveFilter] = useState<ActionFilter>('all');
  const scrollContainerRef = useRef<HTMLDivElement>(null);
  const shouldAutoScroll = useRef(true);

  const chatMessages = useGameStore((s) => s.gameState?.chatMessages ?? []);

  const filteredMessages = useMemo(() => {
    let messages = chatMessages;

    if (activeFilter !== 'all') {
      messages = messages.filter((msg) => classifyMessage(msg) === activeFilter);
    }

    if (searchText.trim()) {
      const query = searchText.toLowerCase();
      messages = messages.filter(
        (msg) =>
          msg.message.toLowerCase().includes(query) ||
          msg.playerName.toLowerCase().includes(query),
      );
    }

    return messages;
  }, [chatMessages, activeFilter, searchText]);

  // Track whether user has scrolled up
  const handleScroll = useCallback(() => {
    const el = scrollContainerRef.current;
    if (!el) return;
    const isAtBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 40;
    shouldAutoScroll.current = isAtBottom;
  }, []);

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    if (shouldAutoScroll.current && scrollContainerRef.current) {
      scrollContainerRef.current.scrollTop = scrollContainerRef.current.scrollHeight;
    }
  }, [filteredMessages.length]);

  const unreadSystemCount = useMemo(() => {
    if (isOpen) return 0;
    return chatMessages.filter((m) => m.isSystem).length;
  }, [isOpen, chatMessages]);

  return (
    <div className={clsx('flex flex-col', className)}>
      {/* Toggle bar */}
      <button
        onClick={() => setIsOpen((prev) => !prev)}
        className={clsx(
          'flex items-center justify-between px-4 py-2 rounded-t-lg',
          'bg-octgn-surface/80 border border-octgn-border/40 border-b-0',
          'text-sm font-medium text-octgn-text hover:bg-octgn-surface-light/50 transition-colors',
          !isOpen && 'rounded-b-lg border-b',
        )}
      >
        <div className="flex items-center gap-2">
          <svg className="w-4 h-4 text-octgn-text-dim" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" strokeLinejoin="round">
            <path d="M2 3h12M2 6.5h8M2 10h10M2 13.5h6" />
          </svg>
          <span className="font-display text-xs tracking-wide uppercase">Game Log</span>
          {!isOpen && unreadSystemCount > 0 && (
            <span className="ml-1 px-1.5 py-0.5 text-[10px] font-bold rounded-full bg-octgn-warning/20 text-octgn-warning border border-octgn-warning/30">
              {unreadSystemCount}
            </span>
          )}
        </div>
        <svg
          className={clsx(
            'w-3.5 h-3.5 text-octgn-text-dim transition-transform duration-200',
            isOpen && 'rotate-180',
          )}
          viewBox="0 0 16 16"
          fill="none"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
        >
          <path d="M4 6l4 4 4-4" />
        </svg>
      </button>

      {/* Panel body */}
      {isOpen && (
        <GlassPanel
          variant="light"
          padding="none"
          glow="none"
          className="border border-octgn-border/40 border-t-0 rounded-t-none flex flex-col animate-fade-in"
        >
          {/* Toolbar */}
          <div className="flex items-center gap-2 px-3 py-2 border-b border-octgn-border/20">
            {/* Search */}
            <div className="relative flex-1">
              <SearchIcon className="absolute left-2.5 top-1/2 -translate-y-1/2 w-3 h-3 text-octgn-text-dim" />
              <input
                type="text"
                placeholder="Search log..."
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                className="w-full h-7 pl-7 pr-3 rounded-md bg-octgn-surface border border-octgn-border/40 text-[11px] text-octgn-text placeholder-octgn-text-dim outline-none focus:border-octgn-primary/50 transition-colors"
              />
            </div>

            {/* Filter pills */}
            <div className="flex items-center gap-1">
              {(Object.keys(filterLabels) as ActionFilter[]).map((filter) => (
                <button
                  key={filter}
                  onClick={() => setActiveFilter(filter)}
                  className={clsx(
                    'px-2 py-1 rounded-md text-[10px] font-medium transition-all duration-150',
                    activeFilter === filter
                      ? 'bg-octgn-primary/15 text-octgn-primary border border-octgn-primary/30'
                      : 'text-octgn-text-dim hover:text-octgn-text-muted hover:bg-white/5 border border-transparent',
                  )}
                >
                  {filterLabels[filter]}
                </button>
              ))}
            </div>
          </div>

          {/* Messages */}
          <div
            ref={scrollContainerRef}
            onScroll={handleScroll}
            className="flex-1 overflow-y-auto max-h-64 min-h-[8rem] scrollbar-thin scrollbar-thumb-octgn-border/40 scrollbar-track-transparent"
          >
            {filteredMessages.length === 0 ? (
              <div className="flex items-center justify-center h-full py-8 text-xs text-octgn-text-dim">
                {searchText ? 'No matching messages' : 'No messages yet'}
              </div>
            ) : (
              <div className="p-2 space-y-0.5">
                {filteredMessages.map((msg) => (
                  <LogEntry key={msg.id} message={msg} />
                ))}
              </div>
            )}
          </div>
        </GlassPanel>
      )}
    </div>
  );
};

const LogEntry: React.FC<{ message: ChatMessage }> = React.memo(({ message }) => {
  const colorClass = getMessageColor(message);
  const category = classifyMessage(message);

  return (
    <div
      className={clsx(
        'flex items-start gap-2 px-2 py-1 rounded-md text-xs hover:bg-white/[0.02] transition-colors group',
        message.isSystem && 'bg-octgn-warning/[0.03]',
      )}
    >
      {/* Timestamp */}
      <span className="text-[10px] font-mono text-octgn-text-dim shrink-0 mt-px opacity-50 group-hover:opacity-100 transition-opacity">
        {formatTimestamp(message.timestamp)}
      </span>

      {/* Category indicator */}
      <span
        className={clsx(
          'w-1.5 h-1.5 rounded-full mt-1.5 shrink-0',
          category === 'system' && 'bg-octgn-warning',
          category === 'action' && 'bg-octgn-accent',
          category === 'chat' && 'bg-octgn-primary',
        )}
      />

      {/* Content */}
      <div className="flex-1 min-w-0">
        {!message.isSystem && (
          <span
            className="font-semibold mr-1.5"
            style={message.color ? { color: message.color } : undefined}
          >
            {message.playerName}:
          </span>
        )}
        <span className={clsx(colorClass, message.isSystem && 'italic')}>
          {message.message}
        </span>
      </div>
    </div>
  );
});

LogEntry.displayName = 'LogEntry';

function SearchIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
      <circle cx="6.5" cy="6.5" r="4.5" />
      <path d="M10 10l4 4" />
    </svg>
  );
}

export default GameLog;
