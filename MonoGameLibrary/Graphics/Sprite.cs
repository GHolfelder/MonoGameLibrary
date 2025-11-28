using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics.Collision;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// This class is the visual object created from a texture region, along with its rendering properties.
/// While multiple sprites might use the same texture region (like multiple enemies of the same type), 
/// each sprite can have unique properties that control how it appears on screen; its position, rotation, 
/// scale, and other visual characteristics.
/// </summary>
public class Sprite
{
    /// <summary>
    /// Gets or Sets the source texture region represented by this sprite.
    /// </summary>
    public TextureRegion Region { get; set; }

    /// <summary>
    /// Gets or Sets the color mask to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Color.White
    /// </remarks>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Gets or Sets the amount of rotation, in radians, to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float Rotation { get; set; } = 0.0f;

    /// <summary>
    /// Gets or Sets the scale factor to apply to the x- and y-axes when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.One
    /// </remarks>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or Sets the xy-coordinate origin point, relative to the top-left corner, of this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.Zero
    /// </remarks>
    public Vector2 Origin { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or Sets the sprite effects to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is SpriteEffects.None
    /// </remarks>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;

    /// <summary>
    /// Gets or Sets the layer depth to apply when rendering this sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float LayerDepth { get; set; } = 0.0f;

    /// <summary>
    /// Gets or Sets the collision component for this sprite.
    /// Set to null to disable collision detection.
    /// </summary>
    public SpriteCollision Collision { get; set; }

    /// <summary>
    /// Gets the width, in pixels, of this sprite. 
    /// </summary>
    /// <remarks>
    /// Width is calculated by multiplying the width of the source texture region by the x-axis scale factor.
    /// </remarks>
    public float Width => Region.Width * Scale.X;

    /// <summary>
    /// Gets the height, in pixels, of this sprite.
    /// </summary>
    /// <remarks>
    /// Height is calculated by multiplying the height of the source texture region by the y-axis scale factor.
    /// </remarks>
    public float Height => Region.Height * Scale.Y;

    /// <summary>
    /// Creates a new sprite.
    /// </summary>
    public Sprite() { }

    /// <summary>
    /// Creates a new sprite using the specified source texture region.
    /// </summary>
    /// <param name="region">The texture region to use as the source texture region for this sprite.</param>
    public Sprite(TextureRegion region)
    {
        Region = region;
    }

    /// <summary>
    /// Sets the origin of this sprite to the center.
    /// </summary>
    /// <remarks>
    /// Note: The origin needs to be set based on the width and height of the source texture region itself, 
    /// regardless of the scale the sprite is rendered at.
    /// </remarks>
    public void CenterOrigin()
    {
        Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
    }

    /// <summary>
    /// Enables collision detection for this sprite using a rectangular collision shape.
    /// </summary>
    /// <param name="width">The width of the collision rectangle.</param>
    /// <param name="height">The height of the collision rectangle.</param>
    /// <param name="offset">Optional offset from the sprite's position (default: Vector2.Zero).</param>
    /// <param name="enableDraw">Whether to draw the collision shape for debugging (default: false).</param>
    /// <param name="drawColor">The color to use when drawing the collision shape (default: Color.Red).</param>
    public void EnableCollision(int width, int height, Vector2 offset = default, bool enableDraw = false, Color drawColor = default)
    {
        var color = drawColor == default ? Color.Red : drawColor;
        var shape = new CollisionRectangle(width, height, offset);
        Collision = new SpriteCollision(shape, enableDraw, color);
    }

    /// <summary>
    /// Enables collision detection for this sprite using a circular collision shape.
    /// </summary>
    /// <param name="radius">The radius of the collision circle.</param>
    /// <param name="offset">Optional offset from the sprite's position (default: Vector2.Zero).</param>
    /// <param name="enableDraw">Whether to draw the collision shape for debugging (default: false).</param>
    /// <param name="drawColor">The color to use when drawing the collision shape (default: Color.Red).</param>
    public void EnableCollision(float radius, Vector2 offset = default, bool enableDraw = false, Color drawColor = default)
    {
        var color = drawColor == default ? Color.Red : drawColor;
        var shape = new CollisionCircle(radius, offset);
        Collision = new SpriteCollision(shape, enableDraw, color);
    }

    /// <summary>
    /// Checks if this sprite's collision shape intersects with another sprite's collision shape.
    /// </summary>
    /// <param name="position">The position of this sprite.</param>
    /// <param name="other">The other sprite to check collision with.</param>
    /// <param name="otherPosition">The position of the other sprite.</param>
    /// <returns>True if the sprites collide, false otherwise or if either sprite has no collision component.</returns>
    public bool CheckCollision(Vector2 position, Sprite other, Vector2 otherPosition)
    {
        if (Collision == null || other.Collision == null)
            return false;

        return Collision.Intersects(position, other.Collision, otherPosition);
    }

    /// <summary>
    /// Submit this sprite for drawing to the current batch.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch instance used for batching draw calls.</param>
    /// <param name="position">The xy-coordinate position to render this sprite at.</param>
    /// <param name="showCollision">Whether to draw the collision shape for debugging (default: false).</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, bool showCollision = false)
    {
        Region.Draw(spriteBatch, position, Color, Rotation, Origin, Scale, Effects, LayerDepth);
        
        // Show collision if requested OR if global developer mode is active
        if ((showCollision || Core.ShowCollisionBoxes) && Collision != null)
        {
            // Call the collision shape directly to bypass EnableDraw check when in developer mode
            if (Core.ShowCollisionBoxes)
            {
                Collision.Shape.Draw(spriteBatch, position, Collision.DrawColor);
            }
            else
            {
                Collision.Draw(spriteBatch, position);
            }
        }
    }

}
