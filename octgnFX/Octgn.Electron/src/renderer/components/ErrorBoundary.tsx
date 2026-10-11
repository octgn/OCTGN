import React from 'react';
import Button from './Button';

interface Props {
  children: React.ReactNode;
  onNavigate?: (page: string) => void;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Page crash:', error, errorInfo);
  }

  handleRecover = () => {
    this.setState({ hasError: false, error: null });
    this.props.onNavigate?.('lobby');
  };

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex flex-col items-center justify-center h-full gap-4 p-8">
          <div className="text-octgn-error text-lg font-display tracking-wide">
            Something went wrong
          </div>
          <p className="text-sm text-octgn-text-muted text-center max-w-md">
            This page encountered an unexpected error. You can try returning to the lobby or restarting the application.
          </p>
          <code className="text-xs text-octgn-text-dim bg-octgn-surface rounded px-3 py-2 max-w-lg overflow-auto">
            {this.state.error?.message}
          </code>
          <div className="flex gap-3 mt-2">
            <Button variant="primary" onClick={this.handleRecover}>
              Return to Lobby
            </Button>
            <Button variant="ghost" onClick={() => this.setState({ hasError: false, error: null })}>
              Try Again
            </Button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
