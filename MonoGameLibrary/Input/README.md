# Input Namespace

The Input namespace provides centralized input handling for keyboard, mouse, and gamepad input with state tracking and edge detection capabilities.

## Core Classes

### Input Management
- **[InputManager](InputManager.cs)** - Central input manager that coordinates keyboard, mouse, and gamepad input
- **[IInputProvider](IInputProvider.cs)** - Interface for abstracting input sources (keyboard, gamepad, AI, network)

### Device-Specific Input
- **[KeyboardInfo](KeyboardInfo.cs)** - Keyboard input state management with previous/current state tracking
- **[MouseInfo](MouseInfo.cs)** - Mouse input state management including position, buttons, and scroll wheel
- **[GamePadInfo](GamePadInfo.cs)** - Gamepad input state management with vibration support
- **[MouseButton](MouseButton.cs)** - Enumeration for mouse button types

## Key Features

### State Tracking Pattern
All input classes follow a **previous/current state pattern** for edge detection:
- `WasKeyJustPressed()` = `CurrentState.IsKeyDown() && PreviousState.IsKeyUp()`
- `WasKeyJustReleased()` = `CurrentState.IsKeyUp() && PreviousState.IsKeyDown()`
- Similar patterns for mouse buttons and gamepad buttons

### Edge Detection
- **Just Pressed**: Detect the exact frame when a button/key is pressed
- **Just Released**: Detect the exact frame when a button/key is released
- **Current State**: Check if input is currently active
- **Previous State**: Access to previous frame's input state

### Gamepad Features
- **Multiple Controllers**: Support for up to 4 gamepads
- **Connection Detection**: Check if gamepad is connected
- **Analog Input**: Access to thumbsticks and triggers
- **Vibration**: Timed vibration with automatic shutoff

## Usage Examples

### Basic Input Setup
```csharp
// In your game's Initialize method
var inputManager = new InputManager();

// In your game's Update method
inputManager.Update(gameTime);
```

### Keyboard Input
```csharp
// Check current state
if (inputManager.Keyboard.IsKeyDown(Keys.Space))
{
    // Space is currently pressed
}

// Check for edge events
if (inputManager.Keyboard.WasKeyJustPressed(Keys.Enter))
{
    // Enter was just pressed this frame
}

if (inputManager.Keyboard.WasKeyJustReleased(Keys.Escape))
{
    // Escape was just released this frame
}
```

### Mouse Input
```csharp
// Position and movement
var mousePos = inputManager.Mouse.Position;
var mouseDelta = inputManager.Mouse.PositionDelta;

// Button states
if (inputManager.Mouse.IsButtonDown(MouseButton.Left))
{
    // Left mouse button is pressed
}

if (inputManager.Mouse.WasButtonJustPressed(MouseButton.Right))
{
    // Right mouse button was just clicked
}

// Scroll wheel
var scrollDelta = inputManager.Mouse.ScrollWheelDelta;
```

### Gamepad Input
```csharp
var gamepad = inputManager.GamePads[0]; // Player 1

if (gamepad.IsConnected)
{
    // Buttons
    if (gamepad.WasButtonJustPressed(Buttons.A))
    {
        // A button was just pressed
    }
    
    // Analog input
    var leftStick = gamepad.LeftThumbStick;
    var rightTrigger = gamepad.RightTrigger;
    
    // Vibration
    gamepad.SetVibration(0.5f, TimeSpan.FromSeconds(1));
}
```

### Input Provider Pattern
```csharp
// Create a custom input provider
public class CoreInputProvider : IInputProvider
{
    public (Vector2 movement, bool sprint) GetMovement(GameTime gameTime)
    {
        var keyboard = Core.Input.Keyboard;
        var gamepad = Core.Input.GamePads[0];
        
        Vector2 movement = Vector2.Zero;
        bool sprint = false;
        
        // Keyboard movement
        if (keyboard.IsKeyDown(Keys.W)) movement.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S)) movement.Y += 1;
        if (keyboard.IsKeyDown(Keys.A)) movement.X -= 1;
        if (keyboard.IsKeyDown(Keys.D)) movement.X += 1;
        
        // Sprint detection
        sprint = keyboard.IsKeyDown(Keys.LeftShift);
        
        // Gamepad override
        if (gamepad.IsConnected && gamepad.LeftThumbStick.LengthSquared() > 0.1f)
        {
            movement = new Vector2(gamepad.LeftThumbStick.X, -gamepad.LeftThumbStick.Y);
            sprint = gamepad.IsButtonDown(Buttons.A);
        }
        
        if (movement.LengthSquared() > 1e-5f)
            movement.Normalize();
            
        return (movement, sprint);
    }
}
```

## Input Management Best Practices

### Update Order
Always call `InputManager.Update(gameTime)` before processing any input in your game loop:

```csharp
protected override void Update(GameTime gameTime)
{
    inputManager.Update(gameTime);
    
    // Now process input
    HandleInput(gameTime);
    
    base.Update(gameTime);
}
```

### Edge Detection Usage
Use edge detection for discrete actions like menu navigation, jumping, or firing:

```csharp
// Good for discrete actions
if (input.WasKeyJustPressed(Keys.Space))
    player.Jump();

// Good for continuous actions  
if (input.IsKeyDown(Keys.W))
    player.MoveForward();
```

### Gamepad Vibration
Remember to manage vibration duration to avoid infinite vibration:

```csharp
// Set vibration with automatic timeout
gamepad.SetVibration(0.8f, TimeSpan.FromMilliseconds(200));

// Or manually stop
gamepad.StopVibration();
```

## Architecture Notes

- **Previous/Current State Pattern**: Enables reliable edge detection across frame boundaries
- **Input Abstraction**: `IInputProvider` allows for AI, network, or replay input sources
- **Centralized Management**: Single `InputManager` coordinates all input devices
- **MonoGame Integration**: Wraps MonoGame's input APIs with higher-level functionality