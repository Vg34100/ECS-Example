using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Stats.Components;

namespace ECS_Base.Mechanics.Stats.Systems
{
    /// <summary>
    /// Counts down delayed removals for entities with death visuals.
    /// </summary>
    public class DeathDespawnSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<DeathDespawnComponent>())
            {
                if (!world.TryGetComponent<DeathDespawnComponent>(entity, out var despawn))
                    continue;

                if (!world.TryGetComponent<StatsComponent>(entity, out var stats) || !stats.IsDead)
                    continue;

                despawn.TimeRemaining -= deltaTime;
                if (despawn.TimeRemaining <= 0f)
                {
                    world.RemoveEntity(entity);
                }
                else
                {
                    world.AddComponent(entity, despawn);
                }
            }
        }
    }
}
