using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace MonoGameLibrary.Graphics;
/// <summary>
/// This class holds all of the individual sprites and animations for the game. 
/// The individual sprites are represented by the TextureRegion class. The animations
/// are represented by the Animation class.
/// </summary>
public class TextureAtlas
{
    /// <summary>
    /// Map containing all texture regions to be used in game. This allows us to 
    /// retrieve specific regions by name.
    /// </summary>
    private Dictionary<string, TextureRegion> _regions;

    /// <summary>
    /// Map containing all animations to be used in game. This allows us to 
    /// retrieve specific animations by name.
    /// </summary>
    private Dictionary<string, Animation> _animations;

    /// <summary>
    /// Gets or Sets the source texture represented by this texture atlas. This holds the 
    /// source texture that contains all the regions.
    /// </summary>
    public Texture2D Texture { get; set; }

    /// <summary>
    /// Creates a new empty texture atlas.
    /// </summary>
    public TextureAtlas()
    {
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    /// <summary>
    /// Creates a new texture atlas instance using the given texture.
    /// </summary>
    /// <param name="texture">The source texture represented by the texture atlas.</param>
    public TextureAtlas(Texture2D texture)
    {
        Texture = texture;
        _regions = new Dictionary<string, TextureRegion>();
        _animations = new Dictionary<string, Animation>();
    }

    /// <summary>
    /// Creates a new texture atlas based on a texture atlas xml configuration file.
    /// This method loads both texture regions and animations from the same XML file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromXml(ContentManager content, string fileName)
    {
        TextureAtlas atlas = FromXmlTexture(content, fileName);
        atlas.LoadAnimationsFromXml(fileName);
        return atlas;
    }

    /// <summary>
    /// Creates a new texture atlas based on a texture atlas xml configuration file.
    /// This method only loads texture regions from the XML file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromXmlTexture(ContentManager content, string fileName)
    {
        TextureAtlas atlas = new TextureAtlas();

        string filePath = Path.Combine(content.RootDirectory, fileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                // The <Texture> element contains the content path for the Texture2D to load.
                // So we will retrieve that value then use the content manager to load the texture.
                string texturePath = root.Element("Texture").Value;
                atlas.Texture = content.Load<Texture2D>(texturePath);

                // The <Regions> element contains individual <Region> elements, each one describing
                // a different texture region within the atlas.  
                //
                // Example:
                // <Regions>
                //      <Region name="spriteOne" x="0" y="0" width="32" height="32" />
                //      <Region name="spriteTwo" x="32" y="0" width="32" height="32" />
                // </Regions>
                //
                // So we retrieve all of the <Region> elements then loop through each one
                // and generate a new TextureRegion instance from it and add it to this atlas.
                var regions = root.Element("Regions")?.Elements("Region");

                if (regions != null)
                {
                    foreach (var region in regions)
                    {
                        string name = region.Attribute("name")?.Value;
                        int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                        int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                        int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                        int height = int.Parse(region.Attribute("height")?.Value ?? "0");

                        if (!string.IsNullOrEmpty(name))
                        {
                            atlas.AddRegion(name, x, y, width, height);
                        }
                    }
                }

                return atlas;
            }
        }
    }

    /// <summary>
    /// Loads animations from an XML file and adds them to this texture atlas.
    /// </summary>
    /// <param name="fileName">The path to the xml file, relative to the content root directory.</param>
    public void LoadAnimationsFromXml(string fileName)
    {
        string filePath = Path.Combine(Core.Content.RootDirectory, fileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XDocument doc = XDocument.Load(reader);
                XElement root = doc.Root;

                // The <Animations> element contains individual <Animation> elements, each one describing
                // a different animation within the atlas.
                var animationElements = root.Element("Animations")?.Elements("Animation");

                if (animationElements != null)
                {
                    foreach (var animationElement in animationElements)
                    {
                        string name = animationElement.Attribute("name")?.Value;
                        float delayInMilliseconds = float.Parse(animationElement.Attribute("delay")?.Value ?? "0");
                        TimeSpan delay = TimeSpan.FromMilliseconds(delayInMilliseconds);

                        List<TextureRegion> frames = new List<TextureRegion>();

                        var frameElements = animationElement.Elements("Frame");

                        if (frameElements != null)
                        {
                            foreach (var frameElement in frameElements)
                            {
                                string regionName = frameElement.Attribute("region").Value;
                                TextureRegion region = GetRegion(regionName);
                                frames.Add(region);
                            }
                        }

                        Animation animation = new Animation(frames, delay);
                        AddAnimation(name, animation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new texture atlas from separate XML files for texture and animations.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="textureFileName">The path to the texture xml file, relative to the content root directory.</param>
    /// <param name="animationFileName">The path to the animation xml file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromXml(ContentManager content, string textureFileName, string animationFileName)
    {
        TextureAtlas atlas = FromXmlTexture(content, textureFileName);
        atlas.LoadAnimationsFromXml(animationFileName);
        return atlas;
    }

    /// <summary>
    /// Creates a new texture atlas from a JSON texture file.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="textureFileName">The path to the texture json file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromJsonTexture(ContentManager content, string textureFileName)
    {
        TextureAtlas atlas = new TextureAtlas();

        string filePath = Path.Combine(content.RootDirectory, textureFileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            JsonDocument doc = JsonDocument.Parse(stream);
            JsonElement root = doc.RootElement;

            // Load the texture
            if (root.TryGetProperty("atlasFile", out JsonElement atlasFileElement))
            {
                // If atlasFile is specified, use it (remove file extension for content pipeline)
                string texturePath = atlasFileElement.GetString();
                // Remove file extension since ContentManager expects asset names without extensions
                texturePath = Path.ChangeExtension(texturePath, null);
                atlas.Texture = content.Load<Texture2D>(texturePath);
            }
            else
            {
                // Otherwise, assume the texture has the same name as the JSON file but without extension
                string texturePath = Path.ChangeExtension(textureFileName, null);
                atlas.Texture = content.Load<Texture2D>(texturePath);
            }

            // Load sprites
            if (root.TryGetProperty("sprites", out JsonElement spritesElement))
            {
                foreach (JsonElement spriteElement in spritesElement.EnumerateArray())
                {
                    string name = spriteElement.GetProperty("name").GetString();
                    int x = spriteElement.GetProperty("x").GetInt32();
                    int y = spriteElement.GetProperty("y").GetInt32();
                    int width = spriteElement.GetProperty("width").GetInt32();
                    int height = spriteElement.GetProperty("height").GetInt32();

                    if (!string.IsNullOrEmpty(name))
                    {
                        atlas.AddRegion(name, x, y, width, height);
                    }
                }
            }
        }

        return atlas;
    }

    /// <summary>
    /// Loads animations from a JSON file and adds them to this texture atlas.
    /// </summary>
    /// <param name="animationFileName">The path to the animation json file, relative to the content root directory.</param>
    public void LoadAnimationsFromJson(string animationFileName)
    {
        string filePath = Path.Combine(Core.Content.RootDirectory, animationFileName);

        using (Stream stream = TitleContainer.OpenStream(filePath))
        {
            JsonDocument doc = JsonDocument.Parse(stream);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("animations", out JsonElement animationsElement))
            {
                foreach (JsonElement animationElement in animationsElement.EnumerateArray())
                {
                    string name = animationElement.GetProperty("name").GetString();
                    
                    // Get default duration (in milliseconds)
                    int defaultDuration = 100; // Default fallback
                    if (animationElement.TryGetProperty("defaultDuration", out JsonElement defaultDurationElement))
                    {
                        defaultDuration = defaultDurationElement.GetInt32();
                    }

                    List<TextureRegion> frames = new List<TextureRegion>();

                    if (animationElement.TryGetProperty("frames", out JsonElement framesElement))
                    {
                        foreach (JsonElement frameElement in framesElement.EnumerateArray())
                        {
                            string spriteName = frameElement.GetProperty("sprite").GetString();
                            TextureRegion region = GetRegion(spriteName);
                            frames.Add(region);
                        }
                    }

                    TimeSpan delay = TimeSpan.FromMilliseconds(defaultDuration);
                    Animation animation = new Animation(frames, delay);
                    AddAnimation(name, animation);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new texture atlas from separate JSON files for texture and animations.
    /// </summary>
    /// <param name="content">The content manager used to load the texture for the atlas.</param>
    /// <param name="textureFileName">The path to the texture json file, relative to the content root directory.</param>
    /// <param name="animationFileName">The path to the animation json file, relative to the content root directory.</param>
    /// <returns>The texture atlas created by this method.</returns>
    public static TextureAtlas FromJson(ContentManager content, string textureFileName, string animationFileName)
    {
        TextureAtlas atlas = FromJsonTexture(content, textureFileName);
        atlas.LoadAnimationsFromJson(animationFileName);
        return atlas;
    }

    /// <summary>
    /// Creates a new sprite using the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="regionName">The name of the region to create the sprite with.</param>
    /// <returns>A new Sprite using the texture region with the specified name.</returns>
    public Sprite CreateSprite(string regionName)
    {
        TextureRegion region = GetRegion(regionName);
        return new Sprite(region);
    }

    /// <summary>
    /// Creates a new animated sprite using the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to use.</param>
    /// <returns>A new AnimatedSprite using the animation with the specified name.</returns>
    public AnimatedSprite CreateAnimatedSprite(string animationName)
    {
        Animation animation = GetAnimation(animationName);
        return new AnimatedSprite(animation);
    }

    /// <summary>
    /// Creates a new region and adds it to this texture atlas.
    /// </summary>
    /// <param name="name">The name to give the texture region.</param>
    /// <param name="x">The top-left x-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="y">The top-left y-coordinate position of the region boundary relative to the top-left corner of the source texture boundary.</param>
    /// <param name="width">The width, in pixels, of the region.</param>
    /// <param name="height">The height, in pixels, of the region.</param>
    public void AddRegion(string name, int x, int y, int width, int height)
    {
        TextureRegion region = new TextureRegion(Texture, x, y, width, height);
        _regions.Add(name, region);
    }

    /// <summary>
    /// Gets the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to retrieve.</param>
    /// <returns>The TextureRegion with the specified name.</returns>
    public TextureRegion GetRegion(string name)
    {
        return _regions[name];
    }

    /// <summary>
    /// Removes the region from this texture atlas with the specified name.
    /// </summary>
    /// <param name="name">The name of the region to remove.</param>
    /// <returns></returns>
    public bool RemoveRegion(string name)
    {
        return _regions.Remove(name);
    }

    /// <summary>
    /// Removes all regions and animations from this texture atlas.
    /// </summary>
    public void Clear()
    {
        _regions.Clear();
        _animations.Clear();
    }

    /// <summary>
    /// Adds the given animation to this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to add.</param>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(string animationName, Animation animation)
    {
        _animations.Add(animationName, animation);
    }

    /// <summary>
    /// Gets the animation from this texture atlas with the specified name.
    /// </summary>
    /// <param name="animationName">The name of the animation to retrieve.</param>
    /// <returns>The animation with the specified name.</returns>
    public Animation GetAnimation(string animationName)
    {
        return _animations[animationName];
    }

    /// <summary>
    /// Removes the animation with the specified name from this texture atlas.
    /// </summary>
    /// <param name="animationName">The name of the animation to remove.</param>
    /// <returns>true if the animation is removed successfully; otherwise, false.</returns>
    public bool RemoveAnimation(string animationName)
    {
        return _animations.Remove(animationName);
    }
}
