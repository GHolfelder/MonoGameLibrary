using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary.Samples;

/// <summary>
/// Example scene demonstrating the collision system usage.
/// Shows sprite-to-sprite collision detection with debug visualization.
/// </summary>
public class CollisionExample : Scene
{
    private Sprite _player;
    private Sprite _enemy;
    private Vector2 _playerPosition;
    private Vector2 _enemyPosition;
    private bool _showCollision = false;
    private bool _collisionDetected = false;

    public override void LoadContent()
    {
        base.LoadContent();

        // Create simple colored rectangles as texture regions for the example
        var playerTexture = CreateColoredTexture(Color.Blue, 32, 32);
        var enemyTexture = CreateColoredTexture(Color.Red, 24, 24);

        // Create sprites
        _player = new Sprite(new TextureRegion(playerTexture, 0, 0, 32, 32));
        _enemy = new Sprite(new TextureRegion(enemyTexture, 0, 0, 24, 24));

        // Set up collision detection
        _player.EnableCollision(32, 32, Vector2.Zero, enableDraw: true, Color.Green);
        _enemy.EnableCollision(20f, Vector2.Zero, enableDraw: true, Color.Orange); // Circular collision

        // Set initial positions
        _playerPosition = new Vector2(100, 100);
        _enemyPosition = new Vector2(200, 150);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Toggle collision visualization with F1
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.F1))
        {
            _showCollision = !_showCollision;
        }

        // Move player with arrow keys
        var speed = 200f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (Core.Input.Keyboard.IsKeyDown(Keys.Left))
            _playerPosition.X -= speed;
        if (Core.Input.Keyboard.IsKeyDown(Keys.Right))
            _playerPosition.X += speed;
        if (Core.Input.Keyboard.IsKeyDown(Keys.Up))
            _playerPosition.Y -= speed;
        if (Core.Input.Keyboard.IsKeyDown(Keys.Down))
            _playerPosition.Y += speed;

        // Move enemy in a circle
        var time = (float)gameTime.TotalGameTime.TotalSeconds;
        _enemyPosition = new Vector2(
            300 + (float)System.Math.Cos(time) * 100,
            200 + (float)System.Math.Sin(time) * 50
        );

        // Check for collision
        _collisionDetected = _player.CheckCollision(_playerPosition, _enemy, _enemyPosition);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        spriteBatch.Begin();

        // Draw sprites with collision visualization
        _player.Draw(spriteBatch, _playerPosition, _showCollision);
        _enemy.Draw(spriteBatch, _enemyPosition, _showCollision);

        // Draw collision status text
        var statusColor = _collisionDetected ? Color.Red : Color.White;
        var statusText = _collisionDetected ? "COLLISION DETECTED!" : "No collision";
        
        // Note: This assumes you have a font loaded in your scene
        // spriteBatch.DrawString(font, statusText, new Vector2(10, 10), statusColor);
        
        // Draw instructions
        var instructions = _showCollision ? "F1: Hide collision shapes" : "F1: Show collision shapes";
        // spriteBatch.DrawString(font, instructions, new Vector2(10, 40), Color.Yellow);
        // spriteBatch.DrawString(font, "Arrow keys: Move player", new Vector2(10, 70), Color.Yellow);

        spriteBatch.End();
    }

    /// <summary>
    /// Helper method to create a solid colored texture for the example.
    /// </summary>
    private Texture2D CreateColoredTexture(Color color, int width, int height)
    {
        var texture = new Texture2D(Core.GraphicsDevice, width, height);
        var colorData = new Color[width * height];
        for (int i = 0; i < colorData.Length; i++)
        {
            colorData[i] = color;
        }
        texture.SetData(colorData);
        return texture;
    }
}