# Monitor Resolution Detection Examples

The Core class now provides monitor resolution detection and automatic window sizing capabilities.

## Basic Usage

### Monitor Information
```csharp
// Get monitor resolution
int monitorWidth = Core.MonitorWidth;      // e.g., 1920
int monitorHeight = Core.MonitorHeight;    // e.g., 1080

// Get display mode details
DisplayMode display = Core.PrimaryDisplayMode;
Console.WriteLine($"Monitor: {display.Width}x{display.Height} @ {display.RefreshRate}Hz");
```

### Automatic Window Sizing
```csharp
// Constructor with automatic sizing (80% of monitor)
var game = new MyGame("My Game", fullScreen: false);

// Constructor with custom percentage
var game = new MyGame("My Game", fullScreen: false, windowSizePercent: 0.6f);

// Traditional fixed-size constructor still available
var game = new MyGame("My Game", 1280, 720, fullScreen: false);
```

### Dynamic Window Management
```csharp
// Resize to specific dimensions
Core.ResizeWindow(1280, 720);

// Resize to percentage of monitor
Core.ResizeWindowToMonitorPercent(0.7f);  // 70% of monitor size

// Center the window on screen
Core.CenterWindow();

// Get recommended size without applying
Point recommendedSize = Core.RecommendedWindowSize;  // 80% of monitor
Point customSize = Core.GetMonitorAwareSize(0.6f);  // 60% of monitor
```

## Practical Examples

### Responsive Game Setup
```csharp
public class MyGame : Core
{
    public MyGame() : base("My Game", fullScreen: false, windowSizePercent: 0.75f)
    {
        // Window automatically sized to 75% of monitor
        // Minimum size enforced (800x600)
        // Margins maintained (doesn't fill entire screen)
    }

    protected override void LoadContent()
    {
        // Center the window after setup
        CenterWindow();
        
        // Log monitor info for debugging
        Console.WriteLine($"Monitor: {MonitorWidth}x{MonitorHeight}");
        Console.WriteLine($"Window: {Graphics.PreferredBackBufferWidth}x{Graphics.PreferredBackBufferHeight}");
    }
}
```

### Runtime Window Management
```csharp
public override void Update(GameTime gameTime)
{
    // F11 to toggle between windowed sizes
    if (Input.Keyboard.WasKeyJustPressed(Keys.F11))
    {
        if (Graphics.PreferredBackBufferWidth < MonitorWidth * 0.8f)
        {
            ResizeWindowToMonitorPercent(0.9f);  // Larger
        }
        else
        {
            ResizeWindowToMonitorPercent(0.6f);  // Smaller
        }
        CenterWindow();
    }
    
    base.Update(gameTime);
}
```

### Multi-Monitor Considerations
```csharp
// Primary monitor info (works on multi-monitor setups)
var primaryDisplay = Core.PrimaryDisplayMode;

// For multi-monitor support, you can access additional adapters:
foreach (var adapter in GraphicsAdapter.Adapters)
{
    var mode = adapter.CurrentDisplayMode;
    Console.WriteLine($"Display: {mode.Width}x{mode.Height}");
}
```

## Size Calculation Logic

The automatic sizing follows these rules:

1. **Base Size**: Percentage of monitor resolution
2. **Minimum Size**: At least 800x600 (ensures usability)
3. **Maximum Size**: Monitor size minus 100px margin
4. **Centering**: Positioned at screen center when possible

### Common Size Examples
- **Laptop (1366x768)**: 80% = 1093x614 (enforced min: 1093x614)
- **Full HD (1920x1080)**: 80% = 1536x864
- **4K (3840x2160)**: 80% = 3072x1728
- **Small monitor (1024x768)**: 80% = 819x614 (enforced min: 819x614)

This ensures your game window is appropriately sized for any monitor configuration!