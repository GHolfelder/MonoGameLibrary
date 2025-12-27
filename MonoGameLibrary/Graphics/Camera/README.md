# MonoGame Library - Camera System

A comprehensive 2D camera system for MonoGame that provides zoom, position control, character following, and seamless integration with the existing content scaling system.

## Overview

The camera system consists of two main classes:

- **`Camera2D`** - The core camera class providing transformation matrices and coordinate conversion
- **`CameraController`** - Input handling for camera controls (keyboard, gamepad, and mouse wheel)

## Features

### Core Camera Features (Camera2D)
- ✅ **Position Control** - World coordinate positioning with bounds clamping
- ✅ **Zoom System** - Configurable zoom levels with min/max constraints
- ✅ **Rotation Support** - Camera rotation in radians
- ✅ **Character Following** - Automatic following with offset support
- ✅ **Smooth Following** - Interpolated camera movement for polished tracking
- ✅ **Performance Monitoring** - Matrix calculation tracking and optimization
- ✅ **Coordinate Conversion** - Screen ↔ World coordinate transformation
- ✅ **Content Scaling Integration** - Works seamlessly with virtual resolution system

### Input Features (CameraController)
- ✅ **Keyboard Controls** - Customizable key bindings for zoom operations
- ✅ **Gamepad Controls** - Controller support with button mapping
- ✅ **Mouse Wheel Support** - Zoom towards cursor position
- ✅ **Reset Functionality** - Quick return to default zoom levels
- ✅ **Continuous Zoom** - Smooth acceleration-based zooming
- ✅ **Input Toggle** - Enable/disable individual input types

### Character Scaling System
- ✅ **Configurable Screen Coverage** - Adjustable character sizing (default 10% of screen height)
- ✅ **Dynamic Zoom Limits** - Zoom constraints based on character dimensions and coverage setting
- ✅ **Resolution Independence** - Works across different screen sizes

## Quick Start

### Basic Setup

```csharp
public class GameplayScene : Scene
{
    private PlayerSprite player;
    private Tilemap tilemap;
    private TextureAtlas atlas;

    public override void LoadContent()
    {
        // Load texture atlas and game objects
        atlas = TextureAtlas.FromJsonTexture(Content, "sprites.json\");
        player = new PlayerSprite(atlas, \"player\", DirectionMode.FourWay);
        var tilemaps = Tilemap.FromJson(Content, \"level1.json\", atlas);
        tilemap = tilemaps[0]; // or tilemaps[\"mapName\"]
        
        // Configure camera for character size and desired screen coverage
        // IMPORTANT: Use effective rendered size if character is scaled
        float effectiveSize = player.FrameSize.Y * player.Scale.Y; // e.g., 32px * 2.0f = 64px
        Core.Camera.SetCharacterScreenCoverage(0.1f, effectiveSize); // 10% screen coverage
        
        // Alternative approaches:
        // Core.Camera.SetCharacterScreenCoverage(0.12f, player.FrameSize.Y); // 12% for unscaled
        // Core.Camera.CharacterScreenCoverage = 0.08f; // Set property then call SetZoomLimitsForCharacter
        
        // Enable smooth following for polished movement
        Core.Camera.SmoothFollowing = true;
        Core.Camera.FollowSpeed = 5.0f;
        
        // Set up character following
        Core.Camera.Follow(player.Position);
    }

    public override void Update(GameTime gameTime)
    {
        player.Update(gameTime);
        
        // Update camera to follow player
        Core.Camera.Follow(player.Position);
        
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        // World rendering (affected by camera)
        BeginScaled(useCamera: true);
        tilemap.Draw(Core.SpriteBatch, Vector2.Zero);
        player.Draw(Core.SpriteBatch);
        Core.SpriteBatch.End();
        
        // UI rendering (not affected by camera)
        BeginScaledUI();
        DrawUI(); // Your UI elements here
        Core.SpriteBatch.End();
    }
}
```

## Core Integration

The camera system is automatically initialized and integrated with the Core singleton:

```csharp
// Access camera anywhere in your code
Core.Camera.Position = new Vector2(100, 100);
Core.Camera.Zoom = 2.0f;

// Access camera controller
Core.CameraController.ZoomIn();
Core.CameraController.ResetZoom();

// Camera matrix is automatically applied
Matrix cameraMatrix = Core.CameraMatrix; // Combines camera + content scaling
```

## Default Controls

### Keyboard
- **Zoom In**: `+` (Plus) or `Numpad +`
- **Zoom Out**: `-` (Minus) or `Numpad -`
- **Reset Zoom**: `R`

### Gamepad (Player 1)
- **Zoom In**: Right Shoulder Button
- **Zoom Out**: Left Shoulder Button
- **Reset Zoom**: Right Stick Button (click)

### Mouse
- **Zoom In/Out**: Mouse Wheel (zooms toward cursor position)

## Advanced Usage

### Custom Input Bindings

```csharp
// Customize keyboard controls
Core.CameraController.SetKeyBindings(
    zoomInKeys: new[] { Keys.PageUp, Keys.Up },
    zoomOutKeys: new[] { Keys.PageDown, Keys.Down },
    resetZoomKey: Keys.Home
);

// Customize gamepad controls
Core.CameraController.SetButtonBindings(
    zoomInButtons: new[] { Buttons.RightTrigger },
    zoomOutButtons: new[] { Buttons.LeftTrigger },
    resetZoomButton: Buttons.Start
);

// Adjust mouse wheel sensitivity
Core.CameraController.MouseWheelZoomSensitivity = 0.3f;
```

### Camera Configuration

```csharp
// Manual zoom limits
Core.Camera.MinZoom = 0.5f;  // Can't zoom out beyond this
Core.Camera.MaxZoom = 4.0f;  // Can't zoom in beyond this

// Character screen coverage (affects default zoom and zoom limits)
Core.Camera.CharacterScreenCoverage = 0.15f; // 15% of screen height

// World bounds (keeps camera within map)
Core.Camera.WorldBounds = new Rectangle(0, 0, mapWidth, mapHeight);

// Smooth following settings
Core.Camera.SmoothFollowing = true;
Core.Camera.FollowSpeed = 8.0f; // Higher = faster following
Core.Camera.FollowOffset = new Vector2(0, -32); // Camera offset from target

// Custom zoom speed
Core.CameraController.ZoomSpeed = 0.2f;
Core.CameraController.ZoomAcceleration = 1.5f;
```

### Coordinate Conversion

```csharp
// Convert between coordinate systems
Vector2 screenPos = new Vector2(400, 300);
Vector2 worldPos = Core.Camera.ScreenToWorld(screenPos);

Vector2 worldPos = player.Position;
Vector2 screenPos = Core.Camera.WorldToScreen(worldPos);

// Mouse coordinates in world space
Vector2 mouseWorldPos = Core.Input.Mouse.WorldPosition;

// Direct access via Core utilities
Vector2 worldPos = Core.ScreenToWorld(screenPos); // Includes camera transformation
Vector2 virtualPos = Core.ScreenToVirtual(screenPos); // Content scaling only
```

### Performance Monitoring

```csharp
// Monitor camera performance
var stats = Core.Camera.PerformanceStats;
Console.WriteLine($"Matrix updates: {stats.MatrixUpdateCount}");
Console.WriteLine($"Average update time: {stats.AverageMatrixUpdateTime:F3}ms");

// Reset statistics
Core.Camera.ResetPerformanceStats();

// Debug camera state and coverage
string diagnostics = Core.Camera.GetDiagnosticInfo(effectiveCharacterSize);
Console.WriteLine(diagnostics);
// Shows: position, zoom, limits, target vs actual coverage, character size, resolution
```

### Advanced Zoom Controls

```csharp
// Set custom character screen coverage and update limits
Core.Camera.SetCharacterScreenCoverage(0.08f, characterHeight); // 8% coverage

// Zoom to specific world position
Vector2 targetPos = new Vector2(500, 300);
Core.CameraController.ZoomToPosition(targetPos, 2.0f);

// Manual zoom with custom amounts
Core.CameraController.ZoomIn(0.5f);   // Large zoom in
Core.CameraController.ZoomOut(0.25f); // Small zoom out

// Direct camera control
Core.Camera.SetZoom(1.5f);
Core.Camera.AdjustZoom(0.1f); // Relative adjustment
```

### Scene Integration Patterns

```csharp
public class MyGameScene : Scene
{
    public override void Draw(GameTime gameTime)
    {
        // Method 1: Separate world and UI batches
        BeginScaled(useCamera: true);
        DrawWorldObjects();
        Core.SpriteBatch.End();
        
        BeginScaledUI();
        DrawUserInterface();
        Core.SpriteBatch.End();
        
        // Method 2: Manual matrix control
        Core.SpriteBatch.Begin(transformMatrix: Core.CameraMatrix);
        DrawWorldObjects();
        Core.SpriteBatch.End();
        
        Core.SpriteBatch.Begin(transformMatrix: Core.ScaleMatrix);
        DrawUserInterface();
        Core.SpriteBatch.End();
    }
}
```

### Disabling Input Types

```csharp
// Disable specific input types
Core.CameraController.KeyboardEnabled = false;  // No keyboard zoom
Core.CameraController.GamePadEnabled = false;   // No gamepad zoom
Core.CameraController.MouseWheelEnabled = false; // No mouse wheel zoom

// Re-enable later
Core.CameraController.KeyboardEnabled = true;
```

## Architecture Notes

### Matrix Chain
The camera system applies transformations in this order:
1. **Camera Transformation** (position, zoom, rotation)
2. **Content Scaling** (virtual resolution to screen resolution)

### Integration with Content Scaling
- Camera operates in **virtual coordinate space** (1920x1080 by default)
- Camera transformations are applied **before** content scaling
- UI rendering can bypass camera transformation using `BeginScaledUI()`

### Performance Optimization
- Matrix calculations are **cached** and only updated when camera properties change
- Performance monitoring tracks matrix calculation frequency and timing
- Dirty flag system prevents unnecessary matrix recalculation

### Coordinate Systems
- **Screen Space**: Actual pixel coordinates on the display
- **Virtual Space**: Content scaling coordinate system (1920x1080)
- **World Space**: Game world coordinates (includes camera transformation)

## Troubleshooting

### Common Issues

**Camera doesn't follow character smoothly**
- Enable smooth following: `Core.Camera.SmoothFollowing = true`
- Adjust follow speed: `Core.Camera.FollowSpeed = 5.0f`

**Zoom limits feel wrong**
- Use character-based limits: `Core.Camera.SetZoomLimitsForCharacter(characterHeight)`
- For scaled characters, use effective size: `SetZoomLimitsForCharacter(characterHeight * scale)`
- Use diagnostics to debug: `Console.WriteLine(Core.Camera.GetDiagnosticInfo(effectiveCharacterSize))`
- Manually set: `Core.Camera.MinZoom = 0.5f; Core.Camera.MaxZoom = 3.0f`

**UI elements zoom with camera**
- Use `BeginScaledUI()` instead of `BeginScaled(useCamera: true)` for UI
- Or use `Core.ScaleMatrix` instead of `Core.CameraMatrix`

**Mouse coordinates don't match cursor position**
- Use world coordinates: `Core.Input.Mouse.WorldPosition`
- Or convert manually: `Core.Camera.ScreenToWorld(mouseScreenPos)`

**Character appears too large/small despite correct coverage percentage**
- Ensure you're using the effective rendered size, not the base sprite size
- For scaled sprites: `effectiveSize = spriteSize * scale`
- Example: 32px sprite with 2x scale = use 64px for camera calculations
- Use diagnostic method to verify: `Core.Camera.GetDiagnosticInfo(effectiveSize)`

**Performance issues with many matrix updates**
- Check performance stats: `Core.Camera.PerformanceStats`
- Avoid setting camera properties every frame unless necessary
- Consider less frequent camera updates for static scenes

### Best Practices

1. **Always call `Core.Camera.Follow()` in your Update loop** for character following
2. **Use `BeginScaled(useCamera: true)`** for world objects that should move with camera
3. **Use `BeginScaledUI()`** for UI elements that should stay fixed on screen
4. **Set zoom limits based on character size** for optimal player experience
5. **Enable smooth following** for polished camera movement
6. **Monitor performance** in complex scenes with many camera updates

## Migration from Manual Camera

If you have existing manual camera code, here's how to migrate:

### Old Manual Camera Pattern
```csharp
// Old way
private Vector2 _cameraPosition;
_cameraPosition = player.Position - new Vector2(400, 300);
Vector2 tilemapPosition = -_cameraPosition;

Core.SpriteBatch.Begin(transformMatrix: Matrix.CreateTranslation(tilemapPosition.X, tilemapPosition.Y, 0));
```

### New Camera2D System
```csharp
// New way
Core.Camera.Follow(player.Position);

BeginScaled(useCamera: true); // Automatically applies camera transformation
```

The new system handles all the matrix mathematics automatically while providing much more functionality.