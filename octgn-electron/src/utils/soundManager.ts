/**
 * Sound Manager for OCTGN
 * 
 * Handles playing game sounds for card movements, shuffles, etc.
 */

export type SoundType =
  | 'cardflip'
  | 'cardmove'
  | 'shuffle'
  | 'deal'
  | 'chat'
  | 'turn'
  | 'notify'
  | 'error';

interface SoundConfig {
  enabled: boolean;
  volume: number;
}

class SoundManager {
  private audioContext: AudioContext | null = null;
  private sounds: Map<string, AudioBuffer> = new Map();
  private config: SoundConfig = {
    enabled: true,
    volume: 0.7,
  };

  async initialize(): Promise<void> {
    try {
      this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      await this.loadDefaultSounds();
    } catch (error) {
      console.warn('Failed to initialize audio context:', error);
    }
  }

  private async loadDefaultSounds(): Promise<void> {
    // For now, we'll generate simple synth sounds
    // In production, these would be loaded from game packages
    const sounds: SoundType[] = ['cardflip', 'cardmove', 'shuffle', 'deal', 'chat', 'turn', 'notify', 'error'];

    for (const sound of sounds) {
      // Generate placeholder sounds
      this.sounds.set(sound, this.generateSound(sound));
    }
  }

  private generateSound(type: SoundType): AudioBuffer {
    if (!this.audioContext) {
      throw new Error('Audio context not initialized');
    }

    const sampleRate = this.audioContext.sampleRate;
    let duration: number;
    let frequency: number;
    let waveType: OscillatorType;

    switch (type) {
      case 'cardflip':
        duration = 0.1;
        frequency = 800;
        waveType = 'sine';
        break;
      case 'cardmove':
        duration = 0.05;
        frequency = 400;
        waveType = 'sine';
        break;
      case 'shuffle':
        duration = 0.3;
        frequency = 200;
        waveType = 'noise';
        break;
      case 'deal':
        duration = 0.08;
        frequency = 600;
        waveType = 'triangle';
        break;
      case 'chat':
        duration = 0.15;
        frequency = 1000;
        waveType = 'sine';
        break;
      case 'turn':
        duration = 0.1;
        frequency = 500;
        waveType = 'sine';
        break;
      case 'notify':
        duration = 0.2;
        frequency = 880;
        waveType = 'sine';
        break;
      case 'error':
        duration = 0.3;
        frequency = 200;
        waveType = 'sawtooth';
        break;
      default:
        duration = 0.1;
        frequency = 440;
        waveType = 'sine';
    }

    const buffer = this.audioContext.createBuffer(1, sampleRate * duration, sampleRate);
    const data = buffer.getChannelData(0);

    for (let i = 0; i < data.length; i++) {
      const t = i / sampleRate;
      const envelope = Math.exp(-t * 10); // Decay envelope

      if (waveType === 'noise') {
        data[i] = (Math.random() * 2 - 1) * envelope * 0.3;
      } else {
        let sample = 0;
        switch (waveType) {
          case 'sine':
            sample = Math.sin(2 * Math.PI * frequency * t);
            break;
          case 'triangle':
            sample = Math.abs((t * frequency * 2) % 2 - 1) * 2 - 1;
            break;
          case 'sawtooth':
            sample = ((t * frequency) % 1) * 2 - 1;
            break;
          case 'square':
            sample = Math.sin(2 * Math.PI * frequency * t) > 0 ? 1 : -1;
            break;
        }
        data[i] = sample * envelope * 0.3;
      }
    }

    return buffer;
  }

  play(type: SoundType): void {
    if (!this.config.enabled || !this.audioContext) {
      return;
    }

    const buffer = this.sounds.get(type);
    if (!buffer) {
      return;
    }

    try {
      const source = this.audioContext.createBufferSource();
      const gainNode = this.audioContext.createGain();

      source.buffer = buffer;
      gainNode.gain.value = this.config.volume;

      source.connect(gainNode);
      gainNode.connect(this.audioContext.destination);

      source.start(0);
    } catch (error) {
      console.warn('Failed to play sound:', error);
    }
  }

  setEnabled(enabled: boolean): void {
    this.config.enabled = enabled;
  }

  setVolume(volume: number): void {
    this.config.volume = Math.max(0, Math.min(1, volume));
  }

  isEnabled(): boolean {
    return this.config.enabled;
  }

  getVolume(): number {
    return this.config.volume;
  }
}

// Singleton instance
export const soundManager = new SoundManager();
