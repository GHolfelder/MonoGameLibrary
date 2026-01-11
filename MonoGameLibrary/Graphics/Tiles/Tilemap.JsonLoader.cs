using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// JSON loading functionality for Tilemap.
/// </summary>
public partial class Tilemap
{
    /// <summary>
    /// Creates a collection of tilemaps by loading and parsing a JSON file with a pre-loaded texture atlas.
    /// Expects a JSON array of map objects.
    /// </summary>
    /// <param name="content">The content manager to use for loading resources.</param>
    /// <param name="jsonFilename">The JSON file path relative to the content directory.</param>
    /// <param name="textureAtlas">The pre-loaded texture atlas containing tile sprites.</param>
    /// <returns>A TilemapCollection containing all maps loaded from JSON.</returns>
    public static TilemapCollection FromJson(ContentManager content, string jsonFilename, TextureAtlas textureAtlas)
    {
        string jsonPath = Path.Combine(content.RootDirectory, jsonFilename);
        
        using (Stream jsonStream = TitleContainer.OpenStream(jsonPath))
        {
            using (StreamReader reader = new StreamReader(jsonStream))
            {
                string jsonContent = reader.ReadToEnd();
                JsonDocument document = JsonDocument.Parse(jsonContent);
                JsonElement root = document.RootElement;

                var collection = new TilemapCollection();

                // Expect JSON array format: [{ "name": "map1", ... }, { "name": "map2", ... }]
                if (root.ValueKind == JsonValueKind.Array)
                {
                    LoadFromMapsArrayFormat(root, textureAtlas, collection);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid JSON tilemap format in file: {jsonFilename}. Expected array of map objects.");
                }

                if (collection.Count == 0)
                {
                    throw new InvalidOperationException($"No valid maps found in JSON file: {jsonFilename}");
                }

                return collection;
            }
        }
    }

    /// <summary>
    /// Loads maps from array format: [{ "name": "map1", ... }, { "name": "map2", ... }]
    /// </summary>
    private static void LoadFromMapsArrayFormat(JsonElement mapsArray, TextureAtlas textureAtlas, TilemapCollection collection)
    {
        foreach (JsonElement mapElement in mapsArray.EnumerateArray())
        {
            // Extract map name from the map element
            string mapName = mapElement.TryGetProperty("name", out JsonElement nameElement) 
                ? nameElement.GetString() 
                : $"Map_{collection.Count}"; // Fallback name if none specified
                
            Tilemap tilemap = LoadSingleMapFromJson(mapElement, textureAtlas, mapName);
            collection.AddMap(tilemap);
        }
    }

    /// <summary>
    /// Loads a single map from a JSON element.
    /// </summary>
    private static Tilemap LoadSingleMapFromJson(JsonElement mapElement, TextureAtlas textureAtlas, string mapName)
    {
        // Parse map properties
        string name = mapElement.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : mapName;
        int width = mapElement.GetProperty("width").GetInt32();
        int height = mapElement.GetProperty("height").GetInt32();
        int tileWidth = mapElement.GetProperty("tileWidth").GetInt32();
        int tileHeight = mapElement.GetProperty("tileHeight").GetInt32();
        string orientation = mapElement.GetProperty("orientation").GetString();
        
        // Create the tilemap
        Tilemap tilemap = new Tilemap(name, width, height, tileWidth, tileHeight, orientation, textureAtlas);

        // Parse background color if present
        if (mapElement.TryGetProperty("backgroundColor", out JsonElement bgColorElement) && bgColorElement.ValueKind != JsonValueKind.Null)
        {
            string bgColorHex = bgColorElement.GetString();
            if (!string.IsNullOrEmpty(bgColorHex))
            {
                tilemap.BackgroundColor = ParseColor(bgColorHex);
            }
        }

        // Parse tilesets
        if (mapElement.TryGetProperty("tilesets", out JsonElement tilesetsElement))
        {
            foreach (JsonElement tilesetElement in tilesetsElement.EnumerateArray())
            {
                tilemap.LoadTilesetFromJson(tilesetElement);
            }
        }

        // Parse tile layers
        if (mapElement.TryGetProperty("tileLayers", out JsonElement layersElement))
        {
            foreach (JsonElement layerElement in layersElement.EnumerateArray())
            {
                tilemap.LoadTileLayerFromJson(layerElement);
            }
        }

        // Parse object layers
        if (mapElement.TryGetProperty("objectLayers", out JsonElement objectLayersElement))
        {
            foreach (JsonElement objectLayerElement in objectLayersElement.EnumerateArray())
            {
                tilemap.LoadObjectLayerFromJson(objectLayerElement);
            }
        }

        return tilemap;
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
                if (tileElement.TryGetProperty("animation", out JsonElement animationElement) && animationElement.ValueKind != JsonValueKind.Null)
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
                }
                
                // Parse collision objects for this tile
                if (tileElement.TryGetProperty("collisionObjects", out JsonElement collisionObjectsElement) && collisionObjectsElement.ValueKind != JsonValueKind.Null)
                {
                    var collisionObjectsList = new List<object>();
                    
                    foreach (JsonElement collisionObjElement in collisionObjectsElement.EnumerateArray())
                    {
                        var collisionObject = new CollisionObject();
                        
                        // Parse collision object properties
                        if (collisionObjElement.TryGetProperty("name", out JsonElement objNameElement))
                            collisionObject.Name = objNameElement.GetString();
                        
                        if (collisionObjElement.TryGetProperty("type", out JsonElement objTypeElement))
                            collisionObject.Type = objTypeElement.GetString();
                        
                        if (collisionObjElement.TryGetProperty("x", out JsonElement xElement))
                            collisionObject.Position = new Vector2((float)Math.Truncate(xElement.GetSingle()), collisionObject.Position.Y);
                        
                        if (collisionObjElement.TryGetProperty("y", out JsonElement yElement))
                            collisionObject.Position = new Vector2(collisionObject.Position.X, (float)Math.Truncate(yElement.GetSingle()));
                        
                        if (collisionObjElement.TryGetProperty("width", out JsonElement widthElement))
                            collisionObject.Width = (int)Math.Truncate(widthElement.GetSingle());
                        
                        if (collisionObjElement.TryGetProperty("height", out JsonElement heightElement))
                            collisionObject.Height = (int)Math.Truncate(heightElement.GetSingle());
                        
                        if (collisionObjElement.TryGetProperty("rotation", out JsonElement rotationElement))
                            collisionObject.Rotation = rotationElement.GetSingle();
                        
                        if (collisionObjElement.TryGetProperty("gid", out JsonElement gidElement))
                            collisionObject.Gid = gidElement.GetInt32();
                        
                        // Determine shape type
                        if (collisionObjElement.TryGetProperty("objectType", out JsonElement objectTypeElement))
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
                                _ => CollisionObjectType.Rectangle
                            };
                        }
                        else
                        {
                            // Legacy shape detection
                            if (collisionObjElement.TryGetProperty("polygon", out JsonElement polygonElement) && polygonElement.ValueKind == JsonValueKind.Array)
                            {
                                collisionObject.ShapeType = CollisionObjectType.Polygon;
                            }
                            else if (collisionObjElement.TryGetProperty("polyline", out JsonElement polylineElement) && polylineElement.ValueKind == JsonValueKind.Array)
                            {
                                collisionObject.ShapeType = CollisionObjectType.Polyline;
                            }
                            else if (collisionObject.Width == 0 && collisionObject.Height == 0)
                            {
                                collisionObject.ShapeType = CollisionObjectType.Point;
                            }
                            else if (collisionObjElement.TryGetProperty("ellipse", out JsonElement ellipseElement) && ellipseElement.GetBoolean())
                            {
                                collisionObject.ShapeType = CollisionObjectType.Ellipse;
                            }
                            else
                            {
                                collisionObject.ShapeType = CollisionObjectType.Rectangle;
                            }
                        }
                        
                        // Parse polygon/polyline points
                        if (collisionObjElement.TryGetProperty("polygon", out JsonElement polygonElement2) && polygonElement2.ValueKind == JsonValueKind.Array)
                        {
                            collisionObject.PolygonPoints = ParsePolygonPoints(polygonElement2);
                        }
                        else if (collisionObjElement.TryGetProperty("polyline", out JsonElement polylineElement2) && polylineElement2.ValueKind == JsonValueKind.Array)
                        {
                            collisionObject.PolygonPoints = ParsePolygonPoints(polylineElement2);
                        }
                        
                        // Parse properties if they exist
                        if (collisionObjElement.TryGetProperty("properties", out JsonElement propertiesElement))
                        {
                            foreach (JsonProperty property in propertiesElement.EnumerateObject())
                            {
                                collisionObject.Properties[property.Name] = property.Value.ToString();
                            }
                        }
                        
                        collisionObjectsList.Add(collisionObject);
                    }
                    
                    // Store collision objects separately
                    int globalGid = tilesetDef.FirstGid + animatedTile.Id;
                    var tileData = new TileData
                    {
                        CollisionObjects = collisionObjectsList.ToArray()
                    };
                    _tileData[globalGid] = tileData;
                }
                
                // Register tiles that have animation (multiple frames)
                bool hasAnimation = animatedTile.Animation.Count >= 2;
                
                if (hasAnimation)
                {
                    int globalGid = tilesetDef.FirstGid + animatedTile.Id;
                    _animatedTiles[globalGid] = animatedTile;
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