#!/bin/bash

# OCTGN Electron Build Script
# Builds the application for all platforms

set -e

echo "🔨 Building OCTGN Electron..."

# Install dependencies if needed
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm install
fi

# Build renderer (Vite)
echo "🎨 Building renderer..."
npm run build:renderer

# Build main process (TypeScript)
echo "⚙️  Building main process..."
npm run build:main

# Package for current platform
echo "📦 Packaging application..."
case "$1" in
    "linux")
        npm run dist:linux
        ;;
    "mac")
        npm run dist:mac
        ;;
    "win")
        npm run dist:win
        ;;
    "all")
        npm run dist
        ;;
    *)
        echo "Usage: ./build.sh [linux|mac|win|all]"
        echo "Building for current platform..."
        npm run pack
        ;;
esac

echo "✅ Build complete!"
echo "Check the 'release' directory for output."
