# MonoGame Library Release Notes - Version 1.0.20

**Release Date**: November 25, 2025  
**Previous Version**: 1.0.19

## 🎯 Major Features

### Comprehensive Content Scaling System
- **Virtual Resolution System**: Design games for a fixed virtual resolution (default 1920x1080) that automatically scales to any screen size
- **Automatic Letterboxing/Pillarboxing**: Maintains aspect ratio with black bars when needed, preventing content distortion
- **Scene Integration**: `BeginScaled()` protected method in Scene base class for one-line content scaling
- **Input Transformation**: Mouse coordinates automatically converted to virtual space for consistent interaction
- **Cross-Resolution Compatibility**: Same visual experience from 1366x768 laptops to 4K monitors

### Steam Deck Optimization & Auto-Detection
- **Automatic Steam Deck Detection**: `Core.IsSteamDeck` property detects Steam Deck hardware and environment
- **Native Resolution Support**: Automatic 1280x800 resolution optimization for Steam Deck's display
- **Fullscreen Automation**: Automatically enables fullscreen mode on Steam Deck for optimal gaming experience
- **Zero Configuration**: Simple `new Core("My Game")` constructor automatically optimizes for Steam Deck
- **Cross-Platform Compatibility**: Seamless operation on Windows, Mac, Linux, and Steam Deck

### Monitor Resolution Awareness
- **Automatic Window Sizing**: Intelligent window sizing based on monitor resolution and DPI
- **Multiple Monitor Support**: Detects primary monitor characteristics for optimal initial window size
- **Scalable Constructors**: Percentage-based window sizing (e.g., 80% of monitor size)

## 🔧 Technical Improvements

### Enhanced Core Architecture
- **Transformation Matrix System**: `Core.ScaleMatrix` provides automatic scaling transformation
- **Coordinate Conversion**: `ScreenToVirtual()` and `VirtualToScreen()` utilities for manual coordinate handling
- **Uniform Scaling**: Maintains aspect ratio through calculated scale factors
- **Viewport Management**: `ScaledViewport` property for advanced rendering scenarios

### Input System Enhancements
- **Virtual Input Coordinates**: `MouseInfo.VirtualPosition`, `VirtualX`, `VirtualY` properties
- **Automatic Transformation**: Input coordinates seamlessly work with scaled content
- **Consistent Interaction**: UI elements respond correctly regardless of screen resolution

### Performance & Compatibility
- **Zero Overhead**: Content scaling has minimal performance impact
- **MonoGame Integration**: Works seamlessly with existing MonoGame rendering pipeline
- **Tilemap Compatibility**: All tilemap and collision systems automatically work with content scaling

## 🚀 New APIs

### Core Class Additions
```csharp
// Virtual resolution and scaling
public static Point VirtualResolution { get; set; }
public static Vector2 ContentScale { get; }
public static Matrix ScaleMatrix { get; }
public static Rectangle ScaledViewport { get; }

// Steam Deck detection
public static bool IsSteamDeck { get; }

// Coordinate conversion utilities
public static Vector2 ScreenToVirtual(Vector2 screenPosition)
public static Vector2 VirtualToScreen(Vector2 virtualPosition)

// Enhanced constructors with Steam Deck optimization
public Core(string title) // Auto-optimizes for Steam Deck
public Core(string title, bool fullScreen = false, float windowSizePercent = 0.8f)

// Monitor resolution utilities
public static Point GetMonitorAwareSize(float sizePercent = 0.8f)
public static void SetVirtualResolution(int width, int height)
```

### Scene Base Class Enhancements
```csharp
// Automatic content scaling for Scene classes
protected void BeginScaled(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                          BlendState blendState = null,
                          SamplerState samplerState = null, 
                          DepthStencilState depthStencilState = null,
                          RasterizerState rasterizerState = null,
                          bool useScaling = true)
```

### MouseInfo Enhancements
```csharp
// Virtual coordinate properties for scaled input
public Vector2 VirtualPosition { get; }
public float VirtualX { get; }
public float VirtualY { get; }
```

## 📚 Documentation & Examples

### Comprehensive Content Scaling Guide
- **[ContentScalingExample.md](../ContentScalingExample.md)**: Complete usage guide with examples
- **Resolution Independence Patterns**: Best practices for cross-resolution development
- **Steam Deck Optimization Examples**: Specific guidance for Steam Deck development
- **Input Handling Patterns**: Virtual coordinate usage examples

### Updated API Documentation
- **Scene README**: Enhanced with `BeginScaled()` usage patterns and virtual coordinate examples
- **Main README**: Added content scaling as featured system with comprehensive examples
- **Corrected Examples**: Fixed all documentation to show proper `BeginScaled()` vs manual scaling usage

## 🎮 Usage Examples

### Basic Content Scaling Setup
```csharp
// Automatic Steam Deck optimization
var core = new Core("My Game"); // Fullscreen 1280x800 on Steam Deck, windowed on PC

// Manual configuration
Core.VirtualResolution = new Point(1920, 1080);
```

### Scene Drawing with Content Scaling
```csharp
protected override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(Color.Black);
    
    BeginScaled(samplerState: SamplerState.PointClamp);
    {
        // All coordinates are in virtual space
        player.Draw(Core.SpriteBatch, playerPosition);
        tilemap.Draw(Core.SpriteBatch, Vector2.Zero);
    }
    Core.SpriteBatch.End();
}
```

### Virtual Input Handling
```csharp
protected override void Update(GameTime gameTime)
{
    // Input automatically scaled to virtual coordinates
    var mousePos = Core.Input.Mouse.VirtualPosition;
    
    if (buttonRect.Contains(mousePos) && Core.Input.Mouse.WasButtonJustPressed(MouseButton.Left))
    {
        // Button interaction works consistently across all resolutions
    }
}
```

### Manual Scaling for Non-Scene Classes
```csharp
public void DrawUI()
{
    Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
    {
        // Drawing operations use virtual coordinates
        Core.SpriteBatch.DrawString(font, "Menu", menuPosition, Color.White);
    }
    Core.SpriteBatch.End();
}
```

## 🔄 Breaking Changes
- **None**: All changes are additive and backward compatible
- **Enhanced Constructors**: New overloads available, existing constructors unchanged
- **Opt-in Features**: Content scaling requires explicit use of `BeginScaled()` or manual scaling

## 🐛 Bug Fixes
- **Steam Deck Detection**: Fixed compilation errors with `IsSteamDeck` property definition
- **Coordinate Transformation**: Proper handling of input coordinates in scaled environments
- **Aspect Ratio Preservation**: Corrected letterboxing/pillarboxing calculations for all aspect ratios

## ⬆️ Upgrade Guide

### From Version 1.0.19
1. **No Breaking Changes**: Existing code continues to work without modification
2. **Optional Scaling**: Add `BeginScaled()` to Scene Draw methods for content scaling
3. **Virtual Input**: Use `Core.Input.Mouse.VirtualPosition` for scaled input handling
4. **Steam Deck**: Games automatically optimize for Steam Deck without code changes

### Recommended Updates
```csharp
// Before (still works)
protected override void Draw(GameTime gameTime)
{
    Core.SpriteBatch.Begin();
    // Drawing code
    Core.SpriteBatch.End();
}

// After (resolution independent)
protected override void Draw(GameTime gameTime)
{
    BeginScaled();
    {
        // Same drawing code, now scales automatically
    }
    Core.SpriteBatch.End();
}
```

## 🎯 Next Version Preview
- Enhanced mobile device detection and optimization
- Additional input device support for Steam Deck (gyro, haptics)
- Advanced scaling modes (integer scaling, custom aspect ratios)
- Performance profiling and optimization tools

---

## 📈 Statistics
- **Lines Added**: 365+
- **New APIs**: 12+
- **Documentation Pages**: 3 major updates
- **Platforms Supported**: Windows, Mac, Linux, Steam Deck
- **Backward Compatibility**: 100%

**Full Changelog**: [v1.0.19...v1.0.20](https://github.com/GHolfelder/MonoGameLibrary/compare/v1.0.19...v1.0.20)