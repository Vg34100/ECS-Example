// Systems/HealthSystem.cs - Fixed version
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Collections.Generic;
using System.Diagnostics;
using ECS_Example.Entities;

namespace ECS_Example.Systems
{
    public class HealthSystem
    {
        public void Update(World world, float deltaTime)
        {
            var entities = world.GetEntities();
            var entitiesToRemove = new List<Entity>();

            // Handle invulnerability timers and collect dead entities
            foreach (var entity in entities)
            {
                if (world.TryGetComponent<Invulnerable>(entity, out var invuln))
                {
                    invuln.TimeRemaining -= deltaTime;
                    if (invuln.TimeRemaining <= 0)
                    {
                        world.RemoveComponent<Invulnerable>(entity);
                    }
                    else
                    {
                        world.AddComponent(entity, invuln);
                    }
                }

                // Check for dead entities
                if (world.TryGetComponent<Health>(entity, out var health) && health.IsDead)
                {
                    if (world.TryGetComponent<PlayerController>(entity, out _))
                    {
                        // Handle player death
                        HandlePlayerDeath(world, entity);
                    }
                    else
                    {
                        // Mark enemy for removal
                        entitiesToRemove.Add(entity);
                    }
                }
            }

            // Remove dead entities after iteration
            foreach (var entity in entitiesToRemove)
            {
                world.RemoveEntity(entity);
                Debug.WriteLine($"Enemy {entity.Id} died!");
            }
        }

        private void HandlePlayerDeath(World world, Entity player)
        {
            Debug.WriteLine("Player died! Restarting game...");
            // Reset player position and health
            if (world.TryGetComponent<Position>(player, out var position))
            {
                position.Value = new Vector2(400, 300);
                world.AddComponent(player, position);
            }

            var health = new Health(3);
            world.AddComponent(player, health);
        }
    }
}