# MonoGame Library

A comprehensive C# library for MonoGame development, providing utilities for graphics, input handling, audio, and scene management.

## Features

### Graphics
- **TextureAtlas**: Manage sprite sheets and texture regions with XML and JSON configuration support
- **Animation**: Frame-based animation system
- **Sprite & AnimatedSprite**: Sprite rendering with animation support
- **Tilemap & Tileset**: Tile-based level creation and rendering
- **TextureRegion**: Efficient texture sub-region management

### Input
- **InputManager**: Centralized input handling for keyboard, mouse, and gamepad
- **KeyboardInfo, MouseInfo, GamePadInfo**: Input state management

### Audio
- **AudioController**: Audio playback and management

### Scenes
- **Scene Management**: Basic scene system for game state management

### Core
- **Core Utilities**: Common game development utilities

## Getting Started

1. Reference this library in your MonoGame project
2. Initialize the systems you need (InputManager, AudioController, etc.)
3. Load texture atlases and tilemaps using the `FromXml` or `FromJson` methods
4. Create sprites and animations using the provided classes

## Configuration

### XML Configuration

#### Texture Atlas
```xml
<TextureAtlas>
    <Texture>path/to/texture</Texture>
    <Regions>
        <Region name="sprite1" x="0" y="0" width="32" height="32" />
    </Regions>
    <Animations>
        <Animation name="walk" delay="100">
            <Frame region="sprite1" />
        </Animation>
    </Animations>
</TextureAtlas>
```

#### Tilemap
```xml
<Tilemap>
    <Tileset region="0 0 256 256" tileWidth="32" tileHeight="32">path/to/tileset</Tileset>
    <Tiles>
        0 1 2
        3 4 5
    </Tiles>
</Tilemap>
```

### JSON Configuration

#### Texture Atlas (Separate Files)

**Texture JSON:**
```json
{
  "width": 1024,
  "height": 1024,
  "sprites": [
    {
      "name": "player_walk_N_0",
      "x": 0,
      "y": 0,
      "width": 64,
      "height": 64,
      "rotated": false
    }
  ],
  "atlasFile": "../Content/images/atlas.png"
}
```

**Animation JSON:**
```json
{
  "animations": [
    {
      "name": "player_walk_N",
      "frames": [
        {"sprite": "player_walk_N_0"},
        {"sprite": "player_walk_N_1"}
      ],
      "defaultDuration": 200,
      "loop": true
    }
  ]
}
```

### Loading Methods

#### XML Loading
```csharp
// Combined XML file (texture + animations)
var atlas = TextureAtlas.FromXml(content, "atlas.xml");

// Separate XML files
var atlas = TextureAtlas.FromXml(content, "texture.xml", "animations.xml");

// Load texture only, then add animations later
var atlas = TextureAtlas.FromXmlTexture(content, "texture.xml");
atlas.LoadAnimationsFromXml("walk_animations.xml");
atlas.LoadAnimationsFromXml("attack_animations.xml");
```

#### JSON Loading
```csharp
// Separate JSON files
var atlas = TextureAtlas.FromJson(content, "sprites.json", "animations.json");

// Load texture only, then add animations later
var atlas = TextureAtlas.FromJsonTexture(content, "sprites.json");
atlas.LoadAnimationsFromJson("walk_animations.json");
atlas.LoadAnimationsFromJson("attack_animations.json");
```

## Requirements

- .NET 9.0
- MonoGame Framework
- System.Text.Json (for JSON configuration support)

## Game-specific classes (temporary, to be generalized)

This library currently includes a small set of opinionated, game-facing classes under `MonoGameLibrary/Graphics` (and an experimental AI interface under `MonoGameLibrary/AI`) to help you get moving quickly. These are not yet part of the stable, reusable API and will be generalized or moved into samples over time. If you use them, expect breaking changes in future minor versions.

- `Graphics/AnimationState.cs`
  - What it is: Fixed enum of animation states (Idle, Walk, Run, Attack, Hurt, Death).
  - Why it’s game-specific: The concrete state set is opinionated and won’t fit all games.
  - Notes: The `IAIBehavior` interface and `WanderBehavior` example have been moved to `AI/AIBehavior.cs` under the `MonoGameLibrary.AI` namespace.
  - Suggested changes (roadmap):
    - Replace hard-coded enum with user-defined identifiers (e.g., strings or an app-defined enum) or provide an extensible registry.
    - Keep this enum as a sample reference rather than core API.

- `Graphics/Direction.cs`
  - What it is: `Direction8`, `Direction4`, and `DirectionHelper` utilities (abbreviations, 8-way to 4-way mapping).
  - Why it’s borderline: These are generally useful, but their location under Graphics is debatable.
  - Suggested changes (roadmap):
    - Move to a utility namespace (e.g., `MonoGameLibrary` or `MonoGameLibrary.Utilities`).
    - Keep the helper small and dependency-free; consider adding vector conversion helpers as needed.

- `Graphics/CharacterSprite.cs`
  - What it is: A base sprite controller that maps (state, direction) → `AnimatedSprite` using a naming pattern; falls back to a placeholder sprite if an animation is missing.
  - Why it’s game-specific: Assumes a particular naming convention and state/direction model.
  - Suggested changes (roadmap):
    - Extract a pluggable naming strategy (e.g., `IAnimationNameFormatter`) instead of the hard-coded "{prefix}_{state}_{dir}" pattern.
    - Abstract animation lookup behind an `IAnimationResolver` so it’s not tied to `TextureAtlas` naming.
    - Consider a `DirectionMode` (FourWay/EightWay) instead of booleans.
    - Replace `Console.WriteLine` with a logging hook; ensure silent behavior in release builds.

- `Graphics/PlayerSprite.cs`
  - What it is: A player-controlled sprite with keyboard/gamepad handling via `Core.Input` and sprint support.
  - Why it’s game-specific: Directly couples gameplay input to rendering/animation.
  - Suggested changes (roadmap):
    - Move to the samples folder; keep the core library input-agnostic.
    - Inject an `IInputProvider` (keyboard/gamepad implementation provided separately) instead of accessing `Core.Input` statically.
    - Keep tuning knobs (`MovementSpeed`, `SprintMultiplier`) as public properties; avoid hidden constants.

- `Graphics/NPCSprite.cs`
  - What it is: An NPC sprite driven by `IAIBehavior` (from `MonoGameLibrary.AI`); includes a small `Vector2Extensions.Normalized()` helper.
  - Why it’s game-specific: Bakes a particular movement model and behavior interface into the sprite.
  - Suggested changes (roadmap):
    - Move to samples or a future `MonoGameLibrary.AI` package.
    - Keep AI behavior injection-based; avoid tight coupling to any one AI system.
    - Relocate generic math helpers (like `Vector2Extensions`) to a utilities file or drop if redundant with MonoGame APIs.

- `AI/AIBehavior.cs`
  - What it is: `IAIBehavior` interface and a simple `WanderBehavior` example under the `MonoGameLibrary.AI` namespace (experimental).
  - Why it’s game-specific: The behavior model is intentionally minimal and opinionated; real games will need custom behaviors.
  - Suggested changes (roadmap):
    - Treat these as examples; consider moving to a `/samples` folder.
    - Keep AI injection-based so sprites remain decoupled from specific behavior systems.
    - Iterate toward a small set of reusable AI interfaces only if they prove broadly useful.

### Using these classes today
- Treat them as examples or starters rather than stable API.
- Prefer composition: keep your own game logic outside and feed state/direction into sprites.
- If you fork/modify, consider moving them to your game project while the library evolves the generalized equivalents.

## License

[Add your license information here]