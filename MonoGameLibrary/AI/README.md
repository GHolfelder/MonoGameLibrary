# AI Namespace

The AI namespace provides interfaces and base classes for implementing artificial intelligence behaviors in games.

## Core Interfaces

### Behavior System
- **[AIBehavior](AIBehavior.cs)** - Interface for implementing AI decision-making behaviors

## Key Features

### Behavior Interface
- **IAIBehavior**: Defines standard interface for AI components
- **Flexible Implementation**: Allows for various AI approaches (state machines, behavior trees, utility AI)
- **Game Integration**: Designed to work with MonoGame's update loop
- **Extensible**: Easy to implement custom AI behaviors

## Interface Definition

### IAIBehavior Interface
```csharp
public interface IAIBehavior
{
    void Update(GameTime gameTime);
}
```

## Usage Examples

### Simple Enemy AI
```csharp
public class SimpleEnemyAI : IAIBehavior
{
    private readonly NPCSprite _enemy;
    private readonly PlayerSprite _player;
    private float _detectionRange = 100f;
    private float _speed = 50f;
    
    public SimpleEnemyAI(NPCSprite enemy, PlayerSprite player)
    {
        _enemy = enemy;
        _player = player;
    }
    
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Calculate distance to player
        Vector2 direction = _player.Position - _enemy.Position;
        float distance = direction.Length();
        
        if (distance <= _detectionRange)
        {
            // Move towards player
            direction.Normalize();
            _enemy.Position += direction * _speed * deltaTime;
            
            // Update sprite facing direction
            if (direction.X > 0)
                _enemy.Direction = Direction.East;
            else if (direction.X < 0)
                _enemy.Direction = Direction.West;
                
            _enemy.State = AnimationState.Walk;
        }
        else
        {
            _enemy.State = AnimationState.Idle;
        }
    }
}
```

### Patrol AI Behavior
```csharp
public class PatrolAI : IAIBehavior
{
    private readonly NPCSprite _guard;
    private readonly Vector2[] _patrolPoints;
    private int _currentTargetIndex = 0;
    private float _speed = 30f;
    private float _reachThreshold = 5f;
    
    public PatrolAI(NPCSprite guard, Vector2[] patrolPoints)
    {
        _guard = guard;
        _patrolPoints = patrolPoints;
    }
    
    public void Update(GameTime gameTime)
    {
        if (_patrolPoints.Length == 0) return;
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 targetPoint = _patrolPoints[_currentTargetIndex];
        
        // Calculate direction to current patrol point
        Vector2 direction = targetPoint - _guard.Position;
        float distance = direction.Length();
        
        if (distance <= _reachThreshold)
        {
            // Reached patrol point, move to next
            _currentTargetIndex = (_currentTargetIndex + 1) % _patrolPoints.Length;
            _guard.State = AnimationState.Idle;
        }
        else
        {
            // Move towards patrol point
            direction.Normalize();
            _guard.Position += direction * _speed * deltaTime;
            
            // Update facing direction
            UpdateFacingDirection(direction);
            _guard.State = AnimationState.Walk;
        }
    }
    
    private void UpdateFacingDirection(Vector2 direction)
    {
        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
        {
            _guard.Direction = direction.X > 0 ? Direction.East : Direction.West;
        }
        else
        {
            _guard.Direction = direction.Y > 0 ? Direction.South : Direction.North;
        }
    }
}
```

### State Machine AI
```csharp
public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Return
}

public class StateMachineAI : IAIBehavior
{
    private readonly NPCSprite _entity;
    private readonly PlayerSprite _player;
    private AIState _currentState = AIState.Idle;
    
    // State parameters
    private Vector2[] _patrolPoints;
    private int _patrolIndex = 0;
    private Vector2 _homePosition;
    private float _detectionRange = 80f;
    private float _attackRange = 30f;
    private float _loseTargetRange = 120f;
    
    public StateMachineAI(NPCSprite entity, PlayerSprite player, Vector2[] patrolPoints)
    {
        _entity = entity;
        _player = player;
        _patrolPoints = patrolPoints;
        _homePosition = entity.Position;
    }
    
    public void Update(GameTime gameTime)
    {
        float distanceToPlayer = Vector2.Distance(_entity.Position, _player.Position);
        
        // State transitions
        switch (_currentState)
        {
            case AIState.Idle:
            case AIState.Patrol:
                if (distanceToPlayer <= _detectionRange)
                    ChangeState(AIState.Chase);
                break;
                
            case AIState.Chase:
                if (distanceToPlayer <= _attackRange)
                    ChangeState(AIState.Attack);
                else if (distanceToPlayer >= _loseTargetRange)
                    ChangeState(AIState.Return);
                break;
                
            case AIState.Attack:
                if (distanceToPlayer > _attackRange)
                    ChangeState(AIState.Chase);
                break;
                
            case AIState.Return:
                if (Vector2.Distance(_entity.Position, _homePosition) < 10f)
                    ChangeState(AIState.Patrol);
                break;
        }
        
        // State behaviors
        ExecuteCurrentState(gameTime);
    }
    
    private void ChangeState(AIState newState)
    {
        _currentState = newState;
        
        // State entry logic
        switch (newState)
        {
            case AIState.Attack:
                _entity.State = AnimationState.Attack;
                break;
            case AIState.Return:
                // Return to patrol starting point
                _patrolIndex = 0;
                break;
        }
    }
    
    private void ExecuteCurrentState(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        switch (_currentState)
        {
            case AIState.Idle:
                _entity.State = AnimationState.Idle;
                break;
                
            case AIState.Patrol:
                ExecutePatrol(deltaTime);
                break;
                
            case AIState.Chase:
                ExecuteChase(deltaTime);
                break;
                
            case AIState.Attack:
                // Attack behavior handled by animation state
                break;
                
            case AIState.Return:
                ExecuteReturn(deltaTime);
                break;
        }
    }
    
    private void ExecutePatrol(float deltaTime)
    {
        if (_patrolPoints.Length == 0) return;
        
        Vector2 target = _patrolPoints[_patrolIndex];
        MoveTowards(target, 30f, deltaTime);
        
        if (Vector2.Distance(_entity.Position, target) < 5f)
            _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
    }
    
    private void ExecuteChase(float deltaTime)
    {
        MoveTowards(_player.Position, 60f, deltaTime);
    }
    
    private void ExecuteReturn(float deltaTime)
    {
        MoveTowards(_homePosition, 50f, deltaTime);
    }
    
    private void MoveTowards(Vector2 target, float speed, float deltaTime)
    {
        Vector2 direction = target - _entity.Position;
        
        if (direction.Length() > 0)
        {
            direction.Normalize();
            _entity.Position += direction * speed * deltaTime;
            
            // Update facing direction
            UpdateFacingDirection(direction);
            _entity.State = AnimationState.Walk;
        }
        else
        {
            _entity.State = AnimationState.Idle;
        }
    }
    
    private void UpdateFacingDirection(Vector2 direction)
    {
        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            _entity.Direction = direction.X > 0 ? Direction.East : Direction.West;
        else
            _entity.Direction = direction.Y > 0 ? Direction.South : Direction.North;
    }
}
```

### AI Behavior Composition
```csharp
public class CompositeAI : IAIBehavior
{
    private readonly List<IAIBehavior> _behaviors;
    private readonly NPCSprite _entity;
    
    public CompositeAI(NPCSprite entity)
    {
        _entity = entity;
        _behaviors = new List<IAIBehavior>();
    }
    
    public void AddBehavior(IAIBehavior behavior)
    {
        _behaviors.Add(behavior);
    }
    
    public void RemoveBehavior(IAIBehavior behavior)
    {
        _behaviors.Remove(behavior);
    }
    
    public void Update(GameTime gameTime)
    {
        // Execute all active behaviors
        foreach (var behavior in _behaviors)
        {
            behavior.Update(gameTime);
        }
    }
}

// Usage example
var compositeAI = new CompositeAI(enemy);
compositeAI.AddBehavior(new PatrolAI(enemy, patrolPoints));
compositeAI.AddBehavior(new AlertBehavior(enemy, alertSystem));
compositeAI.AddBehavior(new HealthMonitorBehavior(enemy));
```

## Integration with Game Systems

### With Character Sprites
```csharp
public class GameplayScene : Scene
{
    private PlayerSprite _player;
    private NPCSprite _enemy;
    private IAIBehavior _enemyAI;
    
    public override void LoadContent()
    {
        var atlas = TextureAtlas.FromXml(Content, "characters.xml");
        
        _player = new PlayerSprite(atlas, "player", DirectionMode.FourWay,
                                  AnimationState.Idle, AnimationState.Walk);
        
        _enemy = new NPCSprite(atlas, "enemy", DirectionMode.FourWay,
                              AnimationState.Idle, AnimationState.Walk);
        
        // Set up AI
        _enemyAI = new SimpleEnemyAI(_enemy, _player);
    }
    
    public override void Update(GameTime gameTime)
    {
        _player.Update(gameTime);
        _enemy.Update(gameTime);
        
        // Update AI behavior
        _enemyAI.Update(gameTime);
    }
}
```

### Multiple AI Entities
```csharp
public class AIManager
{
    private readonly List<(NPCSprite sprite, IAIBehavior behavior)> _aiEntities;
    
    public AIManager()
    {
        _aiEntities = new List<(NPCSprite, IAIBehavior)>();
    }
    
    public void AddAIEntity(NPCSprite sprite, IAIBehavior behavior)
    {
        _aiEntities.Add((sprite, behavior));
    }
    
    public void Update(GameTime gameTime)
    {
        foreach (var (sprite, behavior) in _aiEntities)
        {
            sprite.Update(gameTime);
            behavior.Update(gameTime);
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var (sprite, _) in _aiEntities)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
```

## Design Patterns

### Strategy Pattern
The IAIBehavior interface implements the Strategy pattern, allowing you to:
- Swap AI behaviors at runtime
- Create different difficulty levels
- Implement context-sensitive behaviors
- Test different AI approaches

### Composition over Inheritance
- Use composition to combine multiple behaviors
- Create complex AI from simple, reusable components
- Maintain flexibility and testability

## Best Practices

### Performance Considerations
- Update AI behaviors at appropriate frequencies (not every frame for complex AI)
- Use spatial partitioning for detection ranges
- Pool expensive calculations

### Code Organization
- Keep behavior classes focused on single responsibilities
- Use data-driven approaches for parameters
- Implement debug visualization for AI states

The AI namespace provides a flexible foundation for implementing game AI while integrating seamlessly with the MonoGame Library's sprite and scene systems.