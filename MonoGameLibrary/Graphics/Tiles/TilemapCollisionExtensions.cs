using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics.Collision;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Extension methods for adding collision visualization and detection to Tilemap objects.
/// </summary>
public static class TilemapCollisionExtensions
{
    /// <summary>
    /// Draws collision visualization for a specific layer by treating non-empty tiles as collision areas.
    /// </summary>
    /// <param name="tilemap">The tilemap to draw collision for.</param>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="layerName">The name of the layer to visualize as collision (e.g., "Collision_Static").</param>
    /// <param name="position">The position to draw the tilemap at.</param>
    /// <param name="collisionColor">The color to draw collision rectangles with.</param>
    public static void DrawLayerAsCollision(this Tilemap tilemap, SpriteBatch spriteBatch, 
        string layerName, Vector2 position, Color collisionColor)
    {
        var layer = tilemap.GetLayerByName(layerName);
        if (layer == null || !layer.Visible) return;
        
        Vector2 layerOffset = new Vector2(layer.OffsetX, layer.OffsetY);
        
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = y * layer.Width + x;
                if (tileIndex >= layer.Tiles.Length) continue;
                
                int gid = layer.Tiles[tileIndex];
                if (gid == 0) continue; // Empty tile = no collision
                
                // Draw collision rectangle for this tile
                var tileRect = new Rectangle(
                    (int)(position.X + layerOffset.X + x * tilemap.DrawTileWidth),
                    (int)(position.Y + layerOffset.Y + y * tilemap.DrawTileHeight),
                    (int)tilemap.DrawTileWidth,
                    (int)tilemap.DrawTileHeight
                );
                
                CollisionDraw.DrawRectangle(spriteBatch, tileRect, collisionColor);
            }
        }
    }

    /// <summary>
    /// Draws collision visualization for multiple layers with different colors.
    /// </summary>
    /// <param name="tilemap">The tilemap to draw collision for.</param>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="position">The position to draw the tilemap at.</param>
    /// <param name="layerColors">Dictionary mapping layer names to their visualization colors.</param>
    public static void DrawMultipleLayersAsCollision(this Tilemap tilemap, SpriteBatch spriteBatch,
        Vector2 position, Dictionary<string, Color> layerColors)
    {
        foreach (var (layerName, color) in layerColors)
        {
            tilemap.DrawLayerAsCollision(spriteBatch, layerName, position, color);
        }
    }

    /// <summary>
    /// Gets collision rectangles from a specific layer for collision detection logic.
    /// </summary>
    /// <param name="tilemap">The tilemap to extract collision from.</param>
    /// <param name="layerName">The name of the collision layer.</param>
    /// <param name="worldPosition">The world position offset of the tilemap.</param>
    /// <returns>List of collision rectangles representing solid tiles.</returns>
    public static List<Rectangle> GetCollisionRectangles(this Tilemap tilemap, 
        string layerName, Vector2 worldPosition = default)
    {
        var collisionRects = new List<Rectangle>();
        var layer = tilemap.GetLayerByName(layerName);
        
        if (layer == null || !layer.Visible) return collisionRects;
        
        Vector2 layerOffset = new Vector2(layer.OffsetX, layer.OffsetY);
        
        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = y * layer.Width + x;
                if (tileIndex >= layer.Tiles.Length) continue;
                
                int gid = layer.Tiles[tileIndex];
                if (gid == 0) continue; // Empty tile = no collision
                
                var tileRect = new Rectangle(
                    (int)(worldPosition.X + layerOffset.X + x * tilemap.DrawTileWidth),
                    (int)(worldPosition.Y + layerOffset.Y + y * tilemap.DrawTileHeight),
                    (int)tilemap.DrawTileWidth,
                    (int)tilemap.DrawTileHeight
                );
                
                collisionRects.Add(tileRect);
            }
        }
        
        return collisionRects;
    }

    /// <summary>
    /// Checks if a sprite collides with tiles in a collision layer.
    /// </summary>
    /// <param name="tilemap">The tilemap to check collision against.</param>
    /// <param name="sprite">The sprite to check collision for.</param>
    /// <param name="spritePosition">The sprite's world position.</param>
    /// <param name="collisionLayerName">The name of the collision layer to check against.</param>
    /// <param name="tilemapPosition">The tilemap's world position.</param>
    /// <returns>True if the sprite collides with any tile in the layer.</returns>
    public static bool CheckSpriteCollision(this Tilemap tilemap, Sprite sprite, 
        Vector2 spritePosition, string collisionLayerName, Vector2 tilemapPosition = default)
    {
        if (sprite.Collision == null) return false;
        
        var collisionRects = tilemap.GetCollisionRectangles(collisionLayerName, tilemapPosition);
        
        foreach (var tileRect in collisionRects)
        {
            // Create temporary collision for the tile
            var tileCollisionShape = new CollisionRectangle(tileRect.Width, tileRect.Height, Vector2.Zero);
            var tileCollision = new SpriteCollision(tileCollisionShape);
            
            if (sprite.Collision.Intersects(spritePosition, tileCollision, new Vector2(tileRect.X, tileRect.Y)))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Checks if a character sprite collides with tiles in a collision layer.
    /// </summary>
    /// <param name="tilemap">The tilemap to check collision against.</param>
    /// <param name="characterSprite">The character sprite to check collision for.</param>
    /// <param name="spritePosition">The character sprite's world position.</param>
    /// <param name="collisionLayerName">The name of the collision layer to check against.</param>
    /// <param name="tilemapPosition">The tilemap's world position.</param>
    /// <returns>True if the character sprite collides with any tile in the layer.</returns>
    public static bool CheckCharacterSpriteCollision(this Tilemap tilemap, CharacterSprite characterSprite, 
        Vector2 spritePosition, string collisionLayerName, Vector2 tilemapPosition = default)
    {
        if (characterSprite.Collision == null) return false;
        
        var collisionRects = tilemap.GetCollisionRectangles(collisionLayerName, tilemapPosition);
        
        foreach (var tileRect in collisionRects)
        {
            // Create temporary collision for the tile
            var tileCollisionShape = new CollisionRectangle(tileRect.Width, tileRect.Height, Vector2.Zero);
            var tileCollision = new SpriteCollision(tileCollisionShape);
            
            if (characterSprite.Collision.Intersects(spritePosition, tileCollision, new Vector2(tileRect.X, tileRect.Y)))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Gets the tile coordinates for a world position within the tilemap.
    /// </summary>
    /// <param name="tilemap">The tilemap to query.</param>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <param name="tilemapPosition">The tilemap's world position offset.</param>
    /// <returns>The tile coordinates (x, y) or null if outside the tilemap bounds.</returns>
    public static Point? WorldToTileCoordinates(this Tilemap tilemap, Vector2 worldPosition, Vector2 tilemapPosition = default)
    {
        // Convert world position to tilemap-relative position
        Vector2 localPosition = worldPosition - tilemapPosition;
        
        // Convert to tile coordinates
        int tileX = (int)(localPosition.X / tilemap.DrawTileWidth);
        int tileY = (int)(localPosition.Y / tilemap.DrawTileHeight);
        
        // Check bounds
        if (tileX < 0 || tileX >= tilemap.Width || tileY < 0 || tileY >= tilemap.Height)
            return null;
            
        return new Point(tileX, tileY);
    }

    /// <summary>
    /// Checks if a specific tile position has collision in a layer.
    /// </summary>
    /// <param name="tilemap">The tilemap to check.</param>
    /// <param name="tileX">The tile X coordinate.</param>
    /// <param name="tileY">The tile Y coordinate.</param>
    /// <param name="layerName">The collision layer name to check.</param>
    /// <returns>True if there's a solid tile at the specified coordinates.</returns>
    public static bool HasCollisionAtTile(this Tilemap tilemap, int tileX, int tileY, string layerName)
    {
        var layer = tilemap.GetLayerByName(layerName);
        if (layer == null || !layer.Visible) return false;
        
        if (tileX < 0 || tileX >= layer.Width || tileY < 0 || tileY >= layer.Height) return false;
        
        int tileIndex = tileY * layer.Width + tileX;
        if (tileIndex >= layer.Tiles.Length) return false;
        
        return layer.Tiles[tileIndex] != 0; // Non-zero tile ID = collision
    }

    /// <summary>
    /// Draws collision visualization for object layers.
    /// </summary>
    /// <param name="tilemap">The tilemap to draw collision for.</param>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="objectLayerName">The name of the object layer to visualize.</param>
    /// <param name="position">The position offset to draw the objects at.</param>
    /// <param name="collisionColor">The color to draw collision rectangles with.</param>
    public static void DrawObjectLayerAsCollision(this Tilemap tilemap, SpriteBatch spriteBatch,
        string objectLayerName, Vector2 position, Color collisionColor)
    {
        var objectLayer = tilemap.GetObjectLayer(objectLayerName);
        if (objectLayer == null || !objectLayer.Visible) return;

        foreach (var collisionObject in objectLayer.Objects)
        {
            Vector2 objectPosition = position + collisionObject.Position;
            
            switch (collisionObject.ShapeType)
            {
                case CollisionObjectType.Rectangle:
                    var objectRect = new Rectangle(
                        (int)objectPosition.X,
                        (int)objectPosition.Y,
                        collisionObject.Width,
                        collisionObject.Height
                    );
                    CollisionDraw.DrawRectangle(spriteBatch, objectRect, collisionColor);
                    break;
                    
                case CollisionObjectType.Ellipse:
                    if (collisionObject.IsCircle)
                    {
                        // Draw as circle
                        CollisionDraw.DrawCircle(spriteBatch, 
                            objectPosition + new Vector2(collisionObject.Radius, collisionObject.Radius), 
                            collisionObject.Radius, collisionColor);
                    }
                    else
                    {
                        // Draw ellipse as rectangle for now (could be enhanced with proper ellipse drawing)
                        var ellipseRect = new Rectangle(
                            (int)objectPosition.X,
                            (int)objectPosition.Y,
                            collisionObject.Width,
                            collisionObject.Height
                        );
                        CollisionDraw.DrawRectangle(spriteBatch, ellipseRect, collisionColor);
                    }
                    break;
                    
                case CollisionObjectType.Point:
                    // Draw point as small circle
                    CollisionDraw.DrawCircle(spriteBatch, objectPosition, 2f, collisionColor);
                    break;
                    
                case CollisionObjectType.Polygon:
                    // Draw polygon edges (closed shape)
                    DrawPolygonOutline(spriteBatch, collisionObject.PolygonPoints, objectPosition, collisionColor);
                    break;
                    
                case CollisionObjectType.Polyline:
                    // Draw polyline edges (open path)
                    DrawPolylineOutline(spriteBatch, collisionObject.PolygonPoints, objectPosition, collisionColor);
                    break;
                    
                case CollisionObjectType.Tile:
                    // Draw tile as rectangle
                    var tileRect = new Rectangle(
                        (int)objectPosition.X,
                        (int)objectPosition.Y,
                        collisionObject.Width,
                        collisionObject.Height
                    );
                    CollisionDraw.DrawRectangle(spriteBatch, tileRect, collisionColor);
                    break;
                    
                case CollisionObjectType.Text:
                    // Draw text bounds as rectangle
                    var textRect = new Rectangle(
                        (int)objectPosition.X,
                        (int)objectPosition.Y,
                        collisionObject.Width,
                        collisionObject.Height
                    );
                    CollisionDraw.DrawRectangle(spriteBatch, textRect, collisionColor);
                    break;
            }
        }
    }

    /// <summary>
    /// Draws collision visualization for multiple object layers with different colors.
    /// </summary>
    /// <param name="tilemap">The tilemap to draw collision for.</param>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="position">The position offset to draw the objects at.</param>
    /// <param name="objectLayerColors">Dictionary mapping object layer names to their visualization colors.</param>
    public static void DrawMultipleObjectLayersAsCollision(this Tilemap tilemap, SpriteBatch spriteBatch,
        Vector2 position, Dictionary<string, Color> objectLayerColors)
    {
        foreach (var (layerName, color) in objectLayerColors)
        {
            tilemap.DrawObjectLayerAsCollision(spriteBatch, layerName, position, color);
        }
    }

    /// <summary>
    /// Checks if a sprite collides with objects in an object layer.
    /// </summary>
    /// <param name="tilemap">The tilemap to check collision against.</param>
    /// <param name="sprite">The sprite to check collision for.</param>
    /// <param name="spritePosition">The sprite's world position.</param>
    /// <param name="objectLayerName">The name of the object layer to check against.</param>
    /// <param name="tilemapPosition">The tilemap's world position.</param>
    /// <returns>True if the sprite collides with any object in the layer.</returns>
    public static bool CheckSpriteObjectCollision(this Tilemap tilemap, Sprite sprite,
        Vector2 spritePosition, string objectLayerName, Vector2 tilemapPosition = default)
    {
        if (sprite.Collision == null) return false;

        var objectLayer = tilemap.GetObjectLayer(objectLayerName);
        if (objectLayer == null || !objectLayer.Visible) return false;

        foreach (var collisionObject in objectLayer.Objects)
        {
            // Calculate object's world position
            Vector2 objectWorldPosition = tilemapPosition + collisionObject.Position;

            // Create appropriate collision shape based on object type
            ICollisionShape objectCollisionShape = collisionObject.ShapeType switch
            {
                CollisionObjectType.Rectangle => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero),
                CollisionObjectType.Ellipse when collisionObject.IsCircle => new CollisionCircle(collisionObject.Radius, Vector2.Zero),
                CollisionObjectType.Ellipse => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to rectangle for ellipse
                CollisionObjectType.Point => new CollisionCircle(1f, Vector2.Zero), // Small circle for point
                CollisionObjectType.Polygon => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to bounding box for polygon
                CollisionObjectType.Polyline => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to bounding box for polyline
                CollisionObjectType.Tile => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Rectangle for tile objects
                CollisionObjectType.Text => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Rectangle for text objects
                _ => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero)
            };

            var objectCollision = new SpriteCollision(objectCollisionShape);

            if (sprite.Collision.Intersects(spritePosition, objectCollision, objectWorldPosition))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a character sprite collides with objects in an object layer.
    /// </summary>
    /// <param name="tilemap">The tilemap to check collision against.</param>
    /// <param name="characterSprite">The character sprite to check collision for.</param>
    /// <param name="spritePosition">The character sprite's world position.</param>
    /// <param name="objectLayerName">The name of the object layer to check against.</param>
    /// <param name="tilemapPosition">The tilemap's world position.</param>
    /// <returns>True if the character sprite collides with any object in the layer.</returns>
    public static bool CheckCharacterSpriteObjectCollision(this Tilemap tilemap, CharacterSprite characterSprite,
        Vector2 spritePosition, string objectLayerName, Vector2 tilemapPosition = default)
    {
        if (characterSprite.Collision == null) return false;

        var objectLayer = tilemap.GetObjectLayer(objectLayerName);
        if (objectLayer == null || !objectLayer.Visible) return false;

        foreach (var collisionObject in objectLayer.Objects)
        {
            // Calculate object's world position
            Vector2 objectWorldPosition = tilemapPosition + collisionObject.Position;

            // Create appropriate collision shape based on object type
            ICollisionShape objectCollisionShape = collisionObject.ShapeType switch
            {
                CollisionObjectType.Rectangle => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero),
                CollisionObjectType.Ellipse when collisionObject.IsCircle => new CollisionCircle(collisionObject.Radius, Vector2.Zero),
                CollisionObjectType.Ellipse => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to rectangle for ellipse
                CollisionObjectType.Point => new CollisionCircle(1f, Vector2.Zero), // Small circle for point
                CollisionObjectType.Polygon => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to bounding box for polygon
                CollisionObjectType.Polyline => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Fallback to bounding box for polyline
                CollisionObjectType.Tile => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Rectangle for tile objects
                CollisionObjectType.Text => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero), // Rectangle for text objects
                _ => new CollisionRectangle(collisionObject.Width, collisionObject.Height, Vector2.Zero)
            };

            var objectCollision = new SpriteCollision(objectCollisionShape);

            if (characterSprite.Collision.Intersects(spritePosition, objectCollision, objectWorldPosition))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Draws a polygon outline using line segments (closed shape).
    /// </summary>
    private static void DrawPolygonOutline(SpriteBatch spriteBatch, Vector2[] points, Vector2 offset, Color color)
    {
        if (points.Length < 2) return;
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 start = offset + points[i];
            Vector2 end = offset + points[(i + 1) % points.Length]; // Connect back to first point
            DrawLine(spriteBatch, start, end, color);
        }
    }
    
    /// <summary>
    /// Draws a polyline outline using line segments (open path).
    /// </summary>
    private static void DrawPolylineOutline(SpriteBatch spriteBatch, Vector2[] points, Vector2 offset, Color color)
    {
        if (points.Length < 2) return;
        
        for (int i = 0; i < points.Length - 1; i++) // Don't connect last to first
        {
            Vector2 start = offset + points[i];
            Vector2 end = offset + points[i + 1];
            DrawLine(spriteBatch, start, end, color);
        }
    }
    
    /// <summary>
    /// Draws a line between two points using CollisionDraw.
    /// </summary>
    private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
    {
        CollisionDraw.DrawLine(spriteBatch, start, end, color);
    }
}