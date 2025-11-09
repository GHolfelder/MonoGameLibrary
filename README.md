# MonoGame Library

A comprehensive C# library for MonoGame development, providing utilities for graphics, input handling, audio, and scene management.

## Features

### Graphics
- **TextureAtlas**: Manage sprite sheets and texture regions with XML and JSON configuration support
- **Animation**: Frame-based animation system
- **Sprite & AnimatedSprite**: Sprite rendering with animation support
- **Tilemap & Tileset**: Tile-based level creation and rendering
- **TextureRegion**: Efficient texture sub-region management

### Input
- **InputManager**: Centralized input handling for keyboard, mouse, and gamepad
- **KeyboardInfo, MouseInfo, GamePadInfo**: Input state management

### Audio
- **AudioController**: Audio playback and management

### Scenes
- **Scene Management**: Basic scene system for game state management

### Core
- **Core Utilities**: Common game development utilities

## Getting Started

1. Reference this library in your MonoGame project
2. Initialize the systems you need (InputManager, AudioController, etc.)
3. Load texture atlases and tilemaps using the `FromXml` or `FromJson` methods
4. Create sprites and animations using the provided classes

## Configuration

### XML Configuration

#### Texture Atlas
```xml
<TextureAtlas>
    <Texture>path/to/texture</Texture>
    <Regions>
        <Region name="sprite1" x="0" y="0" width="32" height="32" />
    </Regions>
    <Animations>
        <Animation name="walk" delay="100">
            <Frame region="sprite1" />
        </Animation>
    </Animations>
</TextureAtlas>
```

#### Tilemap
```xml
<Tilemap>
    <Tileset region="0 0 256 256" tileWidth="32" tileHeight="32">path/to/tileset</Tileset>
    <Tiles>
        0 1 2
        3 4 5
    </Tiles>
</Tilemap>
```

### JSON Configuration

#### Texture Atlas (Separate Files)

**Texture JSON:**
```json
{
  "width": 1024,
  "height": 1024,
  "sprites": [
    {
      "name": "player_walk_N_0",
      "x": 0,
      "y": 0,
      "width": 64,
      "height": 64,
      "rotated": false
    }
  ],
  "atlasFile": "../Content/images/atlas.png"
}
```

**Animation JSON:**
```json
{
  "animations": [
    {
      "name": "player_walk_N",
      "frames": [
        {"sprite": "player_walk_N_0"},
        {"sprite": "player_walk_N_1"}
      ],
      "defaultDuration": 200,
      "loop": true
    }
  ]
}
```

### Loading Methods

#### XML Loading
```csharp
// Combined XML file (texture + animations)
var atlas = TextureAtlas.FromXml(content, "atlas.xml");

// Separate XML files
var atlas = TextureAtlas.FromXml(content, "texture.xml", "animations.xml");

// Load texture only, then add animations later
var atlas = TextureAtlas.FromXmlTexture(content, "texture.xml");
atlas.LoadAnimationsFromXml("walk_animations.xml");
atlas.LoadAnimationsFromXml("attack_animations.xml");
```

#### JSON Loading
```csharp
// Separate JSON files
var atlas = TextureAtlas.FromJson(content, "sprites.json", "animations.json");

// Load texture only, then add animations later
var atlas = TextureAtlas.FromJsonTexture(content, "sprites.json");
atlas.LoadAnimationsFromJson("walk_animations.json");
atlas.LoadAnimationsFromJson("attack_animations.json");
```

## Requirements

- .NET 9.0
- MonoGame Framework
- System.Text.Json (for JSON configuration support)

## License

[Add your license information here]