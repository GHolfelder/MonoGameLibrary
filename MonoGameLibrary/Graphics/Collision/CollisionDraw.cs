using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGameLibrary.Graphics.Collision;

/// <summary>
/// Static helper class for drawing primitive shapes for collision visualization.
/// </summary>
public static class CollisionDraw
{
    private static Texture2D s_pixelTexture;

    /// <summary>
    /// Initializes the debug drawing system with a 1x1 white pixel texture.
    /// This should be called during game initialization.
    /// </summary>
    /// <param name="graphicsDevice">The GraphicsDevice to create the pixel texture with.</param>
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        if (s_pixelTexture == null)
        {
            s_pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            s_pixelTexture.SetData(new[] { Color.White });
        }
    }

    /// <summary>
    /// Draws a rectangle outline.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="rectangle">The rectangle to draw.</param>
    /// <param name="color">The color to draw with.</param>
    /// <param name="thickness">The line thickness (default: 1).</param>
    public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness = 1)
    {
        EnsurePixelTexture(spriteBatch.GraphicsDevice);

        // Top line
        spriteBatch.Draw(s_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        
        // Bottom line
        spriteBatch.Draw(s_pixelTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - thickness, rectangle.Width, thickness), color);
        
        // Left line
        spriteBatch.Draw(s_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        
        // Right line
        spriteBatch.Draw(s_pixelTexture, new Rectangle(rectangle.X + rectangle.Width - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    /// <summary>
    /// Draws a circle outline using line segments.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="center">The center point of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="color">The color to draw with.</param>
    /// <param name="segments">The number of line segments to use (default: 32).</param>
    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments = 32)
    {
        EnsurePixelTexture(spriteBatch.GraphicsDevice);

        float angleStep = MathHelper.TwoPi / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;
            
            Vector2 point1 = center + new Vector2(
                (float)Math.Cos(angle1) * radius,
                (float)Math.Sin(angle1) * radius
            );
            
            Vector2 point2 = center + new Vector2(
                (float)Math.Cos(angle2) * radius,
                (float)Math.Sin(angle2) * radius
            );
            
            DrawLine(spriteBatch, point1, point2, color);
        }
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to use for drawing.</param>
    /// <param name="start">The start point of the line.</param>
    /// <param name="end">The end point of the line.</param>
    /// <param name="color">The color to draw with.</param>
    /// <param name="thickness">The line thickness (default: 1).</param>
    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        EnsurePixelTexture(spriteBatch.GraphicsDevice);

        Vector2 delta = end - start;
        float length = delta.Length();
        float angle = (float)Math.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(s_pixelTexture, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Ensures the pixel texture is created if it doesn't exist.
    /// </summary>
    private static void EnsurePixelTexture(GraphicsDevice graphicsDevice)
    {
        if (s_pixelTexture == null || s_pixelTexture.IsDisposed)
        {
            Initialize(graphicsDevice);
        }
    }
}