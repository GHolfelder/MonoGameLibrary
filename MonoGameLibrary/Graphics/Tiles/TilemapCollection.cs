using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Represents a collection of tilemaps loaded from a single JSON file.
/// </summary>
public class TilemapCollection
{
    private readonly Dictionary<string, Tilemap> _tilemaps;
    
    public TilemapCollection()
    {
        _tilemaps = new Dictionary<string, Tilemap>();
    }
    
    /// <summary>
    /// Gets all available map names.
    /// </summary>
    public IEnumerable<string> MapNames => _tilemaps.Keys;
    
    /// <summary>
    /// Gets the number of maps in the collection.
    /// </summary>
    public int Count => _tilemaps.Count;
    
    /// <summary>
    /// Gets a tilemap by name.
    /// </summary>
    /// <param name="mapName">The name of the map to retrieve.</param>
    /// <returns>The tilemap with the specified name.</returns>
    public Tilemap GetMap(string mapName)
    {
        if (_tilemaps.TryGetValue(mapName, out Tilemap tilemap))
        {
            return tilemap;
        }
        throw new ArgumentException($"Map '{mapName}' not found in collection.");
    }
    
    /// <summary>
    /// Tries to get a tilemap by name.
    /// </summary>
    /// <param name="mapName">The name of the map to retrieve.</param>
    /// <param name="tilemap">The tilemap if found.</param>
    /// <returns>True if the map was found, false otherwise.</returns>
    public bool TryGetMap(string mapName, out Tilemap tilemap)
    {
        return _tilemaps.TryGetValue(mapName, out tilemap);
    }
    
    /// <summary>
    /// Adds a tilemap to the collection.
    /// </summary>
    internal void AddMap(Tilemap tilemap)
    {
        _tilemaps[tilemap.Name] = tilemap;
    }
    
    /// <summary>
    /// Gets a tilemap by index (in order of addition).
    /// </summary>
    /// <param name="index">The index of the map.</param>
    /// <returns>The tilemap at the specified index.</returns>
    public Tilemap this[int index]
    {
        get
        {
            if (index < 0 || index >= _tilemaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _tilemaps.Values.ElementAt(index);
        }
    }
    
    /// <summary>
    /// Gets a tilemap by name.
    /// </summary>
    /// <param name="mapName">The name of the map.</param>
    /// <returns>The tilemap with the specified name.</returns>
    public Tilemap this[string mapName] => GetMap(mapName);
}