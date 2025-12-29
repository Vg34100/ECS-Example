using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Level.Components;
using System.Collections.Generic;
using System;

namespace ECS_Base.Mechanics.Collision.Systems
{
    /// <summary>
    /// AABB collision - resolves on axis with smallest penetration
    /// This prevents the teleporting bug when landing on platforms
    /// </summary>
    public class CollisionSystem
    {
        private const int TILE_SIZE = 16;
        private List<Rectangle> _solidObjects = new List<Rectangle>();

        public void Update(World world, float deltaTime)
        {
            // Collect all solid objects
            _solidObjects.Clear();

            // Collect tiles from levels
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    CollectTilesFromLevel(levelComponent.LevelData);
                }
            }

            // Collect static platforms
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<ColliderComponent>(entity, out var collider) &&
                    collider.Type == ColliderComponent.ColliderType.Static &&
                    world.TryGetComponent<PositionComponent>(entity, out var position))
                {
                    _solidObjects.Add(new Rectangle(
                        (int)position.Value.X + collider.Bounds.X,
                        (int)position.Value.Y + collider.Bounds.Y,
                        collider.Bounds.Width,
                        collider.Bounds.Height
                    ));
                }
            }

            // Handle dynamic entities
            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<ColliderComponent>(entity, out var collider) ||
                    collider.Type != ColliderComponent.ColliderType.Dynamic)
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position) ||
                    !world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                    continue;

                // Apply movement
                position.Value += velocity.Value * deltaTime;

                // Get world bounds
                Rectangle entityBounds = GetWorldBounds(position, collider);

                // Find collision with smallest penetration and resolve ONLY that one
                float smallestPenetration = float.MaxValue;
                Rectangle? collisionToResolve = null;
                bool resolveX = false;
                bool pushLeft = false;
                bool pushUp = false;

                // Find the collision with smallest penetration (ignore micro-penetrations to prevent jitter)
                const float MIN_PENETRATION_THRESHOLD = 0.1f;

                foreach (var solid in _solidObjects)
                {
                    if (entityBounds.Intersects(solid))
                    {
                        // Calculate penetration on each axis
                        float leftPen = entityBounds.Right - solid.Left;
                        float rightPen = solid.Right - entityBounds.Left;
                        float topPen = entityBounds.Bottom - solid.Top;
                        float bottomPen = solid.Bottom - entityBounds.Top;

                        // Find minimum penetration on each axis
                        float xPen = Math.Min(leftPen, rightPen);
                        float yPen = Math.Min(topPen, bottomPen);

                        // Find the smallest overall penetration
                        float minPen = Math.Min(xPen, yPen);

                        // Only resolve if penetration is significant (prevents micro-jitter)
                        if (minPen >= MIN_PENETRATION_THRESHOLD && minPen < smallestPenetration)
                        {
                            smallestPenetration = minPen;
                            collisionToResolve = solid;
                            resolveX = (xPen < yPen);
                            pushLeft = (leftPen < rightPen);
                            pushUp = (topPen < bottomPen);
                        }
                    }
                }

                // Resolve ONLY the smallest collision
                if (collisionToResolve.HasValue)
                {
                    var solid = collisionToResolve.Value;

                    if (resolveX)
                    {
                        // Resolve X collision
                        if (pushLeft)
                        {
                            position.Value.X = solid.Left - collider.Bounds.Width - collider.Bounds.X;
                        }
                        else
                        {
                            position.Value.X = solid.Right - collider.Bounds.X;
                        }
                        velocity.Value.X = 0;
                    }
                    else
                    {
                        // Resolve Y collision
                        if (pushUp)
                        {
                            position.Value.Y = solid.Top - collider.Bounds.Height - collider.Bounds.Y;
                            velocity.Value.Y = 0;
                        }
                        else
                        {
                            position.Value.Y = solid.Bottom - collider.Bounds.Y;
                            velocity.Value.Y = 0;
                        }
                    }
                }

                // Update grounded state
                UpdateGroundedState(entity, world, position, collider);

                // Update components
                world.AddComponent(entity, position);
                world.AddComponent(entity, velocity);
            }
        }

        private Rectangle GetWorldBounds(PositionComponent position, ColliderComponent collider)
        {
            return new Rectangle(
                (int)(position.Value.X + collider.Bounds.X),
                (int)(position.Value.Y + collider.Bounds.Y),
                collider.Bounds.Width,
                collider.Bounds.Height
            );
        }

        private void CollectTilesFromLevel(ECS_Base.LevelData.Level level)
        {
            if (level.TileData == null) return;

            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (level.TileData[row, col] > 0)
                    {
                        _solidObjects.Add(new Rectangle(
                            level.X + (col * TILE_SIZE),
                            level.Y + (row * TILE_SIZE),
                            TILE_SIZE,
                            TILE_SIZE
                        ));
                    }
                }
            }
        }

        private void UpdateGroundedState(Entity entity, World world, PositionComponent position,
                                        ColliderComponent collider)
        {
            if (!world.TryGetComponent<GroundedComponent>(entity, out var grounded))
                return;

            grounded.IsGrounded = false;
            grounded.TouchingLeft = false;
            grounded.TouchingRight = false;
            grounded.TouchingTop = false;

            Rectangle entityBounds = GetWorldBounds(position, collider);

            Rectangle groundCheck = new Rectangle(
                entityBounds.X + 2,
                entityBounds.Bottom,
                entityBounds.Width - 4,
                1
            );

            Rectangle leftCheck = new Rectangle(
                entityBounds.Left - 1,
                entityBounds.Y + 2,
                1,
                entityBounds.Height - 4
            );

            Rectangle rightCheck = new Rectangle(
                entityBounds.Right,
                entityBounds.Y + 2,
                1,
                entityBounds.Height - 4
            );

            Rectangle topCheck = new Rectangle(
                entityBounds.X + 2,
                entityBounds.Top - 1,
                entityBounds.Width - 4,
                1
            );

            foreach (var solid in _solidObjects)
            {
                if (groundCheck.Intersects(solid))
                    grounded.IsGrounded = true;
                if (leftCheck.Intersects(solid))
                    grounded.TouchingLeft = true;
                if (rightCheck.Intersects(solid))
                    grounded.TouchingRight = true;
                if (topCheck.Intersects(solid))
                    grounded.TouchingTop = true;
            }

            world.AddComponent(entity, grounded);
        }

        public List<Rectangle> GetCollisionTiles()
        {
            return new List<Rectangle>(_solidObjects);
        }
    }
}
