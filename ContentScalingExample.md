# Content Scaling System - Resolution Independence Guide

The MonoGame Library provides a comprehensive content scaling system that allows your game to look consistent across different screen resolutions and monitor sizes. This system automatically handles letterboxing, pillarboxing, and input coordinate transformation.

## Overview

The content scaling system consists of:
- **Virtual Resolution**: A fixed logical resolution that your game is designed for (default: 1920x1080)
- **Content Scaling**: Automatic uniform scaling with aspect ratio preservation
- **Input Transformation**: Mouse coordinates automatically converted to virtual space
- **Matrix Transformation**: Automatic letterboxing/pillarboxing for different screen sizes

## Scaling Methods Overview

### BeginScaled() - Scene Class Method

`BeginScaled()` is a **protected method** available only in classes that inherit from the `Scene` base class:

```csharp
public class GameScene : Scene
{
    protected override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.Black);
        
        // Available because this class inherits from Scene
        BeginScaled();
        {
            // All drawing operations use virtual coordinates
            player.Draw(Core.SpriteBatch, playerPosition);
            tilemap.Draw(Core.SpriteBatch, Vector2.Zero);
        }
        Core.SpriteBatch.End();
    }
}
```

**Advantages:**
- Simple, one-line setup
- Handles all SpriteBatch parameters automatically
- Integrates with Scene lifecycle

### Manual Scaling - Any Class

For classes that don't inherit from Scene, use manual scaling with `Core.ScaleMatrix`:

```csharp
public class UIManager
{
    public void Draw()
    {
        // Available in any class
        Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
        {
            // Drawing operations use virtual coordinates
            DrawMainMenu();
            DrawHUD();
        }
        Core.SpriteBatch.End();
    }
}
```

**Advantages:**
- Works in any class
- Full control over SpriteBatch parameters
- Can be combined with custom effects or blend states

## Basic Usage

### Setting Up Content Scaling

```csharp
// Option 1: Simple constructor with automatic Steam Deck detection
var core = new Core("My Game"); // Automatically fullscreen on Steam Deck

// Option 2: Use default virtual resolution (1920x1080)
var core = new Core("My Game", fullScreen: false); // Forced windowed (overrides Steam Deck)

// Option 3: Set custom virtual resolution
var core = new Core("My Game");
Core.VirtualResolution = new Point(1280, 720);

// Option 4: Use monitor-aware sizing with virtual resolution
var core = new Core("My Game", fullScreen: false, windowSizePercent: 80);
Core.VirtualResolution = new Point(1920, 1080);

// Option 5: Manual window size control
var core = new Core("My Game", width: 1920, height: 1080, fullScreen: false);
```

### Steam Deck Optimization

The library automatically detects Steam Deck devices and optimizes settings:

```csharp
// Check if running on Steam Deck
if (Core.IsSteamDeck)
{
    // Automatic optimizations applied:
    // - Native 1280x800 resolution
    // - Fullscreen mode enabled
    // - Virtual resolution scaling works perfectly
}

// Simple constructor automatically handles Steam Deck
var core = new Core("My Game"); // Fullscreen 1280x800 on Steam Deck, windowed on PC
```

**Steam Deck Features:**
- **Automatic Detection**: Checks Linux environment, hardware identifiers, and Steam variables
- **Native Resolution**: Uses Steam Deck's 1280x800 resolution automatically  
- **Fullscreen Mode**: Automatically enables fullscreen for optimal Steam Deck experience
- **Perfect Scaling**: Virtual resolution system works seamlessly with Steam Deck's aspect ratio

### Rendering with Content Scaling

In your Scene's `Draw()` method, use `BeginScaled()` for automatic scaling:

```csharp
protected override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(Color.Black);

    // Automatic content scaling with letterboxing/pillarboxing
    BeginScaled(); // Protected method in Scene base class
    {
        // All drawing operations here use virtual coordinates
        Core.SpriteBatch.DrawString(font, "Hello World", new Vector2(100, 100), Color.White);
        
        // Sprites at virtual coordinates
        player.Draw(Core.SpriteBatch, gameTime);
        
        // UI elements at virtual positions
        Core.SpriteBatch.Draw(buttonTexture, new Vector2(50, 950), Color.White);
    }
    Core.SpriteBatch.End();
}
```

**Alternative for non-Scene classes:**
```csharp
protected override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(Color.Black);

    // Manual content scaling using transformation matrix
    Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
    {
        // All drawing operations here use virtual coordinates
        player.Draw(Core.SpriteBatch, gameTime);
    }
    Core.SpriteBatch.End();
}
```

### Input with Content Scaling

Mouse input is automatically transformed to virtual coordinates:

```csharp
protected override void Update(GameTime gameTime)
{
    base.Update(gameTime);

    // Use virtual coordinates for consistent input handling
    var mousePos = Core.Input.Mouse.VirtualPosition;
    var virtualX = Core.Input.Mouse.VirtualX;
    var virtualY = Core.Input.Mouse.VirtualY;

    // Check if mouse is over a button at virtual coordinates
    var buttonRect = new Rectangle(50, 950, 200, 100);
    if (buttonRect.Contains(mousePos) && Core.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
    {
        // Button clicked
    }

    // Manual coordinate conversion if needed
    var screenPos = new Vector2(100, 100);
    var virtualPos = Core.ScreenToVirtual(screenPos);
    var backToScreen = Core.VirtualToScreen(virtualPos);
}
```

## Resolution Examples

### Target Virtual Resolution: 1920x1080

| Screen Resolution | Device Type  | Scale Factor | Letterbox/Pillarbox | Effective Area |
|------------------|--------------|--------------|---------------------|----------------|
| 1920x1080        | PC Monitor   | 1.0          | None                | Full screen    |
| 2560x1440        | PC Monitor   | 1.33         | None                | Full screen    |
| 1366x768         | Laptop       | 0.71         | Letterbox           | Black bars top/bottom |
| 1280x1024        | PC Monitor   | 0.67         | Pillarbox           | Black bars left/right |
| 3840x2160 (4K)   | PC Monitor   | 2.0          | None                | Full screen    |
| **1280x800**     | **Steam Deck** | **0.67**   | **None**            | **Full screen** |

### Steam Deck Considerations

The Steam Deck's 1280x800 resolution (16:10 aspect ratio) works perfectly with the scaling system:
- **No letterboxing**: 16:10 ratio scales uniformly with 1920x1080 (16:9) virtual resolution
- **Crisp scaling**: 0.67x scale factor provides excellent visual quality
- **Touch support**: Virtual input coordinates work with Steam Deck's touchscreen
- **Performance**: Lower resolution improves performance on Steam Deck's mobile GPU

### Design Guidelines

1. **Design for your virtual resolution**: Create all assets and positioning for your chosen virtual resolution (e.g., 1920x1080)

2. **Use virtual coordinates**: Position all UI elements, sprites, and game objects using virtual coordinates

3. **Test on different resolutions**: The system automatically handles scaling, but test to ensure your UI looks good with letterboxing/pillarboxing

4. **Consider safe areas**: On very wide or very tall screens, important UI should be positioned away from the edges

## Advanced Usage

### Custom Scaling Behavior

```csharp
// Access scaling properties
var virtualRes = Core.VirtualResolution;     // Point (1920, 1080)
var contentScale = Core.ContentScale;        // Vector2 with uniform scaling
var scaleMatrix = Core.ScaleMatrix;          // Matrix for SpriteBatch

// Manual scaling calculations
var screenSize = new Point(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height);
var scaleX = screenSize.X / (float)virtualRes.X;
var scaleY = screenSize.Y / (float)virtualRes.Y;
var uniformScale = Math.Min(scaleX, scaleY);  // Maintains aspect ratio
```

### Manual Matrix Application

```csharp
// For non-Scene classes or when you need manual control over the transformation matrix
Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
// Your drawing code here
Core.SpriteBatch.End();

// With additional SpriteBatch parameters
Core.SpriteBatch.Begin(
    sortMode: SpriteSortMode.Deferred,
    blendState: BlendState.AlphaBlend,
    samplerState: SamplerState.PointClamp,
    depthStencilState: null,
    rasterizerState: null,
    effect: null,
    transformMatrix: Core.ScaleMatrix);
// Your drawing code here
Core.SpriteBatch.End();
```

### Coordinate Conversion Utilities

```csharp
// Convert between coordinate spaces
var screenPoint = new Vector2(1280, 720);
var virtualPoint = Core.ScreenToVirtual(screenPoint);

var virtualPoint2 = new Vector2(960, 540);
var screenPoint2 = Core.VirtualToScreen(virtualPoint2);

// Useful for positioning elements or hit testing
```

## Common Scenarios

### Fullscreen Toggle with Scaling

```csharp
private void ToggleFullscreen()
{
    Core.Graphics.ToggleFullScreen();
    // Content scaling automatically adjusts to new resolution
}
```

### Resolution-Independent UI (Scene class)

```csharp
protected override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(Color.Black);
    
    BeginScaled(); // Scene protected method
    {
        // UI positioned in virtual coordinates - works on any screen size
        var playButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 400);
        var settingsButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 500);
        var quitButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 600);
        
        Core.SpriteBatch.Draw(playButtonTexture, playButtonPos, Color.White);
        Core.SpriteBatch.Draw(settingsButtonTexture, settingsButtonPos, Color.White);
        Core.SpriteBatch.Draw(quitButtonTexture, quitButtonPos, Color.White);
    }
    Core.SpriteBatch.End();
}
```

### Resolution-Independent UI (Non-Scene class)

```csharp
public void DrawUI()
{
    // Manual scaling for classes not inheriting from Scene
    Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
    {
        var playButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 400);
        var settingsButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 500);
        var quitButtonPos = new Vector2(Core.VirtualResolution.X / 2 - 100, 600);
        
        Core.SpriteBatch.Draw(playButtonTexture, playButtonPos, Color.White);
        Core.SpriteBatch.Draw(settingsButtonTexture, settingsButtonPos, Color.White);
        Core.SpriteBatch.Draw(quitButtonTexture, quitButtonPos, Color.White);
    }
    Core.SpriteBatch.End();
}

### Dynamic Resolution Changes

```csharp
// Change virtual resolution at runtime
Core.VirtualResolution = new Point(1280, 720);
// All subsequent drawing and input automatically uses new virtual resolution
```

## Best Practices

1. **Choose an appropriate virtual resolution**: Common choices are 1920x1080, 1280x720, or 1366x768
2. **Design UI with scaling in mind**: Avoid tiny text or UI elements that become unusable when scaled down
3. **Test on multiple aspect ratios**: 16:9, 4:3, 21:9, etc.
4. **Use the virtual input coordinates**: Always use `Core.Input.Mouse.VirtualPosition` for consistent behavior
5. **Consider letterbox/pillarbox areas**: Don't place critical UI where it might be cut off or in black bars

## Troubleshooting

**Problem**: Content appears too small or too large
- **Solution**: Adjust your virtual resolution to match your target design size

**Problem**: Mouse input doesn't align with visual elements
- **Solution**: Use `Core.Input.Mouse.VirtualPosition` instead of `Position`

**Problem**: Content is stretched or distorted
- **Solution**: The system uses uniform scaling; check that you're using `BeginScaled()` or the correct transformation matrix

**Problem**: Black bars appear around content
- **Solution**: This is normal letterboxing/pillarboxing to maintain aspect ratio. Adjust virtual resolution if needed.