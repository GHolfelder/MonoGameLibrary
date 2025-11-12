using System;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace MonoGameLibrary.AI;

/// <summary>
/// Interface for AI behavior that can be applied to NPCs
/// </summary>
public interface IAIBehavior
{
    /// <summary>
    /// Updates the AI behavior and returns the desired movement direction and animation state
    /// </summary>
    /// <param name="gameTime">Current game time</param>
    /// <param name="currentPosition">Current position of the character</param>
    /// <param name="currentState">Current animation state</param>
    /// <returns>Tuple containing the desired direction and animation state</returns>
    (Direction8? direction, string state) Update(GameTime gameTime,
        Vector2 currentPosition, string currentState);
}

/// <summary>
/// Simple AI behavior that makes an NPC wander randomly
/// </summary>
public class WanderBehavior : IAIBehavior
{
    private readonly Random _random = new();
    private double _nextActionTime;
    private Direction8? _currentDirection;
    private readonly float _changeDirectionInterval = 2.0f; // Change direction every 2 seconds

    public (Direction8? direction, string state) Update(GameTime gameTime,
        Vector2 currentPosition, string currentState)
    {
        if (gameTime.TotalGameTime.TotalSeconds >= _nextActionTime)
        {
            // 30% chance to stop, 70% chance to move in a random direction
            if (_random.NextDouble() < 0.3)
            {
                _currentDirection = null;
            }
            else
            {
                _currentDirection = (Direction8)_random.Next(8);
            }
            
            _nextActionTime = gameTime.TotalGameTime.TotalSeconds + _changeDirectionInterval;
        }

        string newState = _currentDirection.HasValue ? AnimationState.Walk : AnimationState.Idle;
        return (_currentDirection, newState);
    }
}
