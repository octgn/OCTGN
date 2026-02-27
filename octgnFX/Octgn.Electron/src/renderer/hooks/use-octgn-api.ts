import type { OctgnAPI } from '../../main/preload';

/**
 * Provides typed access to the window.octgn API exposed by the preload script.
 * Throws if called outside of the Electron renderer context.
 */
export function useOctgnApi(): OctgnAPI {
  if (!window.octgn) {
    throw new Error(
      'window.octgn is not available. This hook must be used in the Electron renderer process.',
    );
  }
  return window.octgn;
}
