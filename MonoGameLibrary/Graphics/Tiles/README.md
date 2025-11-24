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
- **Static tile focus**: Optimized for static tile rendering without animation overhead
- **Automatic lookup**: Seamless integration with TextureAtlas texture regions

#### Multi-Layer Z-Ordering
- **Layer-based rendering**: Multiple tile layers in proper depth order
- **Character integration**: Draw entities between layers for depth illusion
- **Selective rendering**: Control which layers render for performance

#### Professional Workflow
- **Performance optimized**: Minimal texture switching during rendering
- **Scene-based**: Different JSON file for each game scene/level
- **Entity support**: Perfect for characters, NPCs, trees, walls, etc.

## Usage Examples

### Shared Atlas Pattern
```csharp
// Load texture atlas once in Scene or Core initialization
TextureAtlas gameAtlas = TextureAtlas.FromJsonTexture(Content, "atlas.json");

// Load multiple tilemaps using shared atlas
Tilemap backgroundMap = Tilemap.FromJson(Content, "maps/background.json", gameAtlas);
Tilemap foregroundMap = Tilemap.FromJson(Content, "maps/foreground.json", gameAtlas);

// All tilemaps share one texture - optimal performance!
backgroundMap.Draw(spriteBatch, Vector2.Zero);
foregroundMap.Draw(spriteBatch, Vector2.Zero);
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

## Object Layer Collision System

The tilemap system includes comprehensive object layer support for collision detection with multiple geometric shapes:

### Supported Object Types
- **Rectangle**: Standard collision boxes for walls and platforms
- **Circle/Ellipse**: Curved collision areas for rounded objects  
- **Polygon**: Complex shapes for irregular collision boundaries
- **Point**: Precise trigger points for events or spawns

### Object Layer JSON Format
```json
{
  "objectLayers": [
    {
      "name": "Collision",
      "visible": true,
      "objects": [
        {
          "name": "wall",
          "x": 100, "y": 100,
          "width": 64, "height": 32
        },
        {
          "name": "trigger",
          "x": 200, "y": 150,
          "width": 32, "height": 32,
          "ellipse": true
        },
        {
          "name": "spawn_point",
          "x": 300, "y": 200,
          "point": true
        },
        {
          "name": "platform",
          "x": 400, "y": 250,
          "polygon": [
            {"x": 0, "y": 0},
            {"x": 64, "y": 0},
            {"x": 32, "y": 32}
          ]
        }
      ]
    }
  ]
}
```

### Collision Detection Usage
```csharp
// Check sprite collision with object layer
if (tilemap.CheckSpriteObjectCollision(player, playerPosition, "Collision"))
{
    // Handle collision with walls, triggers, etc.
}

// Character sprite collision (with movement handling)
if (tilemap.CheckCharacterSpriteObjectCollision(character, characterPos, "Collision"))
{
    // Handle character movement blocking
}

// Visualize collision objects for debugging
tilemap.DrawObjectLayerAsCollision(spriteBatch, "Collision", Color.Red, tilemapPosition);
```

### Shape-Specific Features
- **Automatic shape detection** from JSON properties (`ellipse`, `point`, `polygon`)
- **Appropriate collision algorithms** for each shape type
- **Debug visualization** with shape-specific rendering
- **Performance optimization** using bounding box calculations

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

## Architecture Notes

This JSON-based tilemap system provides:

- **Shared texture atlas**: Single atlas used across all graphics classes for optimal performance
- **Singleton Core pattern**: Follows MonoGame Library's resource management philosophy  
- **Static tile rendering**: Optimized for performance without animation processing overhead
- **Professional z-ordering**: Layer-based depth rendering for character interaction
- **Memory efficiency**: Eliminates duplicate atlas loading across multiple tilemaps
- **Consistent patterns**: Follows library-wide dependency injection pattern for shared resources
- **Separation of concerns**: Static tiles handled separately from animated sprites
- **Performance focused**: Minimal texture switching during rendering

The system is optimized for **production games** requiring professional tilemap rendering with entities that interact naturally with the environment through proper depth sorting.