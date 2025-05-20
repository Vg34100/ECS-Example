// Systems/DamageSystem.cs - Fixed version
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ECS_Example.Entities;

namespace ECS_Example.Systems
{
    public class DamageSystem
    {
        private HashSet<(int, int)> _processedCollisions = new HashSet<(int, int)>();

        public void Update(World world)
        {
            _processedCollisions.Clear();
            var entities = world.GetEntities();

            for (int i = 0; i < entities.Count; i++)
            {
                var entityA = entities[i];
                if (!world.TryGetComponent<Position>(entityA, out var posA) ||
                    !world.TryGetComponent<Collider>(entityA, out var colliderA))
                    continue;

                Rectangle boundsA = new Rectangle(
                    (int)posA.Value.X + colliderA.Bounds.X,
                    (int)posA.Value.Y + colliderA.Bounds.Y,
                    colliderA.Bounds.Width,
                    colliderA.Bounds.Height
                );

                for (int j = i + 1; j < entities.Count; j++)
                {
                    var entityB = entities[j];
                    if (!world.TryGetComponent<Position>(entityB, out var posB) ||
                        !world.TryGetComponent<Collider>(entityB, out var colliderB))
                        continue;

                    Rectangle boundsB = new Rectangle(
                        (int)posB.Value.X + colliderB.Bounds.X,
                        (int)posB.Value.Y + colliderB.Bounds.Y,
                        colliderB.Bounds.Width,
                        colliderB.Bounds.Height
                    );

                    if (boundsA.Intersects(boundsB))
                    {
                        HandleCollision(world, entityA, entityB, boundsA, boundsB);
                    }
                }
            }
        }

        private void HandleCollision(World world, Entity entityA, Entity entityB,
                                    Rectangle boundsA, Rectangle boundsB)
        {
            // Create a unique key for this collision pair
            var collisionKey = (Math.Min(entityA.Id, entityB.Id), Math.Max(entityA.Id, entityB.Id));

            // Skip if we've already processed this collision
            if (_processedCollisions.Contains(collisionKey))
                return;

            _processedCollisions.Add(collisionKey);

            // Check if A can damage B
            bool aDamagedB = CheckDamage(world, entityA, entityB, boundsA, boundsB);

            // Only check if B can damage A if A didn't damage B
            if (!aDamagedB)
            {
                CheckDamage(world, entityB, entityA, boundsB, boundsA);
            }
        }

        private bool CheckDamage(World world, Entity damager, Entity target,
                                Rectangle damagerBounds, Rectangle targetBounds)
        {
            if (!world.TryGetComponent<Damager>(damager, out var damagerComp) ||
                !world.TryGetComponent<Health>(target, out var health) ||
                world.TryGetComponent<Invulnerable>(target, out _))
                return false;

            bool shouldDamage = false;

            switch (damagerComp.Type)
            {
                case DamageType.Contact:
                    shouldDamage = true;
                    break;

                case DamageType.FromAbove:
                    // Check if damager is above target
                    if (world.TryGetComponent<Velocity>(damager, out var damagerVel) &&
                        damagerVel.Value.Y > 0 && // Moving down
                        damagerBounds.Bottom <= targetBounds.Top + 15 && // Above the target
                        damagerBounds.Center.X > targetBounds.Left && // Within horizontal bounds
                        damagerBounds.Center.X < targetBounds.Right)
                    {
                        shouldDamage = true;

                        // Handle bouncing if target is bounceable
                        if (world.TryGetComponent<Bounceable>(target, out var bounce))
                        {
                            damagerVel.Value.Y = bounce.BounceVelocity;
                            world.AddComponent(damager, damagerVel);
                        }
                    }
                    break;
            }

            if (shouldDamage)
            {
                health.TakeDamage(damagerComp.Damage);
                world.AddComponent(target, health);

                // Get invulnerability duration from settings or use default
                float invulnDuration = 1.0f;
                if (world.TryGetComponent<InvulnerableSettings>(target, out var invulnSettings))
                {
                    invulnDuration = invulnSettings.Duration;
                }

                // Make target invulnerable
                world.AddComponent(target, new Invulnerable(invulnDuration));

                // Add effects if it's the player
                if (world.TryGetComponent<PlayerController>(target, out _))
                {
                    world.AddComponent(target, new FlashEffect(0.1f));
                    float stunDuration = 0.3f;
                    if (world.TryGetComponent<StunSettings>(target, out var stunSettings))
                    {
                        stunDuration = stunSettings.DamageStunDuration;
                    }
                    world.AddComponent(target, new Stunned(stunDuration));

                    // Add stronger recoil
                    if (world.TryGetComponent<Velocity>(target, out var targetVel) &&
                        world.TryGetComponent<Position>(damager, out var damagerPos) &&
                        world.TryGetComponent<Position>(target, out var targetPos))
                    {
                        // Calculate knockback direction
                        float knockbackX = 0;
                        float knockbackY = -250; // Upward boost

                        // Stronger horizontal knockback
                        if (targetPos.Value.X < damagerPos.Value.X)
                        {
                            knockbackX = -200; // Knocked left
                        }
                        else
                        {
                            knockbackX = 200; // Knocked right
                        }

                        targetVel.Value.X = knockbackX;
                        targetVel.Value.Y = knockbackY;
                        world.AddComponent(target, targetVel);
                    }
                }

                Debug.WriteLine($"Entity {target.Id} took {damagerComp.Damage} damage! Health: {health.CurrentHealth}/{health.MaxHealth}");
                return true;
            }

            return false;
        }
    }
}