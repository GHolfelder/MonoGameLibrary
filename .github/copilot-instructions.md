# MonoGame Library - AI Agent Instructions

## Architecture Overview

This is a **MonoGame utility library** providing a higher-level API over MonoGame's core framework. The architecture follows a **singleton Core pattern** with global static access to subsystems.

### Core Singleton Pattern
- `Core` class extends `Game` and manages all subsystems via static properties
- **Critical**: Only one Core instance allowed (`s_instance` validation)
- Global access pattern: `Core.Graphics`, `Core.Input`, `Core.SpriteBatch`, etc.
- Scene management with automatic disposal and GC collection during transitions

### Key Subsystems
- **Graphics**: Texture atlases, sprites, animations with XML/JSON-driven configuration
- **Graphics.Tiles**: Tilemaps and tilesets with XML/JSON-driven configuration
- **Graphics.Collision**: Enhanced collision detection with shape caching and tile object support
- **Input**: Centralized input handling with previous/current state tracking for edge detection
- **Audio**: Sound effect instance management with automatic cleanup
- **Scenes**: Base scene class with per-scene content managers for automatic resource cleanup
- **Managers**: Room management system with spatial partitioning and exit detection
- **Utilities**: Spatial data structures (QuadTree) and debugging tools

## Critical Development Patterns

### Enhanced Collision Detection System
The library provides **multi-layered collision detection** with performance optimization:

```csharp
// Character sprite collision with tile objects
var exitTrigger = tilemap.GetFirstCollidingTileObject(
    player, 
    player.Position, 
    "Triggers",     // Object layer name
    "Exit",         // Specific object name (optional)
    mapPosition
);

// Multiple collision detection
var allTriggers = tilemap.GetAllCollidingTileObjects(
    player, player.Position, "Triggers", null, mapPosition);
```

**Key Features**:
- **Lazy caching**: Collision shapes cached on first use for performance
- **Multi-shape support**: Rectangle, Circle, Point, Polygon, Polyline, Text objects
- **Layer-based detection**: Separate collision and trigger layers
- **Name filtering**: Target specific objects by name or detect all

### Room Management System
**Abstract base class pattern** for room transitions and spatial management:

```csharp
public abstract class RoomManagerBase
{
    // Simple API - all data from CharacterSprite
    public abstract CollisionObject CheckExitCollisions(CharacterSprite player);
    
    // Event-driven transitions
    public event Action<string, string> OnRoomTransitionRequested;
    
    // Spatial optimization configuration
    protected virtual SpatialConfig GetSpatialConfigForMap(string mapName, Tilemap tilemap);
}
```

**Concrete implementation** with QuadTree optimization:
- Spatial partitioning for large maps with many exits
- Direction validation using dot product calculations
- Cooldown system to prevent exit bouncing
- Cache management for performance

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

### JSON Configuration System
`TextureAtlas` and `Tilemap` support **JSON-based configuration** for modern workflows:

```json
// Tilemap object layer with trigger configuration
{
  "objectLayers": [
    {
      "name": "Triggers",
      "visible": true,
      "objects": [
        {
          "name": "Exit_North",
          "objectType": "rectangle",
          "x": 320, "y": 0, "width": 64, "height": 32,
          "properties": {
            "targetRoom": "room_02",
            "spawnPoint": "South_Entrance"
          }
        }
      ]
    }
  ]
}
```

### Spatial Data Structures
**QuadTree implementation** for efficient spatial queries:

```csharp
// ISpatialObject interface for QuadTree compatibility
public interface ISpatialObject
{
    Rectangle Bounds { get; }
    Vector2 Position { get; }
}

// Usage in room management
var quadTree = new QuadTree<ExitWrapper>(mapBounds, maxObjects: 8, maxDepth: 6);
quadTree.Insert(exitWrapper);
quadTree.Query(playerPosition, searchRadius, results);
```

### Loading Methods
- **Combined files**: `FromXml(content, fileName)` or `FromJson(content, textureFile, animationFile)`
- **Separate files**: `FromXmlTexture()` + `LoadAnimationsFromXml()` or `FromJsonTexture()` + `LoadAnimationsFromJson()`

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

### When Adding Collision Detection Features
- Extend `ICollisionShape` interface for new collision shapes
- Implement both `IntersectsRectangle()` and `ContainsPoint()` methods
- Use caching pattern: check for cached shape before creating new ones
- Support both individual and batch collision detection
- Follow naming convention: `GetFirstColliding...()` and `GetAllColliding...()`

### When Adding Room Management Features
- Extend `RoomManagerBase` abstract class for new room managers
- Use `CharacterSprite` parameter only (position/velocity accessible from object)
- Implement spatial optimization with QuadTree for large maps
- Follow event-driven pattern with `OnRoomTransitionRequested` event
- Use property-based configuration for exit objects
- Implement proper cooldown and direction validation

### When Adding Input Features
- Implement previous/current state tracking pattern
- Provide `Was[Action]JustPressed/Released()` methods for edge detection
- Update state in `Update(GameTime)` methods

### When Adding Scene Features
- Extend abstract `Scene` base class
- Use scene-specific `Content` property for loading assets
- Implement proper disposal in `Dispose(bool disposing)` override

### When Adding Spatial Data Structures
- Implement `ISpatialObject` interface for QuadTree compatibility
- Use generic constraints for type safety
- Follow recursive subdivision pattern for tree structures
- Implement both rectangular and circular query methods

### Namespace Organization
- Root: `MonoGameLibrary` (Core, utilities)
- `MonoGameLibrary.Graphics` (rendering, sprites, animations)
- `MonoGameLibrary.Graphics.Collision` (collision detection system)
- `MonoGameLibrary.Graphics.Tiles` (tilemap and tileset support)
- `MonoGameLibrary.Input` (input management)
- `MonoGameLibrary.Audio` (sound management)
- `MonoGameLibrary.Scenes` (scene system)
- `MonoGameLibrary.Managers` (room management and spatial systems)
- `MonoGameLibrary.Utilities` (spatial data structures, debugging)

## Integration Points

### MonoGame Framework Dependencies
- Inherits from `Microsoft.Xna.Framework.Game`
- Uses MonoGame's content pipeline for asset loading
- Wraps MonoGame input/audio systems with higher-level APIs

### Key Design Decisions
- **Static access**: Prioritizes convenience over testability
- **XML/JSON configuration**: Enables designer-friendly asset definition and modern workflow support
- **Automatic cleanup**: Reduces memory leak potential through resource caching and disposal
- **Edge detection**: Provides game-friendly input handling
- **Performance optimization**: Lazy caching for collision shapes and spatial partitioning
- **Event-driven architecture**: Decoupled room transitions and trigger systems

### Performance Patterns
- **Lazy initialization**: Collision shapes created and cached on first use
- **Spatial optimization**: QuadTree for efficient spatial queries in large maps
- **Cache management**: Explicit cache clearing methods for runtime object modifications
- **Batch operations**: Support for both individual and bulk collision detection

Focus development on maintaining these established patterns rather than introducing new architectural approaches.