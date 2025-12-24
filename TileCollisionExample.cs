// Example: Using Tile Collision Objects in MonoGame Library
//
// This example demonstrates how to use the new tile collision object detection
// functionality that was added to support collision objects defined on individual tiles.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Graphics.Tiles;
using MonoGameLibrary.Scenes;

/// <summary>
/// Example scene demonstrating tile collision object detection.
/// </summary>
public class TileCollisionExampleScene : Scene
{
    private Tilemap _tilemap;
    private CharacterSprite _player;
    private Vector2 _playerPosition;
    private Vector2 _tilemapPosition;
    private readonly float _playerSpeed = 200f; // pixels per second

    public override void LoadContent()
    {
        // Load texture atlas containing both sprites and tiles
        var textureAtlas = TextureAtlas.FromXml(Content, "atlas.xml");

        // Load tilemap with tile collision objects defined in JSON
        _tilemap = Tilemap.FromJson(Content, "maps.json", textureAtlas);

        // Create player sprite with collision
        _player = new CharacterSprite(textureAtlas, "player");
        _player.SetCollisionRectangle(24, 32, new Vector2(4, 0)); // Adjust collision size

        // Set initial positions
        _playerPosition = new Vector2(100, 100);
        _tilemapPosition = Vector2.Zero;
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Store previous player position for collision resolution
        Vector2 previousPosition = _playerPosition;
        Vector2 movement = Vector2.Zero;

        // Handle input for movement
        if (Core.Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            movement.Y -= 1;
        if (Core.Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            movement.Y += 1;
        if (Core.Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            movement.X -= 1;
        if (Core.Input.Keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            movement.X += 1;

        // Normalize diagonal movement
        if (movement.Length() > 0)
            movement.Normalize();

        // Apply movement
        Vector2 newPosition = _playerPosition + movement * _playerSpeed * deltaTime;

        // Check collision with tile collision objects
        if (!_tilemap.CheckSpriteTileCollision(_player, newPosition, _tilemapPosition))
        {
            // Also check collision with object layers (existing functionality)
            if (!_tilemap.CheckSpriteObjectCollision(_player, newPosition, "Collision", _tilemapPosition))
            {
                // No collision detected, apply movement
                _playerPosition = newPosition;
            }
        }

        // Update player sprite
        _player.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        // Draw tilemap
        _tilemap.Draw(spriteBatch, _tilemapPosition);

        // Draw collision visualization if enabled (F1 key in Core.ShowCollisionBoxes)
        if (Core.ShowCollisionBoxes)
        {
            // Draw tile collision objects in yellow
            _tilemap.DrawTileCollisionObjects(spriteBatch, _tilemapPosition, Color.Yellow);
            
            // Draw object layer collision in red  
            _tilemap.DrawObjectLayerAsCollision(spriteBatch, "Collision", _tilemapPosition, Color.Red);
            
            // Or use the convenience method that draws both:
            // _tilemap.DrawAutoCollision(spriteBatch, _tilemapPosition);
        }

        // Draw player
        _player.Draw(spriteBatch, _playerPosition);
    }

    /// <summary>
    /// Example of how to check collision at specific tile coordinates.
    /// This could be useful for tile-based games or grid movement.
    /// </summary>
    private bool IsPositionBlocked(Vector2 worldPosition)
    {
        // Convert world position to tile coordinates  
        var tileCoords = _tilemap.WorldToTileCoordinates(worldPosition, _tilemapPosition);
        if (!tileCoords.HasValue) return false; // Outside tilemap bounds

        // Check if this tile has collision objects
        var collisionObjects = _tilemap.GetTileCollisionObjects(tileCoords.Value.X, tileCoords.Value.Y);
        return collisionObjects != null && collisionObjects.Length > 0;
    }

    /// <summary>
    /// Example of checking collision with specific tiles for advanced movement systems.
    /// </summary>
    private bool CheckTileCollisionInDirection(Vector2 direction)
    {
        // Calculate the position the player would move to
        Vector2 testPosition = _playerPosition + direction;

        // Check collision at the new position
        return _tilemap.CheckSpriteTileCollision(_player, testPosition, _tilemapPosition);
    }
}

/*
Usage Notes:

1. **JSON Structure**: Your maps.json file should have tiles with collisionObjects defined:
   ```json
   {
     "tilesets": [
       {
         "tiles": [
           {
             "id": 12,
             "collisionObjects": [
               {
                 "objectType": "rectangle",
                 "x": 4,
                 "y": 8,
                 "width": 24,
                 "height": 16
               }
             ]
           }
         ]
       }
     ]
   }
   ```

2. **Performance**: Tile collision detection uses broad-phase optimization, only checking 
   tiles that the sprite might actually be overlapping.

3. **Collision Types**: Supports all the same collision object types as object layers:
   - Rectangle (most common for tiles)
   - Ellipse/Circle
   - Point
   - Polygon
   - Polyline

4. **Visualization**: Use Core.ShowCollisionBoxes = true to see collision objects.
   - Tile collision objects appear in yellow
   - Object layer collision appears in red

5. **Integration**: This works alongside existing object layer collision detection.
   You can use both systems together in the same game.
*/