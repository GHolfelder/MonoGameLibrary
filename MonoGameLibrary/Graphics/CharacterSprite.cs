using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Base class for character sprites that can have multiple animation states and directions
/// Displays placeholder sprite when animations are missing
/// </summary>
public class CharacterSprite
{
    protected readonly Dictionary<(AnimationState state, string direction), AnimatedSprite> _animations;
    protected readonly TextureAtlas _atlas;
    protected readonly bool _use8WayDirections;
    protected readonly Sprite _placeholderSprite;
    
    protected AnimationState _currentState;
    protected Direction8 _currentDirection;
    protected AnimatedSprite _currentAnimation;

    // Animation naming pattern - should be overridable by derived classes
    protected virtual string AnimationNamePattern => "{0}_{1}_{2}"; // e.g., "player_walk_S"

    // Debug property to force placeholder display
    public bool ForceShowPlaceholder { get; set; } = false;

    /// <summary>
    /// Gets or Sets the xy-coordinate position to render this character sprite at.
    /// </summary>
    public Vector2 Position { get; set; }
    
    /// <summary>
    /// Gets or Sets the color mask to apply when rendering this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Color.White
    /// </remarks>
    public Color Color { get; set; } = Color.White;
    
    /// <summary>
    /// Gets or Sets the amount of rotation, in radians, to apply when rendering this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float Rotation { get; set; } = 0.0f;
    
    /// <summary>
    /// Gets or Sets the scale factor to apply to the x- and y-axes when rendering this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.One
    /// </remarks>
    public Vector2 Scale { get; set; } = Vector2.One;
    
    private Vector2 _origin = Vector2.Zero;
    /// <summary>
    /// Gets or Sets the xy-coordinate origin point, relative to the top-left corner, of this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is Vector2.Zero
    /// </remarks>
    public Vector2 Origin 
    { 
        get => _origin;
        set
        {
            _origin = value;
            UpdateAnimationOrigin();
        }
    }
    
    /// <summary>
    /// Gets or Sets the sprite effects to apply when rendering this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is SpriteEffects.None
    /// </remarks>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    
    /// <summary>
    /// Gets or Sets the layer depth to apply when rendering this character sprite.
    /// </summary>
    /// <remarks>
    /// Default value is 0.0f
    /// </remarks>
    public float LayerDepth { get; set; } = 0.0f;

    public AnimationState CurrentState => _currentState;
    public Direction8 CurrentDirection => _currentDirection;
    public bool CurrentAnimationExists => _currentAnimation != null;

    /// <summary>
    /// Creates a new CharacterSprite
    /// </summary>
    /// <param name="atlas">The texture atlas containing the animations</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "player")</param>
    /// <param name="use8WayDirections">Whether to use 8-way or 4-way directional animations</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public CharacterSprite(TextureAtlas atlas, string characterPrefix, bool use8WayDirections = true, 
        params AnimationState[] supportedStates)
    {
        _atlas = atlas;
        _use8WayDirections = use8WayDirections;
        _animations = new Dictionary<(AnimationState, string), AnimatedSprite>();

        // Try to load placeholder sprite (single frame only)
        try
        {
            _placeholderSprite = _atlas.CreateSprite("placeholder");
        }
        catch
        {
            // Placeholder sprite doesn't exist - will be null
            _placeholderSprite = null;
        }

        // Load animations for all supported states and directions
        LoadAnimations(characterPrefix, supportedStates);
        
        // Set initial animation state
        SetAnimationState(FindBestInitialState(supportedStates));
    }

    /// <summary>
    /// Finds the best initial animation state that actually has an animation available
    /// </summary>
    protected virtual AnimationState FindBestInitialState(AnimationState[] supportedStates)
    {
        // Preferred order: Idle -> Walk -> Run -> Attack -> first available
        var preferredOrder = new[] { AnimationState.Idle, AnimationState.Walk, AnimationState.Run, AnimationState.Attack };
        
        string directionAbbr = DirectionHelper.GetDirectionAbbreviation(_currentDirection, _use8WayDirections);
        
        foreach (var preferredState in preferredOrder)
        {
            if (supportedStates.Contains(preferredState))
            {
                // Check if this state has an animation for the current direction
                if (_animations.ContainsKey((preferredState, directionAbbr)))
                {
                    return preferredState;
                }
            }
        }
        
        // Fallback to the first supported state (even if no animation exists)
        return supportedStates.Length > 0 ? supportedStates[0] : AnimationState.Idle;
    }

    /// <summary>
    /// Loads all animations for the character
    /// </summary>
    protected virtual void LoadAnimations(string characterPrefix, AnimationState[] supportedStates)
    {
        foreach (var state in supportedStates)
        {
            foreach (Direction8 direction in Enum.GetValues<Direction8>())
            {
                string directionAbbr = DirectionHelper.GetDirectionAbbreviation(direction, _use8WayDirections);
                string animationName = string.Format(AnimationNamePattern, 
                    characterPrefix, 
                    GetStateString(state), 
                    directionAbbr);
                
                TryLoadAnimation(animationName, state, directionAbbr);
            }
        }
    }

    /// <summary>
    /// Gets the string representation of an animation state for use in animation names
    /// </summary>
    protected virtual string GetStateString(AnimationState state)
    {
        return state.ToString().ToLower();
    }

    /// <summary>
    /// Attempts to load an animation with the given name
    /// </summary>
    protected virtual void TryLoadAnimation(string animationName, AnimationState state, string directionAbbr)
    {
        try
        {
            var animation = _atlas.CreateAnimatedSprite(animationName);
            if (animation != null)
            {
                _animations[(state, directionAbbr)] = animation;
            }
        }
        catch (Exception ex)
        {
            // Animation doesn't exist - this is expected for missing animations
            // Only log in debug builds to avoid spam
            #if DEBUG
            Console.WriteLine($"Debug: Animation '{animationName}' not found: {ex.GetType().Name}");
            #endif
        }
    }

    /// <summary>
    /// Sets the current animation state
    /// </summary>
    public virtual void SetAnimationState(AnimationState state)
    {
        if (_currentState == state) return;

        _currentState = state;
        UpdateCurrentAnimation();
    }

    /// <summary>
    /// Sets the current direction
    /// </summary>
    public virtual void SetDirection(Direction8 direction)
    {
        if (_currentDirection == direction) return;

        _currentDirection = direction;
        UpdateCurrentAnimation();
    }

    /// <summary>
    /// Updates the current animation based on state and direction
    /// If animation doesn't exist, shows placeholder and logs error
    /// </summary>
    protected virtual void UpdateCurrentAnimation()
    {
        string directionAbbr = DirectionHelper.GetDirectionAbbreviation(_currentDirection, _use8WayDirections);
        
        // Try to get the requested animation
        if (_animations.TryGetValue((_currentState, directionAbbr), out var animation))
        {
            _currentAnimation = animation;
            UpdateAnimationOrigin(); // Apply origin to new animation
        }
        else
        {
            // Animation doesn't exist - log error and show placeholder
            string animationKey = $"{_currentState}_{directionAbbr}";
            Console.WriteLine($"ERROR: Animation '{animationKey}' does not exist. Showing placeholder.");
            _currentAnimation = null;
        }
    }

    /// <summary>
    /// Updates the character sprite
    /// </summary>
    public virtual void Update(GameTime gameTime)
    {
        _currentAnimation?.Update(gameTime);
    }

    /// <summary>
    /// Sets the origin of this character sprite to the center.
    /// </summary>
    /// <param name="width">The width of the sprite frame (before scaling)</param>
    /// <param name="height">The height of the sprite frame (before scaling)</param>
    /// <remarks>
    /// Note: The origin needs to be set based on the width and height of the source texture region itself, 
    /// regardless of the scale the sprite is rendered at.
    /// </remarks>
    public virtual void CenterOrigin(float width, float height)
    {
        Origin = new Vector2(width, height) * 0.5f;
    }

    /// <summary>
    /// Sets the origin of this character sprite to the center using typical sprite dimensions.
    /// Assumes 64x64 sprite size - call CenterOrigin(width, height) for custom sizes.
    /// </summary>
    /// <remarks>
    /// Note: The origin needs to be set based on the width and height of the source texture region itself, 
    /// regardless of the scale the sprite is rendered at.
    /// </remarks>
    public virtual void CenterOrigin()
    {
        CenterOrigin(64f, 64f);
    }

    /// <summary>
    /// Updates the rendering properties of the current animation and placeholder
    /// Called automatically when properties change
    /// </summary>
    protected virtual void UpdateAnimationOrigin()
    {
        if (_currentAnimation != null)
        {
            _currentAnimation.Origin = _origin;
        }
        
        if (_placeholderSprite != null)
        {
            _placeholderSprite.Origin = _origin;
        }
    }

    /// <summary>
    /// Draws the character sprite or placeholder sprite
    /// </summary>
    public virtual void Draw(SpriteBatch spriteBatch, Vector2? position = null)
    {
        var drawPosition = position ?? Position;
        
        // Force placeholder for testing if enabled
        if (ForceShowPlaceholder)
        {
            DrawPlaceholderSprite(spriteBatch, drawPosition);
            return;
        }
        
        if (_currentAnimation != null)
        {
            // Apply all sprite properties to match Sprite class behavior
            _currentAnimation.Color = Color;
            _currentAnimation.Rotation = Rotation;
            _currentAnimation.Scale = Scale;
            _currentAnimation.Origin = Origin;
            _currentAnimation.Effects = Effects;
            _currentAnimation.LayerDepth = LayerDepth;
            _currentAnimation.Draw(spriteBatch, drawPosition);
        }
        else
        {
            // Draw placeholder sprite when animation is missing
            DrawPlaceholderSprite(spriteBatch, drawPosition);
        }
    }

    /// <summary>
    /// Draws a placeholder sprite when animation is missing
    /// </summary>
    protected virtual void DrawPlaceholderSprite(SpriteBatch spriteBatch, Vector2 position)
    {
        if (_placeholderSprite != null)
        {
            // Apply all sprite properties to match Sprite class behavior
            _placeholderSprite.Color = Color;
            _placeholderSprite.Rotation = Rotation;
            _placeholderSprite.Scale = Scale;
            _placeholderSprite.Origin = Origin;
            _placeholderSprite.Effects = Effects;
            _placeholderSprite.LayerDepth = LayerDepth;
            _placeholderSprite.Draw(spriteBatch, position);
        }
        // If no placeholder sprite is available, don't draw anything
    }
}