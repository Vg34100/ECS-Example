using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Stats.Components;

namespace ECS_Base.Mechanics.Stats.Systems
{
    /// <summary>
    /// Handles respawning for entities with RespawnComponent.
    /// </summary>
    public class RespawnSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.Query<RespawnComponent, StatsComponent>())
            {
                if (!world.TryGetComponent<RespawnComponent>(entity, out var respawn))
                    continue;

                if (!world.TryGetComponent<StatsComponent>(entity, out var stats))
                    continue;

                if (!stats.IsDead)
                    continue;

                if (respawn.TimeRemaining <= 0f)
                {
                    respawn.TimeRemaining = respawn.Delay;
                }
                else
                {
                    respawn.TimeRemaining -= deltaTime;
                }

                if (respawn.TimeRemaining <= 0f)
                {
                    stats.Health = stats.MaxHealth;
                    stats.IsDead = false;
                    stats.IsInvulnerable = true;
                    world.AddComponent(entity, stats);

                    if (world.TryGetComponent<PositionComponent>(entity, out var pos))
                    {
                        pos.Value = respawn.SpawnPosition;
                        world.AddComponent(entity, pos);
                    }

                    if (world.TryGetComponent<VelocityComponent>(entity, out var vel))
                    {
                        vel.Value = Microsoft.Xna.Framework.Vector2.Zero;
                        world.AddComponent(entity, vel);
                    }

                    // Brief invulnerability after respawn
                    world.AddComponent(entity, new InvulnerabilityComponent(1.0f));
                    respawn.TimeRemaining = 0f;
                }

                world.AddComponent(entity, respawn);
            }
        }
    }
}
