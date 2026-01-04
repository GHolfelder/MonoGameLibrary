# Enhanced Tile Object Collision Detection

The MonoGame Library now supports advanced collision detection between CharacterSprite objects and arbitrary tile objects in tilemap object layers. This enhancement enables room managers, trigger systems, and interactive object detection with performance optimization through caching.

## Overview

The enhanced collision system provides:

1. **Flexible Collision Detection**: Detect collisions with specific named objects or all objects in a layer
2. **Performance Caching**: Collision shapes are cached on first use for optimal performance
3. **Multiple Object Support**: Support for rectangles, circles, points, polygons, polylines, and text objects
4. **Room Management**: Perfect for implementing exits, doors, triggers, and interactive areas

## New Methods

### SpriteCollision Class

```csharp
// Check collision with rectangular bounds
bool IntersectsRectangle(Vector2 worldPosition, Rectangle tileBounds)

// Check if collision shape contains a point
bool ContainsPoint(Vector2 worldPosition, Vector2 point)
```

### TilemapCollisionExtensions Class

```csharp
// Get first colliding object (returns null if no collision)
CollisionObject GetFirstCollidingTileObject(
    CharacterSprite characterSprite,
    Vector2 spritePosition, 
    string objectLayerName,
    string objectName = null,  // Optional: filter by object name
    Vector2 tilemapPosition = default)

// Get all colliding objects (returns empty list if no collision)
List<CollisionObject> GetAllCollidingTileObjects(
    CharacterSprite characterSprite,
    Vector2 spritePosition,
    string objectLayerName, 
    string objectName = null,  // Optional: filter by object name
    Vector2 tilemapPosition = default)

// Cache management
void ClearAllCollisionShapeCache(string objectLayerName = null)
```

### CollisionObject Extensions

```csharp
// Clear cached collision shape for a specific object
void ClearCollisionShapeCache()
```

## Usage Examples

### Basic Exit Detection

```csharp
// Check if player touches any Exit object
var exit = tilemap.GetFirstCollidingTileObject(
    player, 
    playerPosition, 
    "Triggers", 
    "Exit",      // Specific object name
    mapPosition
);

if (exit != null)
{
    // Handle room transition
    string targetRoom = exit.Properties["targetRoom"].ToString();
    LoadRoom(targetRoom);
}
```

### Multiple Trigger Detection

```csharp
// Get all trigger collisions at once
var triggers = tilemap.GetAllCollidingTileObjects(
    player,
    playerPosition,
    "Triggers",  // Layer name
    null,        // Any object name
    mapPosition
);

foreach (var trigger in triggers)
{
    HandleTrigger(trigger.Name, trigger.Properties);
}
```

### Specific Object Detection

```csharp
// Check for specific door interaction
var door = tilemap.GetFirstCollidingTileObject(
    player,
    playerPosition,
    "Interactive",
    "Door_North",  // Exact object name
    mapPosition
);

if (door != null && HasKey("brass_key"))
{
    OpenDoor(door);
}
```

## Performance Optimization

The system uses **lazy caching** to optimize performance:

1. **First Access**: Collision shapes are created and cached when first accessed
2. **Subsequent Access**: Cached shapes are reused, eliminating object creation overhead
3. **Cache Management**: Clear cache when objects are modified at runtime

```csharp
// Clear cache when modifying objects
tilemap.ClearAllCollisionShapeCache("Triggers");

// Or clear cache for specific object
collisionObject.ClearCollisionShapeCache();
```

## Supported Object Types

| Object Type | Collision Shape | Notes |
|-------------|----------------|--------|
| Rectangle | CollisionRectangle | Standard rectangular collision |
| Ellipse (Circle) | CollisionCircle | When width ≈ height |
| Ellipse (Oval) | CollisionRectangle | Fallback to bounding rectangle |
| Point | CollisionCircle | 1-pixel radius circle |
| Polygon | CollisionRectangle | Uses bounding box for performance |
| Polyline | Special handling | Line-segment intersection |
| Tile | CollisionRectangle | Tile object collision |
| Text | CollisionRectangle | Text bounding box |

## Object Layer Configuration

Design your tilemaps with dedicated object layers for different interaction types:

```json
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
    },
    {
      "name": "Interactive",
      "visible": true,
      "objects": [
        {
          "name": "Door_North", 
          "objectType": "rectangle",
          "x": 288, "y": 32, "width": 32, "height": 64,
          "properties": {
            "requiresKey": "brass_key",
            "targetRoom": "locked_room"
          }
        }
      ]
    }
  ]
}
```

## Integration with Room Manager

```csharp
public class RoomManager
{
    public void Update(CharacterSprite player, Vector2 playerPos)
    {
        // Check for room transitions
        CheckExits(player, playerPos);
        
        // Check for interactive objects
        CheckInteractives(player, playerPos);
        
        // Check for trigger areas
        CheckTriggers(player, playerPos);
    }
    
    private void CheckExits(CharacterSprite player, Vector2 pos)
    {
        var exit = currentMap.GetFirstCollidingTileObject(
            player, pos, "Exits", null, mapPosition);
            
        if (exit != null)
        {
            TransitionToRoom(exit);
        }
    }
}
```

## Best Practices

1. **Use Descriptive Object Names**: "Exit_North", "Door_Shop", "Trigger_Cutscene"
2. **Organize by Layer**: Separate triggers, interactive objects, and collision objects
3. **Leverage Properties**: Store room targets, required items, and custom data
4. **Cache Management**: Clear cache only when necessary to maintain performance
5. **Error Handling**: Always check for null returns from collision detection methods

## Compatibility

This enhancement is fully backward compatible with existing collision detection methods. The new methods extend functionality without breaking existing code.

The caching system automatically handles object lifecycle and provides significant performance improvements for frequently-tested collision objects like room exits and doors.