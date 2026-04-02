/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Original OCTGN color palette
        octgn: {
          // Primary dark backgrounds (from WPF DarkBackground #171717)
          dark: '#171717',
          'dark-hover': '#1f1f1f',
          'dark-active': '#252525',
          
          // Window background (from WPF WindowBack #333333)
          primary: '#333333',
          'primary-hover': '#3d3d3d',
          'primary-active': '#474747',
          
          // Accent/border color
          accent: '#4a4a4a',
          'accent-hover': '#5a5a5a',
          'accent-active': '#6a6a6a',
          
          // Signature OCTGN purple-blue (from progress bar #A30093FF)
          highlight: '#9370DB',  // Medium purple - signature color
          'highlight-hover': '#A37FDA',
          'highlight-active': '#B38FE6',
          
          // Blue accent for interactive elements
          blue: '#6886D4',
          'blue-hover': '#7896E4',
          'blue-active': '#88A6F4',
          
          // Status colors
          success: '#50C878',  // Emerald green
          warning: '#FFD700',  // Gold
          error: '#DC143C',    // Crimson
          info: '#4FC3F7',     // Light blue
        },
      },
      fontFamily: {
        sans: ['Segoe UI', 'system-ui', '-apple-system', 'sans-serif'],
        mono: ['Consolas', 'Monaco', 'monospace'],
      },
      boxShadow: {
        'glow': '0 0 20px rgba(147, 112, 219, 0.3)',
        'glow-sm': '0 0 10px rgba(147, 112, 219, 0.2)',
        'glow-lg': '0 0 30px rgba(147, 112, 219, 0.4)',
        'inner-glow': 'inset 0 0 20px rgba(147, 112, 219, 0.1)',
        'card': '0 4px 20px rgba(0, 0, 0, 0.5)',
        'card-hover': '0 8px 30px rgba(0, 0, 0, 0.6)',
      },
      backgroundImage: {
        // Gradient overlays for that glossy WPF feel
        'gradient-control': 'linear-gradient(135deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0.05) 100%)',
        'gradient-button': 'linear-gradient(180deg, rgba(255,255,255,0.1) 0%, rgba(0,0,0,0.1) 100%)',
        'gradient-highlight': 'linear-gradient(135deg, #9370DB 0%, #6886D4 100%)',
        'gradient-dark': 'linear-gradient(180deg, #1f1f1f 0%, #171717 100%)',
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'glow': 'glow 2s ease-in-out infinite alternate',
        'slide-up': 'slideUp 0.2s ease-out',
        'slide-down': 'slideDown 0.2s ease-out',
        'fade-in': 'fadeIn 0.2s ease-out',
        'card-flip': 'cardFlip 0.6s ease-in-out',
      },
      keyframes: {
        glow: {
          '0%': { boxShadow: '0 0 20px rgba(147, 112, 219, 0.3)' },
          '100%': { boxShadow: '0 0 30px rgba(147, 112, 219, 0.5)' },
        },
        slideUp: {
          '0%': { transform: 'translateY(10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        cardFlip: {
          '0%': { transform: 'rotateY(0deg)' },
          '50%': { transform: 'rotateY(90deg)' },
          '100%': { transform: 'rotateY(0deg)' },
        },
      },
      borderRadius: {
        'xl': '12px',
        '2xl': '16px',
        '3xl': '24px',
      },
    },
  },
  plugins: [],
}
