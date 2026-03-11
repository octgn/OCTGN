/**
 * Downloads a .nupkg from the OCTGN NuGet feed and installs it.
 * A .nupkg is a ZIP file; we extract def/definition.xml + assets.
 */
import { mkdir, writeFile } from 'fs/promises';
import { join } from 'path';
import AdmZip from 'adm-zip';
import { parseDefinitionXml } from './definition-parser';
import { getInstallDir } from './game-store';
import type { GameDefinition, InstallProgress } from '../../shared/types';

export type ProgressCallback = (progress: InstallProgress) => void;

export async function installGame(
  gameId: string,
  downloadUrl: string,
  onProgress: ProgressCallback,
): Promise<{ success: boolean; definition?: GameDefinition; error?: string }> {
  // 1. Download
  onProgress({ gameId, phase: 'downloading', percent: 0 });
  let buf: Buffer;
  try {
    const resp = await fetch(downloadUrl, { signal: AbortSignal.timeout(120_000) });
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

  // 2. Extract
  onProgress({ gameId, phase: 'extracting', percent: 80 });
  let definition: GameDefinition | null = null;
  try {
    const zip = new AdmZip(buf);
    const entries = zip.getEntries();

    // Find definition.xml inside the nupkg (typically at def/definition.xml)
    let defEntry = entries.find(
      (e) => e.entryName.toLowerCase() === 'def/definition.xml',
    );
    if (!defEntry) {
      // Fallback: any definition.xml in the archive
      defEntry = entries.find((e) => e.entryName.toLowerCase().endsWith('definition.xml'));
    }
    if (!defEntry) {
      throw new Error('No definition.xml found inside package');
    }

    // Parse the definition to get the real game id
    const xmlBuf = defEntry.getData();
    onProgress({ gameId, phase: 'parsing', percent: 90 });
    definition = parseDefinitionXml(xmlBuf);
    if (!definition) throw new Error('Failed to parse definition.xml');

    const installDir = getInstallDir(definition.id);
    await mkdir(installDir, { recursive: true });

    // Extract all entries under def/ to the install dir
    const defPrefix = defEntry.entryName.includes('/')
      ? defEntry.entryName.substring(0, defEntry.entryName.indexOf('/') + 1)
      : 'def/';

    for (const entry of entries) {
      if (entry.isDirectory) continue;
      if (!entry.entryName.startsWith(defPrefix)) continue;

      const relativePath = entry.entryName.slice(defPrefix.length);
      const destPath = join(installDir, ...relativePath.split('/'));

      await mkdir(join(destPath, '..'), { recursive: true });
      await writeFile(destPath, entry.getData());
    }
  } catch (err) {
    onProgress({ gameId, phase: 'error', percent: 0, error: String(err) });
    return { success: false, error: String(err) };
  }

  onProgress({ gameId, phase: 'done', percent: 100 });
  return { success: true, definition };
}
