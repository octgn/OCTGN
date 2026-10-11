import React from 'react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

type ButtonVariant = 'primary' | 'accent' | 'danger' | 'ghost';
type ButtonSize = 'sm' | 'md' | 'lg';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  icon?: React.ReactNode;
}

const variantStyles: Record<ButtonVariant, string> = {
  primary: clsx(
    'bg-octgn-primary/90 text-white border border-octgn-primary/40',
    'hover:bg-octgn-primary hover:shadow-[0_0_20px_rgba(59,130,246,0.45)] hover:border-octgn-primary/70',
    'active:bg-octgn-primary-hover active:scale-[0.97]',
    'focus-visible:shadow-[0_0_20px_rgba(59,130,246,0.5)]'
  ),
  accent: clsx(
    'bg-octgn-accent/90 text-white border border-octgn-accent/40',
    'hover:bg-octgn-accent hover:shadow-[0_0_20px_rgba(139,92,246,0.45)] hover:border-octgn-accent/70',
    'active:bg-octgn-accent-hover active:scale-[0.97]',
    'focus-visible:shadow-[0_0_20px_rgba(139,92,246,0.5)]'
  ),
  danger: clsx(
    'bg-octgn-danger/90 text-white border border-octgn-danger/40',
    'hover:bg-octgn-danger hover:shadow-[0_0_20px_rgba(239,68,68,0.4)] hover:border-octgn-danger/70',
    'active:bg-red-700 active:scale-[0.97]'
  ),
  ghost: clsx(
    'bg-transparent text-octgn-text-muted border border-transparent',
    'hover:bg-white/5 hover:text-octgn-text hover:border-octgn-border/50',
    'active:bg-white/10 active:scale-[0.97]'
  ),
};

const sizeStyles: Record<ButtonSize, string> = {
  sm: 'h-8 px-3 text-xs gap-1.5 rounded-md',
  md: 'h-10 px-5 text-sm gap-2 rounded-lg',
  lg: 'h-12 px-7 text-base gap-2.5 rounded-lg',
};

const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  children,
  className,
  disabled,
  ...props
}) => {
  return (
    <button
      className={twMerge(
        clsx(
          'relative inline-flex items-center justify-center font-medium tracking-wide whitespace-nowrap',
          'transition-all duration-200 ease-out',
          'disabled:opacity-40 disabled:pointer-events-none disabled:shadow-none',
          variantStyles[variant],
          sizeStyles[size],
          loading && 'pointer-events-none'
        ),
        className
      )}
      disabled={disabled || loading}
      {...props}
    >
      {loading && (
        <svg className="animate-spin h-4 w-4 shrink-0" viewBox="0 0 24 24" fill="none">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v3a5 5 0 00-5 5H4z" />
        </svg>
      )}
      {icon && !loading && <span className="shrink-0">{icon}</span>}
      {children && <span>{children}</span>}
    </button>
  );
};

export default Button;
