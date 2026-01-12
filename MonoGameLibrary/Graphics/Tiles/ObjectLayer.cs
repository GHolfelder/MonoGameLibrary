using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics.Collision;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Represents the type of shape for a collision object.
/// </summary>
public enum CollisionObjectType
{
    Rectangle,
    Ellipse,
    Point,
    Polygon,
    Polyline,
    Tile,
    Text
}

/// <summary>
/// Represents a collision object within an object layer.
/// </summary>
public class CollisionObject
{
    public string Name { get; set; }
    public string Type { get; set; }
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public CollisionObjectType ShapeType { get; set; } = CollisionObjectType.Rectangle;
    public Vector2[] PolygonPoints { get; set; } = Array.Empty<Vector2>();
    public float Rotation { get; set; } = 0f;
    public Dictionary<string, object> Properties { get; set; } = new();
    public string TextContent { get; set; } = string.Empty;
    public int Gid { get; set; } = 0; // Tile GID for tile objects
    
    /// <summary>
    /// Cached collision shape for performance optimization.
    /// Created lazily on first collision detection.
    /// </summary>
    internal ICollisionShape _cachedCollisionShape;
    
    /// <summary>
    /// Gets the radius for circular/elliptical objects (uses Width as diameter).
    /// </summary>
    public float Radius => Width * 0.5f;
    
    /// <summary>
    /// Gets whether this object is circular (ellipse with approximately equal width and height).
    /// Allows for small floating point differences.
    /// </summary>
    public bool IsCircle => ShapeType == CollisionObjectType.Ellipse && Math.Abs(Width - Height) <= 1.0f;
}

/// <summary>
/// Represents an object layer containing collision objects.
/// </summary>
public class ObjectLayer
{
    public string Name { get; set; }
    public bool Visible { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;
    public List<CollisionObject> Objects { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}