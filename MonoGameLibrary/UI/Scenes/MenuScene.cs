using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.UI.Managers;

namespace MonoGameLibrary.UI.Scenes
{
    /// <summary>
    /// Base scene class for menu screens with common navigation functionality
    /// </summary>
    public abstract class MenuScene : UIScene
    {
        protected bool AllowEscapeToExit { get; set; } = true;
        protected bool AllowBackgroundInput { get; set; } = false;
        
        /// <summary>
        /// Menu scenes typically consume all input by default
        /// </summary>
        protected override bool ConsumeAllInput => !AllowBackgroundInput;
        
        public override void Update(GameTime gameTime)
        {
            // Handle common menu navigation before UI processing
            HandleMenuNavigation(gameTime);
            
            base.Update(gameTime);
        }
        
        /// <summary>
        /// Handles common menu navigation like escape key
        /// </summary>
        protected virtual void HandleMenuNavigation(GameTime gameTime)
        {
            if (AllowEscapeToExit && Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            {
                OnEscapePressed();
            }
        }
        
        /// <summary>
        /// Called when escape key is pressed. Override for custom behavior.
        /// </summary>
        protected virtual void OnEscapePressed()
        {
            // Default behavior - exit to previous scene or game
            // Override in derived classes for specific menu behavior
        }
        
        /// <summary>
        /// Sets up a menu UI manager. Override in derived classes.
        /// </summary>
        protected override void InitializeUI()
        {
            // Create menu-specific UI manager in derived classes
            base.InitializeUI();
        }
        
        /// <summary>
        /// Transitions to another menu scene with fade effect
        /// </summary>
        protected virtual void TransitionToMenu(MenuScene targetScene, float fadeDuration = 0.3f)
        {
            if (UIManager != null)
            {
                // Start fade out transition
                UIManager.SwitchToScreen("", true, fadeDuration);
                
                // Schedule scene change after fade
                // Note: In a real implementation, you'd need a proper callback system
                Core.ChangeScene(targetScene);
            }
            else
            {
                Core.ChangeScene(targetScene);
            }
        }
        
        /// <summary>
        /// Transitions back to the game scene
        /// </summary>
        protected virtual void TransitionToGame()
        {
            // Override in derived classes to transition to appropriate game scene
        }
        
        /// <summary>
        /// Exits the game application
        /// </summary>
        protected virtual void ExitGame()
        {
            Core.Instance.Exit();
        }
    }
}