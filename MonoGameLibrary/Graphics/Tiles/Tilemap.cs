using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics.Collision;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// The enhanced Tilemap class with support for multiple layers and z-ordering.
/// Core tilemap functionality - rendering, layer management, and data access.
/// </summary>
public partial class Tilemap
{
    private readonly List<TileLayer> _tileLayers;
    private readonly List<TilesetDefinition> _tilesetDefinitions;
    private readonly Dictionary<int, ITileset> _tilesets;
    private readonly TextureAtlas _textureAtlas;
    private readonly List<ObjectLayer> _objectLayers;
    private readonly Dictionary<int, AnimatedTile> _animatedTiles; // GID -> Animation data (only truly animated tiles)
    private readonly Dictionary<int, TileData> _tileData; // GID -> Collision/property data
    private readonly Dictionary<string, Dictionary<int, AnimatedTileInstance>> _animatedTileInstances; // LayerName -> (TileIndex -> Instance)

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
    /// Gets the object layers containing collision objects and other map objects.
    /// </summary>
    public IReadOnlyList<ObjectLayer> ObjectLayers => _objectLayers.AsReadOnly();

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
        _objectLayers = new List<ObjectLayer>();
        _animatedTiles = new Dictionary<int, AnimatedTile>();
        _tileData = new Dictionary<int, TileData>();
        _animatedTileInstances = new Dictionary<string, Dictionary<int, AnimatedTileInstance>>();
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
    /// Creates animated tile instances for a layer that contains animated tiles.
    /// </summary>
    /// <param name="layer">The tile layer to process.</param>
    private void CreateAnimatedTileInstances(TileLayer layer)
    {
        var instances = new Dictionary<int, AnimatedTileInstance>(); // tile index -> instance
        
        for (int i = 0; i < layer.Tiles.Length; i++)
        {
            int gid = layer.Tiles[i];
            if (gid == 0) continue; // Empty tile
            
            // Only create instances for truly animated tiles (not tiles with just collision data)
            if (_animatedTiles.TryGetValue(gid, out AnimatedTile animatedTile) && 
                animatedTile.Animation.Count >= 2)
            {
                // Find the tileset definition to get the atlas sprite
                TilesetDefinition tilesetDef = null;
                foreach (var def in _tilesetDefinitions)
                {
                    if (def.FirstGid <= gid && (tilesetDef == null || def.FirstGid > tilesetDef.FirstGid))
                    {
                        tilesetDef = def;
                    }
                }
                
                if (tilesetDef != null)
                {
                    // Use the entire tileset atlas region as the base for animation coordinates
                    var atlasRegion = _textureAtlas.GetRegion(tilesetDef.AtlasSprite);
                    var instance = new AnimatedTileInstance(animatedTile, atlasRegion);
                    instances[i] = instance;
                }
            }
        }
        
        if (instances.Count > 0)
        {
            _animatedTileInstances[layer.Name] = instances;
        }
    }

    /// <summary>
    /// Updates all animated tiles in the tilemap.
    /// </summary>
    /// <param name="gameTime">Snapshot of the game timing values.</param>
    public void Update(GameTime gameTime)
    {
        foreach (var layerInstances in _animatedTileInstances.Values)
        {
            foreach (var instance in layerInstances.Values)
            {
                instance.Update(gameTime);
            }
        }
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
        
        // Get animated tile instances for this layer
        _animatedTileInstances.TryGetValue(layer.Name, out Dictionary<int, AnimatedTileInstance> animatedInstances);

        for (int y = 0; y < layer.Height; y++)
        {
            for (int x = 0; x < layer.Width; x++)
            {
                int tileIndex = y * layer.Width + x;
                if (tileIndex >= layer.Tiles.Length) continue;

                int gid = layer.Tiles[tileIndex];
                if (gid == 0) continue; // Empty tile

                Vector2 tilePosition = position + layerOffset + new Vector2(x * DrawTileWidth, y * DrawTileHeight);

                // Check if this specific tile position has an animated instance
                if (animatedInstances != null && animatedInstances.TryGetValue(tileIndex, out AnimatedTileInstance animatedInstance))
                {
                    // Draw animated tile
                    var animatedSourceRect = animatedInstance.GetCurrentSourceRectangle();
                    
                    spriteBatch.Draw(animatedInstance.Texture, tilePosition, animatedSourceRect, layerColor,
                        0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
                else
                {
                    // Draw regular tile
                    var (tileset, localIndex) = GetTilesetForGid(gid);
                    if (tileset == null) continue;

                    TextureRegion tileRegion = tileset.GetTile(localIndex);
                    
                    spriteBatch.Draw(tileRegion.Texture, tilePosition, tileRegion.SourceRectangle, layerColor, 
                        0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
                }
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
    /// Gets an object layer by name.
    /// </summary>
    /// <param name="name">The name of the object layer to find.</param>
    /// <returns>The object layer with the specified name, or null if not found.</returns>
    public ObjectLayer GetObjectLayer(string name)
    {
        return _objectLayers.FirstOrDefault(layer => layer.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets an object layer by index.
    /// </summary>
    /// <param name="index">The index of the object layer.</param>
    /// <returns>The object layer at the specified index.</returns>
    public ObjectLayer GetObjectLayerByIndex(int index)
    {
        return index >= 0 && index < _objectLayers.Count ? _objectLayers[index] : null;
    }

    /// <summary>
    /// Gets the animated tile definition for a given GID.
    /// </summary>
    /// <param name="gid">The global tile ID.</param>
    /// <param name="animatedTile">The animated tile definition if found.</param>
    /// <returns>True if an animated tile was found for this GID.</returns>
    internal bool GetAnimatedTile(int gid, out AnimatedTile animatedTile)
    {
        return _animatedTiles.TryGetValue(gid, out animatedTile);
    }

    /// <summary>
    /// Gets the tile data for a given GID.
    /// </summary>
    /// <param name="gid">The global tile ID.</param>
    /// <param name="tileData">The tile data if found.</param>
    /// <returns>True if tile data was found for this GID.</returns>
    internal bool GetTileData(int gid, out TileData tileData)
    {
        return _tileData.TryGetValue(gid, out tileData);
    }

    /// <summary>
    /// Gets collision objects from a specific object layer or all object layers.
    /// </summary>
    /// <param name="layerName">The name of the object layer, or null to get objects from all layers.</param>
    /// <param name="objectName">The name of the object to filter by, or null to get all objects.</param>
    /// <returns>List of collision objects from the specified layer(s) matching the object name filter.</returns>
    public List<CollisionObject> GetCollisionObjects(string layerName = null, string objectName = null)
    {
        List<CollisionObject> objects;
        
        if (string.IsNullOrEmpty(layerName))
            objects = _objectLayers.SelectMany(layer => layer.Objects).ToList();
        else
        {
            var layer = GetObjectLayer(layerName);
            objects = layer?.Objects ?? new List<CollisionObject>();
        }
        
        if (!string.IsNullOrEmpty(objectName))
            objects = objects.Where(obj => string.Equals(obj.Name, objectName, StringComparison.OrdinalIgnoreCase)).ToList();
        
        return objects;
    }

    /// <summary>
    /// Finds the first collision object from a specific object layer or all object layers.
    /// </summary>
    /// <param name="layerName">The name of the object layer, or null to search all layers.</param>
    /// <param name="objectName">The name of the object to find, or null to get the first object.</param>
    /// <returns>The first collision object matching the criteria, or null if no match is found.</returns>
    public CollisionObject FindFirstCollisionObject(string layerName = null, string objectName = null)
    {
        IEnumerable<CollisionObject> objects;
        
        if (string.IsNullOrEmpty(layerName))
            objects = _objectLayers.SelectMany(layer => layer.Objects);
        else
        {
            var layer = GetObjectLayer(layerName);
            objects = layer?.Objects ?? Enumerable.Empty<CollisionObject>();
        }
        
        if (!string.IsNullOrEmpty(objectName))
            return objects.FirstOrDefault(obj => string.Equals(obj.Name, objectName, StringComparison.OrdinalIgnoreCase));
        
        return objects.FirstOrDefault();
    }

}