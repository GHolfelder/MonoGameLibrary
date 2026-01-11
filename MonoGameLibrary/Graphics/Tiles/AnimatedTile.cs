using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        _animatedTile = animatedTile ?? throw new ArgumentNullException(nameof(animatedTile));
        _atlasRegion = atlasRegion ?? throw new ArgumentNullException(nameof(atlasRegion));
        _currentFrame = 0;
        _elapsed = TimeSpan.Zero;
    }

    public void Update(GameTime gameTime)
    {
        if (_animatedTile.Animation.Count < 2) return;
        
        _elapsed += gameTime.ElapsedGameTime;
        var currentFrameData = _animatedTile.Animation[_currentFrame];
        var frameDuration = TimeSpan.FromMilliseconds(currentFrameData.Duration);
        
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