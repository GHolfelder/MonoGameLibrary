using System;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Interface for resolving and loading animations
/// </summary>
public interface IAnimationResolver
{
    /// <summary>
    /// Attempts to create an animated sprite for the given animation name
    /// </summary>
    /// <param name="animationName">The name of the animation to load</param>
    /// <returns>An AnimatedSprite if found, null otherwise</returns>
    AnimatedSprite TryCreateAnimatedSprite(string animationName);
    
    /// <summary>
    /// Attempts to create a static sprite for placeholders
    /// </summary>
    /// <param name="spriteName">The name of the sprite to load</param>
    /// <returns>A Sprite if found, null otherwise</returns>
    Sprite TryCreateSprite(string spriteName);
}

/// <summary>
/// Default implementation that uses a TextureAtlas for animation resolution
/// </summary>
public class TextureAtlasAnimationResolver : IAnimationResolver
{
    private readonly TextureAtlas _atlas;

    public TextureAtlasAnimationResolver(TextureAtlas atlas)
    {
        _atlas = atlas ?? throw new ArgumentNullException(nameof(atlas));
    }

    public AnimatedSprite TryCreateAnimatedSprite(string animationName)
    {
        try
        {
            return _atlas.CreateAnimatedSprite(animationName);
        }
        catch
        {
            return null;
        }
    }

    public Sprite TryCreateSprite(string spriteName)
    {
        try
        {
            return _atlas.CreateSprite(spriteName);
        }
        catch
        {
            return null;
        }
    }
}
