# MonoGame Library - AI Agent Instructions

## Architecture Overview

This is a **MonoGame utility library** providing a higher-level API over MonoGame's core framework. The architecture follows a **singleton Core pattern** with global static access to subsystems.

### Core Singleton Pattern
- `Core` class extends `Game` and manages all subsystems via static properties
- **Critical**: Only one Core instance allowed (`s_instance` validation)
- Global access pattern: `Core.Graphics`, `Core.Input`, `Core.SpriteBatch`, etc.
- Scene management with automatic disposal and GC collection during transitions

### Key Subsystems
- **Graphics**: Texture atlases, sprites, animations, tilemaps with XML-driven configuration
- **Input**: Centralized input handling with previous/current state tracking for edge detection
- **Audio**: Sound effect instance management with automatic cleanup
- **Scenes**: Base scene class with per-scene content managers for automatic resource cleanup

## Critical Development Patterns

### XML Configuration System
Both `TextureAtlas` and `Tilemap` use **XML-based configuration** loaded via `FromXml()` methods:

```xml
<!-- TextureAtlas XML structure -->
<TextureAtlas>
    <Texture>path/to/spritesheet</Texture>
    <Regions>
        <Region name="player_idle" x="0" y="0" width="32" height="32" />
    </Regions>
    <Animations>
        <Animation name="walk" delay="100">
            <Frame region="player_idle" />
        </Animation>
    </Animations>
</TextureAtlas>
```

### Input State Management
All input classes follow **previous/current state pattern** for edge detection:
- `WasKeyJustPressed()` = `CurrentState.IsKeyDown() && PreviousState.IsKeyUp()`
- Always call `Update()` in game loop to maintain state consistency

### Resource Management
- **Scenes**: Each scene has its own `ContentManager` for automatic cleanup on disposal
- **Audio**: `AudioController` tracks `SoundEffectInstance` objects and auto-disposes when stopped
- **Core transitions**: Manual `GC.Collect()` called during scene changes

### Graphics Rendering Chain
1. `TextureRegion` → defines rectangular area in texture
2. `Sprite` → adds rendering properties (position, rotation, scale, etc.)
3. `AnimatedSprite` → extends Sprite with frame-based animation
4. All rendering goes through `SpriteBatch` via `Draw()` methods

## Build & Development

### Project Structure
- **Target**: .NET 9.0 with MonoGame.Framework.DesktopGL
- **Content Pipeline**: Uses `.mgcb` files and dotnet tools for asset processing
- **Build**: Standard `dotnet build` from solution root

### Common Commands
```bash
# Build library
dotnet build

# Build content (if using Content Pipeline)
dotnet mgcb-editor  # Opens content pipeline editor

# Run tests (when added)
dotnet test
```

## Development Guidelines

### When Adding New Graphics Features
- Follow the `TextureRegion` → `Sprite` → specialized class hierarchy
- Implement `Draw()` methods that accept `SpriteBatch` parameter
- Use consistent property patterns (`Color`, `Rotation`, `Scale`, `Origin`, etc.)

### When Adding Input Features
- Implement previous/current state tracking pattern
- Provide `Was[Action]JustPressed/Released()` methods for edge detection
- Update state in `Update(GameTime)` methods

### When Adding Scene Features
- Extend abstract `Scene` base class
- Use scene-specific `Content` property for loading assets
- Implement proper disposal in `Dispose(bool disposing)` override

### Namespace Organization
- Root: `MonoGameLibrary` (Core, utilities)
- `MonoGameLibrary.Graphics` (rendering, sprites, animations)
- `MonoGameLibrary.Input` (input management)
- `MonoGameLibrary.Audio` (sound management)
- `MonoGameLibrary.Scenes` (scene system)

## Integration Points

### MonoGame Framework Dependencies
- Inherits from `Microsoft.Xna.Framework.Game`
- Uses MonoGame's content pipeline for asset loading
- Wraps MonoGame input/audio systems with higher-level APIs

### Key Design Decisions
- **Static access**: Prioritizes convenience over testability
- **XML configuration**: Enables designer-friendly asset definition
- **Automatic cleanup**: Reduces memory leak potential
- **Edge detection**: Provides game-friendly input handling

Focus development on maintaining these established patterns rather than introducing new architectural approaches.