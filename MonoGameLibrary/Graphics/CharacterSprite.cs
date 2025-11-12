using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Utilities;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Base class for character sprites that can have multiple animation states and directions
/// Displays placeholder sprite when animations are missing
/// </summary>
public class CharacterSprite
{
    protected readonly Dictionary<(string state, string direction), AnimatedSprite> _animations;
    protected readonly IAnimationResolver _animationResolver;
    protected readonly IAnimationNameFormatter _nameFormatter;
    protected readonly DirectionMode _directionMode;
    protected readonly ILogger _logger;
    protected readonly Sprite _placeholderSprite;
    
    protected string _currentState;
    protected Direction8 _currentDirection;
    protected AnimatedSprite _currentAnimation;

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

    public string CurrentState => _currentState;
    public Direction8 CurrentDirection => _currentDirection;
    public bool CurrentAnimationExists => _currentAnimation != null;

    /// <summary>
    /// Creates a new CharacterSprite using a TextureAtlas (convenience constructor)
    /// </summary>
    /// <param name="atlas">The texture atlas containing the animations</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "player")</param>
    /// <param name="directionMode">Direction mode (FourWay or EightWay)</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public CharacterSprite(TextureAtlas atlas, string characterPrefix, DirectionMode directionMode = DirectionMode.EightWay, 
        params string[] supportedStates)
        : this(new TextureAtlasAnimationResolver(atlas), new DefaultAnimationNameFormatter(), 
               characterPrefix, directionMode, NullLogger.Instance, supportedStates)
    {
    }

    /// <summary>
    /// Creates a new CharacterSprite with full control over dependencies
    /// </summary>
    /// <param name="animationResolver">Animation resolver for loading sprites</param>
    /// <param name="nameFormatter">Formatter for animation names</param>
    /// <param name="characterPrefix">The prefix for animation names (e.g., "player")</param>
    /// <param name="directionMode">Direction mode (FourWay or EightWay)</param>
    /// <param name="logger">Logger for diagnostic messages</param>
    /// <param name="supportedStates">The animation states this character supports</param>
    public CharacterSprite(IAnimationResolver animationResolver, IAnimationNameFormatter nameFormatter,
        string characterPrefix, DirectionMode directionMode, ILogger logger, params string[] supportedStates)
    {
        _animationResolver = animationResolver ?? throw new ArgumentNullException(nameof(animationResolver));
        _nameFormatter = nameFormatter ?? throw new ArgumentNullException(nameof(nameFormatter));
        _directionMode = directionMode;
        _logger = logger ?? NullLogger.Instance;
        _animations = new Dictionary<(string, string), AnimatedSprite>();

        // Try to load placeholder sprite (single frame only)
        _placeholderSprite = _animationResolver.TryCreateSprite("placeholder");

        // Load animations for all supported states and directions
        LoadAnimations(characterPrefix, supportedStates);
        
        // Set initial animation state
        SetAnimationState(FindBestInitialState(supportedStates));
    }

    /// <summary>
    /// Finds the best initial animation state that actually has an animation available
    /// </summary>
    protected virtual string FindBestInitialState(string[] supportedStates)
    {
        // Preferred order: Idle -> Walk -> Run -> Attack -> first available
        var preferredOrder = new[] { AnimationState.Idle, AnimationState.Walk, AnimationState.Run, AnimationState.Attack };
        
        string directionAbbr = DirectionHelper.GetDirectionAbbreviation(_currentDirection, _directionMode);
        
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
    protected virtual void LoadAnimations(string characterPrefix, string[] supportedStates)
    {
        foreach (var state in supportedStates)
        {
            foreach (Direction8 direction in Enum.GetValues<Direction8>())
            {
                string directionAbbr = DirectionHelper.GetDirectionAbbreviation(direction, _directionMode);
                string animationName = _nameFormatter.FormatAnimationName(
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
    protected virtual string GetStateString(string state)
    {
        return state.ToLowerInvariant();
    }

    /// <summary>
    /// Attempts to load an animation with the given name
    /// </summary>
    protected virtual void TryLoadAnimation(string animationName, string state, string directionAbbr)
    {
        var animation = _animationResolver.TryCreateAnimatedSprite(animationName);
        if (animation != null)
        {
            _animations[(state, directionAbbr)] = animation;
        }
        else
        {
            // Animation doesn't exist - this is expected for missing animations
            _logger.LogInfo($"Animation '{animationName}' not found");
        }
    }

    /// <summary>
    /// Sets the current animation state
    /// </summary>
    public virtual void SetAnimationState(string state)
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
        string directionAbbr = DirectionHelper.GetDirectionAbbreviation(_currentDirection, _directionMode);
        
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
            _logger.LogError($"Animation '{animationKey}' does not exist. Showing placeholder.");
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