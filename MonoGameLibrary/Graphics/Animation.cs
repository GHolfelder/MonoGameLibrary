using System;
using System.Collections.Generic;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// This class is used to manage the animation data for a sprite.
/// </summary>
public class Animation
{
    /// <summary>
    /// The texture regions that make up the frames of this animation.  The order of the regions within the collection
    /// are the order that the frames should be displayed in.
    /// </summary>
    public List<TextureRegion> Frames { get; set; }

    /// <summary>
    /// The amount of time to delay for each frame before moving to the next frame.
    /// If this list is empty or has fewer entries than Frames, the Delay property is used as a fallback.
    /// </summary>
    public List<TimeSpan> FrameDelays { get; set; }

    /// <summary>
    /// The default amount of time to delay between each frame before moving to the next frame for this animation.
    /// This is used as a fallback when FrameDelays is not specified for a particular frame.
    /// A shorter delay creates faster animations, while a longer delay creates slower ones.
    /// </summary>
    public TimeSpan Delay { get; set; }

    /// <summary>
    /// Creates a new animation. This creates an animation with an empty collection 
    /// of frames and a default delay of 100 milliseconds between each frame.
    /// </summary>
    public Animation()
    {
        Frames = new List<TextureRegion>();
        FrameDelays = new List<TimeSpan>();
        Delay = TimeSpan.FromMilliseconds(100);
    }

    /// <summary>
    /// Creates a new animation with the specified frames and delay.
    /// All frames will use the same delay value.
    /// </summary>
    /// <param name="frames">An ordered collection of the frames for this animation.</param>
    /// <param name="delay">The amount of time to delay between each frame of this animation.</param>
    public Animation(List<TextureRegion> frames, TimeSpan delay)
    {
        Frames = frames;
        FrameDelays = new List<TimeSpan>();
        Delay = delay;
    }

    /// <summary>
    /// Creates a new animation with the specified frames and per-frame delays.
    /// </summary>
    /// <param name="frames">An ordered collection of the frames for this animation.</param>
    /// <param name="frameDelays">An ordered collection of delays for each frame.</param>
    /// <param name="defaultDelay">The default delay to use when a frame doesn't have a specific delay.</param>
    public Animation(List<TextureRegion> frames, List<TimeSpan> frameDelays, TimeSpan defaultDelay)
    {
        Frames = frames;
        FrameDelays = frameDelays;
        Delay = defaultDelay;
    }

    /// <summary>
    /// Gets the delay for the specified frame index.
    /// Returns the per-frame delay if available, otherwise returns the default Delay.
    /// </summary>
    /// <param name="frameIndex">The index of the frame.</param>
    /// <returns>The delay for the specified frame.</returns>
    public TimeSpan GetFrameDelay(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < FrameDelays.Count)
        {
            return FrameDelays[frameIndex];
        }
        return Delay;
    }

}
