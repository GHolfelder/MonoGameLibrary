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

### 🏗️ Core System
Foundation classes and core game architecture providing the singleton pattern and essential utilities.
- **Classes**: Core (Game singleton), Circle (geometric shape utility), CameraTestScene (camera demo)
- **Features**: Centralized system access, content scaling integration, camera system coordination
- **Pattern**: Static property access to all subsystems (`Core.Graphics`, `Core.Input`, `Core.Audio`, etc.)

### 🎨 [Graphics](MonoGameLibrary/Graphics/README.md)
Comprehensive 2D graphics system with sprites, animations, texture atlases, tile-based rendering, collision detection, and professional camera system.
- **Classes**: TextureAtlas, Sprite, AnimatedSprite, CharacterSprite, PlayerSprite, NPCSprite
- **Camera**: [Camera System](MonoGameLibrary/Graphics/Camera/README.md) - Professional 2D camera with zoom, following, and multi-input controls
- **Tiles**: [Tilemap System](MonoGameLibrary/Graphics/Tiles/README.md) - JSON-based tilemaps with z-ordering and object layer collision support
- **Collision**: [Collision System](MonoGameLibrary/Graphics/Collision/README.md) - Rectangle/circle collision detection with visualization
- **Features**: XML/JSON configuration, animation state management, directional sprites, professional tilemap rendering, multi-shape object layer collision

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

## Featured Systems

### 🖥️ Resolution-Independent Content Scaling
Automatic content scaling system for consistent visual experience across all screen resolutions:

```csharp
// Scene-based automatic scaling
public class GameScene : Scene
{
    protected override void Draw(GameTime gameTime)
    {
        BeginScaled(); // Automatic scaling with letterboxing/pillarboxing
        {
            // All coordinates use virtual resolution (default 1920x1080)
            player.Draw(Core.SpriteBatch, playerPosition);
            tilemap.Draw(Core.SpriteBatch, Vector2.Zero);
            
            // UI positioned in virtual coordinates
            Core.SpriteBatch.DrawString(font, "Score: 1000", new Vector2(50, 50), Color.White);
        }
        Core.SpriteBatch.End();
    }
}

// Manual scaling for non-Scene classes
Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
// Drawing code here uses virtual coordinates
Core.SpriteBatch.End();

// Input automatically converted to virtual coordinates
var mousePos = Core.Input.Mouse.VirtualPosition;
var virtualClick = Core.Input.Mouse.VirtualX; // Always in virtual space
```

**Key Features:**
- **Virtual Resolution System** - Design for fixed resolution (default 1920x1080)
- **Uniform Scaling** - Maintains aspect ratio with letterbox/pillarbox when needed
- **Input Transformation** - Mouse coordinates automatically converted to virtual space
- **Scene Integration** - `BeginScaled()` method for automatic scaling in Scene classes
- **Monitor Awareness** - Automatic window sizing based on monitor resolution
- **Cross-Resolution Consistency** - Same visual experience from 1366x768 to 4K displays

📖 **[Complete Content Scaling Documentation](ContentScalingExample.md)**

### 🗺️ JSON Tilemap System with Z-Ordering & Collision
Professional tilemap rendering with proper depth management and comprehensive collision detection:

```csharp
// Load tilemaps from JSON with texture atlas integration
var tilemaps = Tilemap.FromJson(Content, "maps/level1.json", textureAtlas);
Tilemap tilemap = tilemaps["level1"]; // or tilemaps[0] for first map

// Render with proper z-ordering
tilemap.DrawLayersUpTo(spriteBatch, position, 0);    // Background
player.Draw(spriteBatch);                           // Characters  
tilemap.DrawLayersFrom(spriteBatch, position, 1);   // Foreground

// Multi-shape object layer collision detection
if (tilemap.CheckSpriteObjectCollision(player, playerPos, "Collision"))
{
    // Handle collision with walls, triggers, or interactive objects
}

// Visualize collision objects (rectangles, circles, polygons)
tilemap.DrawObjectLayerAsCollision(spriteBatch, "Collision", Color.Red);
```

**Key Features:**
- **Multi-layer support** for professional depth rendering
- **Texture atlas integration** for optimal performance  
- **Z-ordering system** - characters appear behind walls and trees
- **Multi-shape object layers** - rectangles, circles, ellipses, polygons, and points
- **Collision detection** - sprite-to-object and character-to-object collision
- **Collision visualization** - debug rendering for all object shapes
- **JSON configuration** with tileset references to atlas sprites
- **Per-scene tilemaps** for different game areas

📖 **[Complete Tilemap Documentation](MonoGameLibrary/Graphics/Tiles/README.md)**
📖 **[Collision System Documentation](MonoGameLibrary/Graphics/Collision/README.md)**

### 🎯 Comprehensive Collision Detection
Flexible collision system supporting multiple shape types and automatic visualization:

```csharp
// Enable sprite collision
player.EnableCollision(32, 32, enableDraw: true, Color.Green);
enemy.EnableCollision(16f, enableDraw: true, Color.Red); // Circular

// Check sprite-to-sprite collision
if (player.CheckCollision(playerPos, enemy, enemyPos))
{
    // Handle collision between sprites
}

// Object layer collision with multiple shapes
// (assuming tilemap was retrieved from TilemapCollection earlier)
var objectLayer = tilemap.GetObjectLayer("Interactive");
foreach (var obj in objectLayer.Objects)
{
    // Supports rectangles, circles, ellipses, polygons, points
    // Automatic shape detection from JSON properties
}
```

**Supported Object Shapes:**
- **Rectangle**: Standard collision boxes for walls and platforms
- **Circle/Ellipse**: Curved collision areas for rounded objects
- **Polygon**: Complex shapes for irregular collision boundaries  
- **Point**: Precise trigger points for events or spawns
- **Visualization**: Debug rendering for all collision shapes

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

### 4. Professional Tilemap with Z-Ordering
```csharp
public class GameLevel : Scene
{
    private Tilemap _tilemap;
    private Player _player;
    private TextureAtlas _atlas;

    public override void LoadContent()
    {
        // Load texture atlas and tilemaps
        _atlas = TextureAtlas.FromJsonTexture(Content, "atlas.json");
        var tilemaps = Tilemap.FromJson(Content, "maps/forest.json", _atlas);
        _tilemap = tilemaps["forest"]; // Access by name or use tilemaps[0]
        _player = new Player(Content);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Begin();
        
        // Draw background layers (ground, floors)
        _tilemap.DrawLayersUpTo(Core.SpriteBatch, Vector2.Zero, 0);
        
        // Draw characters and entities
        _player.Draw(Core.SpriteBatch);
        
        // Draw foreground layers (walls, tree canopies)
        _tilemap.DrawLayersFrom(Core.SpriteBatch, Vector2.Zero, 1);
        
        Core.SpriteBatch.End();
    }
}
```

## Installation

Add the MonoGame Library project to your solution and reference it from your game project:

```xml
<ProjectReference Include="..\MonoGameLibrary\MonoGameLibrary.csproj" />
```

## Release Notes

Track the evolution of MonoGame Library through our detailed release notes:

### Current Version
- **[Version 1.0.23](releases/ReleaseNotes-1.0.23.md)** *(Latest)* - Multiple Maps & Tile Collision Integration
  - **Multiple Maps Support**: TilemapCollection for loading and managing multiple tilemaps from single JSON files
  - **Flexible JSON Formats**: Support for both object-based and array-based multiple map JSON structures
  - **Tile Collision Integration**: PlayerSprite integration with tilemap collision detection via TilemapCollisionExtensions
  - **Collision Extension Methods**: Easy-to-use extension methods for tile-based collision detection
  - **Complete Documentation**: Comprehensive integration guide with examples and best practices
  - **Breaking Change**: FromJson now returns TilemapCollection instead of single Tilemap

### Previous Releases
- **[Version 1.0.22](releases/ReleaseNotes-1.0.22.md)** - Animated Tile System
  - **Animated Tile Support**: Complete animated tile system with JSON configuration and automatic frame cycling
  - **JSON Animation Loading**: Support for animated tile definitions in tilemap JSON with customizable frame durations
  - **Automatic Animation Updates**: Simple tilemap.Update(gameTime) call enables all tile animations
  - **Animation Framework**: AnimatedTile, AnimatedTileFrame, and AnimatedTileInstance classes
- **[Version 1.0.21](releases/ReleaseNotes-1.0.21.md)** - Comprehensive 2D Camera System
  - Professional camera architecture with position, zoom, rotation, and smooth following
  - Configurable character screen coverage system (adjustable percentage, default 10%)
  - Multi-input controls: keyboard, gamepad, and mouse wheel with customizable bindings
  - Performance monitoring, diagnostic tools, and world coordinate input integration
  - Complete documentation with troubleshooting guide and migration examples
- **[Version 1.0.20](releases/ReleaseNotes-1.0.20.md)** - Content Scaling System & Steam Deck Support
  - Comprehensive virtual resolution system with automatic scaling and letterboxing
  - Steam Deck auto-detection with native resolution and fullscreen optimization
  - Monitor resolution awareness with intelligent window sizing
  - Enhanced input system with virtual coordinate transformation
- **[Version 1.0.19](releases/ReleaseNotes-1.0.19.md)** - Developer Mode & Enhanced Object Layer Support
  - F1/F2 hotkey developer mode with collision visualization  
  - Enhanced object layer support with objectType property
  - Advanced polyline collision detection with geometric precision
  - PlayerSprite object layer integration
- **Version 1.0.18** - Enhanced Tilemap System & Multi-Shape Collision Support
- **Version 1.0.17** - Improved Graphics Pipeline & Animation System  
- **Version 1.0.16** - Core Architecture Improvements
- **Earlier Versions** - Foundation development and initial feature set

📖 **[View All Release Notes](releases/)** - Complete changelog and version history

**Stay Updated**: Release notes document all new features, API changes, bug fixes, and upgrade instructions for each version.

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