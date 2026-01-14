using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics.Camera;
using MonoGameLibrary.Graphics.Collision;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.Utilities;

namespace MonoGameLibrary;

/// <summary>
/// Defines the possible positions for FPS display on screen
/// </summary>
public enum FpsDisplayPosition
{
    UpperLeft,
    Top,
    UpperRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}

public class Core : Game
{
    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    // Scene manager for handling scene transitions and caching
    private static SceneManager s_sceneManager = new SceneManager();

    /// <summary>
    /// Gets the scene manager for advanced scene operations
    /// </summary>
    public static SceneManager SceneManager => s_sceneManager;

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    public static Scene Scene => s_sceneManager.CurrentScene;

    /// <summary>
    /// Gets or sets the virtual resolution used for content scaling (design resolution)
    /// </summary>
    public static Point VirtualResolution { get; set; } = new Point(1920, 1080);

    /// <summary>
    /// Gets the current scaling factor based on window size vs virtual resolution
    /// </summary>
    public static Vector2 ContentScale
    {
        get
        {
            if (Graphics == null) return Vector2.One;
            
            float scaleX = (float)Graphics.PreferredBackBufferWidth / VirtualResolution.X;
            float scaleY = (float)Graphics.PreferredBackBufferHeight / VirtualResolution.Y;
            
            // Use uniform scaling to maintain aspect ratio
            float uniformScale = Math.Min(scaleX, scaleY);
            return new Vector2(uniformScale);
        }
    }

    /// <summary>
    /// Gets the transformation matrix for content scaling
    /// </summary>
    public static Matrix ScaleMatrix
    {
        get
        {
            var scale = ContentScale;
            
            // Calculate centering offset for letterboxing/pillarboxing
            float scaledWidth = VirtualResolution.X * scale.X;
            float scaledHeight = VirtualResolution.Y * scale.Y;
            
            float offsetX = (Graphics.PreferredBackBufferWidth - scaledWidth) / 2f;
            float offsetY = (Graphics.PreferredBackBufferHeight - scaledHeight) / 2f;
            
            return Matrix.CreateScale(scale.X, scale.Y, 1f) * 
                   Matrix.CreateTranslation(offsetX, offsetY, 0f);
        }
    }

    /// <summary>
    /// Gets the combined transformation matrix for content scaling and camera positioning.
    /// Use this for world objects that should be affected by camera zoom and position.
    /// </summary>
    public static Matrix CameraMatrix
    {
        get
        {
            if (Camera == null) return ScaleMatrix;
            return Camera.ViewMatrix * ScaleMatrix;
        }
    }

    /// <summary>
    /// Gets the viewport rectangle for the scaled content area
    /// </summary>
    public static Rectangle ScaledViewport
    {
        get
        {
            var scale = ContentScale;
            float scaledWidth = VirtualResolution.X * scale.X;
            float scaledHeight = VirtualResolution.Y * scale.Y;
            
            float offsetX = (Graphics.PreferredBackBufferWidth - scaledWidth) / 2f;
            float offsetY = (Graphics.PreferredBackBufferHeight - scaledHeight) / 2f;
            
            return new Rectangle((int)offsetX, (int)offsetY, (int)scaledWidth, (int)scaledHeight);
        }
    }

    /// <summary>
    /// Gets whether the current device is a Steam Deck
    /// </summary>
    public static bool IsSteamDeck
    {
        get
        {
            try
            {
                // Check multiple Steam Deck indicators
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Check for Steam Deck specific environment variables
                    var steamDeckVar = Environment.GetEnvironmentVariable("SteamDeck");
                    var steamAppId = Environment.GetEnvironmentVariable("SteamAppId");
                    var steamGameId = Environment.GetEnvironmentVariable("SteamGameId");
                    
                    if (!string.IsNullOrEmpty(steamDeckVar) || 
                        !string.IsNullOrEmpty(steamAppId) || 
                        !string.IsNullOrEmpty(steamGameId))
                    {
                        return true;
                    }
                    
                    // Check for Steam Deck hardware identifiers
                    if (File.Exists("/sys/devices/virtual/dmi/id/product_name"))
                    {
                        var productName = File.ReadAllText("/sys/devices/virtual/dmi/id/product_name").Trim();
                        if (productName.Contains("Jupiter", StringComparison.OrdinalIgnoreCase) ||
                            productName.Contains("Steam Deck", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    
                    // Check for Steam Deck specific GPU
                    if (File.Exists("/proc/cpuinfo"))
                    {
                        var cpuInfo = File.ReadAllText("/proc/cpuinfo");
                        if (cpuInfo.Contains("AuthenticAMD", StringComparison.OrdinalIgnoreCase) &&
                            cpuInfo.Contains("AMD Custom APU", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch
            {
                // If any file access fails, assume not Steam Deck
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }

    /// <summary>
    /// Gets a reference to to the input management system.
    /// </summary>
    public static InputManager Input { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    /// <summary>
    /// Gets a reference to the audio control system.
    /// </summary>
    public static AudioController Audio { get; private set; }

    /// <summary>
    /// Gets the 2D camera system for zoom and positioning control.
    /// </summary>
    public static Graphics.Camera.Camera2D Camera { get; private set; }

    /// <summary>
    /// Gets the camera controller for handling input-based camera operations.
    /// </summary>
    public static Graphics.Camera.CameraController CameraController { get; private set; }

    /// <summary>
    /// Gets the primary monitor's display mode information
    /// </summary>
    public static DisplayMode PrimaryDisplayMode => GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;

    /// <summary>
    /// Gets the primary monitor's width in pixels
    /// </summary>
    public static int MonitorWidth => PrimaryDisplayMode.Width;

    /// <summary>
    /// Gets the primary monitor's height in pixels
    /// </summary>
    public static int MonitorHeight => PrimaryDisplayMode.Height;

    /// <summary>
    /// Gets a recommended window size based on monitor resolution (typically 80% of monitor size)
    /// </summary>
    public static Point RecommendedWindowSize
    {
        get
        {
            int width = (int)(MonitorWidth * 0.8f);
            int height = (int)(MonitorHeight * 0.8f);
            // Ensure minimum usable size
            width = Math.Max(800, width);
            height = Math.Max(600, height);
            return new Point(width, height);
        }
    }

    // FPS Display System
    private static FpsDisplayPosition s_fpsDisplayPosition = FpsDisplayPosition.UpperLeft;
    private static double[] s_frameTimeBuffer = new double[5]; // Much smaller buffer for immediate response
    private static int s_frameTimeIndex = 0;
    private static double s_currentFps = 60.0;
    private static bool s_frameBufferFilled = false;
    
    /// <summary>
    /// Gets or sets the font used for developer mode text rendering (collision markers, debug info, etc.).
    /// Can be set to any SpriteFont loaded by the game.
    /// </summary>
    public static SpriteFont DebugFont
    {
        get => DebugSystem.DebugFont;
        set => DebugSystem.DebugFont = value;
    }
    
    /// <summary>
    /// Gets or sets the scaling factor for debug font rendering.
    /// Default is 1.0f. Use values like 0.8f for smaller text or 1.2f for larger text.
    /// </summary>
    public static float DebugFontScale
    {
        get => DebugSystem.DebugFontScale;
        set => DebugSystem.DebugFontScale = value;
    }
    
    /// <summary>
    /// Gets or sets whether developer mode is active (only available in debug mode)
    /// </summary>
    public static bool DeveloperMode
    {
        get => DebugSystem.DeveloperMode;
        set => DebugSystem.DeveloperMode = value;
    }
    
    /// <summary>
    /// Gets or sets whether collision boxes should be visible (only available in debug mode)
    /// </summary>
    public static bool ShowCollisionBoxes
    {
        get => DebugSystem.ShowCollisionBoxes;
        set => DebugSystem.ShowCollisionBoxes = value;
    }
    
    /// <summary>
    /// Gets or sets whether debug messages should be displayed
    /// </summary>
    public static bool ShowDebugMessages
    {
        get => DebugSystem.ShowDebugMessages;
        set => DebugSystem.ShowDebugMessages = value;
    }
    
    /// <summary>
    /// Gets the current list of debug messages
    /// </summary>
    public static IReadOnlyList<DebugSystem.DebugMessage> DebugMessages => DebugSystem.DebugMessages;
    
    /// <summary>
    /// Toggles developer mode and all associated debug features (only works in debug mode)
    /// </summary>
    public static void ToggleDeveloperMode()
    {
        DebugSystem.ToggleDeveloperMode();
    }
    
    /// <summary>
    /// Toggles collision box visibility (only works when developer mode is active)
    /// </summary>
    public static void ToggleCollisionBoxes()
    {
        DebugSystem.ToggleCollisionBoxes();
    }
    
    /// <summary>
    /// Adds a debug message to the centralized debug messaging system
    /// </summary>
    /// <param name="message">The message to add</param>
    public static void AddDebugMessage(string message)
    {
        DebugSystem.AddDebugMessage(message);
    }
    
    /// <summary>
    /// Gets or sets the position for FPS display on screen
    /// </summary>
    public static FpsDisplayPosition FpsDisplayPosition
    {
        get => s_fpsDisplayPosition;
        set => s_fpsDisplayPosition = value;
    }
    
    /// <summary>
    /// Gets the current frames per second
    /// </summary>
    public static double CurrentFps => s_currentFps;
    
    /// <summary>
    /// Gets the position coordinates for FPS display based on the current FpsDisplayPosition
    /// </summary>
    /// <returns>Vector2 coordinates for the specified position</returns>
    public static Vector2 GetFpsDisplayPosition()
    {
        return s_fpsDisplayPosition switch
        {
            FpsDisplayPosition.UpperLeft => new Vector2(10, 10),
            FpsDisplayPosition.Top => new Vector2(950, 10),
            FpsDisplayPosition.UpperRight => new Vector2(1790, 10),
            FpsDisplayPosition.Right => new Vector2(1790, 530),
            FpsDisplayPosition.BottomRight => new Vector2(1790, 1050),
            FpsDisplayPosition.Bottom => new Vector2(950, 1050),
            FpsDisplayPosition.BottomLeft => new Vector2(10, 1050),
            FpsDisplayPosition.Left => new Vector2(10, 530),
            _ => new Vector2(10, 10)
        };
    }

    /// <summary>
    /// Converts screen coordinates to virtual coordinates for content scaling
    /// </summary>
    /// <param name="screenPosition">Position in screen/window coordinates</param>
    /// <returns>Position in virtual resolution coordinates</returns>
    public static Vector2 ScreenToVirtual(Vector2 screenPosition)
    {
        var scale = ContentScale;
        var viewport = ScaledViewport;
        
        // Adjust for viewport offset and scale
        Vector2 adjustedPosition = screenPosition - new Vector2(viewport.X, viewport.Y);
        return adjustedPosition / scale;
    }

    /// <summary>
    /// Converts virtual coordinates to screen coordinates
    /// </summary>
    /// <param name="virtualPosition">Position in virtual resolution coordinates</param>
    /// <returns>Position in screen/window coordinates</returns>
    public static Vector2 VirtualToScreen(Vector2 virtualPosition)
    {
        var scale = ContentScale;
        var viewport = ScaledViewport;
        
        return (virtualPosition * scale) + new Vector2(viewport.X, viewport.Y);
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates (includes camera transformation).
    /// </summary>
    /// <param name="screenPosition">The screen position to convert</param>
    /// <returns>World coordinates</returns>
    public static Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        var virtualPos = ScreenToVirtual(screenPosition);
        if (Camera == null) return virtualPos;
        return Camera.ScreenToWorld(virtualPos);
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates (includes camera transformation).
    /// </summary>
    /// <param name="worldPosition">The world position to convert</param>
    /// <returns>Screen coordinates</returns>
    public static Vector2 WorldToScreen(Vector2 worldPosition)
    {
        if (Camera == null) return VirtualToScreen(worldPosition);
        var virtualPos = Camera.WorldToScreen(worldPosition);
        return VirtualToScreen(virtualPos);
    }

    /// <summary>
    /// Sets a custom virtual resolution for content scaling
    /// </summary>
    /// <param name="width">Virtual width (design width)</param>
    /// <param name="height">Virtual height (design height)</param>
    public static void SetVirtualResolution(int width, int height)
    {
        VirtualResolution = new Point(width, height);
    }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        Initialize(title, width, height, fullScreen);
    }

    /// <summary>
    /// Creates a new Core instance with automatic window sizing based on monitor resolution.
    /// Automatically enables fullscreen on Steam Deck.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode (automatically true on Steam Deck).</param>
    /// <param name="windowSizePercent">The percentage of monitor size to use (default: 0.8 for 80%).</param>
    public Core(string title, bool fullScreen = false, float windowSizePercent = 0.8f)
    {
        // Automatically enable fullscreen on Steam Deck
        if (IsSteamDeck)
        {
            fullScreen = true;
        }
        
        var monitorSize = GetMonitorAwareSize(windowSizePercent);
        Initialize(title, monitorSize.X, monitorSize.Y, fullScreen);
    }
    
    /// <summary>
    /// Creates a new Core instance optimized for Steam Deck.
    /// Automatically runs in fullscreen with Steam Deck optimized settings.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    public Core(string title)
    {
        if (IsSteamDeck)
        {
            // Steam Deck: Use native resolution in fullscreen
            Initialize(title, 1280, 800, true);
        }
        else
        {
            // Other devices: Use monitor-aware sizing
            var monitorSize = GetMonitorAwareSize(0.8f);
            Initialize(title, monitorSize.X, monitorSize.Y, false);
        }
    }

    /// <summary>
    /// Gets a window size appropriate for the current monitor or device
    /// </summary>
    /// <param name="sizePercent">Percentage of monitor size to use (0.1 to 1.0)</param>
    /// <returns>Point containing width and height</returns>
    public static Point GetMonitorAwareSize(float sizePercent = 0.8f)
    {
        // Steam Deck: Use native resolution
        if (IsSteamDeck)
        {
            return new Point(1280, 800);
        }
        
        // Clamp percentage to reasonable range
        sizePercent = Math.Clamp(sizePercent, 0.1f, 1.0f);
        
        int width = (int)(MonitorWidth * sizePercent);
        int height = (int)(MonitorHeight * sizePercent);
        
        // Ensure minimum usable size
        width = Math.Max(800, width);
        height = Math.Max(600, height);
        
        // Ensure maximum size doesn't exceed monitor
        width = Math.Min(width, MonitorWidth - 100); // Leave some margin
        height = Math.Min(height, MonitorHeight - 100);
        
        return new Point(width, height);
    }

    /// <summary>
    /// Common initialization logic for both constructors
    /// </summary>
    private void Initialize(string title, int width, int height, bool fullScreen)
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;
        
        // Standard production setting: VSync ON by default (user can toggle with F3)
        Graphics.SynchronizeWithVerticalRetrace = true;
        
        // Allow variable frame rates but with VSync limiting
        IsFixedTimeStep = false;
        TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 120.0); // 120 FPS target when VSync off

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title
        Window.Title = title;

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        // Exit on escape is true by default
        ExitOnEscape = true;        
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Initialize collision drawing system.
        CollisionDraw.Initialize(GraphicsDevice);

        // Create a new input manager.
        Input = new InputManager();

        // Create a new audio controller.
        Audio = new AudioController();

        // Create and initialize camera system
        Camera = new Graphics.Camera.Camera2D();
        Camera.ResetToDefaultZoom();
        Camera.SetZoomLimitsForCharacter(64f); // Default character size
        
        // Create camera controller for input handling
        CameraController = new Graphics.Camera.CameraController(Camera);
    }

    /// <summary>
    /// Resizes the game window to the specified dimensions
    /// </summary>
    /// <param name="width">The new width in pixels</param>
    /// <param name="height">The new height in pixels</param>
    public static void ResizeWindow(int width, int height)
    {
        if (Graphics != null)
        {
            Graphics.PreferredBackBufferWidth = width;
            Graphics.PreferredBackBufferHeight = height;
            Graphics.ApplyChanges();
        }
    }

    /// <summary>
    /// Resizes the game window to a percentage of the monitor size
    /// </summary>
    /// <param name="sizePercent">Percentage of monitor size (0.1 to 1.0)</param>
    public static void ResizeWindowToMonitorPercent(float sizePercent = 0.8f)
    {
        var size = GetMonitorAwareSize(sizePercent);
        ResizeWindow(size.X, size.Y);
    }

    /// <summary>
    /// Centers the game window on the screen (only works in windowed mode)
    /// </summary>
    public static void CenterWindow()
    {
        if (Graphics != null && !Graphics.IsFullScreen && s_instance?.Window != null)
        {
            var window = s_instance.Window;
            var centerX = (MonitorWidth - Graphics.PreferredBackBufferWidth) / 2;
            var centerY = (MonitorHeight - Graphics.PreferredBackBufferHeight) / 2;
            
            // Ensure window doesn't go off-screen
            centerX = Math.Max(0, centerX);
            centerY = Math.Max(0, centerY);
            
            window.Position = new Point(centerX, centerY);
        }
    }

    protected override void UnloadContent()
    {
        // Clear scene cache
        s_sceneManager.ClearCache();
        
        // Dispose of the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);

        // Update the audio controller.
        Audio.Update();

        // Update the camera system
        if (Camera != null)
        {
            Camera.Update();
        }
        
        // Update the camera controller for input handling
        if (CameraController != null)
        {
            CameraController.Update(gameTime);
        }
        
        // Update FPS tracking (always track, not just in developer mode)
        UpdateFpsTracking(gameTime);

        if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Exit();
        }

        // Update scene through SceneManager
        s_sceneManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw scene through SceneManager
        s_sceneManager.Draw(gameTime);

        base.Draw(gameTime);
    }

    /// <summary>
    /// Updates the FPS tracking using a rolling 60-frame average
    /// </summary>
    /// <param name="gameTime">Current game time</param>
    private static void UpdateFpsTracking(GameTime gameTime)
    {
        // Store frame time in circular buffer
        s_frameTimeBuffer[s_frameTimeIndex] = gameTime.ElapsedGameTime.TotalSeconds;
        s_frameTimeIndex = (s_frameTimeIndex + 1) % s_frameTimeBuffer.Length;
        
        // Mark buffer as filled after first complete cycle
        if (s_frameTimeIndex == 0)
        {
            s_frameBufferFilled = true;
        }
        
        // Calculate average frame time
        int sampleCount = s_frameBufferFilled ? s_frameTimeBuffer.Length : s_frameTimeIndex;
        if (sampleCount > 0)
        {
            double totalTime = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                totalTime += s_frameTimeBuffer[i];
            }
            
            double averageFrameTime = totalTime / sampleCount;
            s_currentFps = averageFrameTime > 0 ? 1.0 / averageFrameTime : 60.0;
            
            // Debug output every 30 frames to verify FPS calculation (show more precision)
            if (s_frameTimeIndex % 30 == 0 && sampleCount > 5) // Show after 5 samples instead of full buffer
            {
                System.Diagnostics.Debug.WriteLine($"FPS Debug - AvgFrameTime: {averageFrameTime:F6}s, FPS: {s_currentFps:F2}, LastFrameTime: {gameTime.ElapsedGameTime.TotalSeconds:F6}s");
            }
        }
    }

    /// <summary>
    /// Ensures the SceneManager is ready for scene transitions
    /// </summary>
    public static void EnableSceneManager()
    {
        // SceneManager is now the only scene management system
        // This method is kept for API compatibility but no longer needs to do anything
    }

    /// <summary>
    /// Transitions to a scene of type T using the cached SceneManager system
    /// </summary>
    /// <typeparam name="T">The type of scene to transition to</typeparam>
    public static void TransitionTo<T>() where T : Scene, new()
    {
        s_sceneManager.TransitionTo<T>();
    }

    /// <summary>
    /// Changes to a specific scene instance
    /// </summary>
    /// <param name="next">The scene instance to transition to</param>
    public static void ChangeScene(Scene next)
    {
        if (next == null) return;
        
        // Cache GameScene instances for resume functionality
        bool shouldCache = next.GetType().Name == "GameScene";
        s_sceneManager.TransitionTo(next, shouldCache);
    }

}
