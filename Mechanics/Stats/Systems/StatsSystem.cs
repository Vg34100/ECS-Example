using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Progression.Components;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Stats.Systems
{
    /// <summary>
    /// System that manages stats and applies modifiers
    /// </summary>
    public class StatsSystem
    {
        private readonly EventSystem _eventSystem;

        public StatsSystem(EventSystem eventSystem = null)
        {
            _eventSystem = eventSystem;
        }

        /// <summary>
        /// Update stat modifiers and check for deaths
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            var modifiersToRemove = new List<(Entity, StatModifierComponent)>();

            // Update stat modifiers
            foreach (var entity in world.Query<StatModifierComponent>())
            {
                if (!world.TryGetComponent<StatModifierComponent>(entity, out var modifier))
                    continue;

                if (modifier.Duration > 0)
                {
                    modifier.TimeRemaining -= deltaTime;

                    if (modifier.IsExpired)
                    {
                        modifiersToRemove.Add((entity, modifier));
                    }
                    else
                    {
                        world.AddComponent(entity, modifier);
                    }
                }
            }

            // Remove expired modifiers
            foreach (var (entity, modifier) in modifiersToRemove)
            {
                world.RemoveComponent<StatModifierComponent>(entity);
            }

            // Check for newly dead entities and fire events
            foreach (var entity in world.Query<StatsComponent>())
            {
                if (!world.TryGetComponent<StatsComponent>(entity, out var stats))
                    continue;

                // Check if entity just died (health <= 0 but not marked dead yet)
                if (stats.Health <= 0 && !stats.IsDead)
                {
                    stats.IsDead = true;
                    world.AddComponent(entity, stats);

                    // Fire death event if event system is available
                    if (_eventSystem != null && world.TryGetComponent<PositionComponent>(entity, out var position))
                    {
                        _eventSystem.Publish(new EnemyDiedEvent(entity, position.Value, 100));
                    }
                }
            }
        }

        /// <summary>
        /// Apply damage to an entity with stats
        /// </summary>
        public float ApplyDamage(World world, Entity target, float damage, Entity source = default)
        {
            if (!world.TryGetComponent<StatsComponent>(target, out var stats))
                return 0f;

            float actualDamage = stats.TakeDamage(damage);
            world.AddComponent(target, stats);

            if (actualDamage > 0)
            {
                RemoveArmorOnHit(world, target);
                ApplyInvulnerability(world, target);
                ApplyDamageFlash(world, target);
            }

            // Fire damage event
            if (_eventSystem != null && actualDamage > 0)
            {
                _eventSystem.Publish(new PlayerDamagedEvent(
                    (int)actualDamage,
                    (int)stats.Health,
                    source
                ));
            }

            return actualDamage;
        }

        /// <summary>
        /// Heal an entity
        /// </summary>
        public float ApplyHealing(World world, Entity target, float amount)
        {
            if (!world.TryGetComponent<StatsComponent>(target, out var stats))
                return 0f;

            float actualHealing = stats.Heal(amount);
            world.AddComponent(target, stats);

            return actualHealing;
        }

        /// <summary>
        /// Get effective stat value with all modifiers applied
        /// </summary>
        public float GetEffectiveStat(World world, Entity entity, StatType statType)
        {
            if (!world.TryGetComponent<StatsComponent>(entity, out var stats))
                return 0f;

            // Get base value
            float baseValue = statType switch
            {
                StatType.Health => stats.Health,
                StatType.MaxHealth => stats.MaxHealth,
                StatType.Attack => stats.Attack,
                StatType.Defense => stats.Defense,
                StatType.Speed => stats.Speed,
                _ => 0f
            };

            // Apply all modifiers
            float totalAdditive = 0f;
            float totalMultiplier = 1f;

            var modifiers = world.GetComponents<StatModifierComponent>()
                .Where(tuple => tuple.entity.Id == entity.Id && tuple.component.StatType == statType);

            foreach (var (_, modifier) in modifiers)
            {
                totalAdditive += modifier.Value;
                totalMultiplier *= modifier.Multiplier;
            }

            return (baseValue + totalAdditive) * totalMultiplier;
        }

        /// <summary>
        /// Add a stat modifier to an entity
        /// </summary>
        public void AddModifier(World world, Entity entity, StatModifierComponent modifier)
        {
            world.AddComponent(entity, modifier);
        }

        /// <summary>
        /// Remove all modifiers with a specific ID
        /// </summary>
        public void RemoveModifiersById(World world, Entity entity, string id)
        {
            var modifiersToRemove = world.GetComponents<StatModifierComponent>()
                .Where(tuple => tuple.entity.Id == entity.Id && tuple.component.Id == id)
                .ToList();

            foreach (var (modEntity, modifier) in modifiersToRemove)
            {
                world.RemoveComponent<StatModifierComponent>(modEntity);
            }
        }

        private void ApplyInvulnerability(World world, Entity target)
        {
            if (!world.TryGetComponent<InvulnerabilityOnHitComponent>(target, out var onHit))
                return;

            if (world.TryGetComponent<StatsComponent>(target, out var stats))
            {
                stats.IsInvulnerable = true;
                world.AddComponent(target, stats);
            }

            world.AddComponent(target, new InvulnerabilityComponent(onHit.Duration));
        }

        private void ApplyDamageFlash(World world, Entity target)
        {
            if (!world.TryGetComponent<DamageFlashOnHitComponent>(target, out var onHit))
                return;

            Color originalColor;
            if (world.TryGetComponent<SpriteComponent>(target, out var sprite))
            {
                originalColor = sprite.Tint;
            }
            else if (world.TryGetComponent<ShapeComponent>(target, out var shape))
            {
                originalColor = shape.Color;
            }
            else
                return;

            if (world.TryGetComponent<DamageFlashComponent>(target, out var existing))
            {
                existing.TimeRemaining = onHit.Duration;
                existing.FlashColor = onHit.FlashColor;
                world.AddComponent(target, existing);
                return;
            }

            world.AddComponent(target, new DamageFlashComponent(
                originalColor: originalColor,
                flashColor: onHit.FlashColor,
                duration: onHit.Duration
            ));
        }

        private void RemoveArmorOnHit(World world, Entity target)
        {
            if (!world.TryGetComponent<ArmorComponent>(target, out var armor))
                return;

            if (!world.TryGetComponent<StatsComponent>(target, out var stats))
                return;

            stats.MaxHealth = System.Math.Max(1f, stats.MaxHealth - armor.ExtraHealth);
            stats.Health = System.Math.Min(stats.Health, stats.MaxHealth);
            world.AddComponent(target, stats);
            world.RemoveComponent<ArmorComponent>(target);
        }
    }
}
