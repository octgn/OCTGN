import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface SettingsState {
  // Game
  autoConfirmActions: boolean;
  enableAnimations: boolean;
  showCardPreviews: boolean;
  cardPreviewSize: string;

  // Display
  uiScale: string;
  theme: string;
  reducedMotion: boolean;
  showFps: boolean;

  // Audio
  masterVolume: number;
  sfxVolume: number;
  musicVolume: number;
  muteOnMinimize: boolean;
  chatSounds: boolean;

  // Network
  serverUrl: string;

  // Game Defaults
  autoPass: boolean;
  animationSpeed: string;
}

interface SettingsActions {
  update: <K extends keyof SettingsState>(key: K, value: SettingsState[K]) => void;
  reset: () => void;
}

export type SettingsStore = SettingsState & SettingsActions;

const defaultSettings: SettingsState = {
  // Game
  autoConfirmActions: false,
  enableAnimations: true,
  showCardPreviews: true,
  cardPreviewSize: 'medium',

  // Display
  uiScale: '100',
  theme: 'dark',
  reducedMotion: false,
  showFps: false,

  // Audio
  masterVolume: 80,
  sfxVolume: 70,
  musicVolume: 40,
  muteOnMinimize: true,
  chatSounds: true,

  // Network
  serverUrl: 'https://www.octgn.net',

  // Game Defaults
  autoPass: false,
  animationSpeed: 'normal',
};

export const useSettingsStore = create<SettingsStore>()(
  persist(
    (set) => ({
      ...defaultSettings,

      update: (key, value) => {
        set({ [key]: value } as Partial<SettingsState>);
      },

      reset: () => {
        set(defaultSettings);
      },
    }),
    {
      name: 'octgn-settings',
    }
  )
);
