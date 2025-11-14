# Audio Namespace

The Audio namespace provides comprehensive audio management for MonoGame applications, including sound effect playback, music management, and volume control.

## Core Classes

### Audio Management
- **[AudioController](AudioController.cs)** - Central audio management system for sound effects and music playback

## Key Features

### Sound Effect Management
- **Automatic Instance Tracking**: Tracks `SoundEffectInstance` objects for proper cleanup
- **Auto-Disposal**: Automatically disposes stopped sound effect instances
- **Volume Control**: Global master volume control for sound effects
- **3D Audio Support**: Volume, pitch, and pan control per sound effect

### Music Playback
- **Background Music**: Play songs with repeat functionality
- **Volume Control**: Separate volume control for music
- **State Management**: Play, pause, stop, and resume music playback

### Volume and Muting
- **Global Muting**: Mute/unmute all audio with volume restoration
- **Separate Controls**: Independent volume control for music and sound effects
- **Volume Persistence**: Remembers volume levels when muting/unmuting

## Usage Examples

### Basic Setup
```csharp
// AudioController is typically created by Core
// Access via Core.Audio static property
var audio = Core.Audio;
```

### Playing Sound Effects
```csharp
// Simple sound effect playback
SoundEffect jumpSound = Content.Load<SoundEffect>("jump");
audio.PlaySoundEffect(jumpSound);

// Advanced sound effect with parameters
SoundEffect explosionSound = Content.Load<SoundEffect>("explosion");
var instance = audio.PlaySoundEffect(
    explosionSound,
    volume: 0.8f,
    pitch: 0.2f,
    pan: -0.5f,      // Left speaker
    isLooped: false
);

// The instance is automatically tracked and cleaned up
```

### Music Playback
```csharp
// Load and play background music
Song backgroundMusic = Content.Load<Song>("background_theme");

// Play with repeat
audio.PlaySong(backgroundMusic, isRepeating: true);

// Play once only
audio.PlaySong(backgroundMusic, isRepeating: false);
```

### Volume Control
```csharp
// Set music volume (0.0f to 1.0f)
audio.SongVolume = 0.7f;

// Set sound effects volume
audio.SoundEffectVolume = 0.8f;

// Get current volumes
float currentMusicVolume = audio.SongVolume;
float currentSfxVolume = audio.SoundEffectVolume;
```

### Muting and Audio Control
```csharp
// Check if audio is muted
if (!audio.IsMuted)
{
    // Mute all audio
    audio.MuteAudio();
}

// Unmute and restore previous volumes
audio.UnmuteAudio();

// Toggle mute state
audio.ToggleMute();

// Pause all audio (music + sound effects)
audio.PauseAudio();

// Resume all paused audio
audio.ResumeAudio();
```

### Manual Sound Effect Instance Management
```csharp
// For long-running or controllable sounds
SoundEffect engineSound = Content.Load<SoundEffect>("engine");
var engineInstance = audio.PlaySoundEffect(
    engineSound,
    volume: 0.6f,
    pitch: 0.0f,
    pan: 0.0f,
    isLooped: true
);

// Control the instance directly
engineInstance.Volume = 0.8f;
engineInstance.Pitch = 0.3f;
engineInstance.Stop();
// Instance will be automatically cleaned up on next Update()
```

### Update Cycle
The AudioController automatically manages cleanup in its Update method:

```csharp
// Called automatically by Core.Update()
// Cleans up stopped sound effect instances
audio.Update();
```

## Audio File Requirements

### Sound Effects
- **Format**: .wav files (recommended)
- **Loading**: Use `ContentManager.Load<SoundEffect>("filename")`
- **Best Practices**: 
  - Keep sound effects short for memory efficiency
  - Use compressed formats for larger files
  - Consider creating instances for repeated sounds

### Music
- **Format**: .mp3, .ogg, .wav (platform dependent)
- **Loading**: Use `ContentManager.Load<Song>("filename")`
- **Best Practices**:
  - Use compressed formats (.mp3, .ogg) for music
  - Only one song can play at a time
  - Music files should be added to Content Pipeline

## Volume Management

### Volume Ranges
- All volume values range from `0.0f` (silent) to `1.0f` (full volume)
- Values are automatically clamped to this range
- Separate volume controls for music and sound effects

### Muting Behavior
- When muted, volume getters return `0.0f`
- When muted, volume setters are ignored
- Original volume levels are restored when unmuting
- Muting affects both music and sound effects

## Resource Management

### Automatic Cleanup
- `SoundEffectInstance` objects are tracked in an internal list
- Stopped instances are automatically disposed and removed
- No manual cleanup required for most use cases

### Manual Disposal
```csharp
// AudioController implements IDisposable
// Automatically called by Core.UnloadContent()
audio.Dispose();

// Or call manually if needed
audio.Dispose();
```

## Architecture Notes

- **Singleton Pattern**: Accessed via `Core.Audio` static property
- **Automatic Management**: Handles instance lifecycle automatically
- **Thread Safe**: Safe to call from main game thread
- **MonoGame Integration**: Wraps MonoGame's audio APIs (`MediaPlayer`, `SoundEffect`)
- **Resource Efficient**: Automatic cleanup prevents memory leaks