using Microsoft.Xna.Framework;

namespace MonoGameLibrary.UI.Input
{
    /// <summary>
    /// Interface for UI elements that can consume input with priority-based processing
    /// </summary>
    public interface IInputConsumer
    {
        /// <summary>
        /// Priority level for input processing. Higher values are processed first.
        /// Common ranges: Modals (90-100), Overlays (50-89), Game (0-49)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// Determines if this consumer is active and should process input
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Processes input and returns true if input was consumed
        /// </summary>
        /// <param name="context">The input context to process</param>
        /// <param name="gameTime">The current game time</param>
        /// <returns>True if input was consumed and should not be passed to lower priority consumers</returns>
        bool ProcessInput(InputContext context, GameTime gameTime);
    }
}