using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameLibrary.UI.Input
{
    /// <summary>
    /// Manages priority-based input consumption for UI elements
    /// </summary>
    public class PriorityInputManager
    {
        private readonly List<IInputConsumer> _consumers = new List<IInputConsumer>();
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Registers an input consumer with the manager
        /// </summary>
        public void RegisterConsumer(IInputConsumer consumer)
        {
            lock (_lockObject)
            {
                if (!_consumers.Contains(consumer))
                {
                    _consumers.Add(consumer);
                    _consumers.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority descending
                }
            }
        }
        
        /// <summary>
        /// Unregisters an input consumer from the manager
        /// </summary>
        public void UnregisterConsumer(IInputConsumer consumer)
        {
            lock (_lockObject)
            {
                _consumers.Remove(consumer);
            }
        }
        
        /// <summary>
        /// Processes input through all registered consumers in priority order
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <returns>Input context with remaining unconsumed input</returns>
        public InputContext ProcessInput(GameTime gameTime)
        {
            var inputContext = new InputContext
            {
                Keyboard = Keyboard.GetState(),
                Mouse = Mouse.GetState(),
                GamePad = GamePad.GetState(PlayerIndex.One)
            };
            
            lock (_lockObject)
            {
                foreach (var consumer in _consumers.Where(c => c.IsActive))
                {
                    if (consumer.ProcessInput(inputContext, gameTime))
                    {
                        // Input was consumed, mark context as consumed
                        inputContext.Consume();
                        break;
                    }
                }
            }
            
            return inputContext;
        }
        
        /// <summary>
        /// Clears all registered consumers
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _consumers.Clear();
            }
        }
        
        /// <summary>
        /// Gets the number of registered consumers
        /// </summary>
        public int ConsumerCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _consumers.Count;
                }
            }
        }
    }
}