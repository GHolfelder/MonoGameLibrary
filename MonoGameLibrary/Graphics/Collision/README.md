# Graphics.Collision Namespace

The `MonoGameLibrary.Graphics.Collision` namespace provides a comprehensive collision detection system for sprites with optional collision visualization.

## Core Classes

### ICollisionShape Interface
Base interface for all collision shapes, providing:
- **Position offset** relative to parent sprite
- **Intersection testing** with other shapes
- **Bounding rectangle** calculation
- **Collision visualization** capabilities

### Collision Shapes

#### CollisionRectangle
Rectangular collision detection with:
- Width and height properties
- Position offset support
- Rectangle-to-rectangle collision
- Rectangle-to-circle collision

#### CollisionCircle  
Circular collision detection with:
- Radius property
- Position offset support
- Circle-to-circle collision
- Circle-to-rectangle collision

### SpriteCollision Component
Collision component that can be attached to any sprite:
- **Shape property** - holds the collision shape
- **EnableDraw** - toggles collision visualization
- **DrawColor** - color for collision shapes
- **Intersects()** method for collision testing

### CollisionDraw
Static helper class for drawing primitive collision shapes:
- **DrawRectangle()** - draws rectangle outlines
- **DrawCircle()** - draws circle outlines  
- **DrawLine()** - draws lines between points
- **Auto-initialization** - creates pixel texture as needed

## Usage Examples

### Basic Collision Setup

```csharp
// Create sprites with collision
var player = new Sprite(playerTextureRegion);
var enemy = new Sprite(enemyTextureRegion);

// Enable rectangular collision (32x32 pixels)
player.EnableCollision(32, 32);

// Enable circular collision (radius 16 pixels)
enemy.EnableCollision(16f);

// Enable collision with visualization
player.EnableCollision(32, 32, Vector2.Zero, enableDraw: true, Color.Green);
```

### Collision Detection

```csharp
// Check for collision between sprites
Vector2 playerPos = new Vector2(100, 100);
Vector2 enemyPos = new Vector2(120, 110);

if (player.CheckCollision(playerPos, enemy, enemyPos))
{
    // Handle collision
    Console.WriteLine("Player hit enemy!");
}
```

### Advanced Collision Setup

```csharp
// Custom collision shapes with offsets
var character = new Sprite(characterTexture);

// Body collision (main hitbox)
character.EnableCollision(24, 40, new Vector2(-12, -20), enableDraw: true, Color.Red);

// Or create manually for multiple shapes
character.Collision = new SpriteCollision(
    new CollisionRectangle(24, 40, new Vector2(-12, -20)), 
    enableDraw: true, 
    Color.Blue
);
```

### Collision Visualization

```csharp
public void Draw(SpriteBatch spriteBatch)
{
    bool showCollisionShapes = Keyboard.GetState().IsKeyDown(Keys.F1);
    
    // Draw sprites with optional collision visualization
    player.Draw(spriteBatch, playerPosition, showCollisionShapes);
    enemy.Draw(spriteBatch, enemyPosition, showCollisionShapes);
}
```

### Manual Collision Drawing

```csharp
// Draw collision shapes manually
var bounds = new Rectangle(100, 100, 64, 32);
CollisionDraw.DrawRectangle(spriteBatch, bounds, Color.Yellow);

var center = new Vector2(200, 150);
CollisionDraw.DrawCircle(spriteBatch, center, 25f, Color.Cyan);
```

## Integration with Existing Sprites

The collision system works seamlessly with all existing sprite classes:

```csharp
// Works with AnimatedSprite
AnimatedSprite animSprite = new AnimatedSprite(animation);
animSprite.EnableCollision(32, 32);

// Works with PlayerSprite  
PlayerSprite player = new PlayerSprite();
player.EnableCollision(20f, new Vector2(0, -5)); // Offset collision up slightly

// Works with NPCSprite
NPCSprite npc = new NPCSprite();
npc.EnableCollision(28, 48, Vector2.Zero, true, Color.Orange); // Visible orange collision
```

## Performance Considerations

- **Collision shapes are optional** - no performance cost when `Collision` property is null
- **Collision drawing** - only renders when `showCollision` parameter is true
- **Primitive drawing** - uses single pixel texture for efficient line/rectangle rendering
- **Shape intersection** - optimized algorithms for rectangle-rectangle and circle-circle tests

## Tilemap Integration

The collision system is designed to work with tilemap collision objects:

```csharp
// For type-based tilemap collision layers like "Collision_Static"
public void CheckTilemapCollision(Sprite sprite, Vector2 position, List<CollisionObject> staticCollisions)
{
    foreach (var collision in staticCollisions)
    {
        // Tilemap collision objects can use the same ICollisionShape interface
        var tempCollision = new SpriteCollision(collision.Shape);
        if (sprite.Collision?.Intersects(position, tempCollision, collision.Position) == true)
        {
            // Handle tilemap collision
        }
    }
}
```

## Architecture Notes

- **Composition over inheritance** - collision added as optional component rather than derived classes
- **Interface-based design** - `ICollisionShape` allows easy extension with new shape types
- **Non-breaking changes** - all existing sprite functionality preserved
- **Type-safe collision** - compile-time checking for shape combinations
- **MonoGame integration** - uses standard MonoGame primitives and patterns

The collision system provides production-ready collision detection with visualization capabilities while maintaining the clean architecture and performance characteristics of MonoGameLibrary.