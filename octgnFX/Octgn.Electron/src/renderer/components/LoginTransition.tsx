import React, { useState, useEffect } from 'react';

interface LoginTransitionProps {
  /** true while waiting for the API response */
  isLoading: boolean;
  /** true once login succeeded — triggers the success burst */
  isSuccess: boolean;
  /** Called after the full exit animation completes */
  onComplete: () => void;
}

/**
 * Full-screen overlay with an animated spinner shown during login,
 * followed by a "success burst" that transitions out.
 */
const LoginTransition: React.FC<LoginTransitionProps> = ({
  isLoading,
  isSuccess,
  onComplete,
}) => {
  const [phase, setPhase] = useState<'idle' | 'loading' | 'success' | 'exit' | 'done'>('idle');

  useEffect(() => {
    if (isLoading && phase === 'idle') {
      setPhase('loading');
    }
  }, [isLoading, phase]);

  useEffect(() => {
    if (isSuccess && phase === 'loading') {
      setPhase('success');
      // After the success burst, start exit
      const t1 = setTimeout(() => setPhase('exit'), 800);
      const t2 = setTimeout(() => {
        setPhase('done');
        onComplete();
      }, 1400);
      return () => {
        clearTimeout(t1);
        clearTimeout(t2);
      };
    }
  }, [isSuccess, phase, onComplete]);

  // Reset when loading stops without success (error)
  useEffect(() => {
    if (!isLoading && !isSuccess && phase === 'loading') {
      setPhase('idle');
    }
  }, [isLoading, isSuccess, phase]);

  if (phase === 'idle' || phase === 'done') return null;

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-octgn-bg/80 ${
        phase === 'exit' ? 'login-overlay-exit' : 'login-overlay-enter'
      }`}
      style={{ backdropFilter: 'blur(20px)' }}
    >
      {/* Pulse rings */}
      <div className="absolute flex items-center justify-center">
        <div className="login-pulse-ring" />
        <div className="login-pulse-ring" />
        <div className="login-pulse-ring" />
      </div>

      {/* Orbiting particles */}
      <div className="absolute flex items-center justify-center">
        <div className="login-particle" />
        <div className="login-particle" />
        <div className="login-particle" />
      </div>

      {/* Central spinner / success burst */}
      <div
        className={`flex flex-col items-center gap-6 ${
          phase === 'success' ? 'login-success-burst' : ''
        }`}
      >
        <div className="login-spinner" />
        <p className="text-sm text-octgn-text-muted tracking-widest uppercase animate-pulse">
          {phase === 'success' ? 'Welcome' : 'Authenticating'}
        </p>
      </div>

      {/* OCTGN branding watermark */}
      <h1
        className={`absolute font-display text-[120px] font-bold tracking-[0.3em] pointer-events-none select-none ${
          phase === 'success'
            ? 'text-octgn-primary/20 scale-110 transition-all duration-700'
            : 'text-octgn-primary/5'
        }`}
        style={{ filter: 'blur(1px)' }}
      >
        OCTGN
      </h1>
    </div>
  );
};

export default LoginTransition;
