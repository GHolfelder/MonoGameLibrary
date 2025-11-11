using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// NPC character sprite that uses AI behavior for movement and animation
/// </summary>
public class NPCSprite : CharacterSprite
{
    private IAIBehavior _aiBehavior;
    private float _movementSpeed = 80f; // pixels per second (slightly slower than player)
    private Vector2 _velocity = Vector2.Zero;
    private bool _isMoving = false;

    /// <summary>
    /// Gets or sets the AI behavior for this NPC
    /// </summary>
    public IAIBehavior AIBehavior
    {
        get => _aiBehavior;
        set => _aiBehavior = value;
    }

    /// <summary>
    /// Gets or sets the movement speed in pixels per second
    /// </summary>
    public float MovementSpeed
    {
        get => _movementSpeed;
        set => _movementSpeed = value;
    }

    /// <summary>
    /// Gets whether the NPC is currently moving
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// Gets the current velocity of the NPC
    /// </summary>
    public Vector2 Velocity => _velocity;

    /// <summary>
    /// Creates a new NPCSprite
    /// </summary>
    /// <param name="atlas">The texture atlas containing the animations</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "npc")</param>
    /// <param name="aiBehavior">The AI behavior for this NPC</param>
    /// <param name="use8WayDirections">Whether to use 8-way or 4-way directional animations</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public NPCSprite(TextureAtlas atlas, string characterPrefix, IAIBehavior aiBehavior,
        bool use8WayDirections = true, params AnimationState[] supportedStates)
        : base(atlas, characterPrefix, use8WayDirections, supportedStates)
    {
        _aiBehavior = aiBehavior;
    }

    /// <summary>
    /// Updates the NPC sprite using AI behavior
    /// </summary>
    public override void Update(GameTime gameTime)
    {
        if (_aiBehavior != null)
        {
            // Get AI decision
            var (direction, state) = _aiBehavior.Update(gameTime, Position, _currentState);

            // Apply AI decision
            if (direction.HasValue)
            {
                UpdateMovement(gameTime, direction.Value);
                SetDirection(direction.Value);
                _isMoving = true;
            }
            else
            {
                _velocity = Vector2.Zero;
                _isMoving = false;
            }

            // Set animation state (placeholder will be shown if it doesn't exist)
            SetAnimationState(state);
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// Updates movement based on the AI direction
    /// </summary>
    protected virtual void UpdateMovement(GameTime gameTime, Direction8 direction)
    {
        // Convert direction to movement vector
        Vector2 movementVector = GetMovementVectorFromDirection(direction);

        // Calculate velocity
        _velocity = movementVector * _movementSpeed;

        // Update position
        Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>
    /// Converts a Direction8 to a normalized movement vector
    /// </summary>
    protected virtual Vector2 GetMovementVectorFromDirection(Direction8 direction)
    {
        return direction switch
        {
            Direction8.North => new Vector2(0, -1),
            Direction8.Northeast => new Vector2(1, -1).Normalized(),
            Direction8.East => new Vector2(1, 0),
            Direction8.Southeast => new Vector2(1, 1).Normalized(),
            Direction8.South => new Vector2(0, 1),
            Direction8.Southwest => new Vector2(-1, 1).Normalized(),
            Direction8.West => new Vector2(-1, 0),
            Direction8.Northwest => new Vector2(-1, -1).Normalized(),
            _ => Vector2.Zero
        };
    }

    /// <summary>
    /// Sets a new AI behavior for this NPC
    /// </summary>
    /// <param name="behavior">The new AI behavior</param>
    public void SetAIBehavior(IAIBehavior behavior)
    {
        _aiBehavior = behavior;
    }

    /// <summary>
    /// Temporarily stops the NPC by removing its AI behavior
    /// </summary>
    public void Stop()
    {
        _velocity = Vector2.Zero;
        _isMoving = false;
        SetAnimationState(AnimationState.Idle);
    }

    /// <summary>
    /// Resumes the NPC by restoring its AI behavior
    /// </summary>
    /// <param name="behavior">The AI behavior to resume with</param>
    public void Resume(IAIBehavior behavior)
    {
        _aiBehavior = behavior;
    }
}

/// <summary>
/// Extension method to normalize Vector2 (if not available in MonoGame version)
/// </summary>
public static class Vector2Extensions
{
    public static Vector2 Normalized(this Vector2 vector)
    {
        if (vector.LengthSquared() > 0)
        {
            vector.Normalize();
        }
        return vector;
    }
}