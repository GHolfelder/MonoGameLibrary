# Scenes Namespace

The Scenes namespace provides a scene management system for organizing game states and handling resource lifecycle management.

## Core Classes

### Scene Management
- **[Scene](Scenes.cs)** - Abstract base class for game scenes with automatic resource management

## Key Features

### Automatic Resource Management
- **Scene-Specific ContentManager**: Each scene has its own `ContentManager` for isolated asset loading
- **Automatic Cleanup**: Assets are automatically unloaded when scenes end
- **Proper Disposal**: Implements `IDisposable` pattern for resource cleanup
- **Memory Management**: Prevents resource leaks between scene transitions

### Scene Lifecycle
1. **Construction**: Scene is created, `ContentManager` is initialized
2. **Initialize**: `Initialize()` is called, which calls `LoadContent()`
3. **Update/Draw Loop**: Scene runs normally
4. **Disposal**: `Dispose()` is called, `UnloadContent()` cleans up resources

### Scene Transitions
- Managed by `Core.ChangeScene()` method
- Old scene is disposed before new scene is initialized
- Garbage collection is forced during transitions to ensure memory cleanup

## Usage Examples

### Creating a Custom Scene
```csharp
public class GameplayScene : Scene
{
    private Texture2D _backgroundTexture;
    private SpriteFont _font;
    private PlayerSprite _player;

    public override void LoadContent()
    {
        // Load scene-specific assets using the scene's ContentManager
        _backgroundTexture = Content.Load<Texture2D>("gameplay_background");
        _font = Content.Load<SpriteFont>("ui_font");
        
        // Load texture atlas for sprites
        var playerAtlas = TextureAtlas.FromXml(Content, "player_atlas.xml");
        _player = new PlayerSprite(playerAtlas, "player", DirectionMode.EightWay, 
                                  AnimationState.Idle, AnimationState.Walk);
    }

    public override void Update(GameTime gameTime)
    {
        _player.Update(gameTime);
        
        // Scene transition example
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Core.ChangeScene(new MainMenuScene());
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.SpriteBatch.Begin();
        
        // Draw background
        Core.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
        
        // Draw player
        _player.Draw(Core.SpriteBatch);
        
        Core.SpriteBatch.End();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Custom cleanup if needed
            // ContentManager cleanup is handled by base class
        }
        
        base.Dispose(disposing);
    }
}
```

### Scene Transitions
```csharp
// In your current scene's Update method
if (playerWins)
{
    Core.ChangeScene(new VictoryScene());
}

// Or from a menu
if (startButtonPressed)
{
    Core.ChangeScene(new GameplayScene());
}
```

### Menu Scene Example
```csharp
public class MainMenuScene : Scene
{
    private SpriteFont _titleFont;
    private SpriteFont _menuFont;
    private string[] _menuItems = { "Start Game", "Options", "Exit" };
    private int _selectedIndex = 0;

    public override void LoadContent()
    {
        _titleFont = Content.Load<SpriteFont>("title_font");
        _menuFont = Content.Load<SpriteFont>("menu_font");
    }

    public override void Update(GameTime gameTime)
    {
        // Menu navigation
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Up))
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
            
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Down))
            _selectedIndex = Math.Min(_menuItems.Length - 1, _selectedIndex + 1);

        // Menu selection
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        {
            switch (_selectedIndex)
            {
                case 0: // Start Game
                    Core.ChangeScene(new GameplayScene());
                    break;
                case 1: // Options
                    Core.ChangeScene(new OptionsScene());
                    break;
                case 2: // Exit
                    Core.Instance.Exit();
                    break;
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Black);
        
        Core.SpriteBatch.Begin();
        
        // Draw title
        var titlePos = new Vector2(400, 100);
        Core.SpriteBatch.DrawString(_titleFont, "My Game", titlePos, Color.White);
        
        // Draw menu items
        for (int i = 0; i < _menuItems.Length; i++)
        {
            var itemPos = new Vector2(400, 300 + i * 50);
            var color = i == _selectedIndex ? Color.Yellow : Color.White;
            Core.SpriteBatch.DrawString(_menuFont, _menuItems[i], itemPos, color);
        }
        
        Core.SpriteBatch.End();
    }
}
```

## Scene Management Best Practices

### Resource Loading
```csharp
// DO: Load assets in LoadContent using the scene's ContentManager
public override void LoadContent()
{
    _texture = Content.Load<Texture2D>("my_texture");
}

// DON'T: Use Core.Content for scene-specific assets
public override void LoadContent()
{
    _texture = Core.Content.Load<Texture2D>("my_texture"); // Will not be cleaned up!
}
```

### Scene Transitions
```csharp
// Scene transitions are safe and automatic
Core.ChangeScene(new NextScene());

// The current scene will be:
// 1. Disposed (calls UnloadContent and Content.Dispose)
// 2. Garbage collected
// 3. Replaced with the new scene
```

### Custom Cleanup
```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        // Clean up any custom resources
        _customResource?.Dispose();
        _eventSubscription?.Unsubscribe();
    }
    
    // Always call base to ensure proper cleanup
    base.Dispose(disposing);
}
```

## Architecture Notes

### Scene Lifecycle Management
- Each scene gets its own `ContentManager` instance
- Content is automatically unloaded when scene is disposed
- `GC.Collect()` is called during scene transitions
- Prevents memory leaks between scene changes

### Integration with Core
- Scenes are managed by the `Core` class
- Access to Core systems via static properties (`Core.SpriteBatch`, `Core.Input`, etc.)
- Scene transitions are handled automatically by Core's update loop

### Memory Management
- Scene-specific assets are isolated to prevent leaks
- Automatic disposal ensures proper cleanup
- Manual `GC.Collect()` during transitions ensures immediate memory reclamation

The Scene system provides a clean, organized way to structure your game with automatic resource management and smooth transitions between different game states.