import type { OctgnAPI } from '../main/preload';

declare global {
  interface Window {
    octgn: OctgnAPI;
  }
}
