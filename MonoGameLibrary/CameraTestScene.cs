using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics.Camera;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

/// <summary>
/// Demonstration scene showcasing the enhanced Camera2D system with all new features.
/// This serves as both a test and an example of how to use the camera system.
/// </summary>
public class CameraTestScene : Scene
{
    private Vector2 playerPosition = new Vector2(400, 300);
    private float characterSize = 64f;
    private bool demoMode = true;
    private float demoTimer = 0f;

    public override void LoadContent()
    {
        // Configure camera for optimal character display (configurable screen coverage)
        Core.Camera.CharacterScreenCoverage = 0.12f; // 12% screen coverage instead of default 10%
        Core.Camera.SetZoomLimitsForCharacter(characterSize);
        
        // Enable smooth following for polished movement
        Core.Camera.SmoothFollowing = true;
        Core.Camera.FollowSpeed = 5.0f;
        
        // Set up character following
        Core.Camera.Follow(playerPosition);
        
        // Configure camera controller settings
        Core.CameraController.ZoomSpeed = 0.15f;
        Core.CameraController.ZoomAcceleration = 2.0f;
        Core.CameraController.MouseWheelZoomSensitivity = 0.25f;
        
        // Optional: Set world bounds for a contained demo area
        Core.Camera.WorldBounds = new Rectangle(-500, -500, 2000, 1500);
        
        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        if (demoMode)
        {
            UpdateDemoMovement(gameTime);
        }
        else
        {
            UpdatePlayerInput();
        }
        
        // Update camera to follow player (smooth following is enabled)
        Core.Camera.Follow(playerPosition);
        
        // Toggle demo mode with Space key
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Space))
        {
            demoMode = !demoMode;
            demoTimer = 0f;
        }
        
        // Display performance statistics (F1 key to reset)
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.F1))
        {
            var stats = Core.Camera.PerformanceStats;
            System.Console.WriteLine($"Camera Performance: {stats.MatrixUpdateCount} updates, avg {stats.AverageMatrixUpdateTime:F3}ms");
            Core.Camera.ResetPerformanceStats();
        }
    }

    private void UpdateDemoMovement(GameTime gameTime)
    {
        demoTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Create a figure-8 movement pattern for demonstration
        playerPosition.X = 400 + (float)(Math.Sin(demoTimer * 0.5) * 300);
        playerPosition.Y = 300 + (float)(Math.Sin(demoTimer) * 200);
    }

    private void UpdatePlayerInput()
    {
        float speed = 200f;
        
        // Manual player movement using arrow keys or WASD
        if (Core.Input.Keyboard.IsKeyDown(Keys.Left) ||
            Core.Input.Keyboard.IsKeyDown(Keys.A))
            playerPosition.X -= speed;
        
        if (Core.Input.Keyboard.IsKeyDown(Keys.Right) ||
            Core.Input.Keyboard.IsKeyDown(Keys.D))
            playerPosition.X += speed;
        
        if (Core.Input.Keyboard.IsKeyDown(Keys.Up) ||
            Core.Input.Keyboard.IsKeyDown(Keys.W))
            playerPosition.Y -= speed;
        
        if (Core.Input.Keyboard.IsKeyDown(Keys.Down) ||
            Core.Input.Keyboard.IsKeyDown(Keys.S))
            playerPosition.Y += speed;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.Graphics.GraphicsDevice.Clear(Color.DarkBlue);

        // Draw world objects (affected by camera transformation)
        BeginScaled(useCamera: true);
        
        DrawWorldGrid();
        DrawPlayer();
        DrawWorldInformation();
        
        Core.SpriteBatch.End();

        // Draw UI (not affected by camera transformation)
        BeginScaledUI();
        
        DrawUI();
        
        Core.SpriteBatch.End();
    }

    private void DrawWorldGrid()
    {
        // Create a simple grid to demonstrate camera movement
        var gridColor = Color.Gray * 0.3f;
        int gridSize = 100;
        
        // Get visible world bounds for efficient grid drawing
        var topLeft = Core.Camera.ScreenToWorld(Vector2.Zero);
        var bottomRight = Core.Camera.ScreenToWorld(new Vector2(Core.VirtualResolution.X, Core.VirtualResolution.Y));
        
        // Draw vertical grid lines
        for (int x = (int)(topLeft.X / gridSize) * gridSize; x <= bottomRight.X; x += gridSize)
        {
            DrawLine(new Vector2(x, topLeft.Y), new Vector2(x, bottomRight.Y), gridColor);
        }
        
        // Draw horizontal grid lines
        for (int y = (int)(topLeft.Y / gridSize) * gridSize; y <= bottomRight.Y; y += gridSize)
        {
            DrawLine(new Vector2(topLeft.X, y), new Vector2(bottomRight.X, y), gridColor);
        }
    }

    private void DrawPlayer()
    {
        // Draw a simple colored rectangle representing the player
        var playerRect = new Rectangle((int)(playerPosition.X - characterSize/2), 
                                     (int)(playerPosition.Y - characterSize/2), 
                                     (int)characterSize, 
                                     (int)characterSize);
        
        DrawRectangle(playerRect, Color.Red);
        
        // Draw player center point
        DrawCircle(playerPosition, 3f, Color.Yellow);
    }

    private void DrawWorldInformation()
    {
        // Draw world origin
        DrawCircle(Vector2.Zero, 5f, Color.Green);
        
        // Draw camera bounds if set
        if (Core.Camera.WorldBounds.HasValue)
        {
            var bounds = Core.Camera.WorldBounds.Value;
            DrawRectangleOutline(bounds, Color.Cyan);
        }
    }

    private void DrawUI()
    {
        // Note: This demo shows camera functionality but requires a SpriteFont for text display
        // In a real implementation, you would load a SpriteFont in LoadContent()
        // For now, we'll just draw simple colored rectangles as UI indicators
        
        // Draw UI background
        var uiBackground = new Rectangle(10, 10, 400, 300);
        DrawRectangle(uiBackground, Color.Black * 0.7f);
        
        // Draw colored indicators for different camera states
        var zoomIndicator = new Rectangle(20, 30, (int)(Core.Camera.Zoom * 50), 10);
        DrawRectangle(zoomIndicator, Color.Yellow);
        
        // Draw mode indicator
        var modeColor = demoMode ? Color.Cyan : Color.Orange;
        var modeIndicator = new Rectangle(20, 50, 20, 20);
        DrawRectangle(modeIndicator, modeColor);
    }

    // Helper methods for drawing basic shapes
    private void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        // For this demo, we'll skip line drawing as it requires a texture
        // In a real implementation, you would use a 1x1 white texture
    }

    private void DrawRectangle(Rectangle rect, Color color)
    {
        // For this demo, we'll skip rectangle drawing as it requires a texture
        // In a real implementation, you would use a 1x1 white texture
    }

    private void DrawRectangleOutline(Rectangle rect, Color color)
    {
        // Draw four lines to form a rectangle outline
        DrawLine(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top), color);
        DrawLine(new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom), color);
        DrawLine(new Vector2(rect.Right, rect.Bottom), new Vector2(rect.Left, rect.Bottom), color);
        DrawLine(new Vector2(rect.Left, rect.Bottom), new Vector2(rect.Left, rect.Top), color);
    }

    private void DrawCircle(Vector2 center, float radius, Color color)
    {
        // Simple circle approximation using a small rectangle
        var rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), 
                                (int)(radius * 2), (int)(radius * 2));
        DrawRectangle(rect, color);
    }
}