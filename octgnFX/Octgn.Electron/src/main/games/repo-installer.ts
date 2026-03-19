/**
 * Downloads a game repo as a zip from GitHub and installs it.
 * Similar to game-installer.ts but for repo-based distribution.
 */
import { mkdir, writeFile } from 'fs/promises';
import { join, resolve, relative } from 'path';
import AdmZip from 'adm-zip';
import { parseDefinitionXml } from './definition-parser';
import { getInstallDir } from './game-store';
import { constructZipballUrl } from './repo-feed-types';
import type { GameDefinition, InstallProgress } from '../../shared/types';

export type ProgressCallback = (progress: InstallProgress) => void;

export async function installFromRepo(
  owner: string,
  repo: string,
  branch: string,
  gamePath: string,
  gameId: string,
  onProgress: ProgressCallback,
): Promise<{ success: boolean; definition?: GameDefinition; error?: string }> {
  // 1. Download the zipball
  onProgress({ gameId, phase: 'downloading', percent: 0 });
  let buf: Buffer;
  try {
    const url = constructZipballUrl(owner, repo, branch);
    const resp = await fetch(url, {
      signal: AbortSignal.timeout(120_000),
      headers: {
        Accept: 'application/vnd.github+json',
      },
    });
    if (!resp.ok) {
      throw new Error(`Download failed: ${resp.status} ${resp.statusText}`);
    }
    const contentLength = Number(resp.headers.get('content-length') ?? 0);
    const reader = resp.body?.getReader();
    if (!reader) throw new Error('No response body');

    const chunks: Uint8Array[] = [];
    let received = 0;
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      chunks.push(value);
      received += value.length;
      if (contentLength > 0) {
        onProgress({ gameId, phase: 'downloading', percent: Math.round((received / contentLength) * 80) });
      }
    }
    buf = Buffer.concat(chunks);
  } catch (err) {
    onProgress({ gameId, phase: 'error', percent: 0, error: String(err) });
    return { success: false, error: String(err) };
  }

  // 2. Extract files at gamePath
  onProgress({ gameId, phase: 'extracting', percent: 80 });
  let definition: GameDefinition | null = null;
  try {
    const zip = new AdmZip(buf);
    const entries = zip.getEntries();

    // Detect the root folder dynamically — GitHub zips always have a single root folder
    const rootFolder = detectRootFolder(entries);
    if (!rootFolder) {
      throw new Error('Could not detect root folder in zip');
    }

    // Build the prefix to match: rootFolder/gamePath/
    const normalizedGamePath = gamePath.replace(/^\//, '').replace(/\/$/, '');
    const prefix = `${rootFolder}/${normalizedGamePath}/`;

    // Check that at least one entry matches the gamePath
    const matchingEntries = entries.filter(
      (e) => !e.isDirectory && e.entryName.startsWith(prefix),
    );
    if (matchingEntries.length === 0) {
      throw new Error(`No files found at gamePath "${gamePath}" in zip`);
    }

    const installDir = getInstallDir(gameId);
    await mkdir(installDir, { recursive: true });

    // Extract matching entries (with zip-slip protection)
    for (const entry of matchingEntries) {
      const relativePath = entry.entryName.slice(prefix.length);
      const destPath = resolve(installDir, ...relativePath.split('/'));

      // Guard against zip-slip: ensure extracted path stays within installDir
      const rel = relative(installDir, destPath);
      if (rel.startsWith('..') || resolve(destPath) !== destPath) {
        throw new Error(`Zip entry escapes install directory: ${entry.entryName}`);
      }

      await mkdir(join(destPath, '..'), { recursive: true });
      await writeFile(destPath, entry.getData());
    }

    // 3. Parse definition.xml
    onProgress({ gameId, phase: 'parsing', percent: 95 });
    const defEntry = matchingEntries.find(
      (e) => e.entryName.toLowerCase().endsWith('definition.xml'),
    );
    if (!defEntry) {
      throw new Error('No definition.xml found in extracted files');
    }

    definition = parseDefinitionXml(defEntry.getData());
    if (!definition) {
      throw new Error('Failed to parse definition.xml');
    }
  } catch (err) {
    onProgress({ gameId, phase: 'error', percent: 0, error: String(err) });
    return { success: false, error: String(err) };
  }

  onProgress({ gameId, phase: 'done', percent: 100 });
  return { success: true, definition };
}

/**
 * Detect the single root folder in a GitHub zipball.
 * All entries start with "{owner}-{repo}-{shortsha}/".
 */
function detectRootFolder(entries: AdmZip.IZipEntry[]): string | null {
  for (const entry of entries) {
    const slashIndex = entry.entryName.indexOf('/');
    if (slashIndex > 0) {
      return entry.entryName.substring(0, slashIndex);
    }
  }
  return null;
}
