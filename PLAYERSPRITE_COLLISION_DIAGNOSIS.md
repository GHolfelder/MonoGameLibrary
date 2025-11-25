# PlayerSprite Collision Box Diagnosis

## Quick Diagnosis Steps

### Step 1: Check Console Output
When you press F2 and your player draws, you should see one of these messages in the console:

**✅ Good**: `Drawing collision for CharacterSprite at {position}`
**❌ Problem**: `CharacterSprite at {position} has no collision component - cannot draw collision boxes`

### Step 2: Verify Collision is Enabled
PlayerSprite doesn't enable collision by default. You need to call one of these in your game setup:

```csharp
// Option 1: Rectangular collision
_player.EnableCollision(32, 32);  // width, height

// Option 2: Rectangular collision with offset
_player.EnableCollision(24, 30, new Vector2(4, 2));  // width, height, offset

// Option 3: Circular collision
_player.EnableCollision(16.0f);  // radius
```

### Step 3: Complete Example Setup

```csharp
// In your scene initialization or LoadContent:
_player = new PlayerSprite(atlas, "player", DirectionMode.EightWay, inputProvider, "idle", "walk");
_player.Position = new Vector2(100, 100);

// CRITICAL: Enable collision for the player
_player.EnableCollision(24, 32, new Vector2(4, 0));  // Adjust size/offset for your sprite

// Set up tilemap collision if using tilemaps
_player.SetTilemap(_tileMap, Vector2.Zero);
```

### Step 4: Verify in Game
1. Press F1 (red circle should appear - developer mode on)
2. Press F2 (yellow box should appear - collision boxes on)
3. Check console for debug messages
4. Player collision box should now be visible

## Common Issues

### Issue: "has no collision component"
**Cause**: PlayerSprite collision not enabled
**Fix**: Call `_player.EnableCollision(width, height)` in your setup code

### Issue: Collision box wrong size/position
**Cause**: Default collision size doesn't match sprite
**Fix**: Adjust the width, height, and offset parameters:
```csharp
// For a 32x32 sprite with collision slightly smaller:
_player.EnableCollision(24, 28, new Vector2(4, 4));
```

### Issue: Still no collision after EnableCollision
**Cause**: Possible library reference issue
**Fix**: Make sure you rebuilt your game project after updating the library