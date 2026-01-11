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

    /// <summary>
    /// Gets a property value with type conversion.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="key">The property key.</param>
    /// <param name="defaultValue">The default value if not found or conversion fails.</param>
    /// <returns>The property value or default value.</returns>
    public T GetProperty<T>(string key, T defaultValue = default(T))
    {
        if (!Properties.TryGetValue(key, out var value))
            return defaultValue;
            
        if (value is T directValue)
            return directValue;
            
        // Handle string to primitive conversions
        if (value is string stringValue && typeof(T) != typeof(string))
        {
            try
            {
                return (T)Convert.ChangeType(stringValue, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// Sets a property value.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <param name="value">The property value.</param>
    public void SetProperty(string key, object value)
    {
        Properties[key] = value;
    }
    
    /// <summary>
    /// Checks if a property exists.
    /// </summary>
    /// <param name="key">The property key.</param>
    /// <returns>True if the property exists.</returns>
    public bool HasProperty(string key)
    {
        return Properties.ContainsKey(key);
    }
}