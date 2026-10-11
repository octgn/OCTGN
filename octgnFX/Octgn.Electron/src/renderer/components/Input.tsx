import React, { useId, useState } from 'react';
import { clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

interface InputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'size'> {
  label: string;
  error?: string;
  size?: 'sm' | 'md' | 'lg';
}

const sizeMap = {
  sm: 'h-9 text-xs pt-4 pb-1 px-3',
  md: 'h-11 text-sm pt-5 pb-1 px-4',
  lg: 'h-13 text-base pt-6 pb-1 px-4',
};

const labelSizeMap = {
  sm: 'text-[10px] top-1.5 left-3',
  md: 'text-[11px] top-1.5 left-4',
  lg: 'text-xs top-2 left-4',
};

const Input: React.FC<InputProps> = ({
  label,
  error,
  size = 'md',
  className,
  ...props
}) => {
  const id = useId();
  const [focused, setFocused] = useState(false);
  const hasValue = Boolean(props.value || props.defaultValue);
  const isActive = focused || hasValue;

  return (
    <div className="w-full">
      <div className="relative group">
        <input
          id={id}
          className={twMerge(
            clsx(
              'peer w-full rounded-lg bg-octgn-surface border font-normal',
              'outline-none transition-all duration-200',
              sizeMap[size],
              error
                ? 'border-octgn-danger/60 focus:border-octgn-danger focus:shadow-[0_0_12px_rgba(239,68,68,0.25)]'
                : 'border-octgn-border/60 focus:border-octgn-primary/70 focus:shadow-[0_0_12px_rgba(59,130,246,0.2)]',
              'text-octgn-text placeholder-transparent'
            ),
            className
          )}
          placeholder={label}
          onFocus={(e) => {
            setFocused(true);
            props.onFocus?.(e);
          }}
          onBlur={(e) => {
            setFocused(false);
            props.onBlur?.(e);
          }}
          {...props}
        />
        <label
          htmlFor={id}
          className={clsx(
            'absolute pointer-events-none transition-all duration-200',
            labelSizeMap[size],
            isActive
              ? 'opacity-100 translate-y-0 text-octgn-text-dim'
              : 'opacity-0 translate-y-1 text-octgn-text-dim',
            focused && !error && 'text-octgn-primary',
            error && 'text-octgn-danger'
          )}
        >
          {label}
        </label>
      </div>
      {error && (
        <p className="mt-1.5 text-xs text-octgn-danger pl-1 animate-fade-in">{error}</p>
      )}
    </div>
  );
};

export default Input;
