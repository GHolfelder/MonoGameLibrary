# Graphics.Tiles Namespace

The `MonoGameLibrary.Graphics.Tiles` namespace provides comprehensive tilemap and tileset functionality for 2D games, with support for both traditional XML workflows and modern JSON-based texture atlas integration. This includes automatic support for animated tiles that require no changes to existing game code.

## Modular Architecture

The tilemap system has been refactored into focused, single-responsibility modules for better maintainability and discoverability:

### Core Classes

#### Tilemap (Tilemap.cs)
The main tilemap class handling core functionality:
- **Rendering methods**: `Draw()`, `DrawLayer()`, `DrawLayersUpTo()`, `DrawLayersFrom()`
- **Layer access**: `GetLayerByName()`, `GetLayerByIndex()`, `GetObjectLayer()`
- **Animation management**: `Update()`, `CreateAnimatedTileInstances()`
- **Data access**: `GetCollisionObjects()`, `FindFirstCollisionObject()`, `GetTileData()`, `GetTileDataAt()`
- **Properties support**: `GetProperty<T>()`, `SetProperty()`, `HasProperty()` for tilemap-level custom properties
- **Core properties**: Map dimensions, tile sizes, scaling, background color

#### Tilemap JSON Loader (Tilemap.JsonLoader.cs)
JSON parsing functionality as partial class:
- **Static factory methods**: `FromJson()` for loading tilemap collections
- **JSON parsing**: Map, tileset, layer, and object parsing
- **Format validation**: Array-based JSON structure handling
- **Color parsing**: Hex color string conversion

#### TilemapCollection (TilemapCollection.cs)
Collection management for multiple tilemaps:
- **Map storage**: Dictionary-based storage with case-insensitive keys
- **Access patterns**: By name `collection["mapName"]` or index `collection[0]`
- **Safe retrieval**: `TryGetMap()` for error-safe access
- **Enumeration**: `MapNames` property and `Count` for iteration

#### Animation System (AnimatedTile.cs)
Frame-based tile animation components:
- **AnimatedTileFrame**: Individual animation frames with timing and source coordinates
- **AnimatedTile**: Tile definition with animation sequence and properties
- **AnimatedTileInstance**: Per-instance animation state management with atlas region support

#### Layer Data Structures (TileLayer.cs)
Tile layer and data definitions:
- **TileLayer**: Layer properties (name, dimensions, opacity, visibility, offset, tile data, custom properties)
- **TileData**: Tile-specific data including collision objects, custom properties, and helper methods (`GetProperty<T>()`, `SetProperty()`, `HasProperty()`)

#### Object System (ObjectLayer.cs)
Object layer and collision object definitions:
- **CollisionObjectType**: Enumeration of supported shapes (Rectangle, Ellipse, Point, Polygon, Polyline, Tile, Text)
- **CollisionObject**: Object properties, geometry, rotation, custom properties, cached collision shapes
- **ObjectLayer**: Object container with layer properties and object collections

#### Tileset Definitions (TilesetDefinition.cs)
Tileset structure and metadata:
- **TilesetDefinition**: Tileset properties (name, GID range, dimensions, atlas sprite reference, tile definitions)

### Legacy Support

#### Tileset & SpacedTileset
Tileset implementations supporting various layout formats:
- **Tileset**: Standard evenly-spaced tile grids
- **SpacedTileset**: Handles spacing and margins between tiles
- **ITileset Interface**: Common interface for seamless switching

## Supported Formats

### JSON Tilemap Format
Modern approach using texture atlases with animated tile support. JSON files now contain arrays of maps:

```json
[
  {
    "name": "level1",
    "width": 30,
    "height": 20,
    "tileWidth": 32,
    "tileHeight": 32,
    "orientation": "orthogonal",
    "tilesets": [
      {
        "name": "terrain",
        "firstGid": 1,
        "atlasSprite": "terrain_tileset",
        "tileWidth": 32,
        "tileHeight": 32,
        "columns": 4,
        "spacing": 0,
        "margin": 0,
        "tiles": [
          {
            "id": 0,
            "type": null,
            "atlasSprite": null,
            "animation": [
              {
                "tileId": 0,
                "duration": 100,
                "sourceX": 0,
                "sourceY": 0,
                "sourceWidth": 32,
                "sourceHeight": 32
              },
              {
                "tileId": 1,
                "duration": 100,
                "sourceX": 32,
                "sourceY": 0,
                "sourceWidth": 32,
                "sourceHeight": 32
            }
          ]
        }
      ]
    }
  ],
  "tileLayers": [
    {
      "name": "Ground",
      "tiles": [1, 2, 3, 4, ...]
    }
  ]
  },
  {
    "name": "level2",
    // Additional map with basic structure
  }
]
```

**Multiple Maps Support**: JSON files now contain arrays of map objects, allowing multiple related maps to be loaded together. Access maps by name using `tilemaps["mapName"]` or by index using `tilemaps[0]`.

### Key Features

#### Texture Atlas Integration
- **Single texture**: All tiles from packed atlas for optimal performance
- **Sprite references**: Tilesets reference named sprites via `atlasSprite`
- **Automatic animation**: Animated tiles are processed automatically without game code changes
- **Automatic lookup**: Seamless integration with TextureAtlas texture regions

#### Animated Tiles
- **Frame-based animation**: Define animation sequences with custom frame durations
- **Automatic cycling**: Tiles animate automatically when Update() is called on the tilemap
- **Per-tile animation**: Each animated tile instance tracks its own animation state
- **Zero-overhead for static tiles**: Regular tiles have no animation performance cost

#### Animation JSON Format
```json
{
  "tiles": [
    {
      "id": 0,
      "animation": [
        {
          "tileId": 0,
          "duration": 100,
          "sourceX": 0,
          "sourceY": 0,
          "sourceWidth": 32,
          "sourceHeight": 32
        },
        {
          "tileId": 1,
          "duration": 150,
          "sourceX": 32,
          "sourceY": 0,
          "sourceWidth": 32,
          "sourceHeight": 32
        }
      ]
    }
  ]
}
```

#### Multi-Layer Z-Ordering
- **Layer-based rendering**: Multiple tile layers in proper depth order
- **Character integration**: Draw entities between layers for depth illusion
- **Selective rendering**: Control which layers render for performance

#### Custom Render Order (RenderOrder)
- **Flexible layer ordering**: Override default layer rendering order via properties
- **Tiled Map Editor compatibility**: Handle any layer order from map editors
- **Runtime control**: Modify render order programmatically
- **Validation**: Automatic validation of layer names with helpful error messages
- **Backward compatibility**: Falls back to default order when not specified

#### Professional Workflow
- **Performance optimized**: Minimal texture switching during rendering
- **Scene-based**: Different JSON file for each game scene/level
- **Entity support**: Perfect for characters, NPCs, trees, walls, etc.

## Usage Examples

### Shared Atlas Pattern
```csharp
// Load texture atlas once in Scene or Core initialization
TextureAtlas gameAtlas = TextureAtlas.FromJsonTexture(Content, "atlas.json");

// Load multiple tilemap collections using shared atlas
var backgroundMaps = Tilemap.FromJson(Content, "maps/background.json", gameAtlas);
var foregroundMaps = Tilemap.FromJson(Content, "maps/foreground.json", gameAtlas);

// Access specific maps by name or index
Tilemap backgroundMap = backgroundMaps["background"]; // or backgroundMaps[0]
Tilemap foregroundMap = foregroundMaps["foreground"]; // or foregroundMaps[0]

// In your Update loop - required for animated tiles
backgroundMap.Update(gameTime);
foregroundMap.Update(gameTime);

// All tilemaps share one texture - optimal performance!
backgroundMap.Draw(spriteBatch, Vector2.Zero);
foregroundMap.Draw(spriteBatch, Vector2.Zero);
```

### Animated Tiles Usage
```csharp
// Simply call Update() in your game loop - animations work automatically
public override void Update(GameTime gameTime)
{
    tilemap.Update(gameTime);  // Handles all animated tiles automatically
    
    // Your other game logic here...
    base.Update(gameTime);
}

public override void Draw(GameTime gameTime)
{
    spriteBatch.Begin();
    tilemap.Draw(spriteBatch, Vector2.Zero);  // Animated tiles render automatically
    spriteBatch.End();
}
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

### Custom Render Order Control

The RenderOrder property allows you to override the default layer rendering order, providing perfect compatibility with Tiled Map Editor and flexible control over layer z-ordering.

#### Setting Render Order
```csharp
// Set render order programmatically
tilemap.SetRenderOrder("background, middleground, foreground");
tilemap.SetRenderOrder(new[] { "ground", "walls", "ceiling" });

// Access as property
var renderOrder = tilemap.GetProperty<string>("renderOrder", "");

// Clear custom render order (reverts to default)
tilemap.ClearRenderOrder();
```

#### Drawing with Custom Order
```csharp
// With renderOrder: "background, middleground, foreground"
// These methods now respect the custom render order

// Draw up to middleground layer (background + middleground)
tilemap.DrawLayersUpTo(spriteBatch, position, "middleground");

// Draw characters and sprites
player.Draw(spriteBatch);
foreach(var sprite in gameSprites) sprite.Draw(spriteBatch);

// Draw from foreground layer onwards (foreground only)
tilemap.DrawLayersFrom(spriteBatch, position, "foreground");
```

#### Tiled Map Editor Compatibility
```csharp
// Tiled typically exports layers in visual order (top to bottom)
// Use renderOrder to specify proper back-to-front rendering:
// JSON: "renderOrder": "background, middleground, foreground"

// This handles any layer order from map editors automatically
var tilemaps = Tilemap.FromJson(Content, "level1.json", atlas);
var tilemap = tilemaps["level1"];

// Standard z-ordering now works regardless of JSON layer order
tilemap.DrawLayersUpTo(spriteBatch, position, 1);  // background layers
player.Draw(spriteBatch);                          // character
tilemap.DrawLayersFrom(spriteBatch, position, 2);  // foreground layers
```

#### JSON RenderOrder Configuration
```json
{
  "name": "level1",
  "properties": {
    "renderOrder": "background, middleground, foreground",
    "difficulty": "hard"
  },
  "tileLayers": [
    { "name": "foreground" },    // Tiled visual order
    { "name": "middleground" }, 
    { "name": "background" }
  ]
}

// Alternative: top-level property for convenience
{
  "name": "level1",
  "renderOrder": "background, middleground, foreground",
  "tileLayers": [...]
}
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

### Properties System Usage

The tilemap system provides comprehensive support for arbitrary properties at multiple levels, including the special **RenderOrder** property for layer control:

#### Tilemap-Level Properties
```csharp
// Access tilemap properties with type safety
var tilemap = Tilemap.FromJson(Content, "maps/level1.json", atlas)["level1"];
var difficulty = tilemap.GetProperty<string>("difficulty", "normal");
var timeLimit = tilemap.GetProperty<int>("timeLimit", 600);
var hasSecrets = tilemap.GetProperty<bool>("hasSecrets", false);

// Set properties at runtime
tilemap.SetProperty("playerVisited", true);
tilemap.SetProperty("completionTime", gameTime.TotalGameTime.TotalSeconds);
```

#### Individual Tile Properties
```csharp
// Access tile properties by position (most common)
var tileData = tilemap.GetTileDataAt("Ground", tileX, tileY);
if (tileData != null)
{
    var damage = tileData.GetProperty<int>("damage", 0);
    var walkSpeed = tileData.GetProperty<float>("walkSpeed", 1.0f);
    var biome = tileData.GetProperty<string>("biome", "default");
    var isWalkable = tileData.GetProperty<bool>("walkable", true);
    
    // Modify tile properties at runtime
    tileData.SetProperty("visited", true);
    
    // Apply game logic
    if (damage > 0) player.TakeDamage(damage);
    if (walkSpeed != 1.0f) player.ModifySpeed(walkSpeed);
}

// Access by GID (global tile ID)
if (tilemap.GetTileData(gid, out TileData tileData))
{
    var specialEffect = tileData.GetProperty<string>("effect", "");
    if (!string.IsNullOrEmpty(specialEffect))
    {
        EffectManager.TriggerEffect(specialEffect);
    }
}
```

#### Layer Properties
```csharp
// Tile layer properties
var groundLayer = tilemap.GetLayerByName("Ground");
var scrollSpeed = groundLayer.Properties.GetValueOrDefault("scrollSpeed", 1.0f);
var parallaxFactor = (float)groundLayer.Properties.GetValueOrDefault("parallax", 1.0f);

// Object layer properties
var collisionLayer = tilemap.GetObjectLayer("Collision");
var enablePhysics = (bool)collisionLayer.Properties.GetValueOrDefault("enablePhysics", true);
var gravityModifier = (float)collisionLayer.Properties.GetValueOrDefault("gravity", 1.0f);
```

#### Object Properties
```csharp
// Access object properties for game logic
var exitTriggers = tilemap.GetCollisionObjects("Triggers", "Exit");
foreach (var trigger in exitTriggers)
{
    var targetRoom = (string)trigger.Properties.GetValueOrDefault("targetRoom", "");
    var spawnPoint = (string)trigger.Properties.GetValueOrDefault("spawnPoint", "default");
    var requiresKey = (bool)trigger.Properties.GetValueOrDefault("requiresKey", false);
    
    if (player.Intersects(trigger) && (!requiresKey || player.HasKey))
    {
        SceneManager.ChangeRoom(targetRoom, spawnPoint);
    }
}
```

#### JSON Properties Format
```json
{
  "name": "level1",
  "width": 30,
  "height": 20,
  "properties": {
    "difficulty": "hard",
    "timeLimit": 300,
    "hasSecrets": true,
    "recommendedLevel": 5,
    "bgMusic": "dungeon_theme"
  },
  "tilesets": [{
    "name": "terrain",
    "firstGid": 1,
    "tiles": [{
      "id": 5,
      "properties": {
        "damage": 10,
        "walkSpeed": 0.5,
        "biome": "lava",
        "effect": "burn"
      }
    }]
  }],
  "tileLayers": [{
    "name": "Ground",
    "properties": {
      "scrollSpeed": 0.8,
      "parallax": 0.5
    }
  }],
  "objectLayers": [{
    "name": "Triggers",
    "properties": {
      "enablePhysics": false,
      "debugVisible": true
    },
    "objects": [{
      "name": "Exit_North",
      "objectType": "rectangle",
      "properties": {
        "targetRoom": "level2",
        "spawnPoint": "South_Entrance",
        "requiresKey": true
      }
    }]
  }]
}
```

## File Organization

### Recommended Content Structure
```
Content/
├── atlas.json             # Texture atlas definitions
├── animations.json        # Animation definitions  
├── atlas.png              # Packed texture file
└── maps/
    ├── level1.json        # Level 1 tilemaps (array of maps)
    ├── forest_area.json   # Forest area tilemaps (array of maps)
    └── dungeon_set.json   # Dungeon tilemaps (array of maps)
```

**Multiple Maps Per File**: Each JSON file now contains an array of related maps. For example, `forest_area.json` might contain `["forest_entrance", "forest_clearing", "forest_deep"]` maps that can be accessed individually while sharing the same texture atlas.

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
          "objectType": "rectangle",
          "x": 100, "y": 100,
          "width": 64, "height": 32
        },
        {
          "name": "trigger",
          "objectType": "ellipse",
          "x": 200, "y": 150,
          "width": 32, "height": 32
        },
        {
          "name": "spawn_point",
          "objectType": "point",
          "x": 300, "y": 200,
          "width": 0, "height": 0
        },
        {
          "name": "platform",
          "objectType": "polygon",
          "x": 400, "y": 250,
          "polygon": [
            {"x": 0, "y": 0},
            {"x": 64, "y": 0},
            {"x": 32, "y": 32}
          ]
        },
        {
          "name": "path",
          "objectType": "polyline",
          "x": 500, "y": 300,
          "polyline": [
            {"x": 0, "y": 0},
            {"x": 32, "y": 16},
            {"x": 64, "y": 32}
          ]
        },
        {
          "name": "tile_object",
          "objectType": "tile",
          "x": 600, "y": 350,
          "width": 32, "height": 32,
          "gid": 5
        },
        {
          "name": "sign_text",
          "objectType": "text",
          "x": 700, "y": 400,
          "width": 80, "height": 20,
          "text": {
            "content": "Welcome to the game!"
          }
        }
      ]
    }
  ]
}
```

### Shape Detection Logic
The parser uses this **priority order** for object type detection:

1. **objectType property** (preferred): Explicit string values:
   - `"rectangle"` → `CollisionObjectType.Rectangle`
   - `"ellipse"` → `CollisionObjectType.Ellipse` 
   - `"point"` → `CollisionObjectType.Point`
   - `"polygon"` → `CollisionObjectType.Polygon`
   - `"polyline"` → `CollisionObjectType.Polyline`
   - `"tile"` → `CollisionObjectType.Tile`
   - `"text"` → `CollisionObjectType.Text`

2. **Legacy detection** (fallback when no `objectType` property):
   - Polygon objects: Detected by presence of `polygon` array property
   - Polyline objects: Detected by presence of `polyline` array property  
   - Point objects: Detected by `width: 0` and `height: 0`
   - Ellipse/Circle objects: Detected by `ellipse: true` or name containing "Ellipse"/"Circle"
   - Rectangle objects: Default fallback

### Supported Object Types
- **Rectangle**: Standard collision boxes for walls and platforms
- **Ellipse/Circle**: Curved collision areas for rounded objects
- **Point**: Precise trigger points for events or spawns  
- **Polygon**: Complex shapes for irregular collision boundaries
- **Polyline**: Path-based objects for routes and boundaries
- **Tile**: Tile-based objects with GID references
- **Text**: Text objects with collision bounds and content parsing

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

This modular tilemap system provides:

### **Modular Design Benefits**
- **Single responsibility**: Each class handles one specific concern (rendering, data, animation, JSON parsing)
- **Maintainability**: Changes to JSON parsing won't affect rendering code
- **Discoverability**: Developers can find specific functionality easily in focused files
- **Testability**: Individual components can be tested in isolation
- **API preservation**: All existing public APIs remain unchanged after refactoring

### **File Organization**
- **Tilemap.cs** (455 lines): Core functionality - rendering, layer access, animation management, properties support
- **Tilemap.JsonLoader.cs**: JSON parsing as partial class - maintains single logical unit, includes properties parsing
- **TilemapCollection.cs**: Collection management for multiple maps
- **AnimatedTile.cs**: Complete animation system in dedicated module
- **TileLayer.cs & ObjectLayer.cs**: Separated data structures by responsibility with properties support
- **TilesetDefinition.cs**: Isolated tileset metadata structure

### **Properties System Features**
- **Type-safe access**: Generic `GetProperty<T>()` methods with automatic type conversion
- **Multi-level support**: Properties at tilemap, layer, tile, and object levels
- **Runtime modification**: `SetProperty()` and property existence checking with `HasProperty()`
- **JSON integration**: Automatic parsing and loading of properties from JSON format
- **Helper methods**: Convenient access patterns for common use cases (`GetTileDataAt()`, etc.)
- **Default value support**: Fallback values when properties don't exist
- **RenderOrder support**: Special property for custom layer rendering order and Tiled compatibility

### **Performance Features**
- **Shared texture atlas**: Single atlas used across all graphics classes for optimal performance
- **Singleton Core pattern**: Follows MonoGame Library's resource management philosophy  
- **Lazy caching**: Collision shapes cached on first use for performance optimization
- **Professional z-ordering**: Layer-based depth rendering for character interaction
- **Memory efficiency**: Eliminates duplicate atlas loading across multiple tilemaps
- **Consistent patterns**: Follows library-wide dependency injection pattern for shared resources
- **Separation of concerns**: Static tiles handled separately from animated sprites
- **Performance focused**: Minimal texture switching during rendering

The system is optimized for **production games** requiring professional tilemap rendering with entities that interact naturally with the environment through proper depth sorting, while maintaining clean, maintainable code architecture.