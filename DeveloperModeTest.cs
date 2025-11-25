using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary.Tests
{
    /// <summary>
    /// Simple test scene to verify developer mode functionality
    /// </summary>
    public class DeveloperModeTestScene : Scene
    {
        private SpriteFont _font;

        public override void LoadContent()
        {
            // If you have a font file, load it here
            // _font = Content.Load<SpriteFont>("DefaultFont");
        }

        public override void Update(GameTime gameTime)
        {
            // Call base to handle F1/F2 keys
            base.Update(gameTime);
            
            // Exit on Escape
            if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            {
                Core.Instance.Exit();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            Core.SpriteBatch.Begin();
            
            // Draw some test text if font is available
            if (_font != null)
            {
                Core.SpriteBatch.DrawString(_font, 
                    $"Developer Mode: {Core.DeveloperMode}\nCollision Boxes: {Core.ShowCollisionBoxes}\nPress F1/F2 to toggle", 
                    new Vector2(10, 10), Color.White);
            }
            
            Core.SpriteBatch.End();
            
            // Call base to automatically draw developer overlay
            base.Draw(gameTime);
        }
    }

    /// <summary>
    /// Simple test program to verify developer mode
    /// </summary>
    public class DeveloperModeTestGame : Core
    {
        public DeveloperModeTestGame() : base("Developer Mode Test", 800, 600, false)
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            
            // Start with the test scene
            ChangeScene(new DeveloperModeTestScene());
        }
    }

    public class Program
    {
        public static void Main()
        {
            using var game = new DeveloperModeTestGame();
            game.Run();
        }
    }
}