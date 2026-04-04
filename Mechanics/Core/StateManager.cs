using System;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Manages game state transitions with callbacks and event integration.
    /// Use this to control which systems run and handle state-specific logic.
    /// </summary>
    public class StateManager
    {
        private GameState _currentState;
        private GameState _previousState;
        private readonly EventSystem _eventSystem;

        // Callbacks for state changes
        private readonly Dictionary<GameState, Action> _onEnterCallbacks = new Dictionary<GameState, Action>();
        private readonly Dictionary<GameState, Action> _onExitCallbacks = new Dictionary<GameState, Action>();
        private readonly List<Action<GameState, GameState>> _onStateChangedCallbacks = new List<Action<GameState, GameState>>();

        /// <summary>Gets the current game state</summary>
        public GameState CurrentState => _currentState;

        /// <summary>Gets the previous game state</summary>
        public GameState PreviousState => _previousState;

        public StateManager(EventSystem eventSystem = null)
        {
            _eventSystem = eventSystem;
            _currentState = GameState.MainMenu;
            _previousState = GameState.MainMenu;
        }

        /// <summary>
        /// Transition to a new game state.
        /// Triggers exit callbacks for old state, enter callbacks for new state,
        /// and publishes a StateChangedEvent if EventSystem is available.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState)
                return;

            GameState oldState = _currentState;

            // Exit old state
            if (_onExitCallbacks.TryGetValue(_currentState, out var exitCallback))
            {
                exitCallback?.Invoke();
            }

            // Update states
            _previousState = _currentState;
            _currentState = newState;

            // Enter new state
            if (_onEnterCallbacks.TryGetValue(_currentState, out var enterCallback))
            {
                enterCallback?.Invoke();
            }

            // Notify listeners
            foreach (var callback in _onStateChangedCallbacks)
            {
                callback?.Invoke(oldState, newState);
            }

            // Publish event if EventSystem is available
            _eventSystem?.Publish(new StateChangedEvent
            {
                OldState = oldState,
                NewState = newState
            });
        }

        /// <summary>
        /// Register a callback to run when entering a specific state.
        /// </summary>
        public void OnEnter(GameState state, Action callback)
        {
            _onEnterCallbacks[state] = callback;
        }

        /// <summary>
        /// Register a callback to run when exiting a specific state.
        /// </summary>
        public void OnExit(GameState state, Action callback)
        {
            _onExitCallbacks[state] = callback;
        }

        /// <summary>
        /// Register a callback to run whenever any state change occurs.
        /// Receives (oldState, newState) as parameters.
        /// </summary>
        public void OnStateChanged(Action<GameState, GameState> callback)
        {
            if (callback != null && !_onStateChangedCallbacks.Contains(callback))
            {
                _onStateChangedCallbacks.Add(callback);
            }
        }

        /// <summary>
        /// Remove a state changed callback.
        /// </summary>
        public void RemoveStateChangedCallback(Action<GameState, GameState> callback)
        {
            _onStateChangedCallbacks.Remove(callback);
        }

        /// <summary>
        /// Check if currently in a specific state.
        /// </summary>
        public bool IsInState(GameState state)
        {
            return _currentState == state;
        }

        /// <summary>
        /// Check if currently in any of the specified states.
        /// </summary>
        public bool IsInAnyState(params GameState[] states)
        {
            foreach (var state in states)
            {
                if (_currentState == state)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all callbacks. Useful for cleanup or state reset.
        /// </summary>
        public void ClearCallbacks()
        {
            _onEnterCallbacks.Clear();
            _onExitCallbacks.Clear();
            _onStateChangedCallbacks.Clear();
        }

        /// <summary>
        /// Reset to initial state (MainMenu).
        /// </summary>
        public void Reset()
        {
            ChangeState(GameState.MainMenu);
        }
    }
}
