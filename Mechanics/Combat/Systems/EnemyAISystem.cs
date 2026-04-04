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
using System.IO;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Enemy AI - chase player, avoid other enemies, and shoot
    /// </summary>
    public class EnemyAISystem
    {
        private float _slimeDebugTimer;

        public void Update(World world, float deltaTime)
        {
            _slimeDebugTimer -= deltaTime;

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

                Vector2 desiredVelocity = Vector2.Zero;
                bool isSlime = world.TryGetComponent<SlimeMovementComponent>(entity, out var slime);

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
                        if (isSlime)
                        {
                            if (slime.HopTimeRemaining > 0f)
                            {
                                slime.HopTimeRemaining -= deltaTime;
                                desiredVelocity = slime.HopDirection * (enemy.ChaseSpeed * slime.LungeSpeedMultiplier);
                            }
                            else
                            {
                                slime.PauseTimeRemaining -= deltaTime;
                                desiredVelocity = Vector2.Zero;

                                if (slime.PauseTimeRemaining <= 0f)
                                {
                                    slime.HopDirection = toPlayer;
                                    slime.HopTimeRemaining = slime.HopDuration;
                                    slime.PauseTimeRemaining = slime.PauseDuration;
                                    desiredVelocity = slime.HopDirection * (enemy.ChaseSpeed * slime.LungeSpeedMultiplier);
                                }
                            }
                        }
                        else
                        {
                            desiredVelocity = toPlayer * enemy.ChaseSpeed;
                        }
                    }
                    else
                    {
                        // Stop moving when close enough
                        desiredVelocity = Vector2.Zero;
                        if (isSlime)
                        {
                            slime.HopTimeRemaining = 0f;
                            slime.PauseTimeRemaining = 0f;
                        }
                    }
                }
                else if (isSlime)
                {
                    slime.HopTimeRemaining = 0f;
                    slime.PauseTimeRemaining = 0f;
                }

                if (isSlime)
                {
                    // Slime movement should read as a clear wait->burst loop, not steering blended with avoidance.
                    velocity.Value = desiredVelocity;
                }
                else
                {
                    // Combine chase + separation
                    // Separation has higher priority when enemies are very close
                    float separationWeight = Math.Min(nearbyEnemies * 0.5f, 1.5f);
                    velocity.Value = desiredVelocity + (separationForce * enemy.ChaseSpeed * separationWeight);
                }

                // Update components
                world.AddComponent(entity, enemy);
                world.AddComponent(entity, velocity);
                if (isSlime)
                {
                    world.AddComponent(entity, slime);

                    if (_slimeDebugTimer <= 0f)
                    {
                        string message =
                            $"SlimeDebug entity={entity.Id} pos=({position.Value.X:0},{position.Value.Y:0}) " +
                            $"dist={distanceToPlayer:0.0} desired=({desiredVelocity.X:0.0},{desiredVelocity.Y:0.0}) " +
                            $"final=({velocity.Value.X:0.0},{velocity.Value.Y:0.0}) " +
                            $"hop={slime.HopTimeRemaining:0.00} pause={slime.PauseTimeRemaining:0.00}";
                        Console.WriteLine(message);
                        try
                        {
                            var debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt");
                            File.AppendAllText(debugPath, message + Environment.NewLine);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (_slimeDebugTimer <= 0f)
                _slimeDebugTimer = 1.0f;
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
