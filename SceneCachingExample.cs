using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

/// <summary>
/// Example game scene that maintains state between transitions
/// </summary>
public class GameScene : Scene
{
    private Vector2 _playerPosition;
    private int _score;
    private float _timeElapsed;

    public override void LoadContent()
    {
        // Initial values - only set on first load
        _playerPosition = new Vector2(100, 100);
        _score = 0;
        _timeElapsed = 0;
    }

    public override void OnPause()
    {
        // Scene is being paused (transitioning away)
        // State is automatically preserved since scene isn't disposed
        System.Diagnostics.Debug.WriteLine($"GameScene paused - Player at {_playerPosition}, Score: {_score}");
    }

    public override void OnResume()
    {
        // Scene is resuming (transitioning back)
        // All state is preserved
        System.Diagnostics.Debug.WriteLine($"GameScene resumed - Player at {_playerPosition}, Score: {_score}");
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _timeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Simple player movement
        var keyboardState = Keyboard.GetState();
        
        if (keyboardState.IsKeyDown(Keys.W))
            _playerPosition.Y -= 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboardState.IsKeyDown(Keys.S))
            _playerPosition.Y += 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboardState.IsKeyDown(Keys.A))
            _playerPosition.X -= 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (keyboardState.IsKeyDown(Keys.D))
            _playerPosition.X += 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Increase score over time
        _score = (int)_timeElapsed;

        // Transition to settings scene (cached)
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Core.TransitionTo<SettingsScene>();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        BeginScaled();
        
        // Draw simple player representation and UI
        // (This would typically draw sprites, but this is just an example)
        
        Core.SpriteBatch.End();
        
        base.Draw(gameTime);
    }
}

/// <summary>
/// Example settings scene
/// </summary>
public class SettingsScene : Scene
{
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Return to game scene (cached)
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Core.TransitionTo<GameScene>();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        BeginScaled();
        
        // Draw settings UI
        // (This would typically draw UI elements)
        
        Core.SpriteBatch.End();
        
        base.Draw(gameTime);
    }
}