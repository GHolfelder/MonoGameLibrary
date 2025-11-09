# MonoGame Library

A comprehensive C# library for MonoGame development, providing utilities for graphics, input handling, audio, and scene management.

## Features

### Graphics
- **TextureAtlas**: Manage sprite sheets and texture regions with XML configuration support
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
3. Load texture atlases and tilemaps using the `FromXml` methods
4. Create sprites and animations using the provided classes

## XML Configuration

### Texture Atlas
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

### Tilemap
```xml
<Tilemap>
    <Tileset region="0 0 256 256" tileWidth="32" tileHeight="32">path/to/tileset</Tileset>
    <Tiles>
        0 1 2
        3 4 5
    </Tiles>
</Tilemap>
```

## Requirements

- .NET 9.0
- MonoGame Framework

## License

[Add your license information here]