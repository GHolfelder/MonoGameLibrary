using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Utilities;
using MonoGameLibrary.Graphics.Tiles;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Player-controlled character sprite that handles keyboard and gamepad input
/// </summary>
public class PlayerSprite : CharacterSprite
{
    private readonly IInputProvider _inputProvider;
    private float _movementSpeed = 100f; // pixels per second (tuning knob)
    private float _sprintMultiplier = 1.5f; // tuning knob
    private bool _isMoving = false;
    private Tilemap _currentTilemap;
    private Vector2 _tilemapPosition = Vector2.Zero;
    private string _collisionLayerName = "Collision_Static";

    /// <summary>
    /// Gets or sets the movement speed in pixels per second
    /// </summary>
    public float MovementSpeed 
    { 
        get => _movementSpeed; 
        set => _movementSpeed = value; 
    }

    /// <summary>
    /// Gets or sets the sprint speed multiplier
    /// </summary>
    public float SprintMultiplier 
    { 
        get => _sprintMultiplier; 
        set => _sprintMultiplier = value; 
    }

    /// <summary>
    /// Gets whether the player is currently moving
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// Gets or sets the name of the collision layer to check against.
    /// Default is "Collision_Static".
    /// </summary>
    public string CollisionLayerName
    {
        get => _collisionLayerName;
        set => _collisionLayerName = value ?? "Collision_Static";
    }

    /// <summary>
    /// Gets or sets the current tilemap for collision detection
    /// </summary>
    public Tilemap CurrentTilemap
    {
        get => _currentTilemap;
        set => _currentTilemap = value;
    }

    /// <summary>
    /// Gets or sets the tilemap's world position for collision calculations
    /// </summary>
    public Vector2 TilemapPosition
    {
        get => _tilemapPosition;
        set => _tilemapPosition = value;
    }

    /// <summary>
    /// Creates a new PlayerSprite
    /// </summary>
    /// <param name="atlas">The texture atlas containing the animations</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "player")</param>
    /// <param name="directionMode">Direction mode (FourWay or EightWay)</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public PlayerSprite(TextureAtlas atlas, string characterPrefix, DirectionMode directionMode = DirectionMode.EightWay, 
        IInputProvider inputProvider = null, params string[] supportedStates) 
        : base(atlas, characterPrefix, directionMode, supportedStates)
    {
        _inputProvider = inputProvider ?? new CoreInputProvider();
    }

    /// <summary>
    /// Updates the player sprite, handling input and movement
    /// </summary>
    public override void Update(GameTime gameTime)
    {
        HandleInput(gameTime);
        base.Update(gameTime);
    }

    /// <summary>
    /// Handles keyboard and gamepad input for movement
    /// </summary>
    protected virtual void HandleInput(GameTime gameTime)
    {
        var (movementVector, isSprinting) = _inputProvider.GetMovement(gameTime);

        // Apply movement & animation selection
        if (movementVector.LengthSquared() > 0)
        {
            // Normalize diagonal movement
            // (Already normalized by provider, but safe if custom impl skipped it)
            if (movementVector.LengthSquared() > 1.0001f)
                movementVector.Normalize();
            
            // Calculate movement speed
            float currentSpeed = _movementSpeed;
            if (isSprinting)
                currentSpeed *= _sprintMultiplier;
            
            // Calculate potential new position
            Vector2 deltaMovement = movementVector * currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 newPosition = Position + deltaMovement;
            
            // Check for collision before moving
            if (CanMoveTo(newPosition))
            {
                // Safe to move - update position
                Position = newPosition;
            }
            else
            {
                // Collision detected - try moving along each axis separately
                Vector2 horizontalPosition = Position + new Vector2(deltaMovement.X, 0);
                Vector2 verticalPosition = Position + new Vector2(0, deltaMovement.Y);
                
                if (CanMoveTo(horizontalPosition))
                {
                    Position = horizontalPosition; // Can slide horizontally
                }
                else if (CanMoveTo(verticalPosition))
                {
                    Position = verticalPosition; // Can slide vertically
                }
                // If both fail, player stays in current position
            }
            
            // Update direction and animation state
            UpdateDirectionFromMovement(movementVector);
            
            // Use Run animation if sprinting, otherwise Walk
            if (isSprinting)
            {
                SetAnimationState(AnimationState.Run);
            }
            else
            {
                SetAnimationState(AnimationState.Walk);
            }
            
            _isMoving = true;
        }
        else
        {
            // Not moving - use Idle animation
            SetAnimationState(AnimationState.Idle);
            
            _isMoving = false;
        }
    }

    /// <summary>
    /// Updates the facing direction based on movement vector
    /// </summary>
    protected virtual void UpdateDirectionFromMovement(Vector2 movementVector)
    {
        // Calculate 8-way direction from movement vector
        float angle = (float)Math.Atan2(movementVector.Y, movementVector.X);
        
        // Convert angle to direction (adjust for screen coordinates where Y+ is down)
        Direction8 direction = GetDirection8FromAngle(angle);
        SetDirection(direction);
    }

    /// <summary>
    /// Converts an angle in radians to a Direction8
    /// </summary>
    private Direction8 GetDirection8FromAngle(float angle)
    {
        // Convert angle to degrees and normalize to 0-360
        float degrees = MathHelper.ToDegrees(angle);
        if (degrees < 0) degrees += 360;

        // Map to 8 directions (each direction covers 45 degrees)
        // East = 0°, Southeast = 45°, South = 90°, etc.
        return (degrees + 22.5f) switch
        {
            >= 0 and < 45 => Direction8.East,
            >= 45 and < 90 => Direction8.Southeast,
            >= 90 and < 135 => Direction8.South,
            >= 135 and < 180 => Direction8.Southwest,
            >= 180 and < 225 => Direction8.West,
            >= 225 and < 270 => Direction8.Northwest,
            >= 270 and < 315 => Direction8.North,
            >= 315 and < 360 => Direction8.Northeast,
            _ => Direction8.East
        };
    }

    /// <summary>
    /// Checks if the player can move to the specified position without colliding with static obstacles
    /// </summary>
    /// <param name="newPosition">The position to check for collision</param>
    /// <returns>True if the position is safe to move to, false if there would be a collision</returns>
    protected virtual bool CanMoveTo(Vector2 newPosition)
    {
        // If no tilemap is set, allow movement (no collision detection)
        if (_currentTilemap == null)
            return true;

        // If no collision component is enabled, allow movement
        if (Collision == null)
            return true;

        // Check collision against tile collision objects
        if (_currentTilemap.CheckCharacterSpriteTileCollision(this, newPosition, _tilemapPosition))
            return false;

        // Check collision against the specified object layer
        return !_currentTilemap.CheckCharacterSpriteObjectCollision(this, newPosition, _collisionLayerName, _tilemapPosition);
    }

    /// <summary>
    /// Sets the current tilemap and its world position for collision detection
    /// </summary>
    /// <param name="tilemap">The tilemap to use for collision detection</param>
    /// <param name="tilemapWorldPosition">The world position of the tilemap</param>
    public void SetTilemap(Tilemap tilemap, Vector2 tilemapWorldPosition = default)
    {
        _currentTilemap = tilemap;
        _tilemapPosition = tilemapWorldPosition;
    }
}