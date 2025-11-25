# Developer Mode Troubleshooting Guide

## Quick Test Steps

1. **Verify Debug Build**: Make sure you're running a Debug build, not Release
   ```
   dotnet build --configuration Debug
   ```

2. **Check Console Output**: Look for debug messages in the console/output window:
   - "F1 key detected - toggling developer mode"
   - "Developer Mode: True/False"
   - "Show Collision Boxes: True/False"

## Common Issues & Solutions

### Issue 1: No Visual Indicators Appearing
**Cause**: Scene doesn't call `base.Draw(gameTime)`

**Solution**: In your scene's Draw method, make sure to call the base class:
```csharp
public override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(Color.CornflowerBlue);
    
    Core.SpriteBatch.Begin();
    // Your scene drawing code here...
    Core.SpriteBatch.End();
    
    // Call base to automatically draw developer overlay
    base.Draw(gameTime);
}
```

**Note**: The base Scene class now handles SpriteBatch management for developer overlay automatically.

### Issue 2: F1/F2 Keys Not Working
**Cause**: Scene doesn't call `base.Update(gameTime)` or input isn't being updated

**Solutions**:
1. Make sure your scene calls base.Update:
```csharp
public override void Update(GameTime gameTime)
{
    base.Update(gameTime); // This handles F1/F2 keys
    
    // Your scene update code here...
}
```

2. Verify Core.Input is being updated in Core.Update()

### Issue 3: Release Build
**Cause**: Developer mode is disabled in Release builds (#if DEBUG)

**Solution**: Make sure you're running a Debug build

### Issue 4: CollisionDraw Not Initialized
**Cause**: CollisionDraw.Initialize() not called

**Solution**: Verify Core.Initialize() calls `CollisionDraw.Initialize(GraphicsDevice)`

## Manual Testing Code
Add this to your scene's Update method for manual testing:

```csharp
public override void Update(GameTime gameTime)
{
    base.Update(gameTime);
    
    // Manual toggle for testing (remove after testing)
    if (Core.Input.Keyboard.WasKeyJustPressed(Keys.T))
    {
        Core.ToggleDeveloperMode();
        System.Diagnostics.Debug.WriteLine($"Manual test - Dev Mode: {Core.DeveloperMode}");
    }
}
```

## Verification Checklist
- [ ] Running Debug build (not Release)
- [ ] Scene calls `base.Update(gameTime)`
- [ ] Scene calls `base.Draw(gameTime)` 
- [ ] Console shows debug messages when pressing F1/F2
- [ ] CollisionDraw.Initialize() was called in Core.Initialize()