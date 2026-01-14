using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Utilities;

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
    /// Called when scene becomes inactive (transitioning away)
    /// </summary>
    public virtual void OnPause() { }

    /// <summary>
    /// Called when scene becomes active (transitioning back)
    /// </summary>
    public virtual void OnResume() { }

    /// <summary>
    /// Updates this scene.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
    public virtual void Update(GameTime gameTime)
    {
        // Developer mode hotkeys (available when game is in debug mode)
        if (Core.Input.Keyboard.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F1))
        {
            Core.ToggleDeveloperMode();
        }
        
        if (Core.Input.Keyboard.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F2))
        {
            Core.ToggleCollisionBoxes();
        }
        
        // TEMPORARY: F3 to toggle VSync for testing
        if (Core.Input.Keyboard.WasKeyJustPressed(Microsoft.Xna.Framework.Input.Keys.F3))
        {
            Core.Graphics.SynchronizeWithVerticalRetrace = !Core.Graphics.SynchronizeWithVerticalRetrace;
            Core.Graphics.ApplyChanges();
        }
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
    /// Draws developer mode overlay indicators with FPS display
    /// </summary>
    protected virtual void DrawDeveloperOverlay()
    {
        // Use the centralized debug system for drawing
        DebugSystem.DrawDebugOverlay(Core.SpriteBatch, Core.GetFpsDisplayPosition(), Core.CurrentFps, Core.ScaleMatrix);
    }

    /// <summary>
    /// Helper method to begin SpriteBatch with automatic content scaling and optional camera transformation
    /// </summary>
    /// <param name="sortMode">Sprite sorting mode</param>
    /// <param name="blendState">Blend state</param>
    /// <param name="samplerState">Sampler state</param>
    /// <param name="depthStencilState">Depth stencil state</param>
    /// <param name="rasterizerState">Rasterizer state</param>
    /// <param name="useCamera">Whether to apply camera transformation (default: true)</param>
    /// <param name="useScaling">Whether to apply content scaling (default: true)</param>
    protected void BeginScaled(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                              BlendState blendState = null,
                              SamplerState samplerState = null, 
                              DepthStencilState depthStencilState = null,
                              RasterizerState rasterizerState = null,
                              bool useCamera = true,
                              bool useScaling = true)
    {
        Matrix transformMatrix;
        
        if (useScaling && useCamera)
        {
            transformMatrix = Core.CameraMatrix;
        }
        else if (useScaling && !useCamera)
        {
            transformMatrix = Core.ScaleMatrix;
        }
        else
        {
            transformMatrix = Matrix.Identity;
        }
        
        Core.SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, transformMatrix);
    }

    /// <summary>
    /// Helper method to begin SpriteBatch for UI rendering (no camera transformation, only content scaling)
    /// </summary>
    /// <param name="sortMode">Sprite sorting mode</param>
    /// <param name="blendState">Blend state</param>
    /// <param name="samplerState">Sampler state</param>
    /// <param name="depthStencilState">Depth stencil state</param>
    /// <param name="rasterizerState">Rasterizer state</param>
    protected void BeginScaledUI(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                                 BlendState blendState = null,
                                 SamplerState samplerState = null, 
                                 DepthStencilState depthStencilState = null,
                                 RasterizerState rasterizerState = null)
    {
        BeginScaled(sortMode, blendState, samplerState, depthStencilState, rasterizerState, useCamera: false, useScaling: true);
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
