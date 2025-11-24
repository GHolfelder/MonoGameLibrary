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
}