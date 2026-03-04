using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Collision.Components;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Enemy AI - chase player, avoid other enemies, and shoot
    /// </summary>
    public class EnemyAISystem
    {
        public void Update(World world, float deltaTime)
        {
            // Find player position
            var playerEntity = world.GetEntities()
                .FirstOrDefault(e => world.HasComponent<PlayerComponent>(e));

            if (playerEntity == null)
                return;

            if (!world.TryGetComponent<PositionComponent>(playerEntity, out var playerPos))
                return;

            // Collect all enemy positions for separation
            var enemyPositions = new List<(Entity entity, Vector2 position)>();
            foreach (var entity in world.Query<EnemyComponent, PositionComponent>())
            {
                var pos = world.GetComponent<PositionComponent>(entity);
                enemyPositions.Add((entity, pos.Value));
            }

            // Update all enemies
            foreach (var entity in world.Query<EnemyComponent, PositionComponent, VelocityComponent>())
            {
                var enemy = world.GetComponent<EnemyComponent>(entity);
                var position = world.GetComponent<PositionComponent>(entity);
                var velocity = world.GetComponent<VelocityComponent>(entity);

                // Update shoot cooldown
                enemy.TimeSinceLastShot += deltaTime;

                // Calculate direction to player
                Vector2 toPlayer = playerPos.Value - position.Value;
                float distanceToPlayer = toPlayer.Length();

                // Calculate separation force from other enemies
                Vector2 separationForce = Vector2.Zero;
                int nearbyEnemies = 0;

                foreach (var (otherEntity, otherPos) in enemyPositions)
                {
                    if (otherEntity == entity) continue;

                    Vector2 toOther = position.Value - otherPos;
                    float distance = toOther.Length();

                    if (distance < enemy.SeparationRadius && distance > 0.1f)
                    {
                        // Push away from nearby enemies
                        toOther.Normalize();
                        separationForce += toOther / distance; // Stronger when closer
                        nearbyEnemies++;
                    }
                }

                if (nearbyEnemies > 0)
                {
                    separationForce /= nearbyEnemies; // Average
                }

                // AI behavior
                Vector2 desiredVelocity = Vector2.Zero;

                if (distanceToPlayer < enemy.ChaseRange)
                {
                    // Shoot if in range and cooldown ready
                    if (enemy.CanShoot && distanceToPlayer < enemy.ShootRange &&
                        enemy.TimeSinceLastShot >= enemy.ShootCooldown)
                    {
                        ShootAtPlayer(world, position.Value, toPlayer);
                        enemy.TimeSinceLastShot = 0f;
                    }

                    // Chase player but maintain stop distance
                    if (distanceToPlayer > enemy.StopDistance)
                    {
                        toPlayer.Normalize();
                        desiredVelocity = toPlayer * enemy.ChaseSpeed;
                    }
                    else
                    {
                        // Stop moving when close enough
                        desiredVelocity = Vector2.Zero;
                    }
                }

                // Combine chase + separation
                // Separation has higher priority when enemies are very close
                float separationWeight = Math.Min(nearbyEnemies * 0.5f, 1.5f);
                velocity.Value = desiredVelocity + (separationForce * enemy.ChaseSpeed * separationWeight);

                // Update components
                world.AddComponent(entity, enemy);
                world.AddComponent(entity, velocity);
            }
        }

        private void ShootAtPlayer(World world, Vector2 enemyPosition, Vector2 directionToPlayer)
        {
            directionToPlayer.Normalize();

            Vector2 projectileVelocity = directionToPlayer * 150f; // Slower than player projectiles

            var projectile = world.CreateEntity();
            world.AddComponent(projectile, new PositionComponent(enemyPosition.X + 8, enemyPosition.Y + 8));
            world.AddComponent(projectile, new VelocityComponent(projectileVelocity.X, projectileVelocity.Y));
            world.AddComponent(projectile, new ProjectileComponent(
                damage: 1,
                lifetime: 3.0f,
                initialVelocity: projectileVelocity,
                owner: ProjectileComponent.ProjectileOwner.Enemy
            ));
            world.AddComponent(projectile, new ColliderComponent(
                new Rectangle(0, 0, 4, 4),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(projectile, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Red, // Red for enemy projectiles
                new Vector2(4, 4)
            ));
            world.AddComponent(projectile, new SweptCollisionComponent());

            Console.WriteLine($"Enemy shot projectile toward player");
        }
    }
}
