using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Stats.Components
{
    /// <summary>
    /// Respawn settings for an entity.
    /// </summary>
    public struct RespawnComponent
    {
        public Vector2 SpawnPosition;
        public float Delay;
        public float TimeRemaining;

        public RespawnComponent(Vector2 spawnPosition, float delay)
        {
            SpawnPosition = spawnPosition;
            Delay = delay;
            TimeRemaining = 0f;
        }
    }
}
