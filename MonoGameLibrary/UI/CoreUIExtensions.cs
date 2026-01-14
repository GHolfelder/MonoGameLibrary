using MonoGameLibrary.UI.Input;

namespace MonoGameLibrary.UI
{
    /// <summary>
    /// Static UI extensions for the Core class to integrate UI functionality
    /// </summary>
    public static class CoreUIExtensions
    {
        private static PriorityInputManager _uiInputManager;
        
        /// <summary>
        /// Gets the global UI input manager for priority-based input consumption
        /// </summary>
        public static PriorityInputManager UIInputManager
        {
            get
            {
                if (_uiInputManager == null)
                {
                    _uiInputManager = new PriorityInputManager();
                }
                return _uiInputManager;
            }
        }
        
        /// <summary>
        /// Initializes the UI system. Call this during Core initialization.
        /// </summary>
        public static void InitializeUI()
        {
            _uiInputManager = new PriorityInputManager();
        }
        
        /// <summary>
        /// Updates the UI input system. Call this during Core update before scene updates.
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <returns>Input context with remaining unconsumed input</returns>
        public static InputContext UpdateUIInput(Microsoft.Xna.Framework.GameTime gameTime)
        {
            return UIInputManager.ProcessInput(gameTime);
        }
        
        /// <summary>
        /// Clears all UI input consumers. Call this during scene transitions if needed.
        /// </summary>
        public static void ClearUIConsumers()
        {
            UIInputManager?.Clear();
        }
    }
}