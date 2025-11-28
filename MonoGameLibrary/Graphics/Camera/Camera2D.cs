using Microsoft.Xna.Framework;
using System;

namespace MonoGameLibrary.Graphics.Camera;

/// <summary>
/// A 2D camera system that provides zoom, position, and following functionality.
/// Integrates with the content scaling system for resolution-independent rendering.
/// Includes performance monitoring and smooth following capabilities.
/// </summary>
public class Camera2D
{
    private Vector2 _position = Vector2.Zero;
    private float _zoom = 1.0f;
    private float _rotation = 0.0f;
    private Matrix? _viewMatrix;
    private Matrix? _inverseViewMatrix;
    private bool _matrixDirty = true;
    
    // Performance monitoring
    private int _matrixUpdateCount = 0;
    private TimeSpan _totalMatrixUpdateTime = TimeSpan.Zero;
    
    // Smooth following
    private Vector2? _targetPosition;
    private float _followSpeed = 8.0f;
    private bool _smoothFollowing = false;
    
    // Character screen coverage
    private float _characterScreenCoverage = 0.1f; // Default 10%

    /// <summary>
    /// Gets or sets the camera position in world coordinates.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                _matrixDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the camera zoom level. 1.0 is normal size, higher values zoom in, lower values zoom out.
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set
        {
            float clampedZoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
            if (_zoom != clampedZoom)
            {
                _zoom = clampedZoom;
                _matrixDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the camera rotation in radians.
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                _matrixDirty = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum zoom level (zoom out limit).
    /// </summary>
    public float MinZoom { get; set; } = 0.1f;

    /// <summary>
    /// Gets or sets the maximum zoom level (zoom in limit).
    /// </summary>
    public float MaxZoom { get; set; } = 5.0f;

    /// <summary>
    /// Gets or sets the target position to follow. Set to null to disable following.
    /// </summary>
    public Vector2? FollowTarget { get; set; }

    /// <summary>
    /// Gets or sets the offset from the follow target position.
    /// </summary>
    public Vector2 FollowOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the world bounds for camera clamping. Set to null to disable clamping.
    /// </summary>
    public Rectangle? WorldBounds { get; set; }

    /// <summary>
    /// Gets or sets the speed of smooth camera following (higher values = faster).
    /// </summary>
    public float FollowSpeed
    {
        get => _followSpeed;
        set => _followSpeed = MathHelper.Max(value, 0.1f);
    }

    /// <summary>
    /// Gets or sets whether smooth following is enabled. When true, camera smoothly interpolates to follow target.
    /// </summary>
    public bool SmoothFollowing
    {
        get => _smoothFollowing;
        set => _smoothFollowing = value;
    }

    /// <summary>
    /// Gets or sets the default character screen coverage as a percentage (0.0 to 1.0).
    /// Default is 0.1 (10% of screen height). Used for calculating default zoom and zoom limits.
    /// </summary>
    public float CharacterScreenCoverage
    {
        get => _characterScreenCoverage;
        set => _characterScreenCoverage = MathHelper.Clamp(value, 0.01f, 1.0f);
    }

    /// <summary>
    /// Gets performance statistics for matrix calculations.
    /// </summary>
    public CameraPerformanceStats PerformanceStats => new CameraPerformanceStats
    {
        MatrixUpdateCount = _matrixUpdateCount,
        TotalMatrixUpdateTime = _totalMatrixUpdateTime,
        AverageMatrixUpdateTime = _matrixUpdateCount > 0 ? 
            _totalMatrixUpdateTime.TotalMilliseconds / _matrixUpdateCount : 0.0
    };

    /// <summary>
    /// Gets the camera's view transformation matrix.
    /// </summary>
    public Matrix ViewMatrix
    {
        get
        {
            if (_matrixDirty || !_viewMatrix.HasValue)
            {
                UpdateMatrix();
            }
            return _viewMatrix.Value;
        }
    }

    /// <summary>
    /// Gets the inverse of the camera's view transformation matrix.
    /// </summary>
    public Matrix InverseViewMatrix
    {
        get
        {
            if (_matrixDirty || !_inverseViewMatrix.HasValue)
            {
                UpdateMatrix();
            }
            return _inverseViewMatrix.Value;
        }
    }

    /// <summary>
    /// Creates a new Camera2D instance.
    /// </summary>
    public Camera2D()
    {
    }

    /// <summary>
    /// Creates a new Camera2D instance with specified position and zoom.
    /// </summary>
    /// <param name="position">Initial camera position</param>
    /// <param name="zoom">Initial zoom level</param>
    public Camera2D(Vector2 position, float zoom = 1.0f)
    {
        Position = position;
        Zoom = zoom;
    }

    /// <summary>
    /// Updates the camera to follow the specified target position.
    /// </summary>
    /// <param name="targetPosition">The world position to follow</param>
    public void Follow(Vector2 targetPosition)
    {
        FollowTarget = targetPosition;
        _targetPosition = targetPosition + FollowOffset;

        if (!SmoothFollowing)
        {
            Position = _targetPosition.Value;
        }
    }

    /// <summary>
    /// Sets the camera zoom level with constraints applied.
    /// </summary>
    /// <param name="zoom">The new zoom level</param>
    public void SetZoom(float zoom)
    {
        Zoom = zoom;
    }

    /// <summary>
    /// Adjusts the camera zoom by the specified delta amount.
    /// </summary>
    /// <param name="delta">The amount to adjust zoom (positive for zoom in, negative for zoom out)</param>
    public void AdjustZoom(float delta)
    {
        Zoom += delta;
    }

    /// <summary>
    /// Resets the camera zoom to the default level calculated for the configured character screen coverage.
    /// </summary>
    public void ResetToDefaultZoom()
    {
        float characterHeight = 64f; // Standard character size
        float targetHeight = Core.VirtualResolution.Y * CharacterScreenCoverage;
        Zoom = targetHeight / characterHeight;
    }

    /// <summary>
    /// Converts a screen position to world coordinates.
    /// </summary>
    /// <param name="screenPosition">Position in screen space</param>
    /// <returns>Position in world coordinates</returns>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, InverseViewMatrix);
    }

    /// <summary>
    /// Converts a world position to screen coordinates.
    /// </summary>
    /// <param name="worldPosition">Position in world coordinates</param>
    /// <returns>Position in screen space</returns>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition, ViewMatrix);
    }

    /// <summary>
    /// Updates the camera to follow its target if one is set.
    /// Call this in your game's Update method.
    /// </summary>
    /// <param name="gameTime">Game timing information</param>
    public void Update(GameTime gameTime)
    {
        if (FollowTarget.HasValue)
        {
            _targetPosition = FollowTarget.Value + FollowOffset;

            if (SmoothFollowing && _targetPosition.HasValue)
            {
                // Smooth interpolation to target position
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                float lerpAmount = 1.0f - (float)Math.Pow(0.5, FollowSpeed * deltaTime);
                Position = Vector2.Lerp(Position, _targetPosition.Value, lerpAmount);
            }
            else
            {
                Position = _targetPosition.Value;
            }
        }

        // Apply world bounds clamping if set
        if (WorldBounds.HasValue)
        {
            var bounds = WorldBounds.Value;
            var viewportSize = new Vector2(Core.VirtualResolution.X, Core.VirtualResolution.Y) / Zoom;
            var halfViewport = viewportSize / 2f;

            var clampedX = MathHelper.Clamp(Position.X, bounds.Left + halfViewport.X, bounds.Right - halfViewport.X);
            var clampedY = MathHelper.Clamp(Position.Y, bounds.Top + halfViewport.Y, bounds.Bottom - halfViewport.Y);

            Position = new Vector2(clampedX, clampedY);
        }
    }

    /// <summary>
    /// Updates the camera to follow its target if one is set (without GameTime parameter for backwards compatibility).
    /// </summary>
    public void Update()
    {
        Update(new GameTime());
    }

    /// <summary>
    /// Sets dynamic zoom limits based on character size and screen coverage constraints.
    /// </summary>
    /// <param name="characterSize">The size of the character in pixels</param>
    public void SetZoomLimitsForCharacter(float characterSize)
    {
        // Max zoom: character at configured screen coverage (zoom in limit - largest character allowed)
        MaxZoom = (Core.VirtualResolution.Y * CharacterScreenCoverage) / characterSize;

        // Min zoom: much smaller value to allow seeing entire tilemap (zoom out limit)
        MinZoom = MaxZoom * 0.1f; // Allow zooming out to 10x smaller than max

        // Ensure current zoom is within new limits
        Zoom = MathHelper.Clamp(Zoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Sets the character screen coverage percentage and updates zoom limits for the given character size.
    /// </summary>
    /// <param name="coveragePercentage">Screen coverage as a percentage (0.0 to 1.0)</param>
    /// <param name="characterSize">The size of the character in pixels</param>
    public void SetCharacterScreenCoverage(float coveragePercentage, float characterSize)
    {
        CharacterScreenCoverage = coveragePercentage;
        SetZoomLimitsForCharacter(characterSize);
        // Apply the new coverage immediately
        ResetToDefaultZoom();
    }

    /// <summary>
    /// Sets zoom limits where the character coverage is the maximum zoom, and minimum zoom shows the entire tilemap.
    /// </summary>
    /// <param name="characterSize">The size of the character in pixels</param>
    /// <param name="tilemapWidth">Width of the tilemap in pixels</param>
    /// <param name="tilemapHeight">Height of the tilemap in pixels</param>
    public void SetZoomLimitsForTilemap(float characterSize, float tilemapWidth, float tilemapHeight)
    {
        // Max zoom: character at configured screen coverage
        MaxZoom = (Core.VirtualResolution.Y * CharacterScreenCoverage) / characterSize;
        
        // Min zoom: ensures entire tilemap fits on screen
        float zoomForWidth = Core.VirtualResolution.X / tilemapWidth;
        float zoomForHeight = Core.VirtualResolution.Y / tilemapHeight;
        MinZoom = Math.Min(zoomForWidth, zoomForHeight);
        
        // Ensure min zoom doesn't exceed max zoom
        MinZoom = Math.Min(MinZoom, MaxZoom);
        
        // Ensure current zoom is within new limits
        Zoom = MathHelper.Clamp(Zoom, MinZoom, MaxZoom);
    }

    /// <summary>
    /// Resets performance monitoring statistics.
    /// </summary>
    public void ResetPerformanceStats()
    {
        _matrixUpdateCount = 0;
        _totalMatrixUpdateTime = TimeSpan.Zero;
    }

    /// <summary>
    /// Gets diagnostic information about the current camera state for debugging.
    /// </summary>
    /// <param name="characterSize">The character size to calculate coverage for</param>
    /// <returns>A string with camera diagnostic information</returns>
    public string GetDiagnosticInfo(float characterSize)
    {
        float currentCoverage = (characterSize * Zoom) / Core.VirtualResolution.Y;
        return $"Camera Diagnostics:\n" +
               $"  Position: ({Position.X:F1}, {Position.Y:F1})\n" +
               $"  Zoom: {Zoom:F3}\n" +
               $"  Zoom Limits: {MinZoom:F3} - {MaxZoom:F3}\n" +
               $"  Target Coverage: {CharacterScreenCoverage:P1}\n" +
               $"  Current Coverage: {currentCoverage:P1}\n" +
               $"  Character Size: {characterSize}px\n" +
               $"  Virtual Resolution: {Core.VirtualResolution.X}x{Core.VirtualResolution.Y}";
    }

    /// <summary>
    /// Updates the view matrix and inverse matrix with performance monitoring.
    /// </summary>
    private void UpdateMatrix()
    {
        var startTime = DateTime.UtcNow;

        var viewport = new Vector2(Core.VirtualResolution.X, Core.VirtualResolution.Y);
        var origin = viewport / 2f;

        _viewMatrix = Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                     Matrix.CreateRotationZ(Rotation) *
                     Matrix.CreateScale(Zoom, Zoom, 1f) *
                     Matrix.CreateTranslation(new Vector3(origin, 0f));

        var matrix = _viewMatrix.Value;
        Matrix.Invert(ref matrix, out Matrix inverse);
        _inverseViewMatrix = inverse;

        _matrixDirty = false;

        // Update performance statistics
        var endTime = DateTime.UtcNow;
        _matrixUpdateCount++;
        _totalMatrixUpdateTime += endTime - startTime;
    }
}

/// <summary>
/// Performance statistics for camera matrix calculations.
/// </summary>
public struct CameraPerformanceStats
{
    /// <summary>
    /// Total number of matrix updates performed.
    /// </summary>
    public int MatrixUpdateCount { get; init; }

    /// <summary>
    /// Total time spent updating matrices.
    /// </summary>
    public TimeSpan TotalMatrixUpdateTime { get; init; }

    /// <summary>
    /// Average time per matrix update in milliseconds.
    /// </summary>
    public double AverageMatrixUpdateTime { get; init; }
}