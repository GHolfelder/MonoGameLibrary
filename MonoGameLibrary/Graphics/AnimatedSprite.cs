using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// This class represents an animated sprite. Since an animated sprite is essentially 
/// a sprite that changes its texture region over time, this class is derived from the Sprite class.
/// </summary>
public class AnimatedSprite : Sprite
{
    /// <summary>
    /// Current frame of the animation being displayed.
    /// </summary>
    private int _currentFrame;
    /// <summary>
    /// Keeps track of how much time has passed since the last frame change.
    /// </summary>
    private TimeSpan _elapsed;
    /// <summary>
    /// Stores the current animation being played.
    /// </summary>
    private Animation _animation;

    /// <summary>
    /// Gets or Sets the animation for this animated sprite.
    /// </summary>
    /// <remarks>
    /// Note: Sets the source texture region to the first frame of the animation.
    /// </remarks>
    public Animation Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Region = _animation.Frames[0];
        }
    }

    /// <summary>
    /// Creates a new animated sprite.
    /// </summary>
    public AnimatedSprite() { }

    /// <summary>
    /// Creates a new animated sprite with the specified frames and delay.
    /// </summary>
    /// <param name="animation">The animation for this animated sprite.</param>
    public AnimatedSprite(Animation animation)
    {
        Animation = animation;
    }

    /// <summary>
    /// Update this animated sprite.
    /// </summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public void Update(GameTime gameTime)
    {
        _elapsed += gameTime.ElapsedGameTime;
        // Check if it's time to move to the next frame
        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame++;
            // Loop back to the beginning if we exceed the number of frames
            if (_currentFrame >= _animation.Frames.Count)
            {
                _currentFrame = 0;
            }
            // Update the texture region to the current frame
            Region = _animation.Frames[_currentFrame];
        }
    }

}
