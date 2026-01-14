using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.UI.Transitions
{
    /// <summary>
    /// Handles fade transitions between UI screens
    /// </summary>
    public class FadeTransition
    {
        private float _duration;
        private float _elapsedTime;
        private bool _isActive;
        private bool _fadeIn;
        private Color _fadeColor;
        
        public bool IsActive => _isActive;
        public float Progress => _duration > 0 ? MathHelper.Clamp(_elapsedTime / _duration, 0f, 1f) : 1f;
        public float Alpha => _fadeIn ? Progress : 1f - Progress;
        
        /// <summary>
        /// Starts a fade transition
        /// </summary>
        /// <param name="fadeIn">True for fade in, false for fade out</param>
        /// <param name="duration">Duration of the fade in seconds</param>
        /// <param name="fadeColor">Color to fade to/from (usually black)</param>
        public void Start(bool fadeIn, float duration = 0.3f, Color? fadeColor = null)
        {
            _fadeIn = fadeIn;
            _duration = duration;
            _elapsedTime = 0f;
            _isActive = true;
            _fadeColor = fadeColor ?? Color.Black;
        }
        
        /// <summary>
        /// Updates the fade transition
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (!_isActive) return;
            
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_elapsedTime >= _duration)
            {
                _isActive = false;
                _elapsedTime = _duration;
            }
        }
        
        /// <summary>
        /// Draws the fade overlay
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!_isActive && Alpha <= 0f) return;
            
            // Create a 1x1 white texture if needed
            var fadeTexture = new Texture2D(graphicsDevice, 1, 1);
            fadeTexture.SetData(new[] { Color.White });
            
            var screenBounds = graphicsDevice.Viewport.Bounds;
            var fadeColorWithAlpha = _fadeColor * Alpha;
            
            spriteBatch.Draw(fadeTexture, screenBounds, fadeColorWithAlpha);
            
            fadeTexture.Dispose();
        }
    }
}