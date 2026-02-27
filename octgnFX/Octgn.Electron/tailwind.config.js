/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/renderer/**/*.{html,tsx,ts}'],
  theme: {
    extend: {
      colors: {
        octgn: {
          bg: '#0a0e17',
          surface: '#111827',
          'surface-light': '#1f2937',
          border: '#374151',
          primary: '#3b82f6',
          'primary-hover': '#2563eb',
          accent: '#8b5cf6',
          'accent-hover': '#7c3aed',
          success: '#10b981',
          warning: '#f59e0b',
          danger: '#ef4444',
          text: '#f9fafb',
          'text-muted': '#9ca3af',
          'text-dim': '#6b7280',
          gold: '#fbbf24',
        },
      },
      fontFamily: {
        sans: ['Outfit', 'Inter', 'system-ui', 'sans-serif'],
        display: ['Orbitron', 'monospace'],
        mono: ['JetBrains Mono', 'monospace'],
      },
      animation: {
        'glow-pulse': 'glow-pulse 2s ease-in-out infinite alternate',
        'card-hover': 'card-hover 0.3s ease-out',
        'slide-up': 'slide-up 0.3s ease-out',
        'fade-in': 'fade-in 0.2s ease-out',
      },
      keyframes: {
        'glow-pulse': {
          '0%': { boxShadow: '0 0 5px rgba(59, 130, 246, 0.3)' },
          '100%': { boxShadow: '0 0 20px rgba(59, 130, 246, 0.6)' },
        },
        'card-hover': {
          '0%': { transform: 'translateY(0) scale(1)' },
          '100%': { transform: 'translateY(-8px) scale(1.05)' },
        },
        'slide-up': {
          '0%': { transform: 'translateY(20px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'fade-in': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'hero-pattern': 'linear-gradient(135deg, #0a0e17 0%, #111827 50%, #1a1a2e 100%)',
      },
    },
  },
  plugins: [],
};
