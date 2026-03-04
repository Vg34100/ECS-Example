using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Stats.Systems;
using ECS_Base.Mechanics.Movement.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Handles combat - projectile hits, damage dealing, death
    /// </summary>
    public class CombatSystem
    {
        private readonly StatsSystem _statsSystem;

        public CombatSystem(StatsSystem statsSystem = null)
        {
            _statsSystem = statsSystem;
        }

        public void Update(World world, float deltaTime)
        {
            var entitiesToRemove = new List<Entity>();

            // Check projectile collisions
            foreach (var projectileEntity in world.GetEntities())
            {
                if (!world.TryGetComponent<ProjectileComponent>(projectileEntity, out var projectile) ||
                    !world.TryGetComponent<PositionComponent>(projectileEntity, out var projPos) ||
                    !world.TryGetComponent<ColliderComponent>(projectileEntity, out var projCollider))
                    continue;

                Rectangle projBounds = new Rectangle(
                    (int)projPos.Value.X,
                    (int)projPos.Value.Y,
                    projCollider.Bounds.Width,
                    projCollider.Bounds.Height
                );

                // Player projectiles hit enemies
                if (projectile.Owner == ProjectileComponent.ProjectileOwner.Player)
                {
                    foreach (var enemyEntity in world.GetEntities())
                    {
                        if (!world.TryGetComponent<EnemyComponent>(enemyEntity, out _) ||
                            !world.TryGetComponent<StatsComponent>(enemyEntity, out _) ||
                            !world.TryGetComponent<PositionComponent>(enemyEntity, out var enemyPos) ||
                            !world.TryGetComponent<ColliderComponent>(enemyEntity, out var enemyCollider))
                            continue;

                        Rectangle enemyBounds = new Rectangle(
                            (int)enemyPos.Value.X,
                            (int)enemyPos.Value.Y,
                            enemyCollider.Bounds.Width,
                            enemyCollider.Bounds.Height
                        );

                        if (projBounds.Intersects(enemyBounds))
                        {
                            // Deal damage
                            ApplyDamage(world, enemyEntity, projectile.Damage);

                            // Knockback enemy
                            var knockDir = projectile.InitialVelocity;
                            if (knockDir.LengthSquared() > 0.01f)
                            {
                                knockDir.Normalize();
                                world.AddComponent(enemyEntity, new KnockbackComponent(knockDir * 140f, decay: 10f));
                            }

                            // Destroy projectile
                            if (!entitiesToRemove.Contains(projectileEntity))
                                entitiesToRemove.Add(projectileEntity);

                            if (world.TryGetComponent<StatsComponent>(enemyEntity, out var updatedStats))
                                System.Console.WriteLine($"Hit enemy! Enemy health: {updatedStats.Health}");
                            break;
                        }
                    }
                }
                // Enemy projectiles hit player
                else if (projectile.Owner == ProjectileComponent.ProjectileOwner.Enemy)
                {
                    foreach (var playerEntity in world.GetEntities())
                    {
                        if (!world.TryGetComponent<PlayerComponent>(playerEntity, out _) ||
                            !world.TryGetComponent<StatsComponent>(playerEntity, out _) ||
                            !world.TryGetComponent<PositionComponent>(playerEntity, out var playerPos) ||
                            !world.TryGetComponent<ColliderComponent>(playerEntity, out var playerCollider))
                            continue;

                        Rectangle playerBounds = new Rectangle(
                            (int)playerPos.Value.X,
                            (int)playerPos.Value.Y,
                            playerCollider.Bounds.Width,
                            playerCollider.Bounds.Height
                        );

                        if (projBounds.Intersects(playerBounds))
                        {
                            // Reflect if blocking
                            if (world.TryGetComponent<BlockingComponent>(playerEntity, out var blocking) && blocking.IsBlocking &&
                                world.TryGetComponent<VelocityComponent>(projectileEntity, out var projVel))
                            {
                                projVel.Value *= -1f;
                                world.AddComponent(projectileEntity, projVel);
                                projectile.Owner = ProjectileComponent.ProjectileOwner.Player;
                                world.AddComponent(projectileEntity, projectile);
                                continue;
                            }

                            // Deal damage
                            ApplyDamage(world, playerEntity, projectile.Damage);

                            // Knockback from projectile direction
                            var knockDir = projectile.InitialVelocity;
                            if (knockDir.LengthSquared() > 0.01f)
                            {
                                knockDir.Normalize();
                                world.AddComponent(playerEntity, new KnockbackComponent(knockDir * 160f, decay: 10f));
                            }

                            // Destroy projectile
                            if (!entitiesToRemove.Contains(projectileEntity))
                                entitiesToRemove.Add(projectileEntity);

                            if (world.TryGetComponent<StatsComponent>(playerEntity, out var updatedStats))
                                System.Console.WriteLine($"Hit player! Player health: {updatedStats.Health}");
                            break;
                        }
                    }
                }
            }

            // Remove dead entities (enemies and player)
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<StatsComponent>(entity, out var stats))
                {
                    if (stats.IsDead || stats.Health <= 0)
                    {
                        if (world.HasComponent<ECS_Base.Mechanics.Stats.Components.RespawnComponent>(entity))
                            continue;

                        entitiesToRemove.Add(entity);

                        if (world.HasComponent<PlayerComponent>(entity))
                            System.Console.WriteLine("Player died!");
                        else if (world.HasComponent<EnemyComponent>(entity))
                            System.Console.WriteLine("Enemy died!");
                    }
                }
            }

            foreach (var entity in entitiesToRemove)
            {
                world.RemoveEntity(entity);
            }
        }

        private void ApplyDamage(World world, Entity target, float damage)
        {
            if (_statsSystem != null)
            {
                _statsSystem.ApplyDamage(world, target, damage);
                return;
            }

            if (!world.TryGetComponent<StatsComponent>(target, out var stats))
                return;

            stats.TakeDamage(damage);
            world.AddComponent(target, stats);
        }
    }
}
