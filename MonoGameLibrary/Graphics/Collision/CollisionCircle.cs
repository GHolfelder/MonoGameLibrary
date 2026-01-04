using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGameLibrary.Graphics.Collision;

/// <summary>
/// A circular collision shape for collision detection.
/// </summary>
public class CollisionCircle : ICollisionShape
{
    /// <summary>
    /// Gets or sets the position offset of this collision shape relative to its parent sprite.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets or sets the radius of the collision circle.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Creates a new CollisionCircle with the specified radius.
    /// </summary>
    /// <param name="radius">The radius of the collision circle.</param>
    public CollisionCircle(float radius)
    {
        Radius = radius;
        Offset = Vector2.Zero;
    }

    /// <summary>
    /// Creates a new CollisionCircle with the specified radius and offset.
    /// </summary>
    /// <param name="radius">The radius of the collision circle.</param>
    /// <param name="offset">The position offset relative to the parent sprite.</param>
    public CollisionCircle(float radius, Vector2 offset)
    {
        Radius = radius;
        Offset = offset;
    }

    /// <summary>
    /// Checks if this collision circle intersects with another collision shape.
    /// </summary>
    /// <param name="worldPosition">The world position of this collision circle.</param>
    /// <param name="other">The other collision shape to test against.</param>
    /// <param name="otherWorldPosition">The world position of the other collision shape.</param>
    /// <returns>True if the shapes intersect, false otherwise.</returns>
    public bool Intersects(Vector2 worldPosition, ICollisionShape other, Vector2 otherWorldPosition)
    {
        if (other is CollisionCircle otherCircle)
        {
            return CircleCircleIntersection(this, worldPosition, otherCircle, otherWorldPosition);
        }
        else if (other is CollisionRectangle otherRect)
        {
            return CircleRectangleIntersection(this, worldPosition, otherRect.GetBounds(otherWorldPosition));
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
        var center = worldPosition + Offset;
        var diameter = (int)(Radius * 2);
        return new Rectangle((int)(center.X - Radius), (int)(center.Y - Radius), diameter, diameter);
    }

    /// <summary>
    /// Draws this collision circle for debugging purposes.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="worldPosition">The world position of this collision shape.</param>
    /// <param name="color">The color to draw the shape with.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 worldPosition, Color color)
    {
        var center = worldPosition + Offset;
        CollisionDraw.DrawCircle(spriteBatch, center, Radius, color);
    }

    /// <summary>
    /// Helper method to check if two circles intersect.
    /// </summary>
    private static bool CircleCircleIntersection(CollisionCircle circle1, Vector2 pos1, CollisionCircle circle2, Vector2 pos2)
    {
        var center1 = pos1 + circle1.Offset;
        var center2 = pos2 + circle2.Offset;
        var distance = Vector2.Distance(center1, center2);
        var combinedRadius = circle1.Radius + circle2.Radius;
        
        return distance <= combinedRadius;
    }

    /// <summary>
    /// Helper method to check if a circle intersects with a rectangle.
    /// </summary>
    private static bool CircleRectangleIntersection(CollisionCircle circle, Vector2 circleWorldPos, Rectangle rect)
    {
        var circleCenter = circleWorldPos + circle.Offset;

        // Find the closest point on the rectangle to the circle center
        var closestX = MathHelper.Clamp(circleCenter.X, rect.Left, rect.Right);
        var closestY = MathHelper.Clamp(circleCenter.Y, rect.Top, rect.Bottom);
        var closestPoint = new Vector2(closestX, closestY);

        // Calculate the distance between the circle center and the closest point
        var distance = Vector2.Distance(circleCenter, closestPoint);

        return distance <= circle.Radius;
    }

    /// <summary>
    /// Tests if this collision circle intersects with a rectangle.
    /// </summary>
    /// <param name="worldPosition">The world position of this circle.</param>
    /// <param name="rectangle">The rectangle to test against.</param>
    /// <returns>True if intersection occurs, false otherwise.</returns>
    public bool IntersectsRectangle(Vector2 worldPosition, Rectangle rectangle)
    {
        return CircleRectangleIntersection(this, worldPosition, rectangle);
    }

    /// <summary>
    /// Tests if this collision circle contains a specific point.
    /// </summary>
    /// <param name="worldPosition">The world position of this circle.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the circle contains the point, false otherwise.</returns>
    public bool ContainsPoint(Vector2 worldPosition, Vector2 point)
    {
        var center = worldPosition + Offset;
        var distance = Vector2.Distance(center, point);
        return distance <= Radius;
    }
}