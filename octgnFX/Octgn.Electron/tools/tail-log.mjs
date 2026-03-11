/**
 * Tail the OCTGN Electron log file (last N lines).
 * Usage: node tools/tail-log.mjs [lines] [filter]
 * Example: node tools/tail-log.mjs 50 RESOLVER
 *          node tools/tail-log.mjs 100 IMAGE
 */
import { readFile } from 'fs/promises';
import { join } from 'path';

const lines = parseInt(process.argv[2]) || 50;
const filter = process.argv[3] || '';

const logPath = join(process.env.APPDATA, 'Electron', 'octgn-electron.log');

try {
  const content = await readFile(logPath, 'utf-8');
  let allLines = content.split('\n');
  if (filter) {
    allLines = allLines.filter(l => l.includes(filter));
  }
  const tail = allLines.slice(-lines);
  console.log(tail.join('\n'));
} catch (e) {
  console.error(`Failed to read log: ${e.message}`);
  console.error(`Expected path: ${logPath}`);
}
