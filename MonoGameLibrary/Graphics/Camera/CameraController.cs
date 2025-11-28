using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using System;

namespace MonoGameLibrary.Graphics.Camera;

/// <summary>
/// Handles input for camera zoom and control operations.
/// Supports keyboard, gamepad, and mouse wheel inputs with customizable key bindings.
/// </summary>
public class CameraController
{
    private Camera2D _camera;
    private float _zoomSpeed = 0.1f;
    private float _zoomAcceleration = 1.0f;
    
    // Keyboard controls
    private Keys[] _zoomInKeys = { Keys.OemPlus, Keys.Add };
    private Keys[] _zoomOutKeys = { Keys.OemMinus, Keys.Subtract };
    private Keys _resetZoomKey = Keys.R;
    
    // GamePad controls
    private Buttons[] _zoomInButtons = { Buttons.RightShoulder };
    private Buttons[] _zoomOutButtons = { Buttons.LeftShoulder };
    private Buttons _resetZoomButton = Buttons.RightStick;
    
    // Mouse wheel controls
    private float _mouseWheelZoomSensitivity = 0.2f;
    
    /// <summary>
    /// Gets or sets the speed of zoom operations.
    /// </summary>
    public float ZoomSpeed
    {
        get => _zoomSpeed;
        set => _zoomSpeed = MathHelper.Clamp(value, 0.01f, 1.0f);
    }
    
    /// <summary>
    /// Gets or sets the acceleration multiplier for continuous zoom operations.
    /// </summary>
    public float ZoomAcceleration
    {
        get => _zoomAcceleration;
        set => _zoomAcceleration = MathHelper.Max(value, 0.1f);
    }
    
    /// <summary>
    /// Gets or sets the sensitivity of mouse wheel zoom operations.
    /// </summary>
    public float MouseWheelZoomSensitivity
    {
        get => _mouseWheelZoomSensitivity;
        set => _mouseWheelZoomSensitivity = MathHelper.Clamp(value, 0.01f, 2.0f);
    }
    
    /// <summary>
    /// Gets or sets whether keyboard controls are enabled.
    /// </summary>
    public bool KeyboardEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether gamepad controls are enabled.
    /// </summary>
    public bool GamePadEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether mouse wheel controls are enabled.
    /// </summary>
    public bool MouseWheelEnabled { get; set; } = true;

    /// <summary>
    /// Creates a new CameraController for the specified camera.
    /// </summary>
    /// <param name="camera">The camera to control</param>
    public CameraController(Camera2D camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    /// <summary>
    /// Updates the camera controller, processing input for zoom and control operations.
    /// </summary>
    /// <param name="gameTime">Game timing information</param>
    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Process zoom input
        if (KeyboardEnabled)
        {
            ProcessKeyboardZoom(deltaTime);
        }
        
        if (GamePadEnabled)
        {
            ProcessGamePadZoom(deltaTime);
        }
        
        if (MouseWheelEnabled)
        {
            ProcessMouseWheelZoom();
        }
    }

    /// <summary>
    /// Sets custom keyboard key bindings for zoom controls.
    /// </summary>
    /// <param name="zoomInKeys">Keys for zooming in</param>
    /// <param name="zoomOutKeys">Keys for zooming out</param>
    /// <param name="resetZoomKey">Key for resetting zoom</param>
    public void SetKeyBindings(Keys[] zoomInKeys = null, Keys[] zoomOutKeys = null, Keys resetZoomKey = Keys.R)
    {
        if (zoomInKeys != null) _zoomInKeys = zoomInKeys;
        if (zoomOutKeys != null) _zoomOutKeys = zoomOutKeys;
        _resetZoomKey = resetZoomKey;
    }

    /// <summary>
    /// Sets custom gamepad button bindings for zoom controls.
    /// </summary>
    /// <param name="zoomInButtons">Buttons for zooming in</param>
    /// <param name="zoomOutButtons">Buttons for zooming out</param>
    /// <param name="resetZoomButton">Button for resetting zoom</param>
    public void SetButtonBindings(Buttons[] zoomInButtons = null, Buttons[] zoomOutButtons = null, Buttons resetZoomButton = Buttons.RightStick)
    {
        if (zoomInButtons != null) _zoomInButtons = zoomInButtons;
        if (zoomOutButtons != null) _zoomOutButtons = zoomOutButtons;
        _resetZoomButton = resetZoomButton;
    }

    /// <summary>
    /// Manually trigger a zoom in operation.
    /// </summary>
    public void ZoomIn(float amount = 0.1f)
    {
        _camera.AdjustZoom(amount);
    }

    /// <summary>
    /// Manually trigger a zoom out operation.
    /// </summary>
    public void ZoomOut(float amount = 0.1f)
    {
        _camera.AdjustZoom(-amount);
    }

    /// <summary>
    /// Manually trigger a zoom reset operation.
    /// </summary>
    public void ResetZoom()
    {
        _camera.ResetToDefaultZoom();
    }

    /// <summary>
    /// Zooms to a specific position in world coordinates, adjusting camera position to keep that point centered.
    /// </summary>
    /// <param name="worldPosition">The world position to zoom to</param>
    /// <param name="zoomLevel">The target zoom level</param>
    public void ZoomToPosition(Vector2 worldPosition, float zoomLevel)
    {
        // Get screen position before zoom change
        var screenPos = _camera.WorldToScreen(worldPosition);
        
        // Apply zoom change
        _camera.Zoom = zoomLevel;
        
        // Adjust camera position to keep the point at the same screen position
        var newWorldPos = _camera.ScreenToWorld(screenPos);
        var offset = worldPosition - newWorldPos;
        _camera.Position += offset;
    }

    /// <summary>
    /// Processes keyboard input for zoom operations.
    /// </summary>
    private void ProcessKeyboardZoom(float deltaTime)
    {
        var keyboard = Core.Input.Keyboard;
        
        // Check for zoom reset (just pressed)
        if (keyboard.WasKeyJustPressed(_resetZoomKey))
        {
            _camera.ResetToDefaultZoom();
            return;
        }
        
        // Check for continuous zoom in
        bool zoomingIn = false;
        foreach (var key in _zoomInKeys)
        {
            if (keyboard.IsKeyDown(key))
            {
                zoomingIn = true;
                break;
            }
        }
        
        // Check for continuous zoom out
        bool zoomingOut = false;
        foreach (var key in _zoomOutKeys)
        {
            if (keyboard.IsKeyDown(key))
            {
                zoomingOut = true;
                break;
            }
        }
        
        // Apply zoom changes
        if (zoomingIn && !zoomingOut)
        {
            float zoomAmount = _zoomSpeed * _zoomAcceleration * deltaTime;
            _camera.AdjustZoom(zoomAmount);
        }
        else if (zoomingOut && !zoomingIn)
        {
            float zoomAmount = _zoomSpeed * _zoomAcceleration * deltaTime;
            _camera.AdjustZoom(-zoomAmount);
        }
    }

    /// <summary>
    /// Processes gamepad input for zoom operations.
    /// </summary>
    private void ProcessGamePadZoom(float deltaTime)
    {
        var gamePad = Core.Input.GamePads[0]; // Use Player 1 controller
        
        // Check for zoom reset (just pressed)
        if (gamePad.WasButtonJustPressed(_resetZoomButton))
        {
            _camera.ResetToDefaultZoom();
            return;
        }
        
        // Check for continuous zoom in
        bool zoomingIn = false;
        foreach (var button in _zoomInButtons)
        {
            if (gamePad.IsButtonDown(button))
            {
                zoomingIn = true;
                break;
            }
        }
        
        // Check for continuous zoom out
        bool zoomingOut = false;
        foreach (var button in _zoomOutButtons)
        {
            if (gamePad.IsButtonDown(button))
            {
                zoomingOut = true;
                break;
            }
        }
        
        // Apply zoom changes
        if (zoomingIn && !zoomingOut)
        {
            float zoomAmount = _zoomSpeed * _zoomAcceleration * deltaTime;
            _camera.AdjustZoom(zoomAmount);
        }
        else if (zoomingOut && !zoomingIn)
        {
            float zoomAmount = _zoomSpeed * _zoomAcceleration * deltaTime;
            _camera.AdjustZoom(-zoomAmount);
        }
    }

    /// <summary>
    /// Processes mouse wheel input for zoom operations.
    /// </summary>
    private void ProcessMouseWheelZoom()
    {
        var mouse = Core.Input.Mouse;
        int wheelDelta = mouse.ScrollWheelDelta;
        
        if (wheelDelta != 0)
        {
            // Get mouse position in world coordinates before zoom
            var mouseWorldPos = mouse.WorldPosition;
            
            // Calculate zoom amount based on wheel delta
            float zoomAmount = (wheelDelta > 0 ? 1 : -1) * _mouseWheelZoomSensitivity;
            
            // Apply zoom change
            _camera.AdjustZoom(zoomAmount);
            
            // Adjust camera position to zoom towards mouse cursor
            var newMouseWorldPos = mouse.WorldPosition;
            var offset = mouseWorldPos - newMouseWorldPos;
            _camera.Position += offset;
        }
    }
}