# MonoGame Library

A comprehensive utility library for MonoGame that provides higher-level abstractions and tools for common game development tasks. This library simplifies MonoGame development by offering a structured approach to graphics, input, audio, scene management, and AI systems.

## Overview

MonoGame Library is built around a **singleton Core pattern** that provides centralized access to all game systems. It extends MonoGame's `Game` class to offer a more organized and feature-rich development experience while maintaining the flexibility and performance of the underlying framework.

### Key Benefits

- **Simplified Game Structure**: Organized subsystems with consistent APIs
- **Automatic Resource Management**: Scene-based content loading with automatic cleanup
- **Enhanced Input Handling**: Edge detection and multi-device support
- **Flexible Asset Loading**: XML and JSON configuration for sprites and animations
- **Extensible Architecture**: Interface-based design for easy customization

## Architecture

The library follows a **Core Singleton Pattern** where all subsystems are accessible through static properties:

```csharp
Core.Graphics     // Graphics device and rendering
Core.Input        // Keyboard, mouse, and gamepad input
Core.Audio        // Sound effects and music
Core.SpriteBatch  // Shared sprite batch for rendering
Core.Content      // Global content manager
```

## Namespace Documentation

Explore detailed documentation for each namespace:

### 🎨 [Graphics](MonoGameLibrary/Graphics/README.md)
Comprehensive 2D graphics system with sprites, animations, texture atlases, and tile-based rendering.
- **Classes**: TextureAtlas, Sprite, AnimatedSprite, CharacterSprite, PlayerSprite, NPCSprite, Tilemap, Tileset
- **Features**: XML/JSON configuration, animation state management, directional sprites, tile graphics

### 🎮 [Input](MonoGameLibrary/Input/README.md)
Unified input management with edge detection and multi-device support.
- **Classes**: InputManager, KeyboardInfo, MouseInfo, GamePadInfo, IInputProvider
- **Features**: Previous/current state tracking, edge detection, gamepad vibration, input abstraction

### 🔊 [Audio](MonoGameLibrary/Audio/README.md)
Complete audio system for sound effects and music with automatic resource management.
- **Classes**: AudioController
- **Features**: Sound effect instances, music playback, volume control, muting, automatic cleanup

### 🎭 [Scenes](MonoGameLibrary/Scenes/README.md)
Scene management system with automatic resource lifecycle handling.
- **Classes**: Scene (abstract base class)
- **Features**: Scene-specific content managers, automatic asset cleanup, smooth transitions

### 🤖 [AI](MonoGameLibrary/AI/README.md)
Flexible AI behavior system for implementing game artificial intelligence.
- **Interfaces**: IAIBehavior
- **Features**: Behavior composition, state machines, patrol AI, extensible design patterns

### 🛠️ [Utilities](MonoGameLibrary/Utilities/README.md)
Common utilities, logging system, and helper classes.
- **Classes**: ILogger (ConsoleLogger, NullLogger), Direction enums
- **Features**: Flexible logging, directional movement, type-safe enumerations

## Quick Start

### 1. Create Your Game Class
```csharp
public class MyGame : Core
{
    public MyGame()
    {
        // Core automatically initializes all subsystems
    }

    protected override void LoadContent()
    {
        // Start with your initial scene
        ChangeScene(new MainMenuScene());
    }
}
```

### 2. Implement a Scene
```csharp
public class GameplayScene : Scene
{
    private PlayerSprite _player;
    private TextureAtlas _playerAtlas;

    public override void LoadContent()
    {
        // Load assets using scene's ContentManager (automatic cleanup)
        _playerAtlas = TextureAtlas.FromXml(Content, "player_atlas.xml");
        _player = new PlayerSprite(_playerAtlas, "player", DirectionMode.FourWay, 
                                  AnimationState.Idle, AnimationState.Walk);
    }

    public override void Update(GameTime gameTime)
    {
        // Handle input and update game objects
        _player.Update(gameTime);

        // Scene transitions
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Core.ChangeScene(new MainMenuScene());
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Begin();
        _player.Draw(Core.SpriteBatch);
        Core.SpriteBatch.End();
    }
}
```

### 3. Handle Input and Audio
```csharp
// Input with edge detection
if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Space))
{
    player.Jump();
    Core.Audio.PlaySound("jump_sound");
}

// Gamepad support
if (Core.Input.GamePad.IsButtonDown(PlayerIndex.One, Buttons.A))
{
    player.Attack();
}

// Music management
Core.Audio.PlayMusic("background_music");
Core.Audio.SetMusicVolume(0.7f);
```

## Installation

Add the MonoGame Library project to your solution and reference it from your game project:

```xml
<ProjectReference Include="..\MonoGameLibrary\MonoGameLibrary.csproj" />
```

## Requirements

- **.NET 9.0**
- **MonoGame.Framework.DesktopGL 3.8.2.1105**
- **Visual Studio 2022** or compatible IDE

## Building

```bash
# Build the library
dotnet build

# Build in release mode
dotnet build -c Release
```

## Contributing

When contributing to the MonoGame Library, please:

1. Follow the established patterns (Core singleton, previous/current state tracking, automatic cleanup)
2. Add comprehensive documentation to namespace README files
3. Include usage examples in your documentation
4. Maintain backward compatibility
5. Write clean, well-commented code

## Design Philosophy

The MonoGame Library prioritizes:
- **Developer Experience**: Intuitive APIs and automatic resource management
- **Performance**: Built on MonoGame's efficient foundation
- **Flexibility**: Interface-based design allows customization
- **Reliability**: Consistent patterns and thorough error handling
- **Maintainability**: Clear separation of concerns and comprehensive documentation

## License

This project is licensed under the MIT License - see the LICENSE file for details.