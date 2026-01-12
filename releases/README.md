# MonoGame Library Release Notes

This directory contains detailed release notes for each version of MonoGame Library, documenting all features, improvements, and changes.

## Latest Releases

### [Version 1.0.25](ReleaseNotes-1.0.25.md) - January 11, 2026
**Performance Optimizations & API Improvements**
- 30% faster collision detection algorithms with reduced memory allocations
- Standardized method signatures and improved API consistency across all systems
- Enhanced error handling and debugging with descriptive messages and context
- Resolved edge cases in room transitions and tilemap rendering systems
- Improved performance monitoring and diagnostic tools for better optimization

### [Version 1.0.24](ReleaseNotes-1.0.24.md) - January 4, 2026
**Enhanced Character System, Debug Messages & Room Management**
- Character velocity exposure with automatic calculation and game logic integration
- Enhanced debug system with message deduplication and overlay integration
- Professional room management with QuadTree spatial optimization
- Seamless room transitions with exit collision detection and spatial partitioning

### [Version 1.0.23](ReleaseNotes-1.0.23.md) - December 29, 2025
**Enhanced Developer Mode, FPS System & Multiple Maps**
- Enhanced FPS display with 2-decimal precision and dynamic background sizing
- Real-time FPS tracking with rolling buffer system and VSync control (F3 hotkey)
- Improved developer overlay with font scale compensation for object names
- TilemapCollection for managing multiple tilemaps from single JSON files
- Complete tile collision integration system with PlayerSprite enhancement
- Fixed FPS calculation issues and production-ready VSync defaults

### [Version 1.0.22](ReleaseNotes-1.0.22.md) - December 14, 2025
**Animated Tile System**
- Complete animation framework with AnimatedTile, AnimatedTileFrame, and AnimatedTileInstance classes
- JSON configuration for animated tiles with customizable frame durations
- Automatic frame cycling with precise timing and seamless integration
- Per-layer instance management with memory optimization and smooth performance

### [Version 1.0.21](ReleaseNotes-1.0.21.md) - November 27, 2025
**Comprehensive 2D Camera System**
- Professional-grade camera architecture with position, zoom, rotation, and following capabilities
- Configurable character screen coverage system with intelligent zoom limits  
- Multi-input controls (keyboard, gamepad, mouse wheel) with customizable bindings
- Smooth camera following with interpolation and performance monitoring
- World coordinate input integration and comprehensive diagnostic tools
- Complete documentation with examples, troubleshooting, and migration guide

### [Version 1.0.20](ReleaseNotes-1.0.20.md) - November 25, 2025
**Content Scaling System & Steam Deck Support**
- Comprehensive virtual resolution system with automatic scaling and letterboxing/pillarboxing
- Steam Deck auto-detection with native resolution (1280x800) and fullscreen optimization
- Monitor resolution awareness with intelligent window sizing
- Enhanced input system with virtual coordinate transformation
- BeginScaled() Scene method for one-line content scaling
- Cross-platform compatibility from 1366x768 laptops to 4K monitors

## Release History

### [Version 1.0.19](ReleaseNotes-1.0.19.md) - November 25, 2025
**Developer Mode & Enhanced Object Layer Support**
- F1/F2 hotkey developer mode with collision visualization
- Enhanced object layer support with objectType property  
- Advanced polyline collision detection with geometric precision
- PlayerSprite object layer integration
- Zero overhead developer features in release builds

### Version 1.0.18
- Enhanced Tilemap System & Multi-Shape Collision Support
- Comprehensive object layer parsing improvements
- Advanced collision detection algorithms

### Version 1.0.17  
- Improved Graphics Pipeline & Animation System
- Enhanced texture atlas management
- Performance optimizations

### Version 1.0.16
- Core Architecture Improvements
- Scene management enhancements
- Input system refinements

## Reading Release Notes

Each release note file includes:
- **Major Features** - New functionality and capabilities
- **Technical Improvements** - Code quality and performance enhancements  
- **New APIs** - Classes, methods, and properties added
- **Usage Examples** - Code samples demonstrating new features
- **Breaking Changes** - Compatibility notes and upgrade guidance
- **Bug Fixes** - Issues resolved and improvements made

## Version Naming

MonoGame Library follows semantic versioning:
- **Major.Minor.Patch** format (e.g., 1.0.19)
- **Major**: Breaking changes or major architectural shifts
- **Minor**: New features and enhancements (backward compatible)  
- **Patch**: Bug fixes and minor improvements

## Contributing

When contributing features, please update the appropriate release notes file or create a new one following the established format and structure.