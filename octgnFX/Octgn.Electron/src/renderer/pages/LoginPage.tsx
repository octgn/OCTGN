import React, { useState, useCallback, useEffect, useRef, FormEvent } from 'react';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import Input from '../components/Input';
import LoginTransition from '../components/LoginTransition';
import { useAuthStore } from '../stores/auth-store';
import { useAppStore } from '../stores/app-store';

/** Clipboard copy icon (inline SVG) */
const CopyIcon: React.FC<{ className?: string }> = ({ className }) => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth={2}
    strokeLinecap="round"
    strokeLinejoin="round"
    className={className ?? 'w-4 h-4'}
  >
    <rect x="9" y="9" width="13" height="13" rx="2" ry="2" />
    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
  </svg>
);

const LoginPage: React.FC = () => {
  const {
    login,
    isLoading,
    error,
    user,
    rememberMe,
    setRememberMe,
    loadSavedCredentials,
    copyError,
  } = useAuthStore();

  const navigate = useAppStore((s) => s.navigate);

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [copied, setCopied] = useState(false);
  const [credentialsLoaded, setCredentialsLoaded] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);

  // Load saved credentials on mount
  useEffect(() => {
    loadSavedCredentials().then(() => setCredentialsLoaded(true));
  }, [loadSavedCredentials]);

  // Pre-fill from saved credentials
  useEffect(() => {
    if (credentialsLoaded) {
      const state = useAuthStore.getState();
      if (state.savedUsername) setUsername(state.savedUsername);
      if (state.savedPassword) setPassword(state.savedPassword);
    }
  }, [credentialsLoaded]);

  const handleTransitionComplete = useCallback(() => {
    navigate('lobby');
  }, [navigate]);

  const handleSubmit = useCallback(
    async (e: FormEvent) => {
      e.preventDefault();
      if (!username.trim() || !password) return;
      await login(username.trim(), password, rememberMe);
    },
    [username, password, rememberMe, login],
  );

  const handleCopyError = useCallback(() => {
    copyError();
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }, [copyError]);

  const openExternal = useCallback((url: string) => {
    window.open(url, '_blank');
  }, []);

  return (
    <div className="relative flex items-center justify-center h-full mesh-gradient">
      {/* Ambient background particles (CSS only) */}
      <div className="absolute inset-0 animated-bg pointer-events-none" />

      {/* Extra decorative orbs */}
      <div className="absolute top-1/4 left-1/3 w-72 h-72 rounded-full bg-octgn-primary/5 blur-[120px] pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-56 h-56 rounded-full bg-octgn-accent/5 blur-[100px] pointer-events-none" />

      {/* Login form */}
      <div
        className={`relative z-10 w-full max-w-sm mx-4 ${
          isLoading || user ? 'login-form-exit' : 'animate-in'
        }`}
      >
        <GlassPanel variant="heavy" padding="lg" glow="subtle" className="rounded-2xl">
          {/* Logo */}
          <div className="text-center mb-8">
            <h1 className="font-display text-3xl font-bold tracking-wider text-octgn-primary glow-text-blue mb-1">
              OCTGN
            </h1>
            <p className="text-sm text-octgn-text-dim tracking-wide">
              Online Card and Tabletop Gaming Network
            </p>
          </div>

          {/* Login form */}
          <form ref={formRef} onSubmit={handleSubmit} className="space-y-4">
            <Input
              label="Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              autoFocus={!username}
              data-testid="login-username"
            />
            <Input
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              autoFocus={Boolean(username && !password)}
              data-testid="login-password"
            />

            {/* Remember me */}
            <label className="flex items-center gap-2 cursor-pointer select-none group">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="w-4 h-4 rounded border-octgn-border bg-octgn-surface text-octgn-primary
                           focus:ring-octgn-primary/50 focus:ring-offset-0 focus:ring-2
                           cursor-pointer accent-octgn-primary"
                data-testid="login-remember"
              />
              <span className="text-sm text-octgn-text-muted group-hover:text-octgn-text transition-colors">
                Remember me
              </span>
            </label>

            {/* Error display with copy button */}
            {error && (
              <div
                className="rounded-lg bg-octgn-danger/10 border border-octgn-danger/30 px-4 py-2.5 flex items-start gap-2 animate-fade-in"
                data-testid="login-error"
              >
                <p className="text-sm text-octgn-danger flex-1">{error}</p>
                <button
                  type="button"
                  onClick={handleCopyError}
                  className="shrink-0 p-1 rounded hover:bg-octgn-danger/20 text-octgn-danger/70 hover:text-octgn-danger transition-colors"
                  title="Copy error to clipboard"
                  data-testid="login-copy-error"
                >
                  {copied ? (
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 24 24"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth={2}
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      className="w-4 h-4"
                    >
                      <path d="M20 6 9 17l-5-5" />
                    </svg>
                  ) : (
                    <CopyIcon />
                  )}
                </button>
              </div>
            )}

            <Button
              type="submit"
              variant="primary"
              size="lg"
              loading={isLoading}
              disabled={!username.trim() || !password}
              className="w-full mt-2"
              data-testid="login-submit"
            >
              Sign In
            </Button>
          </form>

          {/* Links */}
          <div className="mt-6 flex items-center justify-between text-xs">
            <button
              type="button"
              onClick={() => openExternal('https://www.octgn.net/Register')}
              className="text-octgn-text-muted hover:text-octgn-primary transition-colors"
            >
              Create Account
            </button>
            <button
              type="button"
              onClick={() => openExternal('https://www.octgn.net/ForgotPassword')}
              className="text-octgn-text-muted hover:text-octgn-primary transition-colors"
            >
              Forgot Password
            </button>
          </div>
        </GlassPanel>

        {/* Version */}
        <p className="text-center text-[10px] text-octgn-text-dim/50 mt-4 tracking-widest uppercase">
          OCTGN Electron Client
        </p>
      </div>

      {/* Full-screen login transition overlay */}
      <LoginTransition
        isLoading={isLoading}
        isAuthenticated={!!user}
        hasError={!!error}
        onComplete={handleTransitionComplete}
      />
    </div>
  );
};

export default LoginPage;
