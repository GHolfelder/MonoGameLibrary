using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;

namespace MonoGameLibrary.Input;

/// <summary>
/// Provides high-level input for character control.
/// Implementations may use keyboard, gamepad, AI, or network inputs.
/// </summary>
public interface IInputProvider
{
    /// <summary>
    /// Returns a movement vector (normalized or zero) and a sprint flag for this frame.
    /// </summary>
    (Vector2 movement, bool sprint) GetMovement(GameTime gameTime);
}

/// <summary>
/// Default input provider that reads from Core.Input (keyboard/gamepad).
/// </summary>
public class CoreInputProvider : IInputProvider
{
    public (Vector2 movement, bool sprint) GetMovement(GameTime gameTime)
    {
        var keyboard = Core.Input.Keyboard;
        var gamePad = Core.Input.GamePads[(int)PlayerIndex.One];

        Vector2 movementVector = Vector2.Zero;
        bool isSprinting = false;

        // Keyboard
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) movementVector.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) movementVector.Y += 1;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) movementVector.X -= 1;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) movementVector.X += 1;

        // Sprint keys
        isSprinting = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.Space);

        // Gamepad (overrides keyboard if significant input)
        if (gamePad.IsConnected)
        {
            Vector2 thumbstick = gamePad.LeftThumbStick;
            if (thumbstick.LengthSquared() > 0.1f) // dead zone
            {
                movementVector = new Vector2(thumbstick.X, -thumbstick.Y);
            }
            if (gamePad.IsButtonDown(Buttons.A) || gamePad.IsButtonDown(Buttons.X))
                isSprinting = true;
        }

        // Normalize if needed
        if (movementVector.LengthSquared() > 1e-5f)
            movementVector.Normalize();

        return (movementVector, isSprinting);
    }
}
