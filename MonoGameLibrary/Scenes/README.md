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

### Content Scaling Integration
- **BeginScaled()**: Protected method for automatic content scaling
- **Virtual Coordinates**: All drawing operations use virtual resolution coordinates
- **Automatic Scaling**: Handles letterboxing/pillarboxing automatically
- **Input Transformation**: Mouse coordinates automatically converted to virtual space

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
        Core.GraphicsDevice.Clear(Color.DarkBlue);
        
        // Use BeginScaled() for automatic content scaling
        BeginScaled();
        {
            // All coordinates are in virtual space (e.g., 1920x1080)
            Core.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            
            // Player draws at virtual coordinates
            _player.Draw(Core.SpriteBatch);
            
            // UI elements positioned in virtual space
            Core.SpriteBatch.DrawString(_font, "Score: 1000", new Vector2(50, 50), Color.White);
        }
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

### Content Scaling with BeginScaled()

The Scene base class provides the `BeginScaled()` protected method for automatic content scaling:

```csharp
public class GameScene : Scene
{
    protected override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Black);
        
        // BeginScaled() automatically applies content scaling matrix
        BeginScaled();
        {
            // All drawing operations here use virtual coordinates
            // Works consistently across all screen resolutions
            
            tilemap.Draw(Core.SpriteBatch, Vector2.Zero);
            player.Draw(Core.SpriteBatch, playerPosition);
            
            // UI elements positioned in virtual space
            Core.SpriteBatch.DrawString(font, "Health: 100", new Vector2(50, 50), Color.White);
        }
        Core.SpriteBatch.End();
    }
}
```

**BeginScaled() Features:**
- **Automatic Scaling**: Applies `Core.ScaleMatrix` transformation
- **Letterbox/Pillarbox**: Maintains aspect ratio with black bars when needed
- **Virtual Coordinates**: All positions use virtual resolution (default 1920x1080)
- **Parameter Support**: Accepts SpriteBatch parameters (sortMode, blendState, etc.)

**Alternative Manual Scaling:**
For classes not inheriting from Scene:
```csharp
// Manual scaling approach
Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
// Drawing code here
Core.SpriteBatch.End();
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
        base.Update(gameTime); // Important: handles developer mode hotkeys
        
        // Input uses virtual coordinates automatically
        var mousePos = Core.Input.Mouse.VirtualPosition;
        
        // Menu navigation with virtual coordinates
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Up))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
        }
        else if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Down))
        {
            _selectedIndex = Math.Min(_menuItems.Length - 1, _selectedIndex + 1);
        }
        else if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter) ||
                Core.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            // Check if mouse is over selected menu item (virtual coordinates)
            var menuItemRect = GetMenuItemRect(_selectedIndex); // Returns virtual coordinates
            if (menuItemRect.Contains(mousePos) || Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                SelectMenuItem(_selectedIndex);
            }
        }
    }
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
        Core.GraphicsDevice.Clear(Color.Navy);
        
        BeginScaled(); // Use virtual coordinates
        {
            // Title positioned in virtual space
            var titlePos = new Vector2(Core.VirtualResolution.X / 2, 200);
            var titleOrigin = _titleFont.MeasureString("My Game") / 2;
            Core.SpriteBatch.DrawString(_titleFont, "My Game", titlePos, Color.White, 0f, titleOrigin, 1f, SpriteEffects.None, 0f);
            
            // Menu items in virtual coordinates
            for (int i = 0; i < _menuItems.Length; i++)
            {
                var menuPos = new Vector2(Core.VirtualResolution.X / 2, 400 + i * 60);
                var color = i == _selectedIndex ? Color.Yellow : Color.White;
                var origin = _menuFont.MeasureString(_menuItems[i]) / 2;
                
                Core.SpriteBatch.DrawString(_menuFont, _menuItems[i], menuPos, color, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
        }
        Core.SpriteBatch.End();
    }
    
    private Rectangle GetMenuItemRect(int index)
    {
        // Returns rectangle in virtual coordinates
        var menuPos = new Vector2(Core.VirtualResolution.X / 2, 400 + index * 60);
        var textSize = _menuFont.MeasureString(_menuItems[index]);
        return new Rectangle((int)(menuPos.X - textSize.X / 2), (int)(menuPos.Y - textSize.Y / 2), (int)textSize.X, (int)textSize.Y);
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