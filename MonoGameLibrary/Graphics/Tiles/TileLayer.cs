using System;
using System.Collections.Generic;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Represents a single tile layer in a tilemap.
/// </summary>
public class TileLayer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float Opacity { get; set; } = 1.0f;
    public bool Visible { get; set; } = true;
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public int[] Tiles { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Represents tile-specific data (collision objects, properties, etc.)
/// </summary>
public class TileData
{
    public int Id { get; set; }
    public string Type { get; set; }
    public object[] CollisionObjects { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}