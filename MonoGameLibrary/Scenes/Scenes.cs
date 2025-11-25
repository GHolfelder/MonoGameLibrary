using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Scenes;

/// <summary>
/// The base Scene class is an abstract class for scenes that provides common functionality for all scenes.
/// This class implements the IDisposable interface. This provides a standardized in method to release the 
/// resources held by a scene when it is no longer needed.
/// </summary>
public abstract class Scene : IDisposable
{
    /// <summary>
    /// Gets the ContentManager used for loading scene-specific assets.
    /// </summary>
    /// <remarks>
    /// Assets loaded through this ContentManager will be automatically unloaded when this scene ends.
    /// </remarks>
    protected ContentManager Content { get; }

    /// <summary>
    /// Gets a value that indicates if the scene has been disposed of.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates a new scene instance.
    /// </summary>
    public Scene()
    {
        // Create a content manager for the scene
        Content = new ContentManager(Core.Content.ServiceProvider);

        // Set the root directory for content to the same as the root directory
        // for the game's content.
        Content.RootDirectory = Core.Content.RootDirectory;
    }

    // Finalizer (Destructor), called when object is cleaned up by garbage collector.
    ~Scene() => Dispose(false);

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    /// <remarks>
    /// When overriding this in a derived class, ensure that base.Initialize()
    /// still called as this is when LoadContent is called.
    /// </remarks>
    public virtual void Initialize()
    {
        LoadContent();
    }

    /// <summary>
    /// Override to provide logic to load content for the scene.
    /// </summary>
    public virtual void LoadContent() { }

    /// <summary>
    /// Unloads scene-specific content.
    /// </summary>
    public virtual void UnloadContent()
    {
        Content.Unload();
    }

    /// <summary>
    /// Updates this scene.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public virtual void Update(GameTime gameTime)
    {
#if DEBUG
        // Developer mode hotkeys
        if (Core.Input.Keyboard.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F1))
        {
            System.Diagnostics.Debug.WriteLine("F1 key detected - toggling developer mode");
            Core.ToggleDeveloperMode();
        }
        
        if (Core.Input.Keyboard.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F2))
        {
            System.Diagnostics.Debug.WriteLine("F2 key detected - toggling collision boxes");
            Core.ToggleCollisionBoxes();
        }
#endif
    }

    /// <summary>
    /// Draws this scene.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public virtual void Draw(GameTime gameTime)
    {
        // Draw developer overlay automatically at the end of the frame
        DrawDeveloperOverlay();
    }
    
    /// <summary>
    /// Draws developer mode overlay indicators
    /// </summary>
    protected virtual void DrawDeveloperOverlay()
    {
#if DEBUG
        if (Core.DeveloperMode)
        {
            // Begin a separate SpriteBatch session for developer overlay
            Core.SpriteBatch.Begin();
            
            // Draw small red circle at top center to indicate dev mode
            var screenCenter = new Vector2(Core.Graphics.PreferredBackBufferWidth / 2f, 20);
            MonoGameLibrary.Graphics.Collision.CollisionDraw.DrawCircle(Core.SpriteBatch, screenCenter, 8f, Color.Red);
            
            // Optional: Draw additional indicators for specific modes
            if (Core.ShowCollisionBoxes)
            {
                var boxIndicator = new Vector2(screenCenter.X + 25, 20);
                var rect = new Rectangle((int)boxIndicator.X - 6, (int)boxIndicator.Y - 6, 12, 12);
                MonoGameLibrary.Graphics.Collision.CollisionDraw.DrawRectangle(Core.SpriteBatch, rect, Color.Yellow);
            }
            
            Core.SpriteBatch.End();
        }
#endif
    }

    /// <summary>
    /// Disposes of this scene.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of this scene.
    /// </summary>
    /// <param name="disposing">'
    /// Indicates whether managed resources should be disposed.  This value is only true when called from the main
    /// Dispose method.  When called from the finalizer, this will be false.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            UnloadContent();
            Content.Dispose();
        }

        IsDisposed = true;
    }

}
