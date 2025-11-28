# MonoGame Library Release Notes

This directory contains detailed release notes for each version of MonoGame Library, documenting all features, improvements, and changes.

## Latest Releases

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