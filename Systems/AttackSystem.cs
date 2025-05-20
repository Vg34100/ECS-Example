// Systems/AttackSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ECS_Example.Components;
using System.Diagnostics;
using System;
using ECS_Example.Entities;

namespace ECS_Example.Systems
{
    public class AttackSystem
    {
        private GamePadState _previousGamePadState;

        public AttackSystem()
        {
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        public void Update(World world, float deltaTime)
        {
            var keyboardState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(PlayerIndex.One);

            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<Attack>(entity, out var attack))
                    continue;

                // Update attack cooldown
                if (attack.TimeUntilNextAttack > 0)
                {
                    attack.TimeUntilNextAttack -= deltaTime;
                }

                // Update facing direction based on velocity
                if (world.TryGetComponent<Velocity>(entity, out var velocity))
                {
                    if (velocity.Value.X > 0)
                        attack.IsFacingRight = true;
                    else if (velocity.Value.X < 0)
                        attack.IsFacingRight = false;
                }

                // Handle ongoing attack
                if (attack.IsAttacking)
                {
                    attack.AttackTimer -= deltaTime;

                    // Process hits during attack duration
                    ProcessAttackHits(world, entity, attack);

                    // Check if attack is finished
                    if (attack.AttackTimer <= 0)
                    {
                        attack.IsAttacking = false;
                    }
                }
                // Check if entity can start a new attack
                else if (attack.TimeUntilNextAttack <= 0)
                {
                    bool wantsToAttack = false;

                    // Keyboard input - X key for attack
                    if (keyboardState.IsKeyDown(Keys.X))
                    {
                        wantsToAttack = true;
                    }

                    // Controller input - Square/X button for attack
                    if (gamePadState.IsButtonDown(Buttons.X) && _previousGamePadState.IsButtonUp(Buttons.X))
                    {
                        wantsToAttack = true;
                    }

                    if (wantsToAttack)
                    {
                        // Start attack
                        attack.IsAttacking = true;
                        attack.AttackTimer = attack.AttackDuration;
                        attack.TimeUntilNextAttack = attack.Cooldown;

                        Debug.WriteLine($"Entity {entity.Id} attacking! Facing right: {attack.IsFacingRight}");
                    }
                }

                // Update the component
                world.AddComponent(entity, attack);
            }

            _previousGamePadState = gamePadState;
        }

        private void ProcessAttackHits(World world, Entity attacker, Attack attack)
        {
            if (!world.TryGetComponent<Position>(attacker, out var attackerPos) ||
                !world.TryGetComponent<Collider>(attacker, out var attackerCollider))
                return;

            // Get entity center position and size for better positioning
            float entityCenterX = attackerPos.Value.X + (attackerCollider.Bounds.Width / 2);
            float entityCenterY = attackerPos.Value.Y + (attackerCollider.Bounds.Height / 2);

            // Calculate attack hitbox position based on facing direction
            float hitboxX;
            if (attack.IsFacingRight)
            {
                // For right-facing, position starts at the right edge of player
                hitboxX = attackerPos.Value.X + attackerCollider.Bounds.Width;
            }
            else
            {
                // For left-facing, position ends at the left edge of player
                hitboxX = attackerPos.Value.X - attack.HitboxSize.X;
            }

            // Center the hitbox vertically on the player
            float hitboxY = entityCenterY - (attack.HitboxSize.Y / 2);

            Rectangle attackHitbox = new Rectangle(
                (int)hitboxX,
                (int)hitboxY,
                (int)attack.HitboxSize.X,
                (int)attack.HitboxSize.Y
            );

            // Check all entities for hit
            foreach (var entity in world.GetEntities())
            {
                // Skip self
                if (entity.Id == attacker.Id)
                    continue;

                // Skip entities without health or collider
                if (!world.TryGetComponent<Health>(entity, out var health) ||
                    !world.TryGetComponent<Position>(entity, out var pos) ||
                    !world.TryGetComponent<Collider>(entity, out var collider) ||
                    world.TryGetComponent<Invulnerable>(entity, out _))
                    continue;

                // Create entity hitbox
                Rectangle entityHitbox = new Rectangle(
                    (int)pos.Value.X,
                    (int)pos.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                // Check for hit
                if (attackHitbox.Intersects(entityHitbox))
                {
                    // Apply damage
                    health.TakeDamage(attack.Damage);
                    world.AddComponent(entity, health);



                    // Apply knockback AWAY from the attacker's position (not based on facing direction)
                    if (world.TryGetComponent<Velocity>(entity, out var velocity))
                    {
                        // Get attacker and target positions to determine knockback direction
                        float attackerCenterX = attackerPos.Value.X + (attackerCollider.Bounds.Width / 2);
                        float targetCenterX = pos.Value.X + (collider.Bounds.Width / 2);

                        // Determine direction based on relative positions
                        float knockbackDirection = (targetCenterX > attackerCenterX) ? 1 : -1;

                        // Apply knockback in the appropriate direction
                        velocity.Value.X = 200 * knockbackDirection;
                        velocity.Value.Y = -150; // Small upward boost
                        world.AddComponent(entity, velocity);
                    }


                    // Add invulnerability
                    float invulnDuration = 0.4f;
                    if (world.TryGetComponent<InvulnerableSettings>(entity, out var invulnSettings))
                    {
                        invulnDuration = invulnSettings.Duration;
                    }
                    world.AddComponent(entity, new Invulnerable(invulnDuration));

                    float stunDuration = 0.2f;
                    world.AddComponent(entity, new Stunned(stunDuration));

                    // Visual feedback
                    world.AddComponent(entity, new FlashEffect(0.1f));

                    Debug.WriteLine($"Attack hit entity {entity.Id}! Health: {health.CurrentHealth}/{health.MaxHealth}");
                }
            }
        }
    }
}