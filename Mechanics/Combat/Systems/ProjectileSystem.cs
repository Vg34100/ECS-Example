using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Collision.Components;
using Microsoft.Xna.Framework;
using System;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// Handles projectile spawning and lifetime
    /// Spawns projectiles when attack button is pressed
    /// </summary>
    public class ProjectileSystem
    {
        private float _shootCooldown = 0f;
        private const float SHOOT_DELAY = 0.3f; // Time between shots

        public void Update(World world, float deltaTime)
        {
            _shootCooldown -= deltaTime;

            // Handle projectile shooting from player
            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<InputComponent>(entity, out var input) ||
                    !world.TryGetComponent<PositionComponent>(entity, out var position))
                    continue;

                // Shoot projectile
                if (input.ShootHeld && _shootCooldown <= 0f)
                {
                    if (world.TryGetComponent<ECS_Base.Mechanics.Progression.Components.AmmoComponent>(entity, out var ammo))
                    {
                        if (ammo.Arrows <= 0)
                            continue;

                        ammo.Arrows -= 1;
                        world.AddComponent(entity, ammo);
                    }

                    Vector2 shootDir = input.Movement;
                    if (shootDir.LengthSquared() < 0.01f && world.TryGetComponent<ECS_Base.Mechanics.Movement.Components.FacingComponent>(entity, out var facing))
                    {
                        shootDir = facing.Direction;
                    }
                    SpawnProjectile(world, position.Value, shootDir);
                    _shootCooldown = SHOOT_DELAY;
                }
            }

            // Update projectile lifetimes and check for wall collisions
            var entitiesToRemove = new System.Collections.Generic.List<Entity>();
            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<ProjectileComponent>(entity, out var projectile))
                    continue;

                projectile.TimeAlive += deltaTime;

                // Check if projectile hit a wall (velocity changed significantly from initial)
                bool hitWall = false;
                if (world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                {
                    // If either velocity component changed significantly, we hit a wall
                    // Swept collision removes velocity along collision normal
                    float xDiff = Math.Abs(velocity.Value.X - projectile.InitialVelocity.X);
                    float yDiff = Math.Abs(velocity.Value.Y - projectile.InitialVelocity.Y);

                    // If either axis lost significant velocity, destroy the projectile
                    if (xDiff > 50f || yDiff > 50f)
                    {
                        hitWall = true;
                    }
                }

                if (projectile.TimeAlive >= projectile.Lifetime || hitWall)
                {
                    entitiesToRemove.Add(entity);
                }
                else
                {
                    world.AddComponent(entity, projectile);
                }
            }

            // Remove expired or collided projectiles
            foreach (var entity in entitiesToRemove)
            {
                world.RemoveEntity(entity);
            }
        }

        private void SpawnProjectile(World world, Vector2 position, Vector2 direction)
        {
            // Default direction if not moving
            if (direction.LengthSquared() < 0.01f)
            {
                direction = new Vector2(1, 0); // Shoot right by default
            }
            else
            {
                direction.Normalize();
            }

            Vector2 projectileVelocity = new Vector2(direction.X * 250f, direction.Y * 250f);

            var projectile = world.CreateEntity();
            world.AddComponent(projectile, new PositionComponent(position.X + 8, position.Y + 8)); // Offset from player center
            world.AddComponent(projectile, new VelocityComponent(projectileVelocity.X, projectileVelocity.Y)); // Fast!
            world.AddComponent(projectile, new ProjectileComponent(
                damage: 2,
                lifetime: 2.0f,
                initialVelocity: projectileVelocity,
                owner: ProjectileComponent.ProjectileOwner.Player
            ));
            world.AddComponent(projectile, new ColliderComponent(
                new Rectangle(0, 0, 4, 4),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(projectile, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Yellow,
                new Vector2(4, 4)
            ));
            world.AddComponent(projectile, new SweptCollisionComponent()); // Use swept for fast projectiles!

            Console.WriteLine($"Spawned projectile at {position} moving {direction}");
        }
    }
}
