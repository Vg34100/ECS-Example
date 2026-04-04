using System;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Global event system for decoupled communication between systems
    /// Uses publish/subscribe pattern with type-safe events
    /// </summary>
    public class EventSystem
    {
        private Dictionary<Type, List<Delegate>> _subscribers;
        private float _gameTime;

        public EventSystem()
        {
            _subscribers = new Dictionary<Type, List<Delegate>>();
            _gameTime = 0f;
        }

        /// <summary>
        /// Subscribe to an event type
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);

            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers
        /// </summary>
        public void Publish<T>(T gameEvent) where T : GameEvent
        {
            Type eventType = typeof(T);

            // Set timestamp
            gameEvent.Timestamp = _gameTime;

            if (_subscribers.ContainsKey(eventType))
            {
                // Create a copy of subscribers list to avoid modification during iteration
                var handlers = new List<Delegate>(_subscribers[eventType]);

                foreach (var handler in handlers)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(gameEvent);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't crash
                        Console.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all subscriptions (useful for cleanup/reset)
        /// </summary>
        public void Clear()
        {
            _subscribers.Clear();
        }

        /// <summary>
        /// Clear all subscriptions for a specific event type
        /// </summary>
        public void Clear<T>() where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Clear();
            }
        }

        /// <summary>
        /// Update game time (call this each frame)
        /// </summary>
        public void Update(float deltaTime)
        {
            _gameTime += deltaTime;
        }

        /// <summary>
        /// Get count of subscribers for an event type (useful for testing)
        /// </summary>
        public int GetSubscriberCount<T>() where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_subscribers.ContainsKey(eventType))
            {
                return _subscribers[eventType].Count;
            }
            return 0;
        }

        /// <summary>
        /// Check if there are any subscribers for an event type
        /// </summary>
        public bool HasSubscribers<T>() where T : GameEvent
        {
            return GetSubscriberCount<T>() > 0;
        }
    }
}
