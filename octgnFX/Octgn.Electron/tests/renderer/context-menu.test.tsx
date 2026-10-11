import { describe, it, expect, vi, afterEach, beforeEach } from 'vitest';
import { render, screen, fireEvent, cleanup, act } from '@testing-library/react';
import React from 'react';
import ContextMenu from '../../src/renderer/components/ContextMenu';
import type { ContextMenuItemDef } from '../../src/renderer/components/ContextMenu';

afterEach(cleanup);

describe('ContextMenu', () => {
  it('fires onClick when an action item is clicked', () => {
    const onClick = vi.fn();
    const onClose = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Flip Card', onClick },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    const button = screen.getByText('Flip Card');
    fireEvent.click(button);

    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it('calls onClose after onClick fires', () => {
    const callOrder: string[] = [];
    const onClick = vi.fn(() => callOrder.push('onClick'));
    const onClose = vi.fn(() => callOrder.push('onClose'));
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Peek', onClick },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    fireEvent.click(screen.getByText('Peek'));

    expect(onClick).toHaveBeenCalledTimes(1);
    expect(onClose).toHaveBeenCalledTimes(1);
    expect(callOrder).toEqual(['onClick', 'onClose']);
  });

  it('does not fire onClick for disabled items', () => {
    const onClick = vi.fn();
    const onClose = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Disabled Action', onClick, disabled: true },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    const button = screen.getByText('Disabled Action');
    fireEvent.click(button);

    expect(onClick).not.toHaveBeenCalled();
    expect(onClose).not.toHaveBeenCalled();
  });

  it('renders header items', () => {
    const items: ContextMenuItemDef[] = [
      { type: 'header', label: 'Card Name' },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={vi.fn()} />);

    expect(screen.getByText('Card Name')).toBeTruthy();
  });

  it('renders submenu items', () => {
    const onClick = vi.fn();
    const items: ContextMenuItemDef[] = [
      {
        type: 'submenu',
        label: 'Rotate',
        children: [
          { type: 'action', label: '90°', onClick },
        ],
      },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={vi.fn()} />);

    expect(screen.getByText('Rotate')).toBeTruthy();
  });

  it('renders separator without crashing', () => {
    const onClick = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Item A', onClick },
      { type: 'separator' },
      { type: 'action', label: 'Item B', onClick },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={vi.fn()} />);

    expect(screen.getByText('Item A')).toBeTruthy();
    expect(screen.getByText('Item B')).toBeTruthy();
  });

  it('renders shortcut text', () => {
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Kill', shortcut: 'Del', onClick: vi.fn() },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={vi.fn()} />);

    expect(screen.getByText('Kill')).toBeTruthy();
    expect(screen.getByText('Del')).toBeTruthy();
  });

  it('returns null when items is empty', () => {
    const { container } = render(
      <ContextMenu x={100} y={100} items={[]} onClose={vi.fn()} />
    );

    // Portal renders nothing when items is empty
    expect(container.innerHTML).toBe('');
  });

  it('does not fire onClick when button has disabled attribute set', () => {
    const onClick = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Test', onClick, disabled: true },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={vi.fn()} />);

    const button = screen.getByText('Test').closest('button')!;
    expect(button.disabled).toBe(true);
  });

  it('closes menu when backdrop is clicked (outside click)', () => {
    const onClose = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Flip', onClick: vi.fn() },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    // The backdrop is a fixed inset-0 div rendered before the menu panel
    const backdrop = document.querySelector('.fixed.inset-0');
    expect(backdrop).toBeTruthy();
    fireEvent.mouseDown(backdrop!);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('fires onClick with full mousedown→mouseup→click sequence', () => {
    const onClick = vi.fn();
    const onClose = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Flip Card', onClick },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    const button = screen.getByText('Flip Card');
    fireEvent.mouseDown(button);
    fireEvent.mouseUp(button);
    fireEvent.click(button);

    expect(onClick).toHaveBeenCalledTimes(1);
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('closes on Escape key', () => {
    const onClose = vi.fn();
    const items: ContextMenuItemDef[] = [
      { type: 'action', label: 'Test', onClick: vi.fn() },
    ];

    render(<ContextMenu x={100} y={100} items={items} onClose={onClose} />);

    fireEvent.keyDown(document, { key: 'Escape' });

    expect(onClose).toHaveBeenCalledTimes(1);
  });
});
