using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics.Collision;

/// <summary>
/// A collision component that can be attached to sprites for collision detection.
/// </summary>
public class SpriteCollision
{
    /// <summary>
    /// Gets or sets the collision shape used for this sprite collision.
    /// </summary>
    public ICollisionShape Shape { get; set; }

    /// <summary>
    /// Gets or sets whether the collision shape should be drawn for debugging purposes.
    /// </summary>
    public bool EnableDraw { get; set; } = false;

    /// <summary>
    /// Gets or sets the color used when drawing the collision shape for debugging.
    /// </summary>
    public Color DrawColor { get; set; } = Color.Red;

    /// <summary>
    /// Creates a new SpriteCollision component with the specified collision shape.
    /// </summary>
    /// <param name="shape">The collision shape to use for this component.</param>
    public SpriteCollision(ICollisionShape shape)
    {
        Shape = shape;
    }

    /// <summary>
    /// Creates a new SpriteCollision component with the specified collision shape and draw settings.
    /// </summary>
    /// <param name="shape">The collision shape to use for this component.</param>
    /// <param name="enableDraw">Whether to draw the collision shape for debugging.</param>
    /// <param name="drawColor">The color to use when drawing the collision shape.</param>
    public SpriteCollision(ICollisionShape shape, bool enableDraw, Color drawColor)
    {
        Shape = shape;
        EnableDraw = enableDraw;
        DrawColor = drawColor;
    }

    /// <summary>
    /// Checks if this sprite collision intersects with another sprite collision.
    /// </summary>
    /// <param name="worldPosition">The world position of this sprite.</param>
    /// <param name="other">The other sprite collision to test against.</param>
    /// <param name="otherWorldPosition">The world position of the other sprite.</param>
    /// <returns>True if the collision shapes intersect, false otherwise.</returns>
    public bool Intersects(Vector2 worldPosition, SpriteCollision other, Vector2 otherWorldPosition)
    {
        return Shape.Intersects(worldPosition, other.Shape, otherWorldPosition);
    }

    /// <summary>
    /// Gets the bounding rectangle of this collision component in world coordinates.
    /// </summary>
    /// <param name="worldPosition">The world position of the parent sprite.</param>
    /// <returns>A Rectangle representing the bounds of this collision component.</returns>
    public Rectangle GetBounds(Vector2 worldPosition)
    {
        return Shape.GetBounds(worldPosition);
    }

    /// <summary>
    /// Draws the collision shape if EnableDraw is true.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="worldPosition">The world position of the parent sprite.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 worldPosition)
    {
        if (EnableDraw)
        {
            Shape.Draw(spriteBatch, worldPosition, DrawColor);
        }
    }

    /// <summary>
    /// Checks if this sprite collision intersects with a rectangular tile bounds.
    /// </summary>
    /// <param name="worldPosition">The world position of this sprite.</param>
    /// <param name="tileBounds">The bounds of the tile to test against.</param>
    /// <returns>True if this collision shape intersects with the tile bounds, false otherwise.</returns>
    public bool IntersectsRectangle(Vector2 worldPosition, Rectangle tileBounds)
    {
        return Shape.IntersectsRectangle(worldPosition, tileBounds);
    }

    /// <summary>
    /// Checks if this sprite collision contains a specific point.
    /// </summary>
    /// <param name="worldPosition">The world position of this sprite.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the collision shape contains the point, false otherwise.</returns>
    public bool ContainsPoint(Vector2 worldPosition, Vector2 point)
    {
        return Shape.ContainsPoint(worldPosition, point);
    }
}