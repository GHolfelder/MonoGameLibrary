# Utilities Namespace

The Utilities namespace provides common helper classes, interfaces, and enumerations used throughout the MonoGame Library.

## Core Components

### Logging System
- **[ILogger](ILogger.cs)** - Logging interface with multiple implementations
  - `ConsoleLogger` - Writes log messages to console
  - `NullLogger` - No-op logger for production builds

### Enumerations
- **[Direction](Direction.cs)** - Direction enumerations for movement and orientation

## Features

### Flexible Logging System
- **Interface-Based**: `ILogger` allows for different logging implementations
- **Multiple Implementations**: Console logging for debugging, null logger for release builds
- **Consistent API**: Standard logging levels (Info, Warning, Error, Debug)
- **Easy Integration**: Can be injected into any class that needs logging

### Direction System
- **Comprehensive Direction Support**: Four-way and eight-way movement
- **Type Safety**: Enum-based directions prevent invalid values
- **Game-Friendly**: Designed for common game movement patterns

## Usage Examples

### Logging System

#### Basic Logging Usage
```csharp
public class GameManager
{
    private readonly ILogger _logger;
    
    public GameManager(ILogger logger)
    {
        _logger = logger;
    }
    
    public void StartGame()
    {
        _logger.LogInfo("Game started");
        
        try
        {
            InitializeGame();
            _logger.LogInfo("Game initialization complete");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to initialize game: {ex.Message}");
            throw;
        }
    }
    
    public void LoadLevel(string levelName)
    {
        _logger.LogInfo($"Loading level: {levelName}");
        
        if (string.IsNullOrEmpty(levelName))
        {
            _logger.LogWarning("Level name is empty, loading default level");
            levelName = "default";
        }
        
        _logger.LogDebug($"Level loading completed for: {levelName}");
    }
}
```

#### Logger Implementations
```csharp
// Console logger for development
ILogger consoleLogger = new ConsoleLogger();
consoleLogger.LogInfo("This will appear in console");
consoleLogger.LogError("Error messages are displayed in red");

// Null logger for production (no output)
ILogger nullLogger = new NullLogger();
nullLogger.LogInfo("This message will be ignored");

// Usage in dependency injection or factory pattern
public static ILogger CreateLogger(bool debugMode)
{
    return debugMode ? new ConsoleLogger() : new NullLogger();
}
```

#### Integration with Scenes
```csharp
public abstract class Scene : IDisposable
{
    protected readonly ILogger Logger;
    
    protected Scene(ILogger logger = null)
    {
        Logger = logger ?? new NullLogger();
    }
    
    public virtual void Initialize()
    {
        Logger.LogInfo($"Initializing scene: {GetType().Name}");
        LoadContent();
        Logger.LogInfo($"Scene initialization complete: {GetType().Name}");
    }
    
    protected virtual void LoadContent()
    {
        Logger.LogDebug("Loading content...");
    }
}

// Usage
public class GameplayScene : Scene
{
    public GameplayScene() : base(new ConsoleLogger())
    {
    }
    
    protected override void LoadContent()
    {
        Logger.LogInfo("Loading gameplay assets");
        
        try
        {
            var playerTexture = Content.Load<Texture2D>("player");
            Logger.LogDebug("Player texture loaded successfully");
        }
        catch (ContentLoadException ex)
        {
            Logger.LogError($"Failed to load player texture: {ex.Message}");
        }
    }
}
```

### Direction System

#### Basic Direction Usage
```csharp
public class MovementController
{
    public void MoveCharacter(Direction direction, float speed, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 velocity = GetVelocityFromDirection(direction) * speed * deltaTime;
        
        // Apply movement
        character.Position += velocity;
        
        // Update sprite facing
        character.Direction = direction;
    }
    
    private Vector2 GetVelocityFromDirection(Direction direction)
    {
        return direction switch
        {
            Direction.North => new Vector2(0, -1),
            Direction.South => new Vector2(0, 1),
            Direction.East => new Vector2(1, 0),
            Direction.West => new Vector2(-1, 0),
            Direction.NorthEast => new Vector2(1, -1).Normalized(),
            Direction.NorthWest => new Vector2(-1, -1).Normalized(),
            Direction.SouthEast => new Vector2(1, 1).Normalized(),
            Direction.SouthWest => new Vector2(-1, 1).Normalized(),
            _ => Vector2.Zero
        };
    }
}
```

#### Input to Direction Conversion
```csharp
public Direction GetDirectionFromInput()
{
    var keyboard = Core.Input.Keyboard;
    bool up = keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up);
    bool down = keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down);
    bool left = keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left);
    bool right = keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right);
    
    // Eight-way movement
    if (up && right) return Direction.NorthEast;
    if (up && left) return Direction.NorthWest;
    if (down && right) return Direction.SouthEast;
    if (down && left) return Direction.SouthWest;
    
    // Four-way movement
    if (up) return Direction.North;
    if (down) return Direction.South;
    if (left) return Direction.West;
    if (right) return Direction.East;
    
    return Direction.South; // Default facing direction
}
```

#### Direction Utilities
```csharp
public static class DirectionExtensions
{
    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            Direction.NorthEast => Direction.SouthWest,
            Direction.NorthWest => Direction.SouthEast,
            Direction.SouthEast => Direction.NorthWest,
            Direction.SouthWest => Direction.NorthEast,
            _ => direction
        };
    }
    
    public static bool IsCardinal(this Direction direction)
    {
        return direction == Direction.North || 
               direction == Direction.South || 
               direction == Direction.East || 
               direction == Direction.West;
    }
    
    public static bool IsDiagonal(this Direction direction)
    {
        return direction == Direction.NorthEast || 
               direction == Direction.NorthWest || 
               direction == Direction.SouthEast || 
               direction == Direction.SouthWest;
    }
    
    public static float ToAngle(this Direction direction)
    {
        return direction switch
        {
            Direction.North => MathF.PI * 1.5f,
            Direction.NorthEast => MathF.PI * 1.75f,
            Direction.East => 0f,
            Direction.SouthEast => MathF.PI * 0.25f,
            Direction.South => MathF.PI * 0.5f,
            Direction.SouthWest => MathF.PI * 0.75f,
            Direction.West => MathF.PI,
            Direction.NorthWest => MathF.PI * 1.25f,
            _ => 0f
        };
    }
}
```

## Integration Examples

### Custom Logger Implementation
```csharp
public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly object _lock = new object();
    
    public FileLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }
    
    public void LogInfo(string message)
    {
        WriteToFile("INFO", message);
    }
    
    public void LogWarning(string message)
    {
        WriteToFile("WARN", message);
    }
    
    public void LogError(string message)
    {
        WriteToFile("ERROR", message);
    }
    
    public void LogDebug(string message)
    {
        WriteToFile("DEBUG", message);
    }
    
    private void WriteToFile(string level, string message)
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logEntry = $"[{timestamp}] [{level}] {message}";
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
    }
}
```

### Direction-Based Animation System
```csharp
public class DirectionalAnimationManager
{
    private readonly Dictionary<Direction, string> _animationMappings;
    
    public DirectionalAnimationManager()
    {
        _animationMappings = new Dictionary<Direction, string>
        {
            { Direction.North, "walk_up" },
            { Direction.South, "walk_down" },
            { Direction.East, "walk_right" },
            { Direction.West, "walk_left" },
            { Direction.NorthEast, "walk_up_right" },
            { Direction.NorthWest, "walk_up_left" },
            { Direction.SouthEast, "walk_down_right" },
            { Direction.SouthWest, "walk_down_left" }
        };
    }
    
    public string GetAnimationName(Direction direction, AnimationState state)
    {
        string directionSuffix = _animationMappings.GetValueOrDefault(direction, "walk_down");
        
        return state switch
        {
            AnimationState.Idle => directionSuffix.Replace("walk_", "idle_"),
            AnimationState.Walk => directionSuffix,
            AnimationState.Run => directionSuffix.Replace("walk_", "run_"),
            AnimationState.Attack => directionSuffix.Replace("walk_", "attack_"),
            _ => directionSuffix
        };
    }
}
```

## Design Patterns

### Strategy Pattern (Logging)
The `ILogger` interface uses the Strategy pattern to allow different logging behaviors:
- Swap logging implementations at runtime
- Configure logging based on build type
- Easy to test with mock loggers

### Value Objects (Direction)
Direction enum acts as a value object:
- Immutable and type-safe
- Can be extended with utility methods
- Clear, expressive code

## Best Practices

### Logging Guidelines
```csharp
// DO: Use appropriate log levels
logger.LogInfo("User logged in"); // Important events
logger.LogDebug("Updating position to (10, 20)"); // Detailed debugging
logger.LogWarning("Config file not found, using defaults"); // Potential issues
logger.LogError("Failed to save game"); // Actual problems

// DON'T: Log sensitive information
logger.LogInfo($"User password: {password}"); // Security risk

// DO: Include context in error messages
logger.LogError($"Failed to load texture '{textureName}': {ex.Message}");

// DON'T: Log in tight loops without throttling
for (int i = 0; i < 1000000; i++)
{
    logger.LogDebug($"Processing item {i}"); // Performance issue
}
```

### Direction Usage
```csharp
// DO: Use Direction enum for type safety
public void MoveCharacter(Direction direction) { }

// DON'T: Use magic numbers or strings
public void MoveCharacter(int direction) { } // Unclear what values are valid
public void MoveCharacter(string direction) { } // Error-prone
```

The Utilities namespace provides essential building blocks that promote clean, maintainable code throughout the MonoGame Library while following established design patterns and best practices.