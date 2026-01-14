using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.UI.Input
{
    /// <summary>
    /// Represents the current input state that can be consumed by UI elements
    /// </summary>
    public class InputContext
    {
        public KeyboardState Keyboard { get; set; }
        public MouseState Mouse { get; set; }
        public GamePadState GamePad { get; set; }
        
        public bool IsConsumed { get; private set; }
        
        /// <summary>
        /// Marks this input context as consumed, preventing further processing
        /// </summary>
        public void Consume()
        {
            IsConsumed = true;
        }
        
        /// <summary>
        /// Creates a masked copy of this input context with specified input types cleared
        /// </summary>
        public InputContext CreateMasked(bool maskKeyboard = false, bool maskMouse = false, bool maskGamePad = false)
        {
            return new InputContext
            {
                Keyboard = maskKeyboard ? new KeyboardState() : Keyboard,
                Mouse = maskMouse ? new MouseState() : Mouse,
                GamePad = maskGamePad ? new GamePadState() : GamePad
            };
        }
        
        /// <summary>
        /// Returns true if any input type has active input
        /// </summary>
        public bool HasAnyInput => 
            Keyboard.GetPressedKeys().Length > 0 || 
            (Mouse.LeftButton == ButtonState.Pressed || Mouse.RightButton == ButtonState.Pressed || Mouse.MiddleButton == ButtonState.Pressed) ||
            (GamePad.Buttons.A == ButtonState.Pressed || GamePad.Buttons.B == ButtonState.Pressed || 
             GamePad.Buttons.X == ButtonState.Pressed || GamePad.Buttons.Y == ButtonState.Pressed ||
             GamePad.ThumbSticks.Left.Length() > 0.1f || GamePad.ThumbSticks.Right.Length() > 0.1f);
    }
}