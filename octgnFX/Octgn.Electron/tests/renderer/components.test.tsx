import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, cleanup } from '@testing-library/react';
import React from 'react';

// ---------------------------------------------------------------------------
// Mock window.octgn
// ---------------------------------------------------------------------------

const mockOctgn = {
  login: vi.fn(),
  logout: vi.fn(),
  getSession: vi.fn(),
  getGames: vi.fn(),
  hostGame: vi.fn(),
  joinGame: vi.fn(),
  leaveGame: vi.fn(),
  minimize: vi.fn(),
  maximize: vi.fn(),
  quit: vi.fn(),
  getVersion: vi.fn(),
  onGameStateUpdate: vi.fn(() => vi.fn()),
  gameAction: vi.fn(),
  gameChat: vi.fn(),
  loadDeck: vi.fn(),
  openFileDialog: vi.fn(),
};

Object.defineProperty(globalThis, 'window', {
  value: { ...globalThis.window, octgn: mockOctgn },
  writable: true,
});

// Import components AFTER mock setup
import Toast from '@renderer/components/Toast';
import ToastContainer from '@renderer/components/ToastContainer';
import DeckLoader from '@renderer/components/DeckLoader';
import GameLog from '@renderer/components/GameLog';
import { useToastStore } from '@renderer/stores/toast-store';
import { useGameStore } from '@renderer/stores/game-store';
import type { Toast as ToastData } from '@renderer/stores/toast-store';
import type { ChatMessage } from '@shared/types';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeToast(overrides: Partial<ToastData> = {}): ToastData {
  return {
    id: 'toast-1',
    message: 'Test message',
    type: 'info',
    duration: 4000,
    createdAt: Date.now(),
    ...overrides,
  };
}

function makeChatMessage(overrides: Partial<ChatMessage> = {}): ChatMessage {
  return {
    id: 'msg-1',
    playerId: 1,
    playerName: 'Alice',
    message: 'Hello world',
    timestamp: Date.now(),
    isSystem: false,
    ...overrides,
  };
}

// ---------------------------------------------------------------------------
// Toast Component
// ---------------------------------------------------------------------------

describe('Toast', () => {
  afterEach(() => {
    cleanup();
    useToastStore.setState({ toasts: [] });
  });

  it('should render the toast message', () => {
    const toast = makeToast({ message: 'Something happened' });
    render(<Toast toast={toast} />);

    expect(screen.getByText('Something happened')).toBeInTheDocument();
  });

  it('should render info toast', () => {
    const toast = makeToast({ type: 'info' });
    render(<Toast toast={toast} />);

    expect(screen.getByText('Test message')).toBeInTheDocument();
  });

  it('should render success toast', () => {
    const toast = makeToast({ type: 'success', message: 'Success!' });
    render(<Toast toast={toast} />);

    expect(screen.getByText('Success!')).toBeInTheDocument();
  });

  it('should render error toast', () => {
    const toast = makeToast({ type: 'error', message: 'Error occurred' });
    render(<Toast toast={toast} />);

    expect(screen.getByText('Error occurred')).toBeInTheDocument();
  });

  it('should render warning toast', () => {
    const toast = makeToast({ type: 'warning', message: 'Be careful' });
    render(<Toast toast={toast} />);

    expect(screen.getByText('Be careful')).toBeInTheDocument();
  });

  it('should have a dismiss button', () => {
    const toast = makeToast();
    render(<Toast toast={toast} />);

    const button = screen.getByRole('button');
    expect(button).toBeInTheDocument();
  });

  it('should call removeToast when dismiss button is clicked', () => {
    vi.useFakeTimers();
    const toast = makeToast({ id: 'toast-dismiss' });
    useToastStore.setState({ toasts: [toast] });

    render(<Toast toast={toast} />);
    fireEvent.click(screen.getByRole('button'));

    // The dismiss handler sets isExiting then calls removeToast after 300ms
    vi.advanceTimersByTime(300);
    expect(useToastStore.getState().toasts.find((t) => t.id === 'toast-dismiss')).toBeUndefined();

    vi.useRealTimers();
  });
});

// ---------------------------------------------------------------------------
// ToastContainer Component
// ---------------------------------------------------------------------------

describe('ToastContainer', () => {
  afterEach(() => {
    cleanup();
    useToastStore.setState({ toasts: [] });
  });

  it('should render nothing when there are no toasts', () => {
    useToastStore.setState({ toasts: [] });
    const { container } = render(<ToastContainer />);

    expect(container.innerHTML).toBe('');
  });

  it('should render multiple toasts', () => {
    useToastStore.setState({
      toasts: [
        makeToast({ id: '1', message: 'First toast' }),
        makeToast({ id: '2', message: 'Second toast' }),
        makeToast({ id: '3', message: 'Third toast' }),
      ],
    });

    render(<ToastContainer />);

    expect(screen.getByText('First toast')).toBeInTheDocument();
    expect(screen.getByText('Second toast')).toBeInTheDocument();
    expect(screen.getByText('Third toast')).toBeInTheDocument();
  });

  it('should render toasts of different types', () => {
    useToastStore.setState({
      toasts: [
        makeToast({ id: '1', type: 'success', message: 'Win' }),
        makeToast({ id: '2', type: 'error', message: 'Fail' }),
      ],
    });

    render(<ToastContainer />);

    expect(screen.getByText('Win')).toBeInTheDocument();
    expect(screen.getByText('Fail')).toBeInTheDocument();
  });
});

// ---------------------------------------------------------------------------
// DeckLoader Component
// ---------------------------------------------------------------------------

describe('DeckLoader', () => {
  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('should render a button with "Load Deck" text', () => {
    render(<DeckLoader />);

    expect(screen.getByRole('button', { name: /load deck/i })).toBeInTheDocument();
  });

  it('should call openFileDialog when clicked', async () => {
    mockOctgn.openFileDialog.mockResolvedValue(null);

    render(<DeckLoader />);
    fireEvent.click(screen.getByRole('button', { name: /load deck/i }));

    expect(mockOctgn.openFileDialog).toHaveBeenCalled();
  });
});

// ---------------------------------------------------------------------------
// GameLog Component
// ---------------------------------------------------------------------------

describe('GameLog', () => {
  afterEach(() => {
    cleanup();
  });

  it('should render the "Game Log" heading', () => {
    useGameStore.setState({
      gameState: {
        gameId: 'g1',
        gameName: 'Test',
        players: [],
        localPlayerId: 1,
        table: { cards: [] },
        turnNumber: 0,
        activePlayer: 1,
        phase: 0,
        chatMessages: [],
        isStarted: false,
      },
      isConnected: false,
      isLoading: false,
      error: null,
    });

    render(<GameLog />);

    expect(screen.getByText('Game Log')).toBeInTheDocument();
  });

  it('should show "No messages yet" when there are no chat messages', () => {
    useGameStore.setState({
      gameState: {
        gameId: 'g1',
        gameName: 'Test',
        players: [],
        localPlayerId: 1,
        table: { cards: [] },
        turnNumber: 0,
        activePlayer: 1,
        phase: 0,
        chatMessages: [],
        isStarted: false,
      },
      isConnected: true,
      isLoading: false,
      error: null,
    });

    render(<GameLog />);

    expect(screen.getByText('No messages yet')).toBeInTheDocument();
  });

  it('should render chat message entries', () => {
    useGameStore.setState({
      gameState: {
        gameId: 'g1',
        gameName: 'Test',
        players: [],
        localPlayerId: 1,
        table: { cards: [] },
        turnNumber: 0,
        activePlayer: 1,
        phase: 0,
        chatMessages: [
          makeChatMessage({ id: 'msg-1', playerName: 'Alice', message: 'Hello everyone' }),
          makeChatMessage({ id: 'msg-2', playerName: 'Bob', message: 'Hi Alice' }),
        ],
        isStarted: true,
      },
      isConnected: true,
      isLoading: false,
      error: null,
    });

    render(<GameLog />);

    expect(screen.getByText('Hello everyone')).toBeInTheDocument();
    expect(screen.getByText('Hi Alice')).toBeInTheDocument();
    expect(screen.getByText('Alice:')).toBeInTheDocument();
    expect(screen.getByText('Bob:')).toBeInTheDocument();
  });

  it('should render system messages', () => {
    useGameStore.setState({
      gameState: {
        gameId: 'g1',
        gameName: 'Test',
        players: [],
        localPlayerId: 1,
        table: { cards: [] },
        turnNumber: 0,
        activePlayer: 1,
        phase: 0,
        chatMessages: [
          makeChatMessage({ id: 'sys-1', isSystem: true, message: 'Game started' }),
        ],
        isStarted: true,
      },
      isConnected: true,
      isLoading: false,
      error: null,
    });

    render(<GameLog />);

    expect(screen.getByText('Game started')).toBeInTheDocument();
  });

  it('should toggle log panel visibility when header is clicked', () => {
    useGameStore.setState({
      gameState: {
        gameId: 'g1',
        gameName: 'Test',
        players: [],
        localPlayerId: 1,
        table: { cards: [] },
        turnNumber: 0,
        activePlayer: 1,
        phase: 0,
        chatMessages: [],
        isStarted: false,
      },
      isConnected: true,
      isLoading: false,
      error: null,
    });

    render(<GameLog />);

    // Panel starts open - "No messages yet" is visible
    expect(screen.getByText('No messages yet')).toBeInTheDocument();

    // Click the toggle button to close
    fireEvent.click(screen.getByText('Game Log'));

    // "No messages yet" should no longer be visible
    expect(screen.queryByText('No messages yet')).not.toBeInTheDocument();

    // Click again to re-open
    fireEvent.click(screen.getByText('Game Log'));
    expect(screen.getByText('No messages yet')).toBeInTheDocument();
  });
});
