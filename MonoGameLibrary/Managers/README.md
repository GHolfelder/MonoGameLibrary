# Managers Namespace

The Managers namespace provides high-level systems for managing game rooms, transitions, and spatial optimization in MonoGame Library.

## Core Components

### Room Management System
- **[RoomManagerBase.cs](RoomManagerBase.cs)** - Abstract base class for room management systems
  - Lifecycle management and save/load functionality
  - Event-driven room transitions with `OnRoomTransitionRequested` event
  - Property-based QuadTree configuration with validation
  - Exit collision detection with cooldown system (0.5s)

- **[RoomManager.cs](RoomManager.cs)** - Concrete implementation for room transitions
  - QuadTree spatial optimization for large maps
  - Multi-layer exit detection ("Exits" and "Triggers" layers)
  - Performance tracking and debug metrics
  - Character velocity integration for movement validation

## Key Features

### Event-Driven Architecture
- **Decoupled Transitions**: Room manager raises events for transitions, allowing flexible handling
- **Property Configuration**: Exit objects configure target rooms and spawn points via properties
- **State Management**: Save/load functionality for persistence across sessions

### Spatial Optimization
- **QuadTree Partitioning**: Efficient spatial queries for maps with many exit points
- **Property-Based Configuration**: Configure QuadTree parameters via tilemap properties
- **Performance Monitoring**: Real-time debug metrics for optimization analysis
- **Fallback Support**: Linear search when QuadTree is disabled or unavailable

### Collision Integration
- **Enhanced Detection**: Uses tilemap collision system with shape caching
- **Direction Validation**: Dot product calculations ensure realistic exit usage
- **Multi-Shape Support**: Rectangle, Circle, Point, Polygon collision objects
- **Layer-Based Detection**: Separate collision and trigger layers for organization

## Usage Examples

### Basic Room Manager Setup

```csharp
public class GameManager
{
    private RoomManager _roomManager;
    private TilemapCollection _maps;
    
    public void Initialize()
    {
        // Create room manager and subscribe to transitions
        _roomManager = new RoomManager();
        _roomManager.OnRoomTransitionRequested += HandleRoomTransition;
        
        // Load tilemap collection
        _maps = TilemapCollection.FromJson(Content, "maps/game-maps.json", textureAtlas);
        
        // Set initial map
        _roomManager.SetCurrentMap("level1", _maps["level1"]);
    }
    
    private void HandleRoomTransition(string targetRoom, string entranceExit)
    {
        // Load new room and position player
        if (_maps.TryGetMap(targetRoom, out var targetMap))
        {
            _roomManager.SetCurrentMap(targetRoom, targetMap);
            
            // Position player at entrance
            var entranceObject = targetMap.GetCollisionObjects("Exits")
                ?.FirstOrDefault(obj => obj.Name == entranceExit);
            if (entranceObject != null)
            {
                player.Position = entranceObject.Position;
            }
        }
    }
}
```

### Exit Collision Detection

```csharp
public void Update(GameTime gameTime)
{
    // Update room manager cooldown
    _roomManager.Update(gameTime);
    
    // Check for exit collisions
    var exitCollision = _roomManager.CheckExitCollisions(player);
    if (exitCollision != null)
    {
        // Room transition will be triggered automatically via event
        // if player velocity and direction are valid
    }
}
```

### QuadTree Configuration

Configure QuadTree performance via tilemap properties in JSON:

```json
{
  "name": "large_outdoor_map",
  "width": 100,
  "height": 75,
  "properties": {
    "quadTreeEnabled": true,
    "maxExitsPerNode": 12,
    "maxQuadTreeDepth": 8,
    "exitDetectionRadius": 48.0
  },
  "tileLayers": [...],
  "objectLayers": [
    {
      "name": "Exits",
      "objects": [
        {
          "name": "Exit_North",
          "x": 1600, "y": 0, "width": 64, "height": 32,
          "properties": {
            "targetRoom": "mountain_path",
            "entranceExit": "South_Entrance"
          }
        }
      ]
    }
  ]
}
```

### Performance Monitoring

```csharp
// Get performance statistics
var stats = _roomManager.GetPerformanceStats();
Core.AddDebugMessage(stats);

// Reset performance counters
_roomManager.ResetPerformanceStats();

// Performance metrics are automatically logged every 60 queries/searches
// when developer mode is active (F1 to toggle)
```

### Custom Exit Object Properties

Exit objects support flexible property configuration:

```json
{
  "name": "Exit_SecretCave",
  "objectType": "rectangle",
  "x": 800, "y": 600, "width": 32, "height": 32,
  "properties": {
    "targetRoom": "secret_cave",
    "entranceExit": "Cave_Entrance",
    "requiresKey": "silver_key",
    "lockMessage": "This door requires a silver key"
  }
}
```

## Configuration Reference

### QuadTree Properties

Configure spatial partitioning via tilemap properties:

| Property | Type | Default | Range | Description |
|----------|------|---------|-------|-------------|
| `quadTreeEnabled` | bool | true | - | Enable/disable QuadTree optimization |
| `maxExitsPerNode` | int | 8 | 1-50 | Maximum exits per QuadTree node before subdivision |
| `maxQuadTreeDepth` | int | 6 | 1-10 | Maximum subdivision depth for QuadTree |
| `exitDetectionRadius` | float | 32.0 | 1.0-1000.0 | Search radius for nearby exits (pixels) |

### Recommended Values

| Map Size | Exits | maxExitsPerNode | maxQuadTreeDepth | exitDetectionRadius |
|----------|-------|-----------------|------------------|-------------------|
| Small (< 30x30) | < 10 | 4 | 4 | 32.0 |
| Medium (30x60) | 10-25 | 8 | 6 | 40.0 |
| Large (60x100) | 25-50 | 12 | 8 | 48.0 |
| Huge (> 100x100) | > 50 | 16 | 10 | 64.0 |

### Exit Object Properties

Standard properties for exit collision objects:

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `targetRoom` | string | Yes | Name of target room/map |
| `entranceExit` | string | No | Name of entrance exit in target room (default: "default") |
| `requiresKey` | string | No | Required key/item for access |
| `lockMessage` | string | No | Message displayed when locked |
| `oneWay` | bool | No | Prevent return through this exit |

## Advanced Features

### Direction Validation

Exits validate player movement direction using dot product calculations:

```csharp
// Player must be moving toward exit (within 60 degrees)
Vector2 exitDirection = Vector2.Normalize(exitPosition - player.Position);
float dotProduct = Vector2.Dot(Vector2.Normalize(player.Velocity), exitDirection);
bool isValidDirection = dotProduct > 0.5f; // 0.5 = 60 degrees tolerance
```

### Cooldown System

Exit cooldown prevents rapid bouncing between rooms:

- **Default Duration**: 0.5 seconds
- **Automatic Reset**: Cooldown decreases each frame
- **Event Blocking**: Transitions blocked during cooldown period

### State Persistence

Room managers support save/load for game state:

```csharp
// Save current state
var state = _roomManager.SaveState();
SaveToFile(state, "roommanager.save");

// Load previous state
var savedState = LoadFromFile("roommanager.save");
_roomManager.LoadState(savedState);
```

## Performance Considerations

### QuadTree vs Linear Search

- **QuadTree**: Efficient for maps with many exits (> 15-20)
- **Linear Search**: Better for small maps with few exits (< 10)
- **Memory**: QuadTree uses additional memory for spatial structure
- **Setup Cost**: QuadTree creation overhead vs query performance gains

### Debug Information

When developer mode is active (F1):
- Performance statistics logged every 60 operations
- QuadTree configuration displayed on map load
- Collision detection results shown in real-time
- Validation warnings for invalid property values

## Integration Examples

### Character Sprite Integration

```csharp
public class Player : CharacterSprite
{
    public void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Velocity is automatically tracked by CharacterSprite
        // and used by room manager for direction validation
    }
}
```

### Tiled Map Editor Workflow

1. **Create Object Layers**: Add "Exits" and/or "Triggers" layers
2. **Add Exit Objects**: Place rectangles where exits should be
3. **Configure Properties**: Set targetRoom and entranceExit for each exit
4. **Set Map Properties**: Configure QuadTree optimization for large maps
5. **Export JSON**: Use Tiled's JSON export for MonoGame Library

### Error Handling

```csharp
// Room manager handles missing targets gracefully
_roomManager.OnRoomTransitionRequested += (targetRoom, entranceExit) =>
{
    if (!_maps.TryGetMap(targetRoom, out var targetMap))
    {
        Core.AddDebugMessage($"Warning: Target room '{targetRoom}' not found");
        return;
    }
    
    // Proceed with transition...
};
```

## Best Practices

### Map Organization
- **Consistent Naming**: Use descriptive names for rooms and exits
- **Layer Organization**: Separate "Exits" for room transitions, "Triggers" for special events
- **Property Standards**: Establish consistent property naming across maps

### Performance Optimization
- **Configure QuadTree**: Set appropriate values based on map size and exit count
- **Monitor Metrics**: Use debug information to identify performance bottlenecks
- **Cache Strategy**: Room manager automatically caches spatial configurations

### Development Workflow
- **Enable Debug Mode**: Use F1 to toggle developer overlay during testing
- **Test Transitions**: Verify all exits work correctly and have proper targets
- **Validate Properties**: Check for validation warnings in debug output

## Design Patterns

### Abstract Factory Pattern
RoomManagerBase provides interface for different room management strategies:
- Extend RoomManagerBase for custom collision detection
- Override GetSpatialConfigForMap for custom optimization
- Implement SaveState/LoadState for persistence

### Observer Pattern
Event-driven architecture with OnRoomTransitionRequested:
- Decouples room management from game state
- Allows multiple listeners for transition events
- Supports complex transition logic without coupling

### Strategy Pattern
Spatial optimization uses strategy pattern:
- QuadTree strategy for large maps with many exits
- Linear search strategy for small maps
- Runtime switching based on map properties

The Managers namespace provides a complete solution for room-based games with performance optimization, flexible configuration, and comprehensive debug support.