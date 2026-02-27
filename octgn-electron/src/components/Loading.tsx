import { useEffect, useState } from 'react';

interface LoadingScreenProps {
  message?: string;
  progress?: number;
}

export default function LoadingScreen({ message = 'Loading...', progress }: LoadingScreenProps) {
  const [dots, setDots] = useState('');

  useEffect(() => {
    const interval = setInterval(() => {
      setDots((d) => (d.length >= 3 ? '' : d + '.'));
    }, 500);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="fixed inset-0 bg-octgn-dark flex items-center justify-center">
      <div className="text-center">
        {/* Logo */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-octgn-highlight">OCTGN</h1>
          <p className="text-sm text-gray-400 mt-1">Electron Edition</p>
        </div>

        {/* Spinner */}
        <div className="relative w-16 h-16 mx-auto mb-6">
          <div className="absolute inset-0 border-4 border-octgn-accent rounded-full"></div>
          <div className="absolute inset-0 border-4 border-transparent border-t-octgn-highlight rounded-full animate-spin"></div>
        </div>

        {/* Message */}
        <p className="text-gray-300 text-lg">
          {message}
          <span className="inline-block w-6 text-left">{dots}</span>
        </p>

        {/* Progress bar */}
        {progress !== undefined && (
          <div className="mt-4 w-64 mx-auto">
            <div className="h-2 bg-octgn-accent rounded-full overflow-hidden">
              <div
                className="h-full bg-octgn-highlight transition-all duration-300"
                style={{ width: `${Math.min(100, Math.max(0, progress))}%` }}
              />
            </div>
            <p className="text-sm text-gray-400 mt-2">{Math.round(progress)}%</p>
          </div>
        )}
      </div>
    </div>
  );
}

interface LoadingOverlayProps {
  isLoading: boolean;
  message?: string;
  children: React.ReactNode;
}

export function LoadingOverlay({ isLoading, message, children }: LoadingOverlayProps) {
  if (!isLoading) {
    return <>{children}</>;
  }

  return (
    <div className="relative">
      {children}
      <div className="absolute inset-0 bg-octgn-dark/80 flex items-center justify-center backdrop-blur-sm">
        <div className="text-center">
          <div className="w-8 h-8 border-2 border-octgn-highlight border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
          <p className="text-gray-300 text-sm">{message || 'Loading...'}</p>
        </div>
      </div>
    </div>
  );
}

export function LoadingSpinner({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) {
  const sizeClasses = {
    sm: 'w-4 h-4 border-2',
    md: 'w-8 h-8 border-2',
    lg: 'w-12 h-12 border-3',
  };

  return (
    <div
      className={`${sizeClasses[size]} border-octgn-highlight border-t-transparent rounded-full animate-spin`}
    />
  );
}

export function SkeletonCard() {
  return (
    <div className="animate-pulse">
      <div className="bg-octgn-accent rounded-lg" style={{ width: 200, height: 280 }}>
        <div className="p-4 space-y-3">
          <div className="h-4 bg-octgn-primary rounded w-3/4"></div>
          <div className="h-3 bg-octgn-primary rounded w-1/2"></div>
          <div className="h-3 bg-octgn-primary rounded w-2/3"></div>
        </div>
      </div>
    </div>
  );
}

export function SkeletonText({ lines = 3 }: { lines?: number }) {
  return (
    <div className="animate-pulse space-y-2">
      {Array.from({ length: lines }).map((_, i) => (
        <div
          key={i}
          className="h-4 bg-octgn-accent rounded"
          style={{ width: `${100 - (i * 10)}%` }}
        />
      ))}
    </div>
  );
}
