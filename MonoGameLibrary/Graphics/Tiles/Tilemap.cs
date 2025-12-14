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
/// Represents a single frame of a tile animation.
/// </summary>
public class AnimatedTileFrame
{
    public int TileId { get; set; }
    public int Duration { get; set; }  // Duration in milliseconds
    public int SourceX { get; set; }
    public int SourceY { get; set; }
    public int SourceWidth { get; set; }
    public int SourceHeight { get; set; }
}

/// <summary>
/// Represents an animated tile with its animation frames.
/// </summary>
public class AnimatedTile
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string AtlasSprite { get; set; }
    public List<AnimatedTileFrame> Animation { get; set; } = new();
    public object[] CollisionObjects { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Manages animation state for an animated tile instance.
/// </summary>
public class AnimatedTileInstance
{
    private readonly AnimatedTile _animatedTile;
    private readonly TextureRegion _atlasRegion;
    private int _currentFrame;
    private TimeSpan _elapsed;

    public AnimatedTileInstance(AnimatedTile animatedTile, TextureRegion atlasRegion)
    {
        _animatedTile = animatedTile;
        _atlasRegion = atlasRegion;
        _currentFrame = 0;
        _elapsed = TimeSpan.Zero;
    }

    public void Update(GameTime gameTime)
    {
        if (_animatedTile.Animation.Count < 2) return; // Need at least 2 frames for animation

        _elapsed += gameTime.ElapsedGameTime;
        
        var currentAnimationFrame = _animatedTile.Animation[_currentFrame];
        var frameDuration = TimeSpan.FromMilliseconds(currentAnimationFrame.Duration);
        
        if (_elapsed >= frameDuration)
        {
            _elapsed -= frameDuration;
            _currentFrame = (_currentFrame + 1) % _animatedTile.Animation.Count;
        }
    }

    public Rectangle GetCurrentSourceRectangle()
    {
        if (_animatedTile.Animation.Count == 0)
            return _atlasRegion.SourceRectangle; // Fallback to first tile
            
        var frame = _animatedTile.Animation[_currentFrame];
        
        // Use animation frame coordinates relative to the atlas region
        return new Rectangle(
            _atlasRegion.SourceRectangle.X + frame.SourceX, 
            _atlasRegion.SourceRectangle.Y + frame.SourceY, 
            frame.SourceWidth, 
            frame.SourceHeight);
    }

    public Texture2D Texture => _atlasRegion.Texture;
}

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
    public List<AnimatedTile> Tiles { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
}

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
    /// Gets the radius for circular/elliptical objects (uses Width as diameter).
    /// </summary>
    public float Radius => Width * 0.5f;
    
    /// <summary>
    /// Gets whether this object is circular (ellipse with equal width and height).
    /// </summary>
    public bool IsCircle => ShapeType == CollisionObjectType.Ellipse && Width == Height;
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

/// <summary>
/// The enhanced Tilemap class with support for multiple layers and z-ordering.
/// </summary>
public class Tilemap
{
    private readonly List<TileLayer> _tileLayers;
    private readonly List<TilesetDefinition> _tilesetDefinitions;
    private readonly Dictionary<int, ITileset> _tilesets;
    private readonly TextureAtlas _textureAtlas;
    private readonly List<ObjectLayer> _objectLayers;
    private readonly Dictionary<int, AnimatedTile> _animatedTiles; // GID -> AnimatedTile
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
        _animatedTileInstances = new Dictionary<string, Dictionary<int, AnimatedTileInstance>>();
    }

    /// <summary>
    /// Creates a new tilemap by loading and parsing a JSON file with a pre-loaded texture atlas.
    /// </summary>
    /// <param name="content">The content manager to use for loading resources.</param>
    /// <param name="jsonFilename">The JSON file path relative to the content directory.</param>
    /// <param name="textureAtlas">The pre-loaded texture atlas containing tile sprites.</param>
    /// <returns>A new Tilemap instance loaded from JSON.</returns>
    public static Tilemap FromJson(ContentManager content, string jsonFilename, TextureAtlas textureAtlas)
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
                
                // Use the provided texture atlas instead of loading a new one
                // This follows the singleton Core pattern for shared resource management

                // Create the tilemap
                Tilemap tilemap = new Tilemap(name, width, height, tileWidth, tileHeight, orientation, textureAtlas);

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

                // Parse object layers
                if (root.TryGetProperty("objectLayers", out JsonElement objectLayersElement))
                {
                    foreach (JsonElement objectLayerElement in objectLayersElement.EnumerateArray())
                    {
                        tilemap.LoadObjectLayerFromJson(objectLayerElement);
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

        // Parse tiles array for animated tiles
        if (tilesetElement.TryGetProperty("tiles", out JsonElement tilesElement))
        {
            foreach (JsonElement tileElement in tilesElement.EnumerateArray())
            {
                var animatedTile = new AnimatedTile();
                
                if (tileElement.TryGetProperty("id", out JsonElement idElement))
                    animatedTile.Id = idElement.GetInt32();
                    
                if (tileElement.TryGetProperty("type", out JsonElement typeElement))
                    animatedTile.Type = typeElement.GetString();
                    
                if (tileElement.TryGetProperty("atlasSprite", out JsonElement atlasSpriteElement))
                    animatedTile.AtlasSprite = atlasSpriteElement.GetString();
                
                // Parse animation frames
                if (tileElement.TryGetProperty("animation", out JsonElement animationElement))
                {
                    foreach (JsonElement frameElement in animationElement.EnumerateArray())
                    {
                        var frame = new AnimatedTileFrame();
                        
                        if (frameElement.TryGetProperty("tileId", out JsonElement tileIdElement))
                            frame.TileId = tileIdElement.GetInt32();
                            
                        if (frameElement.TryGetProperty("duration", out JsonElement durationElement))
                            frame.Duration = durationElement.GetInt32();
                            
                        if (frameElement.TryGetProperty("sourceX", out JsonElement sourceXElement))
                            frame.SourceX = sourceXElement.GetInt32();
                            
                        if (frameElement.TryGetProperty("sourceY", out JsonElement sourceYElement))
                            frame.SourceY = sourceYElement.GetInt32();
                            
                        if (frameElement.TryGetProperty("sourceWidth", out JsonElement sourceWidthElement))
                            frame.SourceWidth = sourceWidthElement.GetInt32();
                            
                        if (frameElement.TryGetProperty("sourceHeight", out JsonElement sourceHeightElement))
                            frame.SourceHeight = sourceHeightElement.GetInt32();
                        
                        animatedTile.Animation.Add(frame);
                    }
                    
                    // Only register animated tiles that have multiple frames
                    if (animatedTile.Animation.Count >= 2)
                    {
                        int globalGid = tilesetDef.FirstGid + animatedTile.Id;
                        _animatedTiles[globalGid] = animatedTile;
                    }
                }
                
                tilesetDef.Tiles.Add(animatedTile);
            }
        }

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

        // Create animated tile instances for this layer
        CreateAnimatedTileInstances(layer);

        _tileLayers.Add(layer);
    }

    /// <summary>
    /// Loads an object layer from JSON.
    /// </summary>
    private void LoadObjectLayerFromJson(JsonElement objectLayerElement)
    {
        var objectLayer = new ObjectLayer();
        
        // Load basic layer properties
        if (objectLayerElement.TryGetProperty("name", out JsonElement nameElement))
            objectLayer.Name = nameElement.GetString();
        
        if (objectLayerElement.TryGetProperty("visible", out JsonElement visibleElement))
            objectLayer.Visible = visibleElement.GetBoolean();
        
        if (objectLayerElement.TryGetProperty("opacity", out JsonElement opacityElement))
            objectLayer.Opacity = opacityElement.GetSingle();
        
        // Load objects array
        if (objectLayerElement.TryGetProperty("objects", out JsonElement objectsElement))
        {
            foreach (JsonElement objElement in objectsElement.EnumerateArray())
            {
                var collisionObject = new CollisionObject();
                
                // Load basic object properties
                if (objElement.TryGetProperty("name", out JsonElement objNameElement))
                    collisionObject.Name = objNameElement.GetString();
                
                if (objElement.TryGetProperty("type", out JsonElement objTypeElement))
                    collisionObject.Type = objTypeElement.GetString();
                
                if (objElement.TryGetProperty("x", out JsonElement xElement))
                    collisionObject.Position = new Vector2((float)Math.Truncate(xElement.GetSingle()), collisionObject.Position.Y);
                
                if (objElement.TryGetProperty("y", out JsonElement yElement))
                    collisionObject.Position = new Vector2(collisionObject.Position.X, (float)Math.Truncate(yElement.GetSingle()));
                
                if (objElement.TryGetProperty("width", out JsonElement widthElement))
                    collisionObject.Width = (int)Math.Truncate(widthElement.GetSingle());
                
                if (objElement.TryGetProperty("height", out JsonElement heightElement))
                    collisionObject.Height = (int)Math.Truncate(heightElement.GetSingle());
                
                if (objElement.TryGetProperty("rotation", out JsonElement rotationElement))
                    collisionObject.Rotation = rotationElement.GetSingle();
                
                if (objElement.TryGetProperty("gid", out JsonElement gidElement))
                    collisionObject.Gid = gidElement.GetInt32();
                
                // Determine shape type based on objectType property first, then fallback to legacy detection
                if (objElement.TryGetProperty("objectType", out JsonElement objectTypeElement))
                {
                    string objectType = objectTypeElement.GetString()?.ToLowerInvariant();
                    collisionObject.ShapeType = objectType switch
                    {
                        "rectangle" => CollisionObjectType.Rectangle,
                        "ellipse" => CollisionObjectType.Ellipse,
                        "point" => CollisionObjectType.Point,
                        "polygon" => CollisionObjectType.Polygon,
                        "polyline" => CollisionObjectType.Polyline,
                        "tile" => CollisionObjectType.Tile,
                        "text" => CollisionObjectType.Text,
                        _ => CollisionObjectType.Rectangle // Default fallback
                    };
                }
                else
                {
                    // Legacy shape detection for objects without objectType property
                    if (objElement.TryGetProperty("polygon", out JsonElement polygonElement) && polygonElement.ValueKind == JsonValueKind.Array)
                    {
                        collisionObject.ShapeType = CollisionObjectType.Polygon;
                    }
                    else if (objElement.TryGetProperty("polyline", out JsonElement polylineElement) && polylineElement.ValueKind == JsonValueKind.Array)
                    {
                        collisionObject.ShapeType = CollisionObjectType.Polyline;
                    }
                    else if (collisionObject.Width == 0 && collisionObject.Height == 0)
                    {
                        collisionObject.ShapeType = CollisionObjectType.Point;
                    }
                    else if (objElement.TryGetProperty("ellipse", out JsonElement ellipseElement) && ellipseElement.GetBoolean())
                    {
                        collisionObject.ShapeType = CollisionObjectType.Ellipse;
                    }
                    else if (collisionObject.Name != null && 
                             (collisionObject.Name.Contains("Ellipse", StringComparison.OrdinalIgnoreCase) ||
                              collisionObject.Name.Contains("Circle", StringComparison.OrdinalIgnoreCase)))
                    {
                        collisionObject.ShapeType = CollisionObjectType.Ellipse;
                    }
                    else
                    {
                        collisionObject.ShapeType = CollisionObjectType.Rectangle;
                    }
                }
                
                // Parse polygon/polyline points regardless of how shape type was determined
                if (objElement.TryGetProperty("polygon", out JsonElement polygonElement2) && polygonElement2.ValueKind == JsonValueKind.Array)
                {
                    collisionObject.PolygonPoints = ParsePolygonPoints(polygonElement2);
                }
                else if (objElement.TryGetProperty("polyline", out JsonElement polylineElement2) && polylineElement2.ValueKind == JsonValueKind.Array)
                {
                    collisionObject.PolygonPoints = ParsePolygonPoints(polylineElement2);
                }
                
                // Parse text content for text objects
                if (objElement.TryGetProperty("text", out JsonElement textElement) && textElement.ValueKind == JsonValueKind.Object)
                {
                    if (textElement.TryGetProperty("content", out JsonElement contentElement))
                    {
                        collisionObject.TextContent = contentElement.GetString() ?? string.Empty;
                    }
                }
                
                // Load custom properties if they exist
                if (objElement.TryGetProperty("properties", out JsonElement propertiesElement))
                {
                    foreach (JsonProperty property in propertiesElement.EnumerateObject())
                    {
                        collisionObject.Properties[property.Name] = property.Value.ToString();
                    }
                }
                
                objectLayer.Objects.Add(collisionObject);
            }
        }
        
        // Load layer properties if they exist
        if (objectLayerElement.TryGetProperty("properties", out JsonElement layerPropertiesElement))
        {
            foreach (JsonProperty property in layerPropertiesElement.EnumerateObject())
            {
                objectLayer.Properties[property.Name] = property.Value.ToString();
            }
        }
        
        _objectLayers.Add(objectLayer);
    }

    /// <summary>
    /// Parses polygon points from JSON element.
    /// </summary>
    private static Vector2[] ParsePolygonPoints(JsonElement polygonElement)
    {
        var points = new List<Vector2>();
        
        foreach (JsonElement pointElement in polygonElement.EnumerateArray())
        {
            if (pointElement.TryGetProperty("x", out JsonElement xElement) &&
                pointElement.TryGetProperty("y", out JsonElement yElement))
            {
                points.Add(new Vector2(xElement.GetSingle(), yElement.GetSingle()));
            }
        }
        
        return points.ToArray();
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
            
            if (_animatedTiles.TryGetValue(gid, out AnimatedTile animatedTile))
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
    /// Gets collision objects from a specific object layer or all object layers.
    /// </summary>
    /// <param name="layerName">The name of the object layer, or null to get objects from all layers.</param>
    /// <returns>List of collision objects from the specified layer(s).</returns>
    public List<CollisionObject> GetCollisionObjects(string layerName = null)
    {
        if (string.IsNullOrEmpty(layerName))
            return _objectLayers.SelectMany(layer => layer.Objects).ToList();
        
        var layer = GetObjectLayer(layerName);
        return layer?.Objects ?? new List<CollisionObject>();
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