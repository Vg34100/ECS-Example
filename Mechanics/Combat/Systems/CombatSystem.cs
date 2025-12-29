using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Handles combat - projectile hits, damage dealing, death
    /// </summary>
    public class CombatSystem
    {
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
                            !world.TryGetComponent<HealthComponent>(enemyEntity, out var health) ||
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
                            health.CurrentHealth -= projectile.Damage;
                            world.AddComponent(enemyEntity, health);

                            // Destroy projectile
                            if (!entitiesToRemove.Contains(projectileEntity))
                                entitiesToRemove.Add(projectileEntity);

                            System.Console.WriteLine($"Hit enemy! Enemy health: {health.CurrentHealth}");
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
                            !world.TryGetComponent<HealthComponent>(playerEntity, out var health) ||
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
                            // Deal damage
                            health.CurrentHealth -= projectile.Damage;
                            world.AddComponent(playerEntity, health);

                            // Destroy projectile
                            if (!entitiesToRemove.Contains(projectileEntity))
                                entitiesToRemove.Add(projectileEntity);

                            System.Console.WriteLine($"Hit player! Player health: {health.CurrentHealth}");
                            break;
                        }
                    }
                }
            }

            // Remove dead entities (enemies and player)
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<HealthComponent>(entity, out var health))
                {
                    if (health.CurrentHealth <= 0)
                    {
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
    }
}
