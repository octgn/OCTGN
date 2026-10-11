import React from 'react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

interface GlassPanelProps {
  children: React.ReactNode;
  className?: string;
  variant?: 'default' | 'heavy' | 'light';
  glow?: 'none' | 'blue' | 'violet' | 'subtle';
  padding?: 'none' | 'sm' | 'md' | 'lg';
  as?: React.ElementType;
}

const variantClass: Record<string, string> = {
  default: 'glass',
  heavy: 'glass-heavy',
  light: 'glass-light',
};

const glowClass: Record<string, string> = {
  none: '',
  blue: 'glow-blue',
  violet: 'glow-violet',
  subtle: 'shadow-[0_0_30px_rgba(59,130,246,0.08)]',
};

const paddingClass: Record<string, string> = {
  none: '',
  sm: 'p-3',
  md: 'p-5',
  lg: 'p-8',
};

const GlassPanel: React.FC<GlassPanelProps> = ({
  children,
  className,
  variant = 'default',
  glow = 'subtle',
  padding = 'md',
  as: Tag = 'div',
}) => {
  return (
    <Tag
      className={twMerge(
        clsx(
          'rounded-xl',
          variantClass[variant],
          glowClass[glow],
          paddingClass[padding]
        ),
        className
      )}
    >
      {children}
    </Tag>
  );
};

export default GlassPanel;
