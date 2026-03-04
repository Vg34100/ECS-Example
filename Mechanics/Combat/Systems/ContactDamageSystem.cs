using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Stats.Systems;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Applies damage when player collides with contact-damage entities.
    /// </summary>
    public class ContactDamageSystem
    {
        private readonly StatsSystem _statsSystem;
        private const bool DEBUG_LOG = false;

        public ContactDamageSystem(StatsSystem statsSystem = null)
        {
            _statsSystem = statsSystem;
        }

        public void Update(World world, float deltaTime)
        {
            var player = world.GetEntities().FirstOrDefault(e => world.HasComponent<PlayerComponent>(e));
            if (player == null)
                return;

            if (!world.TryGetComponent<PositionComponent>(player, out var playerPos) ||
                !world.TryGetComponent<ColliderComponent>(player, out var playerCollider))
                return;

            var playerBounds = new Rectangle(
                (int)playerPos.Value.X + playerCollider.Bounds.X,
                (int)playerPos.Value.Y + playerCollider.Bounds.Y,
                playerCollider.Bounds.Width,
                playerCollider.Bounds.Height
            );

            foreach (var entity in world.Query<ContactDamageComponent, PositionComponent, ColliderComponent>())
            {
                if (entity == player)
                    continue;

                var damage = world.GetComponent<ContactDamageComponent>(entity);
                var pos = world.GetComponent<PositionComponent>(entity);
                var col = world.GetComponent<ColliderComponent>(entity);

                Rectangle bounds;
                if (world.TryGetComponent<DamageZoneComponent>(entity, out var damageZone))
                {
                    bounds = new Rectangle(
                        (int)pos.Value.X + damageZone.LocalBounds.X,
                        (int)pos.Value.Y + damageZone.LocalBounds.Y,
                        damageZone.LocalBounds.Width,
                        damageZone.LocalBounds.Height
                    );
                }
                else
                {
                    bounds = new Rectangle(
                        (int)pos.Value.X + col.Bounds.X,
                        (int)pos.Value.Y + col.Bounds.Y,
                        col.Bounds.Width,
                        col.Bounds.Height
                    );
                }

                if (!playerBounds.Intersects(bounds))
                    continue;

                if (DEBUG_LOG)
                {
                    System.Console.WriteLine($"Contact damage: player {player.Id} hit by {entity.Id}");
                }

                if (_statsSystem != null)
                {
                    _statsSystem.ApplyDamage(world, player, damage.Damage, entity);
                }
                else
                {
                    if (world.TryGetComponent<ECS_Base.Mechanics.Stats.Components.StatsComponent>(player, out var stats))
                    {
                        stats.TakeDamage(damage.Damage);
                        world.AddComponent(player, stats);
                    }
                }

                // Knockback feedback (clamp vertical to avoid launch)
                Vector2 toPlayer = playerPos.Value - pos.Value;
                if (toPlayer.LengthSquared() > 0.01f)
                {
                    toPlayer.Normalize();
                    var knock = toPlayer * 140f;
                    knock.Y = MathHelper.Clamp(knock.Y, -60f, 60f);
                    world.AddComponent(player, new KnockbackComponent(knock, decay: 10f));
                }
            }
        }
    }
}
