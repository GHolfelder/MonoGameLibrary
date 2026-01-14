using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.UI.Input;
using MonoGameLibrary.UI.Transitions;
using System;

namespace MonoGameLibrary.UI.Managers
{
    /// <summary>
    /// Base class for UI managers that handles screen management and input consumption
    /// </summary>
    public abstract class UIManagerBase : IInputConsumer
    {
        protected FadeTransition FadeTransition { get; private set; }
        protected bool IsInitialized { get; private set; }
        
        public virtual int Priority => 50;
        public virtual bool IsActive => IsInitialized;
        
        /// <summary>
        /// Event fired when a screen transition is completed
        /// </summary>
        public event Action<string> ScreenTransitionCompleted;
        
        protected UIManagerBase()
        {
            FadeTransition = new FadeTransition();
        }
        
        /// <summary>
        /// Initializes the UI manager
        /// </summary>
        /// <param name="content">Content manager for loading assets</param>
        /// <param name="gumProjectPath">Path to project file (unused in base implementation)</param>
        public virtual void Initialize(ContentManager content, string gumProjectPath = null)
        {
            InitializeScreens();
            IsInitialized = true;
        }
        
        /// <summary>
        /// Initializes screens for this UI manager. Override in derived classes.
        /// </summary>
        protected virtual void InitializeScreens()
        {
            // Override in derived classes to set up screens
        }
        
        /// <summary>
        /// Updates the UI manager
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            FadeTransition.Update(gameTime);
            
            if (IsActive)
            {
                UpdateScreen(gameTime);
            }
        }
        
        /// <summary>
        /// Updates the current screen. Override for custom screen logic.
        /// </summary>
        protected virtual void UpdateScreen(GameTime gameTime)
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Draws the UI manager
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (IsActive)
            {
                DrawScreen(gameTime, spriteBatch);
            }
            
            // Draw fade transition overlay
            if (FadeTransition.IsActive || FadeTransition.Alpha > 0f)
            {
                FadeTransition.Draw(spriteBatch, graphicsDevice);
            }
        }
        
        /// <summary>
        /// Draws the current screen. Override for custom rendering.
        /// </summary>
        protected virtual void DrawScreen(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Override in derived classes
        }
        
        /// <summary>
        /// Switches to a different screen with optional fade transition
        /// </summary>
        /// <param name="screenName">Name of the screen to switch to</param>
        /// <param name="useFadeTransition">Whether to use fade transition</param>
        /// <param name="fadeDuration">Duration of fade transition</param>
        public virtual void SwitchToScreen(string screenName, bool useFadeTransition = true, float fadeDuration = 0.3f)
        {
            if (useFadeTransition)
            {
                // Start fade out, then switch screen when fade completes
                FadeTransition.Start(false, fadeDuration / 2f);
                PerformScreenSwitch(screenName);
                FadeTransition.Start(true, fadeDuration / 2f);
            }
            else
            {
                PerformScreenSwitch(screenName);
            }
        }
        
        /// <summary>
        /// Performs the actual screen switch
        /// </summary>
        protected virtual void PerformScreenSwitch(string screenName)
        {
            OnScreenChanged(screenName);
            ScreenTransitionCompleted?.Invoke(screenName);
        }
        
        /// <summary>
        /// Called when screen changes. Override for custom logic.
        /// </summary>
        protected virtual void OnScreenChanged(string screenName)
        {
            // Override in derived classes for screen-specific logic
        }
        
        /// <summary>
        /// Processes input for the UI manager
        /// </summary>
        public virtual bool ProcessInput(InputContext context, GameTime gameTime)
        {
            if (!IsActive) return false;
            
            return HandleUIInput(context, gameTime);
        }
        
        /// <summary>
        /// Handles UI-specific input. Override in derived classes.
        /// </summary>
        protected virtual bool HandleUIInput(InputContext context, GameTime gameTime)
        {
            // Override in derived classes for specific input handling
            return false;
        }
        
        /// <summary>
        /// Disposes of UI resources
        /// </summary>
        public virtual void Dispose()
        {
            IsInitialized = false;
        }
    }
}