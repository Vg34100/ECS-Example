using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// System that updates all TimerComponents.
    /// Handles timer advancement, callbacks, and automatic component removal.
    /// </summary>
    public class TimerSystem
    {
        /// <summary>
        /// Update all timers in the world
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            var timersToRemove = new List<Entity>();

            foreach (var entity in world.Query<TimerComponent>())
            {
                if (!world.TryGetComponent<TimerComponent>(entity, out var timer))
                    continue;

                // Skip paused or finished timers
                if (timer.IsPaused || (!timer.IsRepeating && timer.IsFinished))
                    continue;

                // Advance timer
                timer.Elapsed += deltaTime;

                // Check if timer completed
                if (timer.Elapsed >= timer.Duration)
                {
                    // Invoke callback
                    timer.OnComplete?.Invoke();

                    if (timer.IsRepeating)
                    {
                        // Reset for next iteration
                        timer.Elapsed -= timer.Duration;
                    }
                    else
                    {
                        // Mark as finished
                        timer.IsFinished = true;

                        // Queue for removal if requested
                        if (timer.RemoveOnComplete)
                        {
                            timersToRemove.Add(entity);
                        }
                    }
                }

                // Write modified timer back to world
                world.AddComponent(entity, timer);
            }

            // Remove completed one-shot timers
            foreach (var entity in timersToRemove)
            {
                world.RemoveComponent<TimerComponent>(entity);
            }
        }

        /// <summary>
        /// Create a one-shot timer that fires after a delay
        /// </summary>
        public static TimerComponent CreateDelayedAction(float delay, System.Action callback, bool removeOnComplete = true)
        {
            return new TimerComponent(delay, isRepeating: false, onComplete: callback)
            {
                RemoveOnComplete = removeOnComplete
            };
        }

        /// <summary>
        /// Create a repeating timer that fires every interval
        /// </summary>
        public static TimerComponent CreateRepeatingTimer(float interval, System.Action callback)
        {
            return new TimerComponent(interval, isRepeating: true, onComplete: callback);
        }

        /// <summary>
        /// Create a cooldown timer (like a one-shot, but you check IsFinished rather than using callback)
        /// </summary>
        public static TimerComponent CreateCooldown(float duration, string name = "")
        {
            return new TimerComponent(duration, isRepeating: false, onComplete: null)
            {
                Name = name,
                RemoveOnComplete = false
            };
        }
    }
}
