using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics.Collision;

/// <summary>
/// A rectangular collision shape for collision detection.
/// </summary>
public class CollisionRectangle : ICollisionShape
{
    /// <summary>
    /// Gets or sets the position offset of this collision shape relative to its parent sprite.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets or sets the width of the collision rectangle.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the collision rectangle.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Creates a new CollisionRectangle with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the collision rectangle.</param>
    /// <param name="height">The height of the collision rectangle.</param>
    public CollisionRectangle(int width, int height)
    {
        Width = width;
        Height = height;
        Offset = Vector2.Zero;
    }

    /// <summary>
    /// Creates a new CollisionRectangle with the specified dimensions and offset.
    /// </summary>
    /// <param name="width">The width of the collision rectangle.</param>
    /// <param name="height">The height of the collision rectangle.</param>
    /// <param name="offset">The position offset relative to the parent sprite.</param>
    public CollisionRectangle(int width, int height, Vector2 offset)
    {
        Width = width;
        Height = height;
        Offset = offset;
    }

    /// <summary>
    /// Checks if this collision rectangle intersects with another collision shape.
    /// </summary>
    /// <param name="worldPosition">The world position of this collision rectangle.</param>
    /// <param name="other">The other collision shape to test against.</param>
    /// <param name="otherWorldPosition">The world position of the other collision shape.</param>
    /// <returns>True if the shapes intersect, false otherwise.</returns>
    public bool Intersects(Vector2 worldPosition, ICollisionShape other, Vector2 otherWorldPosition)
    {
        var thisRect = GetBounds(worldPosition);

        if (other is CollisionRectangle otherRect)
        {
            var otherBounds = otherRect.GetBounds(otherWorldPosition);
            return thisRect.Intersects(otherBounds);
        }
        else if (other is CollisionCircle otherCircle)
        {
            return CircleRectangleIntersection(otherCircle, otherWorldPosition, thisRect);
        }

        return false;
    }

    /// <summary>
    /// Gets the bounding rectangle of this collision shape in world coordinates.
    /// </summary>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <returns>A Rectangle representing the bounds of this collision shape.</returns>
    public Rectangle GetBounds(Vector2 worldPosition)
    {
        var position = worldPosition + Offset;
        return new Rectangle((int)position.X, (int)position.Y, Width, Height);
    }

    /// <summary>
    /// Draws this collision rectangle for debugging purposes.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <param name="color">The color to draw the shape with.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 worldPosition, Color color)
    {
        var bounds = GetBounds(worldPosition);
        CollisionDraw.DrawRectangle(spriteBatch, bounds, color);
    }

    /// <summary>
    /// Tests if this collision rectangle intersects with another rectangle.
    /// </summary>
    /// <param name="worldPosition">The world position of this rectangle.</param>
    /// <param name="rectangle">The rectangle to test against.</param>
    /// <returns>True if intersection occurs, false otherwise.</returns>
    public bool IntersectsRectangle(Vector2 worldPosition, Rectangle rectangle)
    {
        var bounds = GetBounds(worldPosition);
        return bounds.Intersects(rectangle);
    }

    /// <summary>
    /// Tests if this collision rectangle contains a specific point.
    /// </summary>
    /// <param name="worldPosition">The world position of this rectangle.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the rectangle contains the point, false otherwise.</returns>
    public bool ContainsPoint(Vector2 worldPosition, Vector2 point)
    {
        var bounds = GetBounds(worldPosition);
        return bounds.Contains((int)point.X, (int)point.Y);
    }

    /// <summary>
    /// Helper method to check if a circle intersects with a rectangle.
    /// </summary>
    private static bool CircleRectangleIntersection(CollisionCircle circle, Vector2 circleWorldPos, Rectangle rect)
    {
        var circleBounds = circle.GetBounds(circleWorldPos);
        var circleCenter = new Vector2(circleBounds.Center.X, circleBounds.Center.Y);
        var radius = circle.Radius;

        // Find the closest point on the rectangle to the circle center
        var closestX = MathHelper.Clamp(circleCenter.X, rect.Left, rect.Right);
        var closestY = MathHelper.Clamp(circleCenter.Y, rect.Top, rect.Bottom);
        var closestPoint = new Vector2(closestX, closestY);

        // Calculate the distance between the circle center and the closest point
        var distance = Vector2.Distance(circleCenter, closestPoint);

        return distance <= radius;
    }
}