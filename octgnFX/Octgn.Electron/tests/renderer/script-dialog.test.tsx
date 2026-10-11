import { describe, it, expect, vi, afterEach, beforeEach } from 'vitest';
import { render, screen, fireEvent, cleanup, waitFor } from '@testing-library/react';
import React from 'react';

// Mock window.octgn
beforeEach(() => {
  (globalThis as any).window = {
    ...(globalThis as any).window,
    octgn: {
      sendDialogResponse: vi.fn(),
      onDialogRequest: vi.fn(() => () => {}),
    },
  };
});

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});

// We'll import the component after creating it
// For now, define the props interface we expect
interface DialogRequest {
  requestId: string;
  type: string;
  params: Record<string, unknown>;
}

interface ScriptDialogProps {
  request: DialogRequest;
  onRespond: (requestId: string, result: unknown) => void;
}

describe('ScriptDialog', () => {
  let ScriptDialog: React.FC<ScriptDialogProps>;

  beforeEach(async () => {
    const mod = await import('../../src/renderer/components/ScriptDialog');
    ScriptDialog = mod.default;
  });

  describe('confirm dialog', () => {
    it('renders question text and Yes/No buttons', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r1', type: 'confirm', params: { message: 'Delete this card?' } }}
          onRespond={onRespond}
        />
      );

      expect(screen.getByText('Delete this card?')).toBeTruthy();
      expect(screen.getByText('Yes')).toBeTruthy();
      expect(screen.getByText('No')).toBeTruthy();
    });

    it('calls onRespond with true when Yes is clicked', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r1', type: 'confirm', params: { message: 'Sure?' } }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Yes'));
      expect(onRespond).toHaveBeenCalledWith('r1', true);
    });

    it('calls onRespond with false when No is clicked', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r1', type: 'confirm', params: { message: 'Sure?' } }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('No'));
      expect(onRespond).toHaveBeenCalledWith('r1', false);
    });
  });

  describe('askInteger dialog', () => {
    it('renders question and input with default value', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r2', type: 'askInteger', params: { question: 'How many cards?', defaultAnswer: 5 } }}
          onRespond={onRespond}
        />
      );

      expect(screen.getByText('How many cards?')).toBeTruthy();
      const input = screen.getByRole('spinbutton') as HTMLInputElement;
      expect(input.value).toBe('5');
    });

    it('calls onRespond with entered number on OK', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r2', type: 'askInteger', params: { question: 'How many?', defaultAnswer: 5 } }}
          onRespond={onRespond}
        />
      );

      const input = screen.getByRole('spinbutton') as HTMLInputElement;
      fireEvent.change(input, { target: { value: '10' } });
      fireEvent.click(screen.getByText('OK'));

      expect(onRespond).toHaveBeenCalledWith('r2', 10);
    });

    it('calls onRespond with null on Cancel', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r2', type: 'askInteger', params: { question: 'How many?', defaultAnswer: 5 } }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Cancel'));
      expect(onRespond).toHaveBeenCalledWith('r2', null);
    });

    it('does not accept negative numbers', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r2', type: 'askInteger', params: { question: 'How many?', defaultAnswer: 5 } }}
          onRespond={onRespond}
        />
      );

      const input = screen.getByRole('spinbutton') as HTMLInputElement;
      fireEvent.change(input, { target: { value: '-3' } });
      fireEvent.click(screen.getByText('OK'));

      // Should not respond with negative - either prevent or clamp to 0
      if (onRespond.mock.calls.length > 0) {
        const [, result] = onRespond.mock.calls[0];
        expect(result).toBeGreaterThanOrEqual(0);
      }
    });

    it('submits on Enter key', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r2', type: 'askInteger', params: { question: 'How many?', defaultAnswer: 3 } }}
          onRespond={onRespond}
        />
      );

      const input = screen.getByRole('spinbutton');
      fireEvent.keyDown(input, { key: 'Enter' });

      expect(onRespond).toHaveBeenCalledWith('r2', 3);
    });
  });

  describe('askString dialog', () => {
    it('renders question and text input with default value', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r3', type: 'askString', params: { question: 'Enter name:', defaultAnswer: 'Bob' } }}
          onRespond={onRespond}
        />
      );

      expect(screen.getByText('Enter name:')).toBeTruthy();
      const input = screen.getByRole('textbox') as HTMLInputElement;
      expect(input.value).toBe('Bob');
    });

    it('calls onRespond with entered text on OK', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r3', type: 'askString', params: { question: 'Name?', defaultAnswer: 'Bob' } }}
          onRespond={onRespond}
        />
      );

      const input = screen.getByRole('textbox');
      fireEvent.change(input, { target: { value: 'Alice' } });
      fireEvent.click(screen.getByText('OK'));

      expect(onRespond).toHaveBeenCalledWith('r3', 'Alice');
    });

    it('calls onRespond with null on Cancel', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r3', type: 'askString', params: { question: 'Name?', defaultAnswer: 'Bob' } }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Cancel'));
      expect(onRespond).toHaveBeenCalledWith('r3', null);
    });
  });

  describe('askChoice dialog', () => {
    it('renders question and choice list', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{
            requestId: 'r4', type: 'askChoice',
            params: { question: 'Pick one:', choices: ['Red', 'Blue', 'Green'], colors: [], buttons: [] }
          }}
          onRespond={onRespond}
        />
      );

      expect(screen.getByText('Pick one:')).toBeTruthy();
      expect(screen.getByText('Red')).toBeTruthy();
      expect(screen.getByText('Blue')).toBeTruthy();
      expect(screen.getByText('Green')).toBeTruthy();
    });

    it('calls onRespond with selected index when choice is clicked', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{
            requestId: 'r4', type: 'askChoice',
            params: { question: 'Pick:', choices: ['A', 'B', 'C'], colors: [], buttons: [] }
          }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('B'));
      expect(onRespond).toHaveBeenCalledWith('r4', 1);
    });

    it('calls onRespond with -1 on Cancel', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{
            requestId: 'r4', type: 'askChoice',
            params: { question: 'Pick:', choices: ['A'], colors: [], buttons: [] }
          }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Cancel'));
      expect(onRespond).toHaveBeenCalledWith('r4', -1);
    });
  });

  describe('askMarker dialog', () => {
    it('renders marker picker title', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r5', type: 'askMarker', params: {} }}
          onRespond={onRespond}
        />
      );

      // Should show some kind of marker picker UI
      expect(screen.getByText('Select a marker')).toBeTruthy();
    });

    it('calls onRespond with null on Cancel', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r5', type: 'askMarker', params: {} }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Cancel'));
      expect(onRespond).toHaveBeenCalledWith('r5', null);
    });
  });

  describe('askCard dialog', () => {
    it('renders card search title', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r6', type: 'askCard', params: { properties: {}, operator: null, title: 'Select a Card' } }}
          onRespond={onRespond}
        />
      );

      expect(screen.getByText('Select a Card')).toBeTruthy();
    });

    it('calls onRespond with null on Cancel', () => {
      const onRespond = vi.fn();
      render(
        <ScriptDialog
          request={{ requestId: 'r6', type: 'askCard', params: { properties: {}, operator: null, title: 'Pick' } }}
          onRespond={onRespond}
        />
      );

      fireEvent.click(screen.getByText('Cancel'));
      expect(onRespond).toHaveBeenCalledWith('r6', null);
    });
  });

  describe('modal overlay', () => {
    it('renders with a backdrop overlay', () => {
      const onRespond = vi.fn();
      const { container } = render(
        <ScriptDialog
          request={{ requestId: 'r1', type: 'confirm', params: { message: 'Test' } }}
          onRespond={onRespond}
        />
      );

      // Should have an overlay/backdrop element
      const overlay = container.querySelector('[data-testid="dialog-overlay"]');
      expect(overlay).toBeTruthy();
    });
  });
});
