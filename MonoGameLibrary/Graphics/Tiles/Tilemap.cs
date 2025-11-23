using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
    public object[] Tiles { get; set; } = Array.Empty<object>();
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// The enhanced Tilemap class with support for multiple layers and z-ordering.
/// </summary>
public class Tilemap
{
    private readonly List<TileLayer> _tileLayers;
    private readonly List<TilesetDefinition> _tilesetDefinitions;
    private readonly Dictionary<int, ITileset> _tilesets;
    private readonly TextureAtlas _textureAtlas;

    /// <summary>
    /// Gets the map name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the width of the map in tiles.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the map in tiles.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the width of each tile in pixels.
    /// </summary>
    public int TileWidth { get; private set; }

    /// <summary>
    /// Gets the height of each tile in pixels.
    /// </summary>
    public int TileHeight { get; private set; }

    /// <summary>
    /// Gets the map orientation (typically "orthogonal").
    /// </summary>
    public string Orientation { get; private set; }

    /// <summary>
    /// Gets the background color of the map.
    /// </summary>
    public Color? BackgroundColor { get; private set; }

    /// <summary>
    /// Gets the tile layers in rendering order (back to front).
    /// </summary>
    public IReadOnlyList<TileLayer> TileLayers => _tileLayers.AsReadOnly();

    /// <summary>
    /// Gets or sets the scale factor to draw tiles at.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets the actual width each tile is drawn at.
    /// </summary>
    public float DrawTileWidth => TileWidth * Scale.X;

    /// <summary>
    /// Gets the actual height each tile is drawn at.
    /// </summary>
    public float DrawTileHeight => TileHeight * Scale.Y;

    /// <summary>
    /// Creates a new tilemap with multiple layers.
    /// </summary>
    /// <param name="name">The name of the tilemap.</param>
    /// <param name="width">The width of the map in tiles.</param>
    /// <param name="height">The height of the map in tiles.</param>
    /// <param name="tileWidth">The width of each tile in pixels.</param>
    /// <param name="tileHeight">The height of each tile in pixels.</param>
    /// <param name="orientation">The map orientation.</param>
    /// <param name="textureAtlas">The texture atlas containing tile sprites.</param>
    private Tilemap(string name, int width, int height, int tileWidth, int tileHeight, string orientation, TextureAtlas textureAtlas)
    {
        Name = name;
        Width = width;
        Height = height;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Orientation = orientation;
        _textureAtlas = textureAtlas;
        _tileLayers = new List<TileLayer>();
        _tilesetDefinitions = new List<TilesetDefinition>();
        _tilesets = new Dictionary<int, ITileset>();
    }

    /// <summary>
    /// Creates a tilemap from a JSON file with texture atlas integration.
    /// </summary>
    /// <param name="content">The content manager to load assets.</param>
    /// <param name="jsonFilename">The JSON file path relative to the content directory.</param>
    /// <returns>A new Tilemap instance loaded from JSON.</returns>
    public static Tilemap FromJson(ContentManager content, string jsonFilename)
    {
        string jsonPath = Path.Combine(content.RootDirectory, jsonFilename);
        
        using (Stream jsonStream = TitleContainer.OpenStream(jsonPath))
        {
            using (StreamReader reader = new StreamReader(jsonStream))
            {
                string jsonContent = reader.ReadToEnd();
                JsonDocument document = JsonDocument.Parse(jsonContent);
                JsonElement root = document.RootElement;

                // Parse map properties
                string name = root.GetProperty("name").GetString();
                int width = root.GetProperty("width").GetInt32();
                int height = root.GetProperty("height").GetInt32();
                int tileWidth = root.GetProperty("tileWidth").GetInt32();
                int tileHeight = root.GetProperty("tileHeight").GetInt32();
                string orientation = root.GetProperty("orientation").GetString();
                
                // Load the texture atlas
                string atlasFile = root.GetProperty("atlasFile").GetString();
                string atlasPath = Path.ChangeExtension(atlasFile, null); // Remove .png extension for content loading
                TextureAtlas atlas = TextureAtlas.FromJson(content, atlasPath + ".json", atlasPath + "_animations.json");

                // Create the tilemap
                Tilemap tilemap = new Tilemap(name, width, height, tileWidth, tileHeight, orientation, atlas);

                // Parse background color if present
                if (root.TryGetProperty("backgroundColor", out JsonElement bgColorElement) && bgColorElement.ValueKind != JsonValueKind.Null)
                {
                    string bgColorHex = bgColorElement.GetString();
                    if (!string.IsNullOrEmpty(bgColorHex))
                    {
                        tilemap.BackgroundColor = ParseColor(bgColorHex);
                    }
                }

                // Parse tilesets
                if (root.TryGetProperty("tilesets", out JsonElement tilesetsElement))
                {
                    foreach (JsonElement tilesetElement in tilesetsElement.EnumerateArray())
                    {
                        tilemap.LoadTilesetFromJson(tilesetElement);
                    }
                }

                // Parse tile layers
                if (root.TryGetProperty("tileLayers", out JsonElement layersElement))
                {
                    foreach (JsonElement layerElement in layersElement.EnumerateArray())
                    {
                        tilemap.LoadTileLayerFromJson(layerElement);
                    }
                }

                return tilemap;
            }
        }
    }

    /// <summary>
    /// Loads a tileset definition from JSON and creates the corresponding Tileset.
    /// </summary>
    private void LoadTilesetFromJson(JsonElement tilesetElement)
    {
        var tilesetDef = new TilesetDefinition
        {
            Name = tilesetElement.GetProperty("name").GetString(),
            FirstGid = tilesetElement.GetProperty("firstGid").GetInt32(),
            TileWidth = tilesetElement.GetProperty("tileWidth").GetInt32(),
            TileHeight = tilesetElement.GetProperty("tileHeight").GetInt32(),
            TileCount = tilesetElement.GetProperty("tileCount").GetInt32(),
            Columns = tilesetElement.GetProperty("columns").GetInt32(),
            Margin = tilesetElement.GetProperty("margin").GetInt32(),
            Spacing = tilesetElement.GetProperty("spacing").GetInt32(),
            AtlasSprite = tilesetElement.GetProperty("atlasSprite").GetString()
        };

        _tilesetDefinitions.Add(tilesetDef);

        // Create tileset from atlas sprite
        TextureRegion atlasRegion = _textureAtlas.GetRegion(tilesetDef.AtlasSprite);
        
        ITileset tileset;
        if (tilesetDef.Spacing > 0 || tilesetDef.Margin > 0)
        {
            tileset = new SpacedTileset(atlasRegion, tilesetDef.TileWidth, tilesetDef.TileHeight, tilesetDef.Spacing, tilesetDef.Margin);
        }
        else
        {
            tileset = new Tileset(atlasRegion, tilesetDef.TileWidth, tilesetDef.TileHeight);
        }

        _tilesets[tilesetDef.FirstGid] = tileset;
    }

    /// <summary>
    /// Loads a tile layer from JSON.
    /// </summary>
    private void LoadTileLayerFromJson(JsonElement layerElement)
    {
        var layer = new TileLayer
        {
            Id = layerElement.GetProperty("id").GetInt32(),
            Name = layerElement.GetProperty("name").GetString(),
            Width = layerElement.GetProperty("width").GetInt32(),
            Height = layerElement.GetProperty("height").GetInt32(),
            Opacity = layerElement.GetProperty("opacity").GetSingle(),
            Visible = layerElement.GetProperty("visible").GetBoolean(),
            OffsetX = layerElement.GetProperty("offsetX").GetSingle(),
            OffsetY = layerElement.GetProperty("offsetY").GetSingle()
        };

        // Load tile data
        JsonElement tilesElement = layerElement.GetProperty("tiles");
        layer.Tiles = new int[tilesElement.GetArrayLength()];
        int index = 0;
        foreach (JsonElement tileElement in tilesElement.EnumerateArray())
        {
            layer.Tiles[index++] = tileElement.GetInt32();
        }

        _tileLayers.Add(layer);
    }

    /// <summary>
    /// Gets the tileset and local tile index for a given global tile ID.
    /// </summary>
    private (ITileset tileset, int localIndex) GetTilesetForGid(int gid)
    {
        if (gid == 0) return (null, -1); // Empty tile

        // Find the tileset with the highest firstGid that's still <= gid
        TilesetDefinition bestTileset = null;
        foreach (var tilesetDef in _tilesetDefinitions)
        {
            if (tilesetDef.FirstGid <= gid && (bestTileset == null || tilesetDef.FirstGid > bestTileset.FirstGid))
            {
                bestTileset = tilesetDef;
            }
        }

        if (bestTileset == null) return (null, -1);

        var tileset = _tilesets[bestTileset.FirstGid];
        int localIndex = gid - bestTileset.FirstGid;
        
        return (tileset, localIndex);
    }

    /// <summary>
    /// Draws the entire tilemap with proper layer ordering (back to front).
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="position">The position to draw the tilemap at.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        foreach (var layer in _tileLayers)
        {
            if (!layer.Visible) continue;

            DrawLayer(spriteBatch, layer, position);
        }
    }

    /// <summary>
    /// Draws a specific layer of the tilemap.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="layer">The layer to draw.</param>
    /// <param name="position">The position to draw the layer at.</param>
    public void DrawLayer(SpriteBatch spriteBatch, TileLayer layer, Vector2 position)
    {
        Vector2 layerOffset = new Vector2(layer.OffsetX, layer.OffsetY);
        Color layerColor = Color.White * layer.Opacity;

        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = y * layer.Width + x;
                if (tileIndex >= layer.Tiles.Length) continue;

                int gid = layer.Tiles[tileIndex];
                if (gid == 0) continue; // Empty tile

                var (tileset, localIndex) = GetTilesetForGid(gid);
                if (tileset == null) continue;

                TextureRegion tileRegion = tileset.GetTile(localIndex);
                Vector2 tilePosition = position + layerOffset + new Vector2(x * DrawTileWidth, y * DrawTileHeight);

                spriteBatch.Draw(tileRegion.Texture, tilePosition, tileRegion.SourceRectangle, layerColor, 
                    0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
            }
        }
    }

    /// <summary>
    /// Draws layers up to a specified layer index (useful for character rendering between layers).
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="position">The position to draw the tilemap at.</param>
    /// <param name="upToLayerIndex">The maximum layer index to draw (inclusive).</param>
    public void DrawLayersUpTo(SpriteBatch spriteBatch, Vector2 position, int upToLayerIndex)
    {
        for (int i = 0; i <= upToLayerIndex && i < _tileLayers.Count; i++)
        {
            var layer = _tileLayers[i];
            if (!layer.Visible) continue;

            DrawLayer(spriteBatch, layer, position);
        }
    }

    /// <summary>
    /// Draws layers from a specified layer index onwards (useful for foreground layers).
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw with.</param>
    /// <param name="position">The position to draw the tilemap at.</param>
    /// <param name="fromLayerIndex">The minimum layer index to draw (inclusive).</param>
    public void DrawLayersFrom(SpriteBatch spriteBatch, Vector2 position, int fromLayerIndex)
    {
        for (int i = fromLayerIndex; i < _tileLayers.Count; i++)
        {
            var layer = _tileLayers[i];
            if (!layer.Visible) continue;

            DrawLayer(spriteBatch, layer, position);
        }
    }

    /// <summary>
    /// Gets a tile layer by name.
    /// </summary>
    /// <param name="name">The name of the layer to find.</param>
    /// <returns>The tile layer with the specified name, or null if not found.</returns>
    public TileLayer GetLayerByName(string name)
    {
        return _tileLayers.FirstOrDefault(layer => layer.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a tile layer by index.
    /// </summary>
    /// <param name="index">The index of the layer.</param>
    /// <returns>The tile layer at the specified index.</returns>
    public TileLayer GetLayerByIndex(int index)
    {
        return index >= 0 && index < _tileLayers.Count ? _tileLayers[index] : null;
    }

    /// <summary>
    /// Parses a hex color string to a Color.
    /// </summary>
    private static Color ParseColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor)) return Color.Transparent;
        
        hexColor = hexColor.TrimStart('#');
        if (hexColor.Length == 6)
        {
            return new Color(
                Convert.ToByte(hexColor.Substring(0, 2), 16),
                Convert.ToByte(hexColor.Substring(2, 2), 16),
                Convert.ToByte(hexColor.Substring(4, 2), 16)
            );
        }
        
        return Color.Transparent;
    }
}