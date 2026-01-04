using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics.Collision;

/// <summary>
/// Interface for collision shapes that can be used for collision detection.
/// </summary>
public interface ICollisionShape
{
    /// <summary>
    /// Gets the position offset of this collision shape relative to its parent sprite.
    /// </summary>
    Vector2 Offset { get; set; }

    /// <summary>
    /// Checks if this collision shape intersects with another collision shape.
    /// </summary>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <param name="other">The other collision shape to test against.</param>
    /// <param name="otherWorldPosition">The world position of the other collision shape.</param>
    /// <returns>True if the shapes intersect, false otherwise.</returns>
    bool Intersects(Vector2 worldPosition, ICollisionShape other, Vector2 otherWorldPosition);

    /// <summary>
    /// Gets the bounding rectangle of this collision shape in world coordinates.
    /// </summary>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <returns>A Rectangle representing the bounds of this collision shape.</returns>
    Rectangle GetBounds(Vector2 worldPosition);

    /// <summary>
    /// Draws this collision shape for debugging purposes.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <param name="color">The color to draw the shape with.</param>
    void Draw(SpriteBatch spriteBatch, Vector2 worldPosition, Color color);
}