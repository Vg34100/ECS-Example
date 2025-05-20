// Systems/PatrolSystem.cs - Platformer-style patrol
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System;
using ECS_Example.Entities;

namespace ECS_Example.Systems
{
    public class PatrolSystem
    {
        public void Update(World world)
        {
            var entities = world.GetEntities();

            foreach (var entity in entities)
            {
                if (!world.TryGetComponent<Patrol>(entity, out var patrol) ||
                    !world.TryGetComponent<Position>(entity, out var position) ||
                    !world.TryGetComponent<Velocity>(entity, out var velocity) ||
                    !world.TryGetComponent<Collider>(entity, out var collider))
                    continue;

                // Skip patrol logic if entity is invulnerable (recently hit)
                if (world.TryGetComponent<Invulnerable>(entity, out _))
                    continue;

                // Create bounds for this entity
                Rectangle bounds = new Rectangle(
                    (int)position.Value.X,
                    (int)position.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                bool shouldTurn = false;

                // Check for wall collision
                if (CheckWallCollision(world, entities, bounds, patrol.FacingRight))
                {
                    shouldTurn = true;
                }

                // If avoiding falls, check for edge
                if (patrol.AvoidFalling && !shouldTurn)
                {
                    if (CheckForEdge(world, entities, position, collider, patrol.FacingRight))
                    {
                        shouldTurn = true;
                    }
                }

                // Turn around if needed
                if (shouldTurn)
                {
                    patrol.FacingRight = !patrol.FacingRight;
                    world.AddComponent(entity, patrol);
                }

                // Set velocity based on direction
                velocity.Value.X = patrol.FacingRight ? patrol.Speed : -patrol.Speed;
                world.AddComponent(entity, velocity);
            }
        }

        private bool CheckWallCollision(World world, System.Collections.Generic.List<Entity> entities,
                                       Rectangle bounds, bool facingRight)
        {
            // Create a thin rectangle to check for walls
            Rectangle checkRect = facingRight
                ? new Rectangle(bounds.Right, bounds.Y + 5, 2, bounds.Height - 10)
                : new Rectangle(bounds.Left - 2, bounds.Y + 5, 2, bounds.Height - 10);

            foreach (var other in entities)
            {
                if (!world.TryGetComponent<Collider>(other, out var otherCollider) ||
                    otherCollider.Type != Collider.ColliderType.Static ||
                    !world.TryGetComponent<Position>(other, out var otherPosition))
                    continue;

                Rectangle otherBounds = new Rectangle(
                    (int)otherPosition.Value.X,
                    (int)otherPosition.Value.Y,
                    otherCollider.Bounds.Width,
                    otherCollider.Bounds.Height
                );

                if (checkRect.Intersects(otherBounds))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckForEdge(World world, System.Collections.Generic.List<Entity> entities,
                                 Position position, Collider collider, bool facingRight)
        {
            // Check a point below and ahead of the entity
            int checkX = facingRight
                ? (int)(position.Value.X + collider.Bounds.Width + 5)
                : (int)(position.Value.X - 5);

            Rectangle checkRect = new Rectangle(
                checkX,
                (int)(position.Value.Y + collider.Bounds.Height + 1),
                2,
                5
            );

            // Check if there's ground below the next step
            foreach (var other in entities)
            {
                if (!world.TryGetComponent<Collider>(other, out var otherCollider) ||
                    otherCollider.Type != Collider.ColliderType.Static ||
                    !world.TryGetComponent<Position>(other, out var otherPosition))
                    continue;

                Rectangle otherBounds = new Rectangle(
                    (int)otherPosition.Value.X,
                    (int)otherPosition.Value.Y,
                    otherCollider.Bounds.Width,
                    otherCollider.Bounds.Height
                );

                if (checkRect.Intersects(otherBounds))
                {
                    return false; // There is ground
                }
            }

            return true; // No ground found - it's an edge!
        }
    }
}