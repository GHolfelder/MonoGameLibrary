# Graphics Namespace

The Graphics namespace provides comprehensive 2D graphics functionality for MonoGame applications, including sprite rendering, animation systems, texture management, and tile-based graphics.

## Core Classes

### Texture Management
- **[TextureAtlas](TextureAtlas.cs)** - Manages sprite sheets and texture regions with XML/JSON configuration support
- **[TextureRegion](TextureRegion.cs)** - Represents a rectangular region within a texture for efficient sub-texture rendering

### Sprite System
- **[Sprite](Sprite.cs)** - Basic sprite class with rendering properties (position, rotation, scale, color, etc.)
- **[AnimatedSprite](AnimatedSprite.cs)** - Sprite with frame-based animation support
- **[Animation](Animation.cs)** - Manages animation frame data and timing

### Character Sprites (Game-Specific)
- **[CharacterSprite](CharacterSprite.cs)** - Base class for character sprites with multi-directional animations and placeholder support
- **[PlayerSprite](PlayerSprite.cs)** - Player-controlled character with input handling via `IInputProvider`
- **[NPCSprite](NPCSprite.cs)** - AI-controlled character using `IAIBehavior` interface

### Tile-Based Graphics
- **[Tilemap](Tiles/Tilemap.cs)** - Arranges tiles from a tileset into game levels with XML configuration support
- **[Tileset](Tiles/Tileset.cs)** - Manages collections of tiles from texture atlases

### Animation System
- **[AnimationState](AnimationState.cs)** - String constants for common animation states (idle, walk, run, attack, etc.)
- **[IAnimationResolver](IAnimationResolver.cs)** - Interface for loading animations from different sources
- **[IAnimationNameFormatter](IAnimationNameFormatter.cs)** - Interface for formatting animation names from state and direction

## Key Features

### Configuration Support
- **XML Configuration**: Load texture atlases and tilemaps from XML files
- **JSON Configuration**: Alternative JSON format for texture atlases and animations
- **Flexible Loading**: Support for combined or separate texture/animation files

### Animation System
- **Frame-based animations** with configurable timing
- **Per-frame timing** support for variable frame durations
- **Multi-directional character animations** (4-way or 8-way movement)
- **Missing animation handling** with placeholder sprites

### Rendering Pipeline
1. `TextureRegion` → Defines rectangular area in texture
2. `Sprite` → Adds rendering properties 
3. `AnimatedSprite` → Extends with frame-based animation
4. All rendering goes through MonoGame's `SpriteBatch`

## Usage Examples

### Loading a Texture Atlas
```csharp
// XML format
var atlas = TextureAtlas.FromXml(content, "character_atlas.xml");

// JSON format (separate files)
var atlas = TextureAtlas.FromJson(content, "sprites.json", "animations.json");
```

### Creating Sprites
```csharp
// Static sprite
var sprite = atlas.CreateSprite("player_idle");

// Animated sprite
var animatedSprite = atlas.CreateAnimatedSprite("player_walk_north");
```

### Character Sprite with Input
```csharp
var player = new PlayerSprite(
    atlas, 
    "player", 
    DirectionMode.EightWay,
    inputProvider,
    AnimationState.Idle, 
    AnimationState.Walk, 
    AnimationState.Run
);
```

### Tilemap Creation
```csharp
// Import the Tiles namespace
using MonoGameLibrary.Graphics.Tiles;

var tilemap = Tilemap.FromXml(content, "level1.xml");
tilemap.Draw(spriteBatch);
```

## Configuration Examples

### XML Texture Atlas
```xml
<TextureAtlas>
    <Texture>character_sheet</Texture>
    <Regions>
        <Region name="player_idle" x="0" y="0" width="32" height="32" />
    </Regions>
    <Animations>
        <Animation name="player_walk" delay="100">
            <Frame region="player_idle" />
        </Animation>
    </Animations>
</TextureAtlas>
```

### JSON Texture Configuration
```json
{
  "sprites": [
    {"name": "player_walk_N_0", "x": 0, "y": 0, "width": 64, "height": 64}
  ],
  "atlasFile": "../Content/images/atlas.png"
}
```

## Notes

The character sprite classes (PlayerSprite, NPCSprite) are currently marked as game-specific and may be moved to samples in future versions. They serve as examples of how to build game-specific functionality on top of the core graphics classes.