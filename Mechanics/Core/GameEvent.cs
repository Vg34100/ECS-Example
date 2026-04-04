using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Base class for all game events
    /// Events are used for decoupled communication between systems
    /// </summary>
    public abstract class GameEvent
    {
        public float Timestamp { get; set; }

        protected GameEvent()
        {
            Timestamp = 0f; // Set by EventSystem when published
        }
    }

    // ===== Common Game Events =====

    /// <summary>
    /// Fired when an enemy dies
    /// </summary>
    public class EnemyDiedEvent : GameEvent
    {
        public Entity Enemy { get; set; }
        public Vector2 Position { get; set; }
        public int ScoreValue { get; set; }

        public EnemyDiedEvent(Entity enemy, Vector2 position, int scoreValue = 100)
        {
            Enemy = enemy;
            Position = position;
            ScoreValue = scoreValue;
        }
    }

    /// <summary>
    /// Fired when player takes damage
    /// </summary>
    public class PlayerDamagedEvent : GameEvent
    {
        public int Damage { get; set; }
        public int RemainingHealth { get; set; }
        public Entity Source { get; set; } // What damaged the player

        public PlayerDamagedEvent(int damage, int remainingHealth, Entity source)
        {
            Damage = damage;
            RemainingHealth = remainingHealth;
            Source = source;
        }
    }

    /// <summary>
    /// Fired when player dies
    /// </summary>
    public class PlayerDiedEvent : GameEvent
    {
        public Vector2 Position { get; set; }

        public PlayerDiedEvent(Vector2 position)
        {
            Position = position;
        }
    }

    /// <summary>
    /// Fired when a level is completed
    /// </summary>
    public class LevelCompleteEvent : GameEvent
    {
        public string LevelName { get; set; }
        public float CompletionTime { get; set; }
        public int Score { get; set; }

        public LevelCompleteEvent(string levelName, float completionTime, int score)
        {
            LevelName = levelName;
            CompletionTime = completionTime;
            Score = score;
        }
    }

    /// <summary>
    /// Fired when an item is collected
    /// </summary>
    public class ItemCollectedEvent : GameEvent
    {
        public string ItemId { get; set; }
        public Entity Collector { get; set; }

        public ItemCollectedEvent(string itemId, Entity collector)
        {
            ItemId = itemId;
            Collector = collector;
        }
    }

    /// <summary>
    /// Fired when game state changes
    /// </summary>
    public class StateChangedEvent : GameEvent
    {
        public GameState OldState { get; set; }
        public GameState NewState { get; set; }

        public StateChangedEvent()
        {
            // Parameterless constructor for object initializer syntax
        }
    }
}
