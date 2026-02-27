import React, { useState, useCallback, FormEvent } from 'react';
import GlassPanel from '../components/GlassPanel';
import Button from '../components/Button';
import Input from '../components/Input';
import { useAuthStore } from '../stores/auth-store';

const LoginPage: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login, isLoading, error } = useAuthStore();

  const handleSubmit = useCallback(
    async (e: FormEvent) => {
      e.preventDefault();
      if (!username.trim() || !password) return;
      await login(username.trim(), password);
    },
    [username, password, login]
  );

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

      <div className="relative z-10 w-full max-w-sm mx-4 animate-in">
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
          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              label="Username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              autoFocus
            />
            <Input
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />

            {error && (
              <div className="rounded-lg bg-octgn-danger/10 border border-octgn-danger/30 px-4 py-2.5 text-sm text-octgn-danger animate-fade-in">
                {error}
              </div>
            )}

            <Button
              type="submit"
              variant="primary"
              size="lg"
              loading={isLoading}
              disabled={!username.trim() || !password}
              className="w-full mt-2"
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
    </div>
  );
};

export default LoginPage;
