using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Utilities;

/// <summary>
/// Centralized debug and developer mode system for MonoGame Library
/// </summary>
public static class DebugSystem
{
    // Debug Message System
    private static List<DebugMessage> s_debugMessages = new List<DebugMessage>();
    private const int MAX_DEBUG_MESSAGES = 10;
    
    // Developer Mode System
    private static bool s_developerMode = false;
    private static bool s_showCollisionBoxes = false;
    private static SpriteFont s_debugFont = null;
    private static float s_debugFontScale = 1.0f;
    
    /// <summary>
    /// Represents a debug message with content, count, and timestamp
    /// </summary>
    public class DebugMessage
    {
        /// <summary>
        /// Gets or sets the content of the debug message
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Gets or sets the number of times this message has been repeated
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when this message was last seen
        /// </summary>
        public DateTime LastSeen { get; set; }
        
        /// <summary>
        /// Gets the display text for this debug message
        /// </summary>
        /// <returns>Formatted text showing content and count if greater than 1</returns>
        public string GetDisplayText()
        {
            return Count > 1 ? $"{Content} (x{Count})" : Content;
        }
    }
    
    /// <summary>
    /// Gets whether the application is running in debug mode
    /// </summary>
    public static bool IsDebugMode
    {
        get
        {
            bool isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            // Also check if debugger is attached (covers game in debug mode)
            return isDebug || System.Diagnostics.Debugger.IsAttached;
        }
    }
    
    /// <summary>
    /// Gets or sets whether developer mode is active (only available in debug mode)
    /// </summary>
    public static bool DeveloperMode
    {
        get => IsDebugMode && s_developerMode;
        set { if (IsDebugMode) s_developerMode = value; }
    }
    
    /// <summary>
    /// Gets or sets whether collision boxes should be visible (only available in debug mode)
    /// </summary>
    public static bool ShowCollisionBoxes
    {
        get => IsDebugMode && s_showCollisionBoxes && s_developerMode;
        set { if (IsDebugMode) s_showCollisionBoxes = value; }
    }
    
    /// <summary>
    /// Gets or sets whether debug messages should be displayed
    /// </summary>
    public static bool ShowDebugMessages { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the font used for developer mode text rendering (collision markers, debug info, etc.).
    /// Can be set to any SpriteFont loaded by the game.
    /// </summary>
    public static SpriteFont DebugFont
    {
        get => s_debugFont;
        set => s_debugFont = value;
    }
    
    /// <summary>
    /// Gets or sets the scaling factor for debug font rendering.
    /// Default is 1.0f. Use values like 0.8f for smaller text or 1.2f for larger text.
    /// </summary>
    public static float DebugFontScale
    {
        get => s_debugFontScale;
        set => s_debugFontScale = Math.Max(0.1f, value); // Prevent negative or zero scaling
    }
    
    /// <summary>
    /// Gets the current list of debug messages
    /// </summary>
    public static IReadOnlyList<DebugMessage> DebugMessages => s_debugMessages;
    
    /// <summary>
    /// Toggles developer mode and all associated debug features (only works in debug mode)
    /// </summary>
    public static void ToggleDeveloperMode()
    {
        if (IsDebugMode)
        {
            s_developerMode = !s_developerMode;
            if (!s_developerMode)
            {
                // Turn off all debug features when exiting dev mode
                s_showCollisionBoxes = false;
            }
        }
    }
    
    /// <summary>
    /// Toggles collision box visibility (only works when developer mode is active)
    /// </summary>
    public static void ToggleCollisionBoxes()
    {
        if (IsDebugMode && s_developerMode)
        {
            s_showCollisionBoxes = !s_showCollisionBoxes;
        }
    }
    
    /// <summary>
    /// Adds a debug message to the centralized debug messaging system
    /// </summary>
    /// <param name="message">The message to add</param>
    public static void AddDebugMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        
        var now = DateTime.Now;
        
        // Check for duplicate messages within the last 5 seconds
        var existingMessage = s_debugMessages.FirstOrDefault(m => 
            m.Content == message && (now - m.LastSeen).TotalSeconds <= 5);
        
        if (existingMessage != null)
        {
            // Update existing message
            existingMessage.Count++;
            existingMessage.LastSeen = now;
        }
        else
        {
            // Add new message
            s_debugMessages.Add(new DebugMessage
            {
                Content = message,
                Count = 1,
                LastSeen = now
            });
        }
        
        // Trim list to maximum size, keeping most recent messages
        if (s_debugMessages.Count > MAX_DEBUG_MESSAGES)
        {
            s_debugMessages = s_debugMessages
                .OrderBy(m => m.LastSeen)
                .Skip(s_debugMessages.Count - MAX_DEBUG_MESSAGES)
                .ToList();
        }
    }
    
    /// <summary>
    /// Draws the complete debug overlay including FPS display, debug messages, and collision indicators
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with</param>
    /// <param name="basePosition">The base position for the debug overlay</param>
    /// <param name="currentFps">The current FPS value</param>
    /// <param name="scaleMatrix">The scale matrix for virtual coordinates</param>
    public static void DrawDebugOverlay(SpriteBatch spriteBatch, Vector2 basePosition, double currentFps, Matrix scaleMatrix)
    {
        if (DeveloperMode && DebugFont != null)
        {
            // Begin a separate SpriteBatch session for developer overlay using virtual coordinates
            spriteBatch.Begin(transformMatrix: scaleMatrix);
            
            // Prepare FPS text and measure its size for dynamic background sizing
            var fpsText = $"{currentFps:F2} fps";
            float fontScale = DebugFontScale;
            Vector2 textSize = DebugFont.MeasureString(fpsText) * fontScale;
            
            // Calculate background size based on text dimensions with padding
            int padding = 10;
            int backgroundHeight = (int)textSize.Y + padding;
            float maxWidth = textSize.X;
            
            // Add space for debug messages if they're shown
            if (ShowDebugMessages && DebugMessages.Count > 0)
            {
                float scaledSpacing = 5f * fontScale;
                backgroundHeight += (int)scaledSpacing; // Space between FPS and messages
                
                foreach (var message in DebugMessages)
                {
                    string displayText = message.GetDisplayText();
                    Vector2 messageSize = DebugFont.MeasureString(displayText) * fontScale;
                    backgroundHeight += (int)(messageSize.Y + scaledSpacing);
                    maxWidth = Math.Max(maxWidth, messageSize.X);
                }
            }
            
            // Add space for collision indicator to the right of FPS text if enabled
            if (ShowCollisionBoxes)
            {
                maxWidth = Math.Max(maxWidth, textSize.X + 25); // Space for indicator to the right
            }
            
            // Draw semi-transparent background sized to fit content
            var backgroundRect = new Rectangle(
                (int)(basePosition.X - 5), 
                (int)(basePosition.Y - 5), 
                (int)(maxWidth + padding), 
                backgroundHeight);
            MonoGameLibrary.Graphics.Collision.CollisionDraw.DrawFilledRectangle(
                spriteBatch, backgroundRect, Color.Black * 0.5f);
            
            // Draw FPS text at base position using same scale as object names
            spriteBatch.DrawString(DebugFont, fpsText, basePosition, Color.White, 
                0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
            
            // Draw yellow square indicator to the right of FPS text when collision boxes enabled
            // (enlarged from 12x12 to 16x16)
            if (ShowCollisionBoxes)
            {
                var squarePosition = new Vector2(basePosition.X + textSize.X + 10, basePosition.Y);
                var rect = new Rectangle((int)squarePosition.X, (int)squarePosition.Y, 16, 16);
                MonoGameLibrary.Graphics.Collision.CollisionDraw.DrawFilledRectangle(
                    spriteBatch, rect, Color.Yellow);
            }
            
            // Draw debug messages below FPS display when enabled
            if (ShowDebugMessages && DebugMessages.Count > 0)
            {
                float scaledSpacing = 5f * fontScale;
                var messagePosition = new Vector2(basePosition.X, basePosition.Y + textSize.Y + scaledSpacing);
                
                foreach (var message in DebugMessages)
                {
                    string displayText = message.GetDisplayText();
                    spriteBatch.DrawString(DebugFont, displayText, messagePosition, Color.Yellow,
                        0f, Vector2.Zero, fontScale, SpriteEffects.None, 0f);
                    
                    // Move to next line position
                    Vector2 messageSize = DebugFont.MeasureString(displayText) * fontScale;
                    messagePosition.Y += messageSize.Y + scaledSpacing;
                }
            }
            
            spriteBatch.End();
        }
        else if (DeveloperMode && ShowCollisionBoxes)
        {
            // Fallback for when no debug font is set - just draw collision indicator
            spriteBatch.Begin(transformMatrix: scaleMatrix);
            
            // Draw yellow square indicator to the right of where FPS would be when collision boxes enabled
            var squarePosition = new Vector2(basePosition.X + 80, basePosition.Y); // Approximate position to the right
            var rect = new Rectangle((int)squarePosition.X, (int)squarePosition.Y, 16, 16);
            MonoGameLibrary.Graphics.Collision.CollisionDraw.DrawFilledRectangle(
                spriteBatch, rect, Color.Yellow);
            
            spriteBatch.End();
        }
    }
}