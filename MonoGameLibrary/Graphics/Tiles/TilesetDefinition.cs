using System;
using System.Collections.Generic;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Represents a tileset definition within a tilemap.
/// </summary>
public class TilesetDefinition
{
    public string Name { get; set; }
    public int FirstGid { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int TileCount { get; set; }
    public int Columns { get; set; }
    public int Margin { get; set; }
    public int Spacing { get; set; }
    public string AtlasSprite { get; set; }
    public List<AnimatedTile> Tiles { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}