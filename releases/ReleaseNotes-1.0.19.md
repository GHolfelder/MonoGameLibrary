# MonoGame Library Release Notes - Version 1.0.19

**Release Date**: November 25, 2025  
**Previous Version**: 1.0.18

## 🎯 Major Features

### Developer Mode System
- **F1/F2 Hotkeys**: Added comprehensive developer mode with visual indicators
  - **F1**: Toggle developer mode (red circle indicator at top center)
  - **F2**: Toggle collision box visibility (yellow box indicator + collision visualization)
- **Automatic Collision Visualization**: All sprites, character sprites, and tilemaps show collision boxes when developer mode is active
- **Debug Build Only**: Zero overhead in release builds through conditional compilation
- **Scene Base Class Integration**: Automatic handling through `base.Update()` and `base.Draw()` calls

### Enhanced Object Layer Support
- **ObjectType Property**: Added explicit shape type specification (rectangle, ellipse, point, polygon, polyline, tile, text)
- **Float Coordinate Handling**: Automatic parsing and truncation of decimal coordinates to integer collision boundaries
- **Real-World JSON Compatibility**: Enhanced parsing for production tilemap formats from tools like Tiled

### Advanced Collision Detection
- **Polyline Line-Based Collision**: Precise geometric collision detection treating polylines as actual line barriers
- **Endpoint Precision**: Strict intersection algorithms preventing false positives beyond line endpoints
- **Multi-Shape Support**: Comprehensive collision system supporting all Tiled object types

## 🔧 Technical Improvements

### Collision System Enhancements
- **PlayerSprite Object Layer Integration**: Updated to use object layer collision instead of tile-based collision
- **Geometric Precision**: Implemented epsilon-based bounds checking for line intersection calculations
- **Collision Visualization**: Added `DrawAutoCollision()` extension method for tilemaps

### Code Quality Improvements
- **EnableDraw Bypass**: Developer mode collision visualization works regardless of individual collision EnableDraw settings
- **Clean API**: Streamlined collision detection with consistent property access patterns
- **Performance Optimized**: Early rejection tests and efficient geometric algorithms

## 🚀 New APIs

### Core Class Additions
```csharp
// Developer mode properties (Debug builds only)
public static bool DeveloperMode { get; set; }
public static bool ShowCollisionBoxes { get; set; }
public static void ToggleDeveloperMode()
public static void ToggleCollisionBoxes()
```

### Tilemap Extensions
```csharp
// Automatic collision visualization
tilemap.DrawAutoCollision(spriteBatch, position, Color.Red);
```

### Enhanced Object Layer Support
- **ObjectType Property**: Explicit shape type identification
- **Float Coordinate Parsing**: Handles decimal positions from modern tilemap editors
- **Improved Shape Detection**: Better compatibility with various JSON export formats

## 🎮 Usage Examples

### Developer Mode Setup
```csharp
// In your scene Update method:
base.Update(gameTime); // Handles F1/F2 hotkeys

// In your scene Draw method:
base.Draw(gameTime); // Shows developer indicators

// Collision boxes show automatically when F2 pressed
```

### Enhanced Tilemap Collision
```csharp
// Automatic collision visualization for all object layers
tilemap.DrawAutoCollision(Core.SpriteBatch, Vector2.Zero);

// PlayerSprite now uses object layer collision
player.SetTilemap(tilemap, Vector2.Zero);
// Movement automatically checks object layer collision
```

## 🔄 Breaking Changes

**None** - This release maintains full backward compatibility while adding new functionality.

## 🐛 Bug Fixes

- **Polyline Collision Precision**: Fixed collision detection extending beyond actual line endpoints
- **Float Coordinate Handling**: Proper parsing of decimal coordinates from modern tilemap editors
- **CharacterSprite Collision Visibility**: Fixed collision boxes not appearing for simple Draw() method calls
- **ObjectType Shape Detection**: Improved parsing for various JSON export formats

## 📈 Performance Improvements

- **Early Bounds Checking**: Faster line intersection calculations with early rejection
- **Conditional Compilation**: Zero runtime overhead for developer mode features in release builds
- **Optimized Geometric Algorithms**: More efficient collision detection with mathematical precision

## 🎉 Developer Experience

- **Visual Feedback**: Instant collision visualization with F1/F2 hotkeys
- **Clean Integration**: Works seamlessly with existing Scene base class patterns
- **Production Ready**: Debug features compile out completely in release builds
- **Comprehensive Support**: Works with sprites, character sprites, tilemaps, and all object layer shapes

---

**Upgrade Notes**: Simply update your MonoGameLibrary package reference to 1.0.19. No code changes required for existing functionality. Add `base.Update()` and `base.Draw()` calls to your scenes to enable developer mode features.