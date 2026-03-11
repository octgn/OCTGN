/**
 * Dev launcher script for Windows compatibility
 * Waits for Vite and main process to be ready, then launches Electron
 */

const { spawn } = require('child_process');
const http = require('http');
const fs = require('fs');
const path = require('path');

const VITE_PORT = 32456;
const MAX_RETRIES = 30;
const RETRY_DELAY = 1000;

function checkViteReady() {
  return new Promise((resolve) => {
    const req = http.get(`http://localhost:${VITE_PORT}`, (res) => {
      resolve(res.statusCode === 200);
    });
    req.on('error', () => resolve(false));
    req.setTimeout(1000, () => {
      req.destroy();
      resolve(false);
    });
  });
}

function checkMainReady() {
  return fs.existsSync(path.join(__dirname, '..', 'dist', 'main.js'));
}

async function waitForAll() {
  console.log('Waiting for Vite dev server and main process...');
  
  let viteReady = false;
  let mainReady = false;
  let retries = 0;

  while (retries < MAX_RETRIES) {
    if (!viteReady) {
      viteReady = await checkViteReady();
      if (viteReady) {
        console.log('✓ Vite dev server ready');
      }
    }

    if (!mainReady) {
      mainReady = checkMainReady();
      if (mainReady) {
        console.log('✓ Main process compiled');
      }
    }

    if (viteReady && mainReady) {
      return true;
    }

    retries++;
    process.stdout.write('.');
    await new Promise(r => setTimeout(r, RETRY_DELAY));
  }

  return false;
}

async function launch() {
  const ready = await waitForAll();
  console.log('');
  
  if (!ready) {
    console.error('Failed to start - timeout waiting for services');
    console.error('Make sure you have run: npm install');
    process.exit(1);
  }

  console.log('🚀 Launching Electron...');
  
  // Use npx to run electron on Windows for better compatibility
  const child = spawn(
    process.platform === 'win32' ? 'npx.cmd' : 'npx',
    ['electron', '.'],
    {
      stdio: 'inherit',
      env: {
        ...process.env,
        ELECTRON_ENABLE_LOGGING: '1',
        NODE_ENV: 'development',
      },
      cwd: path.join(__dirname, '..'),
      shell: process.platform === 'win32',
    }
  );

  child.on('error', (err) => {
    console.error('Failed to start Electron:', err);
    process.exit(1);
  });

  child.on('close', (code) => {
    process.exit(code || 0);
  });
}

launch().catch((err) => {
  console.error('Failed to launch:', err);
  process.exit(1);
});
