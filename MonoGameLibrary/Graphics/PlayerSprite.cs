using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;
using MonoGameLibrary.Utilities;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Player-controlled character sprite that handles keyboard and gamepad input
/// </summary>
public class PlayerSprite : CharacterSprite
{
    private float _movementSpeed = 100f; // pixels per second
    private float _sprintMultiplier = 1.5f;
    private bool _isMoving = false;

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
    /// Creates a new PlayerSprite
    /// </summary>
    /// <param name="atlas">The texture atlas containing the animations</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "player")</param>
    /// <param name="directionMode">Direction mode (FourWay or EightWay)</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public PlayerSprite(TextureAtlas atlas, string characterPrefix, DirectionMode directionMode = DirectionMode.EightWay, 
        params string[] supportedStates) 
        : base(atlas, characterPrefix, directionMode, supportedStates)
    {
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
        var keyboard = Core.Input.Keyboard;
        var gamePad = Core.Input.GamePads[(int)PlayerIndex.One];
        
        Vector2 movementVector = Vector2.Zero;
        bool isSprinting = false;

        // Handle keyboard input
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
            movementVector.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
            movementVector.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
            movementVector.X -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
            movementVector.X += 1;

        // Check for sprint input
        isSprinting = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.Space);

        // Handle gamepad input (takes priority over keyboard if connected)
        if (gamePad.IsConnected)
        {
            Vector2 thumbstick = gamePad.LeftThumbStick;
            if (thumbstick.LengthSquared() > 0.1f) // Dead zone
            {
                movementVector = new Vector2(thumbstick.X, -thumbstick.Y); // Invert Y for screen coordinates
            }
            
            // Gamepad sprint button
            if (gamePad.IsButtonDown(Buttons.A) || gamePad.IsButtonDown(Buttons.X))
                isSprinting = true;
        }

        // Apply movement
        if (movementVector.LengthSquared() > 0)
        {
            // Normalize diagonal movement
            movementVector.Normalize();
            
            // Calculate movement speed
            float currentSpeed = _movementSpeed;
            if (isSprinting)
                currentSpeed *= _sprintMultiplier;
            
            // Update position
            Vector2 deltaMovement = movementVector * currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += deltaMovement;
            
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
}