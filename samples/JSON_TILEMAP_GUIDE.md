# JSON Tilemap System with Z-Ordering

## Overview

The MonoGame Library now features a **completely redesigned tilemap system** that integrates seamlessly with texture atlases and provides professional z-ordering capabilities for proper depth rendering.

## Key Features

### ✅ **Texture Atlas Integration**
- Tilesets reference sprites within your texture atlas via `atlasSprite` property
- No need for separate tileset PNG files - everything is packed efficiently
- Automatic sprite region lookup from atlas definitions

### ✅ **Multi-Layer Support**
- Multiple tile layers rendered in proper back-to-front order
- Layer visibility, opacity, and offset controls
- Named layer access for dynamic manipulation

### ✅ **Professional Z-Ordering**
- Characters and entities can render between tile layers
- Trees, walls, and objects appear correctly in front/behind player
- Precise control over rendering order

### ✅ **Spacing & Margin Support**
- `SpacedTileset` automatically handles tileset spacing and margins
- Compatible with complex tileset layouts

## JSON Format Specification

### Map Structure
```json
{
  "name": "level1",
  "width": 30,
  "height": 20,
  "tileWidth": 32,
  "tileHeight": 32,
  "orientation": "orthogonal",
  "backgroundColor": "#2e3440",
  "atlasFile": "atlas.png",
  "tilesets": [...],
  "tileLayers": [...],
  "objectLayers": [],
  "imageLayers": [],
  "properties": {}
}
```

### Tileset Definition
```json
{
  "name": "glass",
  "firstGid": 1,
  "tileWidth": 32,
  "tileHeight": 32,
  "tileCount": 6,
  "columns": 3,
  "margin": 0,
  "spacing": 0,
  "atlasSprite": "level0_GlassAllx0001",
  "tiles": [],
  "properties": {}
}
```

### Tile Layer Definition
```json
{
  "id": 1,
  "name": "Background",
  "width": 30,
  "height": 20,
  "opacity": 1.0,
  "visible": true,
  "offsetX": 0,
  "offsetY": 0,
  "tiles": [0, 0, 1, 2, 5, 2, 0, ...],
  "properties": {}
}
```

## Usage Examples

### Basic Loading
```csharp
// Load tilemap with automatic atlas integration
Tilemap tilemap = Tilemap.FromJson(Content, "maps/level1.json");

// Draw entire tilemap (all layers, back to front)
tilemap.Draw(spriteBatch, Vector2.Zero);
```

### Advanced Z-Ordering for Characters
```csharp
public class GameScene
{
    private Tilemap _tilemap;
    private Player _player;

    public void LoadContent()
    {
        _tilemap = Tilemap.FromJson(Content, "maps/forest.json");
        _player = new Player();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw background layers (ground, grass, etc.)
        _tilemap.DrawLayersUpTo(spriteBatch, Vector2.Zero, 0);

        // Draw player character
        _player.Draw(spriteBatch);

        // Draw foreground layers (trees, walls, roofs)
        _tilemap.DrawLayersFrom(spriteBatch, Vector2.Zero, 1);
    }
}
```

### Dynamic Layer Control
```csharp
// Get specific layers
TileLayer backgroundLayer = tilemap.GetLayerByName("Background");
TileLayer foregroundLayer = tilemap.GetLayerByName("Foreground");

// Control visibility
backgroundLayer.Visible = false;
foregroundLayer.Opacity = 0.5f;

// Draw specific layer
tilemap.DrawLayer(spriteBatch, backgroundLayer, position);
```

### Multi-Entity Z-Ordering
```csharp
public void Draw(SpriteBatch spriteBatch)
{
    // Layer 0: Ground tiles
    tilemap.DrawLayersUpTo(spriteBatch, mapPosition, 0);
    
    // Draw entities at ground level (items, effects)
    DrawGroundEntities(spriteBatch);
    
    // Layer 1: Character level (walls, furniture)
    tilemap.DrawLayer(spriteBatch, tilemap.GetLayerByIndex(1), mapPosition);
    
    // Draw characters and NPCs
    player.Draw(spriteBatch);
    foreach (var npc in npcs)
        npc.Draw(spriteBatch);
    
    // Layer 2+: Foreground (tree tops, roofs, overhangs)
    tilemap.DrawLayersFrom(spriteBatch, mapPosition, 2);
    
    // Draw flying entities (birds, UI elements)
    DrawFlyingEntities(spriteBatch);
}
```

## Layer Organization Strategy

### Recommended Layer Setup

1. **Layer 0 - Ground**: Floor tiles, grass, water, base terrain
2. **Layer 1 - Obstacles**: Walls, furniture, objects at character height
3. **Layer 2 - Overhead**: Tree canopies, roofs, bridges, overhangs
4. **Layer 3 - Sky**: Clouds, distant background elements

### Character Positioning
- **Behind walls**: Character renders between Layer 0 and Layer 1
- **Under trees**: Character renders between Layer 1 and Layer 2
- **Indoor scenes**: Character renders between furniture (Layer 1) and ceiling (Layer 2)

## Workflow Integration

### Texture Atlas Requirements
```json
// Your atlas.json must contain tileset sprites
{
  "sprites": [
    {"name": "level0_GlassAllx0001", "x": 0, "y": 0, "width": 96, "height": 64}
  ]
}
```

### Content Organization
```
Content/
├── atlas.json              // Texture atlas sprite definitions
├── atlas_animations.json   // Animation definitions
├── atlas.png              // Packed texture file
└── maps/
    ├── level1.json        // Tilemap definition
    ├── forest.json        // Forest level
    └── dungeon.json       // Dungeon level
```

## Performance Benefits

1. **Single Texture**: All tiles from one atlas - minimal texture switching
2. **Efficient Batching**: SpriteBatch can batch efficiently within layers
3. **Selective Rendering**: Only draw visible layers for performance
4. **Memory Efficiency**: No duplicate tile texture data

## Migration from XML Tilesets

The new JSON system completely replaces XML-based tilesets while providing:
- Better performance through atlas integration
- Professional z-ordering capabilities
- More flexible layer management
- Easier content pipeline integration

This system is designed for **production games** requiring professional tilemap rendering with proper depth sorting for characters and interactive objects.