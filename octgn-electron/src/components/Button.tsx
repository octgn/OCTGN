import { ButtonHTMLAttributes, forwardRef } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost' | 'success';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  icon?: string;
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ variant = 'secondary', size = 'md', loading, icon, children, className = '', disabled, ...props }, ref) => {
    const variantClasses = {
      primary: 'btn-primary',
      secondary: 'btn-secondary',
      danger: 'btn-danger',
      ghost: 'btn-ghost',
      success: 'bg-gradient-to-br from-octgn-success to-emerald-600 border-octgn-success/50 hover:from-green-500 hover:to-green-600 text-white font-semibold',
    };

    const sizeClasses = {
      sm: 'btn-sm',
      md: '',
      lg: 'btn-lg',
    };

    return (
      <button
        ref={ref}
        className={`
          btn ${variantClasses[variant]} ${sizeClasses[size]}
          ${disabled || loading ? 'opacity-50 cursor-not-allowed' : ''}
          ${className}
        `}
        disabled={disabled || loading}
        {...props}
      >
        {loading ? (
          <span className="flex items-center justify-center">
            <svg className="animate-spin -ml-1 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Loading...
          </span>
        ) : (
          <>
            {icon && <span className="mr-2">{icon}</span>}
            {children}
          </>
        )}
      </button>
    );
  }
);

Button.displayName = 'Button';

export default Button;

// Icon button for toolbars
export function IconButton({ 
  icon, 
  onClick, 
  title, 
  active = false,
  danger = false,
}: { 
  icon: string; 
  onClick?: () => void; 
  title?: string;
  active?: boolean;
  danger?: boolean;
}) {
  return (
    <button
      onClick={onClick}
      title={title}
      className={`
        w-8 h-8 rounded-lg flex items-center justify-center
        transition-all duration-200
        ${active 
          ? 'bg-octgn-highlight/20 text-octgn-highlight' 
          : danger
            ? 'text-gray-400 hover:text-red-400 hover:bg-red-500/10'
            : 'text-gray-400 hover:text-white hover:bg-white/10'
        }
      `}
    >
      <span className="text-lg">{icon}</span>
    </button>
  );
}

// Button group for toolbars
export function ButtonGroup({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex rounded-lg overflow-hidden border border-octgn-accent">
      {children}
    </div>
  );
}

export function ButtonGroupItem({ 
  active, 
  onClick, 
  children 
}: { 
  active?: boolean; 
  onClick?: () => void; 
  children: React.ReactNode;
}) {
  return (
    <button
      onClick={onClick}
      className={`
        px-3 py-1.5 text-sm font-medium
        border-r border-octgn-accent last:border-r-0
        transition-colors duration-200
        ${active 
          ? 'bg-octgn-highlight text-white' 
          : 'bg-octgn-primary text-gray-300 hover:bg-octgn-accent hover:text-white'
        }
      `}
    >
      {children}
    </button>
  );
}
