using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Graphics.Collision;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    // The scene that is currently active.
    private static Scene s_activeScene;

    // The next scene to switch to, if there is one.
    private static Scene s_nextScene;

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

    private static bool s_developerMode = false;
    private static bool s_showCollisionBoxes = false;
    
    /// <summary>
    /// Gets whether the application is running in debug mode
    /// </summary>
    private static bool IsDebugMode
    {
        get
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            // Also check if debugger is attached (covers game in debug mode)
            return isDebug || System.Diagnostics.Debugger.IsAttached;
        }
    }
    
    /// <summary>
    /// Gets or sets whether developer mode is active (only available in debug mode)
    /// </summary>
    public static bool DeveloperMode
    {
        get => IsDebugMode && s_developerMode;
        set { if (IsDebugMode) s_developerMode = value; }
    }
    
    /// <summary>
    /// Gets or sets whether collision boxes should be visible (only available in debug mode)
    /// </summary>
    public static bool ShowCollisionBoxes
    {
        get => IsDebugMode && s_showCollisionBoxes && s_developerMode;
        set { if (IsDebugMode) s_showCollisionBoxes = value; }
    }
    
    /// <summary>
    /// Toggles developer mode and all associated debug features (only works in debug mode)
    /// </summary>
    public static void ToggleDeveloperMode()
    {
        if (IsDebugMode)
        {
            s_developerMode = !s_developerMode;
            if (!s_developerMode)
            {
                // Turn off all debug features when exiting dev mode
                s_showCollisionBoxes = false;
            }
        }
    }
    
    /// <summary>
    /// Toggles collision box visibility (only works when developer mode is active)
    /// </summary>
    public static void ToggleCollisionBoxes()
    {
        if (IsDebugMode && s_developerMode)
        {
            s_showCollisionBoxes = !s_showCollisionBoxes;
        }
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
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    /// <param name="windowSizePercent">The percentage of monitor size to use (default: 0.8 for 80%).</param>
    public Core(string title, bool fullScreen = false, float windowSizePercent = 0.8f)
    {
        var monitorSize = GetMonitorAwareSize(windowSizePercent);
        Initialize(title, monitorSize.X, monitorSize.Y, fullScreen);
    }

    /// <summary>
    /// Gets a window size appropriate for the current monitor
    /// </summary>
    /// <param name="sizePercent">Percentage of monitor size to use (0.1 to 1.0)</param>
    /// <returns>Point containing width and height</returns>
    public static Point GetMonitorAwareSize(float sizePercent = 0.8f)
    {
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

        if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Exit();
        }

        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (s_nextScene != null)
        {
            TransitionScene();
        }

        // If there is an active scene, update it.
        if (s_activeScene != null)
        {
            s_activeScene.Update(gameTime);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        if (s_activeScene != null)
        {
            s_activeScene.Draw(gameTime);
        }

        base.Draw(gameTime);
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (s_activeScene != next)
        {
            s_nextScene = next;
        }
    }

    private static void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        if (s_activeScene != null)
        {
            s_activeScene.Dispose();
        }

        // Force the garbage collector to collect to ensure memory is cleared.
        GC.Collect();

        // Change the currently active scene to the new scene.
        s_activeScene = s_nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        s_nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        if (s_activeScene != null)
        {
            s_activeScene.Initialize();
        }
    }
}
