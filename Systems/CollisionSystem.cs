// Systems/CollisionSystem.cs - Fixed corner collision bugs for entity-to-entity
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Example.Systems
{
    public class CollisionSystem
    {
        private const float CONTACT_THRESHOLD = 2.0f; // Distance to consider "touching"
        private DebugSystem _debugSystem;

        public CollisionSystem(DebugSystem debugSystem = null)
        {
            _debugSystem = debugSystem;
        }

        public void Update(World world)
        {
            var entities = world.GetEntities();

            foreach (var entity in entities)
            {
                if (!world.TryGetComponent<Collider>(entity, out var collider) ||
                    collider.Type != Collider.ColliderType.Dynamic)
                    continue;

                if (!world.TryGetComponent<Position>(entity, out var position) ||
                    !world.TryGetComponent<Velocity>(entity, out var velocity))
                    continue;

                bool hasGravity = world.TryGetComponent<Gravity>(entity, out var gravity);
                bool isPlayer = world.TryGetComponent<PlayerController>(entity, out _);

                Rectangle bounds = new Rectangle(
                    (int)position.Value.X,
                    (int)position.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                bool touchingGround = false;
                bool touchingLeft = false;
                bool touchingRight = false;
                bool touchingTop = false;

                // Collect all colliding entities and their collision info
                var collisions = new List<EntityCollisionInfo>();
                foreach (var other in entities)
                {
                    if (other.Id == entity.Id) continue;

                    if (!world.TryGetComponent<Collider>(other, out var otherCollider) ||
                        otherCollider.Type != Collider.ColliderType.Static)
                        continue;

                    if (!world.TryGetComponent<Position>(other, out var otherPosition))
                        continue;

                    Rectangle otherBounds = new Rectangle(
                        (int)otherPosition.Value.X,
                        (int)otherPosition.Value.Y,
                        otherCollider.Bounds.Width,
                        otherCollider.Bounds.Height
                    );

                    if (bounds.Intersects(otherBounds))
                    {
                        collisions.Add(new EntityCollisionInfo(otherBounds, bounds));
                    }
                }

                // Sort collisions by priority: vertical first, then horizontal
                var verticalCollisions = collisions.Where(c => c.ResolveVertically).OrderBy(c => c.VerticalPenetration);
                var horizontalCollisions = collisions.Where(c => !c.ResolveVertically).OrderBy(c => c.HorizontalPenetration);

                // Resolve vertical collisions first
                foreach (var collision in verticalCollisions)
                {
                    ResolveVerticalCollision(ref position, ref velocity, collision);

                    // Update bounds after each resolution
                    bounds = new Rectangle(
                        (int)position.Value.X,
                        (int)position.Value.Y,
                        collider.Bounds.Width,
                        collider.Bounds.Height
                    );
                }

                // Then resolve horizontal collisions
                foreach (var collision in horizontalCollisions)
                {
                    // Re-check if still colliding after vertical resolution
                    if (bounds.Intersects(collision.OtherBounds))
                    {
                        ResolveHorizontalCollision(ref position, ref velocity, bounds, collision.OtherBounds);

                        // Update bounds after each resolution
                        bounds = new Rectangle(
                            (int)position.Value.X,
                            (int)position.Value.Y,
                            collider.Bounds.Width,
                            collider.Bounds.Height
                        );
                    }
                }

                // Second pass: Check for resting contact with all static entities
                foreach (var other in entities)
                {
                    if (other.Id == entity.Id) continue;

                    if (!world.TryGetComponent<Collider>(other, out var otherCollider) ||
                        otherCollider.Type != Collider.ColliderType.Static)
                        continue;

                    if (!world.TryGetComponent<Position>(other, out var otherPosition))
                        continue;

                    Rectangle otherBounds = new Rectangle(
                        (int)otherPosition.Value.X,
                        (int)otherPosition.Value.Y,
                        otherCollider.Bounds.Width,
                        otherCollider.Bounds.Height
                    );

                    CheckRestingContact(bounds, otherBounds, ref touchingGround, ref touchingLeft, ref touchingRight, ref touchingTop);
                }

                // Apply velocity corrections for stable resting
                if (touchingGround && velocity.Value.Y > -10 && velocity.Value.Y < 50)
                {
                    velocity.Value.Y = 0;
                }

                if ((touchingLeft && velocity.Value.X < 0 && velocity.Value.X > -30) ||
                    (touchingRight && velocity.Value.X > 0 && velocity.Value.X < 30))
                {
                    velocity.Value.X = 0;
                }

                // Update components
                world.AddComponent(entity, position);
                world.AddComponent(entity, velocity);

                // Update grounded state
                if (hasGravity)
                {
                    gravity.IsGrounded = touchingGround;
                    world.AddComponent(entity, gravity);
                }

                // Update debug collision info for player (combine with tile collisions)
                if (isPlayer && _debugSystem != null)
                {
                    _debugSystem.PlayerTouchingLeft = _debugSystem.PlayerTouchingLeft || touchingLeft;
                    _debugSystem.PlayerTouchingRight = _debugSystem.PlayerTouchingRight || touchingRight;
                    _debugSystem.PlayerTouchingTop = _debugSystem.PlayerTouchingTop || touchingTop;
                    _debugSystem.PlayerTouchingBottom = _debugSystem.PlayerTouchingBottom || touchingGround;
                }
            }
        }

        private void ResolveVerticalCollision(ref Position position, ref Velocity velocity, EntityCollisionInfo collision)
        {
            if (collision.TopPenetration < collision.BottomPenetration)
            {
                // Entity is above the other (standing on it)
                position.Value.Y -= collision.TopPenetration;
                if (velocity.Value.Y > 0)
                    velocity.Value.Y = 0;
            }
            else
            {
                // Entity hit from below (ceiling)
                position.Value.Y += collision.BottomPenetration;
                if (velocity.Value.Y < 0)
                    velocity.Value.Y = 0;
            }
        }

        private void ResolveHorizontalCollision(ref Position position, ref Velocity velocity, Rectangle entityBounds, Rectangle otherBounds)
        {
            float leftPenetration = entityBounds.Right - otherBounds.Left;
            float rightPenetration = otherBounds.Right - entityBounds.Left;

            if (leftPenetration < rightPenetration)
            {
                position.Value.X -= leftPenetration;
            }
            else
            {
                position.Value.X += rightPenetration;
            }
            velocity.Value.X = 0;
        }

        private void CheckRestingContact(Rectangle bounds, Rectangle otherBounds,
                                       ref bool touchingGround, ref bool touchingLeft,
                                       ref bool touchingRight, ref bool touchingTop)
        {
            // Bottom contact (grounded) - more precise check with margins
            if (bounds.Bottom >= otherBounds.Top - CONTACT_THRESHOLD &&
                bounds.Bottom <= otherBounds.Top + CONTACT_THRESHOLD &&
                bounds.Right > otherBounds.Left + 1 &&  // Small margin to avoid corner artifacts
                bounds.Left < otherBounds.Right - 1)
            {
                touchingGround = true;
            }

            // Top contact (ceiling) - more precise check
            if (bounds.Top >= otherBounds.Bottom - CONTACT_THRESHOLD &&
                bounds.Top <= otherBounds.Bottom + CONTACT_THRESHOLD &&
                bounds.Right > otherBounds.Left + 1 &&
                bounds.Left < otherBounds.Right - 1)
            {
                touchingTop = true;
            }

            // Left contact (wall on left side)
            if (bounds.Left >= otherBounds.Right - CONTACT_THRESHOLD &&
                bounds.Left <= otherBounds.Right + CONTACT_THRESHOLD &&
                bounds.Bottom > otherBounds.Top + 1 &&
                bounds.Top < otherBounds.Bottom - 1)
            {
                touchingLeft = true;
            }

            // Right contact (wall on right side)
            if (bounds.Right >= otherBounds.Left - CONTACT_THRESHOLD &&
                bounds.Right <= otherBounds.Left + CONTACT_THRESHOLD &&
                bounds.Bottom > otherBounds.Top + 1 &&
                bounds.Top < otherBounds.Bottom - 1)
            {
                touchingRight = true;
            }
        }

        // Helper class to organize entity collision data
        private class EntityCollisionInfo
        {
            public Rectangle OtherBounds { get; }
            public float LeftPenetration { get; }
            public float RightPenetration { get; }
            public float TopPenetration { get; }
            public float BottomPenetration { get; }
            public float HorizontalPenetration => Math.Min(LeftPenetration, RightPenetration);
            public float VerticalPenetration => Math.Min(TopPenetration, BottomPenetration);
            public bool ResolveVertically => VerticalPenetration < HorizontalPenetration;

            public EntityCollisionInfo(Rectangle otherBounds, Rectangle entityBounds)
            {
                OtherBounds = otherBounds;
                LeftPenetration = entityBounds.Right - otherBounds.Left;
                RightPenetration = otherBounds.Right - entityBounds.Left;
                TopPenetration = entityBounds.Bottom - otherBounds.Top;
                BottomPenetration = otherBounds.Bottom - entityBounds.Top;
            }
        }
    }
}