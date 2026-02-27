import React, { useState, useEffect, useRef } from 'react';

interface LoginTransitionProps {
  /** true while the login API call is in progress */
  isLoading: boolean;
  /** true when the user has been authenticated */
  isAuthenticated: boolean;
  /** true when there's an error */
  hasError: boolean;
  /** Called after the full exit animation completes */
  onComplete: () => void;
}

type AnimPhase = 'idle' | 'loading' | 'success' | 'exit' | 'done';

/**
 * Full-screen overlay with an animated spinner shown during login,
 * followed by a "success burst" that transitions out.
 */
const LoginTransition: React.FC<LoginTransitionProps> = ({
  isLoading,
  isAuthenticated,
  hasError,
  onComplete,
}) => {
  const [animPhase, setAnimPhase] = useState<AnimPhase>('idle');
  const onCompleteRef = useRef(onComplete);
  onCompleteRef.current = onComplete;

  useEffect(() => {
    // Start loading overlay
    if (isLoading) {
      setAnimPhase('loading');
      return;
    }

    // Success: animate burst → exit → navigate
    if (isAuthenticated) {
      setAnimPhase('success');
      return;
    }

    // Error: dismiss overlay
    if (hasError) {
      setAnimPhase('idle');
    }
  }, [isLoading, isAuthenticated, hasError]);

  // success → exit after 800ms
  useEffect(() => {
    if (animPhase === 'success') {
      const timer = setTimeout(() => setAnimPhase('exit'), 800);
      return () => clearTimeout(timer);
    }
  }, [animPhase]);

  // exit → done after 600ms (overlay fade-out duration)
  useEffect(() => {
    if (animPhase === 'exit') {
      const timer = setTimeout(() => {
        setAnimPhase('done');
        onCompleteRef.current();
      }, 600);
      return () => clearTimeout(timer);
    }
  }, [animPhase]);

  if (animPhase === 'idle' || animPhase === 'done') return null;

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-octgn-bg/80 ${
        animPhase === 'exit' ? 'login-overlay-exit' : 'login-overlay-enter'
      }`}
      style={{ backdropFilter: 'blur(20px)' }}
      data-testid="login-overlay"
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
          animPhase === 'success' ? 'login-success-burst' : ''
        }`}
      >
        <div className="login-spinner" />
        <p className="text-sm text-octgn-text-muted tracking-widest uppercase animate-pulse">
          {animPhase === 'success' ? 'Welcome' : 'Authenticating'}
        </p>
      </div>

      {/* OCTGN branding watermark */}
      <h1
        className={`absolute font-display text-[120px] font-bold tracking-[0.3em] pointer-events-none select-none ${
          animPhase === 'success'
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
