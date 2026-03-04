using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Stats.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Combat.Systems
{
    /// <summary>
    /// System that handles melee combat
    /// </summary>
    public class MeleeCombatSystem
    {
        private readonly StatsSystem _statsSystem;

        public MeleeCombatSystem(StatsSystem statsSystem = null)
        {
            _statsSystem = statsSystem;
        }

        /// <summary>
        /// Update melee weapons and attacks
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            // Update weapon cooldowns
            foreach (var entity in world.Query<MeleeWeaponComponent>())
            {
                if (!world.TryGetComponent<MeleeWeaponComponent>(entity, out var weapon))
                    continue;

                weapon.TimeSinceLastAttack += deltaTime;
                world.AddComponent(entity, weapon);
            }

            // Update active attacks
            var attacksToRemove = new List<Entity>();

            foreach (var entity in world.Query<MeleeAttackComponent>())
            {
                if (!world.TryGetComponent<MeleeAttackComponent>(entity, out var attack))
                    continue;

                attack.TimeElapsed += deltaTime;

                if (!attack.IsActive)
                {
                    attacksToRemove.Add(entity);
                    continue;
                }

                // Check for hits
                if (world.TryGetComponent<PositionComponent>(entity, out var attackPos))
                {
                    CheckForHits(world, entity, attack, attackPos.Value);
                }

                world.AddComponent(entity, attack);
            }

            // Remove finished attacks
            foreach (var entity in attacksToRemove)
            {
                world.RemoveEntity(entity);
            }
        }

        /// <summary>
        /// Initiate a melee attack
        /// </summary>
        public Entity StartAttack(World world, Entity attacker, Vector2 direction)
        {
            if (!world.TryGetComponent<MeleeWeaponComponent>(attacker, out var weapon))
                return default;

            if (!weapon.CanAttack)
                return default;

            if (!world.TryGetComponent<PositionComponent>(attacker, out var position))
                return default;

            var attackerCenter = position.Value;
            if (world.TryGetComponent<ColliderComponent>(attacker, out var collider))
            {
                attackerCenter += new Vector2(
                    collider.Bounds.Width * 0.5f,
                    collider.Bounds.Height * 0.5f
                );
            }

            // Reset weapon cooldown
            weapon.TimeSinceLastAttack = 0f;
            world.AddComponent(attacker, weapon);

            // Create attack entity
            var attackEntity = world.CreateEntity();

            var attack = new MeleeAttackComponent(
                attacker.Id,
                weapon.Damage,
                direction,
                weapon.Range,
                weapon.AttackArc,
                weapon.AttackDuration,
                weapon.Knockback
            );

            world.AddComponent(attackEntity, attack);

            // Position attack at attacker's position
            var attackPos = attackerCenter + (direction * (weapon.Range * 0.5f));
            world.AddComponent(attackEntity, new PositionComponent { Value = attackPos });

            // Visual swing box (rotated)
            Vector2 size = new Microsoft.Xna.Framework.Vector2(weapon.Range, weapon.Range * 0.4f);
            float rotation = (float)System.Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2;
            world.AddComponent(attackEntity, new Rendering.Components.RotatedRectComponent(
                center: attackPos,
                size: size,
                rotation: rotation,
                color: new Microsoft.Xna.Framework.Color(255, 255, 255, 90)
            ));

            return attackEntity;
        }

        /// <summary>
        /// Check if attack hits any enemies
        /// </summary>
        private void CheckForHits(World world, Entity attackEntity, MeleeAttackComponent attack, Vector2 attackPosition)
        {
            // Find all entities with stats (potential targets)
            foreach (var targetEntity in world.Query<StatsComponent, PositionComponent>())
            {
                // Don't hit self
                if (targetEntity.Id == attack.AttackerEntityId)
                    continue;

                // Don't hit same entity twice
                if (attack.AlreadyHit.Contains(targetEntity.Id))
                    continue;

                if (!world.TryGetComponent<PositionComponent>(targetEntity, out var targetPos))
                    continue;

                Vector2 toTarget = targetPos.Value - attackPosition;
                Vector2 forward = attack.Direction;
                if (forward.LengthSquared() > 0f)
                    forward.Normalize();
                else
                    forward = new Vector2(1, 0);

                Vector2 right = new Vector2(-forward.Y, forward.X);

                float localX = Vector2.Dot(toTarget, right);
                float localY = Vector2.Dot(toTarget, forward);

                float halfX = attack.Range * 0.5f;
                float halfY = attack.Range * 0.2f;

                if (Math.Abs(localX) > halfX || Math.Abs(localY) > halfY)
                    continue;

                // Hit!
                attack.AlreadyHit.Add(targetEntity.Id);

                // Apply damage
                if (_statsSystem != null)
                {
                    _statsSystem.ApplyDamage(world, targetEntity, attack.Damage);
                }

                // Apply knockback
                if (attack.Knockback > 0 && world.TryGetComponent<VelocityComponent>(targetEntity, out var velocity))
                {
                    Vector2 knockbackDir = targetPos.Value - attackPosition;
                    if (knockbackDir.LengthSquared() > 0)
                    {
                        knockbackDir.Normalize();
                        velocity.Value += knockbackDir * attack.Knockback;
                        world.AddComponent(targetEntity, velocity);
                    }
                }
            }

            // Update attack with new AlreadyHit list
            world.AddComponent(attackEntity, attack);
        }

        /// <summary>
        /// Check if entity can attack
        /// </summary>
        public bool CanAttack(World world, Entity entity)
        {
            if (!world.TryGetComponent<MeleeWeaponComponent>(entity, out var weapon))
                return false;

            return weapon.CanAttack;
        }
    }
}
