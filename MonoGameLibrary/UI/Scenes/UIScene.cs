using Microsoft.Xna.Framework;
using MonoGameLibrary.Scenes;
using MonoGameLibrary.UI.Managers;
using MonoGameLibrary.UI.Input;

namespace MonoGameLibrary.UI.Scenes
{
    /// <summary>
    /// Base scene class for UI-focused scenes that integrates with UI managers and input consumption
    /// </summary>
    public abstract class UIScene : Scene
    {
        protected UIManagerBase UIManager { get; set; }
        protected PriorityInputManager InputManager { get; private set; }
        
        /// <summary>
        /// Indicates whether this scene should consume all input when active
        /// </summary>
        protected virtual bool ConsumeAllInput => true;
        
        public override void Initialize()
        {
            InputManager = new PriorityInputManager();
            InitializeUI();
            base.Initialize();
        }
        
        /// <summary>
        /// Initializes the UI manager for this scene. Override in derived classes.
        /// </summary>
        protected virtual void InitializeUI()
        {
            if (UIManager != null)
            {
                UIManager.Initialize(Core.Content);
                InputManager.RegisterConsumer(UIManager);
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            // Process input through priority system first
            var inputContext = InputManager.ProcessInput(gameTime);
            
            // Update UI manager
            UIManager?.Update(gameTime);
            
            // Only call base scene update if input wasn't consumed or we allow passthrough
            if (!ConsumeAllInput || !inputContext.IsConsumed)
            {
                base.Update(gameTime);
            }
        }
        
        public override void Draw(GameTime gameTime)
        {
            // Draw world content first (if any)
            DrawWorld(gameTime);
            
            // Draw UI on top using BeginScaledUI for proper scaling
            BeginScaledUI();
            {
                UIManager?.Draw(gameTime, Core.SpriteBatch, Core.GraphicsDevice);
                Core.SpriteBatch.End();
            }
            
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Draws world content before UI. Override in scenes that need world rendering.
        /// </summary>
        protected virtual void DrawWorld(GameTime gameTime)
        {
            // Override in derived classes that need world rendering
        }
        
        /// <summary>
        /// Called when the scene is being disposed
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UIManager?.Dispose();
                InputManager?.Clear();
            }
            base.Dispose(disposing);
        }
    }
}