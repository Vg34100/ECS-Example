// Systems/CollisionSystem.cs - Fixed collision logic
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Diagnostics;
using System;

namespace ECS_Example.Systems
{
    public class CollisionSystem
    {
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

                Rectangle bounds = new Rectangle(
                    (int)position.Value.X,
                    (int)position.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                bool touchingGround = false;

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
                        // Calculate penetration depth on each side
                        float leftPenetration = bounds.Right - otherBounds.Left;
                        float rightPenetration = otherBounds.Right - bounds.Left;
                        float topPenetration = bounds.Bottom - otherBounds.Top;
                        float bottomPenetration = otherBounds.Bottom - bounds.Top;

                        // Find minimum penetration
                        float minX = Math.Min(leftPenetration, rightPenetration);
                        float minY = Math.Min(topPenetration, bottomPenetration);

                        // Resolve on the axis with smallest penetration
                        if (minX < minY)
                        {
                            // Resolve horizontally
                            if (leftPenetration < rightPenetration)
                                position.Value.X -= leftPenetration;
                            else
                                position.Value.X += rightPenetration;

                            velocity.Value.X = 0;
                        }
                        else
                        {
                            // Resolve vertically
                            if (topPenetration < bottomPenetration)
                            {
                                // Player is above the platform (standing on it)
                                position.Value.Y -= topPenetration;
                                velocity.Value.Y = 0;
                                touchingGround = true;
                                //Debug.WriteLine($"GROUNDED! Entity {entity.Id} on top of {other.Id}");
                            }
                            else
                            {
                                // Player hit from below (ceiling)
                                position.Value.Y += bottomPenetration;
                                velocity.Value.Y = 0;
                                //Debug.WriteLine("Hit ceiling");
                            }
                        }

                        world.AddComponent(entity, position);
                        world.AddComponent(entity, velocity);
                    }
                }

                // Update grounded state
                if (hasGravity)
                {
                    if (gravity.IsGrounded != touchingGround)
                    {
                        //Debug.WriteLine($"Entity {entity.Id} grounded state: {gravity.IsGrounded} -> {touchingGround}");
                    }
                    gravity.IsGrounded = touchingGround;
                    world.AddComponent(entity, gravity);
                }
            }
        }
    }
}