import { useState } from 'react';

interface ErrorBoundaryState {
  hasError: boolean;
  error?: Error;
}

interface ErrorBoundaryProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="min-h-screen bg-octgn-dark flex items-center justify-center p-4">
          <div className="max-w-md w-full bg-octgn-primary rounded-lg shadow-xl p-6">
            <div className="text-center">
              <div className="text-6xl mb-4">💥</div>
              <h1 className="text-2xl font-bold text-white mb-2">Something went wrong</h1>
              <p className="text-gray-400 mb-4">
                An unexpected error occurred. Please try restarting the application.
              </p>
              
              <details className="text-left mb-4">
                <summary className="text-sm text-gray-500 cursor-pointer hover:text-gray-400">
                  Error details
                </summary>
                <pre className="mt-2 p-3 bg-octgn-dark rounded text-xs text-red-400 overflow-auto">
                  {this.state.error?.message}
                  {'\n\n'}
                  {this.state.error?.stack}
                </pre>
              </details>

              <button
                onClick={() => window.location.reload()}
                className="btn btn-primary w-full"
              >
                Reload Application
              </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

// Need React for class component
import React from 'react';

export function useErrorHandler() {
  const [error, setError] = useState<Error | null>(null);

  const handleError = (error: Error) => {
    console.error('Error caught:', error);
    setError(error);
  };

  const clearError = () => {
    setError(null);
  };

  const retry = (fn: () => void) => {
    clearError();
    try {
      fn();
    } catch (e) {
      handleError(e instanceof Error ? e : new Error(String(e)));
    }
  };

  return { error, handleError, clearError, retry };
}

export function ErrorMessage({
  error,
  onRetry,
  onDismiss,
}: {
  error: Error | string;
  onRetry?: () => void;
  onDismiss?: () => void;
}) {
  const message = typeof error === 'string' ? error : error.message;

  return (
    <div className="bg-red-900/20 border border-red-500 rounded-lg p-4">
      <div className="flex items-start space-x-3">
        <span className="text-2xl">⚠️</span>
        <div className="flex-1">
          <h3 className="text-red-400 font-medium">Error</h3>
          <p className="text-gray-300 text-sm mt-1">{message}</p>
        </div>
      </div>
      
      <div className="flex space-x-2 mt-3">
        {onRetry && (
          <button onClick={onRetry} className="btn btn-secondary text-sm">
            Retry
          </button>
        )}
        {onDismiss && (
          <button onClick={onDismiss} className="btn btn-ghost text-sm">
            Dismiss
          </button>
        )}
      </div>
    </div>
  );
}
