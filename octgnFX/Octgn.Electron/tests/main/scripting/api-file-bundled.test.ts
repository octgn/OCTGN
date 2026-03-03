import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

/**
 * The Python API version file (3.1.0.2.py) must exist alongside the compiled
 * JS output so that PythonScope can find it at runtime via __dirname.
 *
 * tsc only compiles .ts files — the build must explicitly copy .py assets
 * to the dist directory. This test ensures the copy step works.
 */
describe('Python API version file bundling', () => {
  const srcVersionsDir = path.join(__dirname, '../../../src/main/scripting/versions');
  const distVersionsDir = path.join(__dirname, '../../../dist/main/scripting/versions');

  it('source versions directory contains 3.1.0.2.py', () => {
    const srcFile = path.join(srcVersionsDir, '3.1.0.2.py');
    expect(fs.existsSync(srcFile)).toBe(true);
  });

  it('dist versions directory contains 3.1.0.2.py after build', () => {
    const distFile = path.join(distVersionsDir, '3.1.0.2.py');
    expect(fs.existsSync(distFile)).toBe(true);
  });

  it('dist 3.1.0.2.py matches src content', () => {
    const srcFile = path.join(srcVersionsDir, '3.1.0.2.py');
    const distFile = path.join(distVersionsDir, '3.1.0.2.py');

    // Skip if dist file doesn't exist (caught by previous test)
    if (!fs.existsSync(distFile)) return;

    const srcContent = fs.readFileSync(srcFile, 'utf-8');
    const distContent = fs.readFileSync(distFile, 'utf-8');
    expect(distContent).toBe(srcContent);
  });
});
