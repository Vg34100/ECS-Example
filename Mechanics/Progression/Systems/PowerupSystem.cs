using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Progression.Components;
using ECS_Base.Mechanics.Stats.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.Mechanics.Progression.Systems
{
    /// <summary>
    /// Applies powerups on player pickup.
    /// </summary>
    public class PowerupSystem
    {
        public void Update(World world, float deltaTime)
        {
            var player = world.GetEntities().FirstOrDefault(e => world.HasComponent<PlayerComponent>(e));
            if (player == null)
                return;

            if (!world.TryGetComponent<PositionComponent>(player, out var playerPos) ||
                !world.TryGetComponent<ColliderComponent>(player, out var playerCol))
                return;

            Rectangle playerBounds = new Rectangle(
                (int)playerPos.Value.X + playerCol.Bounds.X,
                (int)playerPos.Value.Y + playerCol.Bounds.Y,
                playerCol.Bounds.Width,
                playerCol.Bounds.Height
            );

            var toRemove = new List<Entity>();

            foreach (var entity in world.Query<PowerupComponent, PositionComponent, ColliderComponent>())
            {
                var powerup = world.GetComponent<PowerupComponent>(entity);
                var pos = world.GetComponent<PositionComponent>(entity);
                var col = world.GetComponent<ColliderComponent>(entity);

                Rectangle bounds = new Rectangle(
                    (int)pos.Value.X + col.Bounds.X,
                    (int)pos.Value.Y + col.Bounds.Y,
                    col.Bounds.Width,
                    col.Bounds.Height
                );

                if (!playerBounds.Intersects(bounds))
                    continue;

                ApplyPowerup(world, player, powerup);
                toRemove.Add(entity);
            }

            foreach (var entity in toRemove)
            {
                world.RemoveEntity(entity);
            }
        }

        private void ApplyPowerup(World world, Entity player, PowerupComponent powerup)
        {
            if (world.TryGetComponent<StatsComponent>(player, out var stats))
            {
                switch (powerup.Type)
                {
                    case PowerupType.MaxHealthUp:
                        stats.MaxHealth += powerup.Amount;
                        stats.Health = stats.MaxHealth; // full heal on pickup
                        world.AddComponent(player, new ArmorComponent(powerup.Amount));
                        break;
                    case PowerupType.Heal:
                        stats.Health = System.Math.Min(stats.Health + powerup.Amount, stats.MaxHealth);
                        break;
                    case PowerupType.SpeedBoost:
                        stats.Speed += powerup.Amount;
                        break;
                }

                world.AddComponent(player, stats);
            }
        }
    }
}
