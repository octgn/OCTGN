import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { Button } from '../components';

export default function LoginPage() {
  const navigate = useNavigate();
  const { login, isLoading, error, clearError, isAuthenticated } = useAuthStore();
  
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  // Redirect if already authenticated
  if (isAuthenticated) {
    navigate('/');
    return null;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    clearError();
    
    const success = await login(username, password);
    if (success) {
      navigate('/');
    }
  };

  return (
    <div className="min-h-screen bg-octgn-dark flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo */}
        <div className="text-center mb-8">
          <div className="w-24 h-24 rounded-2xl bg-gradient-to-br from-octgn-highlight to-octgn-blue flex items-center justify-center mx-auto mb-6 shadow-glow">
            <span className="text-5xl">🃏</span>
          </div>
          <h1 className="text-4xl font-bold text-gradient mb-2">OCTGN</h1>
          <p className="text-gray-400">Online Card and Tabletop Gaming Network</p>
        </div>

        {/* Login Form */}
        <div className="glass rounded-2xl p-8">
          <h2 className="text-2xl font-bold text-white mb-6 text-center">Sign In</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Username */}
            <div>
              <label className="block text-sm text-gray-400 mb-2">Username</label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="input w-full"
                placeholder="Enter your username"
                required
                autoFocus
              />
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm text-gray-400 mb-2">Password</label>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="input w-full pr-12"
                  placeholder="Enter your password"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-white"
                >
                  {showPassword ? '🙈' : '👁️'}
                </button>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className="p-4 rounded-lg bg-red-500/20 border border-red-500/50 text-red-400 text-sm">
                {error}
              </div>
            )}

            {/* Submit Button */}
            <Button
              type="submit"
              variant="primary"
              className="w-full"
              loading={isLoading}
              disabled={!username || !password}
            >
              Sign In
            </Button>
          </form>

          {/* Links */}
          <div className="mt-6 text-center space-y-3">
            <a
              href="https://www.octgn.net/Account/Register"
              target="_blank"
              rel="noopener noreferrer"
              className="block text-octgn-highlight hover:text-octgn-highlight-hover"
            >
              Create an Account
            </a>
            <a
              href="https://www.octgn.net/Account/ForgotPassword"
              target="_blank"
              rel="noopener noreferrer"
              className="block text-gray-500 hover:text-gray-400 text-sm"
            >
              Forgot Password?
            </a>
          </div>
        </div>

        {/* Play Offline */}
        <div className="mt-6 text-center">
          <button
            onClick={() => navigate('/play/local')}
            className="text-gray-500 hover:text-white transition-colors"
          >
            Play Offline →
          </button>
        </div>

        {/* Footer */}
        <div className="mt-8 text-center text-sm text-gray-600">
          <p>By signing in, you agree to OCTGN's</p>
          <p>
            <a href="https://www.octgn.net/Terms" className="text-octgn-highlight hover:underline">
              Terms of Service
            </a>
            {' & '}
            <a href="https://www.octgn.net/Privacy" className="text-octgn-highlight hover:underline">
              Privacy Policy
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
