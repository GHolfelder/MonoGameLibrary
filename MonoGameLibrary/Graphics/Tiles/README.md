# Graphics.Tiles Namespace

The `MonoGameLibrary.Graphics.Tiles` namespace provides comprehensive tilemap and tileset functionality for 2D games, with support for both traditional XML workflows and modern JSON-based texture atlas integration.

## Core Classes

### Tilemap
The main tilemap class supporting multiple rendering approaches:

- **JSON-based loading** with texture atlas integration
- **Multi-layer support** for professional z-ordering
- **Proper depth rendering** for characters behind walls/trees
- **Layer management** with visibility and opacity controls

### Tileset & SpacedTileset
Tileset implementations supporting various layout formats:

- **Tileset**: Standard evenly-spaced tile grids
- **SpacedTileset**: Handles spacing and margins between tiles
- **ITileset Interface**: Common interface for seamless switching

### TileLayer
Represents individual tile layers with:

- Layer properties (name, visibility, opacity, offset)
- Tile data arrays with global ID references
- Custom properties and metadata support

## Supported Formats

### JSON Tilemap Format
Modern approach using texture atlases:

```json
{
  "name": "level1",
  "width": 30,
  "height": 20,
  "tileWidth": 32,
  "tileHeight": 32,
  "orientation": "orthogonal",
  "atlasFile": "atlas.png",
  "tilesets": [
    {
      "name": "terrain",
      "firstGid": 1,
      "atlasSprite": "terrain_tileset",
      "tileWidth": 32,
      "tileHeight": 32,
      "columns": 4,
      "spacing": 0,
      "margin": 0
    }
  ],
  "tileLayers": [
    {
      "name": "Ground",
      "tiles": [1, 2, 3, 4, ...]
    }
  ]
}
```

### Key Features

#### Texture Atlas Integration
- **Single texture**: All tiles from packed atlas for optimal performance
- **Sprite references**: Tilesets reference named sprites via `atlasSprite`
- **Automatic lookup**: Seamless integration with TextureAtlas system

#### Multi-Layer Z-Ordering
- **Layer-based rendering**: Multiple tile layers in proper depth order
- **Character integration**: Draw entities between layers for depth illusion
- **Selective rendering**: Control which layers render for performance

#### Professional Workflow
- **Performance optimized**: Minimal texture switching during rendering
- **Scene-based**: Different JSON file for each game scene/level
- **Entity support**: Perfect for characters, NPCs, trees, walls, etc.

## Usage Examples

### Basic Loading
```csharp
// Load tilemap from JSON with automatic atlas integration
Tilemap tilemap = Tilemap.FromJson(Content, "maps/level1.json");

// Draw all layers in order
tilemap.Draw(spriteBatch, Vector2.Zero);
```

### Z-Ordering for Character Depth
```csharp
// Draw background layers (ground, floors)
tilemap.DrawLayersUpTo(spriteBatch, mapPosition, 0);

// Draw characters and entities
player.Draw(spriteBatch);
foreach(var npc in npcs) npc.Draw(spriteBatch);

// Draw foreground layers (walls, tree canopies, roofs)
tilemap.DrawLayersFrom(spriteBatch, mapPosition, 1);
```

### Dynamic Layer Control
```csharp
// Access layers by name or index
TileLayer backgroundLayer = tilemap.GetLayerByName("Background");
TileLayer foregroundLayer = tilemap.GetLayerByIndex(1);

// Control visibility and opacity
backgroundLayer.Visible = false;
foregroundLayer.Opacity = 0.5f;

// Draw specific layers
tilemap.DrawLayer(spriteBatch, backgroundLayer, position);
```

## File Organization

### Recommended Content Structure
```
Content/
├── atlas.json             # Texture atlas definitions
├── animations.json        # Animation definitions  
├── atlas.png              # Packed texture file
└── maps/
    ├── level1.json        # Level 1 tilemap
    ├── forest.json        # Forest area tilemap
    └── dungeon.json       # Dungeon tilemap
```

### Integration with TextureAtlas
The tilemap system integrates seamlessly with the existing TextureAtlas functionality:

1. **Atlas Loading**: Automatically loads texture atlas during tilemap creation
2. **Sprite Lookup**: Uses `atlasSprite` names to find texture regions
3. **Performance**: Single texture for entire tilemap reduces draw calls

## Advanced Features

### Layer Organization Strategy
- **Layer 0**: Ground tiles (grass, stone, water)
- **Layer 1**: Character-level obstacles (walls, furniture)  
- **Layer 2+**: Overhead elements (tree canopies, roofs)

### Entity Positioning
- Render characters between ground and obstacle layers
- Draw environmental objects (trees) with split rendering:
  - Trunk at character level
  - Canopy in foreground layer

### Performance Optimization
- **Efficient batching**: SpriteBatch optimization within layers
- **Selective rendering**: Only draw visible/active layers
- **Memory efficiency**: No duplicate texture data

## Spacing and Margin Support

The `SpacedTileset` class automatically handles tilesets with spacing and margins:

```json
{
  "spacing": 2,    // 2 pixels between tiles
  "margin": 1      // 1 pixel border around tileset
}
```

This is essential for tilesets exported from tools like Tiled Map Editor or when using pre-made tileset graphics with built-in spacing.

## Migration Notes

This JSON-based system is designed to **replace** traditional XML-based tilemap workflows while providing:

- Better performance through texture atlas integration
- Professional z-ordering capabilities for character depth
- More flexible layer management system
- Easier integration with content pipelines
- Support for complex game scenarios requiring proper depth rendering

The system is optimized for **production games** requiring professional tilemap rendering with entities that interact naturally with the environment through proper depth sorting.