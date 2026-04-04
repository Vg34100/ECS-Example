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
    /// Swept AABB collision detection - prevents tunneling at high speeds
    /// Only processes entities with SweptCollisionComponent
    /// Uses swept collision to check the path from old position to new position
    /// </summary>
    public class SweptCollisionSystem
    {
        private const int TILE_SIZE = 16;
        private List<Rectangle> _solidObjects = new List<Rectangle>();
        private HashSet<int> _solidValues = null;
        private HashSet<int> _waterValues = null;
        private bool _waterIsSolid = true;

        public void Update(World world, float deltaTime)
        {
            // Collect all solid objects (shared with regular collision system)
            _solidObjects.Clear();
            UpdateTileCollisionConfig(world);

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

            // Process only entities with SweptCollisionComponent
            foreach (var entity in world.GetEntities())
            {
                // Must have SweptCollisionComponent to use this system
                if (!world.HasComponent<SweptCollisionComponent>(entity))
                    continue;

                if (!world.TryGetComponent<ColliderComponent>(entity, out var collider) ||
                    collider.Type != ColliderComponent.ColliderType.Dynamic)
                    continue;

                if (!world.TryGetComponent<PositionComponent>(entity, out var position) ||
                    !world.TryGetComponent<VelocityComponent>(entity, out var velocity))
                    continue;

                // Store old position before movement
                Vector2 oldPosition = position.Value;

                // Calculate new position
                Vector2 newPosition = oldPosition + velocity.Value * deltaTime;

                // Perform swept collision
                SweptAABB(ref position, ref velocity, collider, oldPosition, newPosition);

                // Update grounded state
                UpdateGroundedState(entity, world, position, collider);

                // Update components
                world.AddComponent(entity, position);
                world.AddComponent(entity, velocity);
            }
        }

        private void SweptAABB(ref PositionComponent position, ref VelocityComponent velocity,
                              ColliderComponent collider, Vector2 oldPos, Vector2 newPos)
        {
            // Get entity bounds
            Rectangle entityBounds = new Rectangle(
                (int)oldPos.X + collider.Bounds.X,
                (int)oldPos.Y + collider.Bounds.Y,
                collider.Bounds.Width,
                collider.Bounds.Height
            );

            // Calculate movement
            Vector2 movement = newPos - oldPos;

            // Find earliest collision time along the sweep
            float closestCollisionTime = 1.0f; // 1.0 = full movement, no collision
            Vector2 normalAtCollision = Vector2.Zero;

            foreach (var solid in _solidObjects)
            {
                // Perform swept AABB test
                float collisionTime = SweptAABBTest(entityBounds, movement, solid, out Vector2 normal);

                if (collisionTime < closestCollisionTime)
                {
                    closestCollisionTime = collisionTime;
                    normalAtCollision = normal;
                }
            }

            // Apply movement up to collision point
            position.Value = oldPos + movement * closestCollisionTime;

            // If we hit something, adjust velocity
            if (closestCollisionTime < 1.0f)
            {
                // Remove velocity component along collision normal
                float dotProduct = velocity.Value.X * normalAtCollision.X + velocity.Value.Y * normalAtCollision.Y;
                velocity.Value.X -= dotProduct * normalAtCollision.X;
                velocity.Value.Y -= dotProduct * normalAtCollision.Y;
            }
        }

        private float SweptAABBTest(Rectangle movingBox, Vector2 movement, Rectangle staticBox, out Vector2 normal)
        {
            normal = Vector2.Zero;

            // Expand the static box by the moving box's size (Minkowski sum)
            Rectangle expandedBox = new Rectangle(
                staticBox.X - movingBox.Width,
                staticBox.Y - movingBox.Height,
                staticBox.Width + movingBox.Width,
                staticBox.Height + movingBox.Height
            );

            // Ray-cast from moving box's position through the movement vector
            Vector2 rayOrigin = new Vector2(movingBox.X, movingBox.Y);

            // Check if ray intersects expanded box
            float entryTime, exitTime;
            if (!RayIntersectsRect(rayOrigin, movement, expandedBox, out entryTime, out exitTime))
            {
                return 1.0f; // No collision
            }

            // If collision happened in the past or future, ignore
            if (entryTime < 0.0f || entryTime > 1.0f)
            {
                return 1.0f; // No collision in this frame
            }

            // Calculate collision normal
            Vector2 entryPoint = rayOrigin + movement * entryTime;

            // Determine which face we hit
            float epsilon = 0.001f;
            if (Math.Abs(entryPoint.X - expandedBox.Left) < epsilon)
                normal = new Vector2(-1, 0); // Left face
            else if (Math.Abs(entryPoint.X - expandedBox.Right) < epsilon)
                normal = new Vector2(1, 0); // Right face
            else if (Math.Abs(entryPoint.Y - expandedBox.Top) < epsilon)
                normal = new Vector2(0, -1); // Top face
            else if (Math.Abs(entryPoint.Y - expandedBox.Bottom) < epsilon)
                normal = new Vector2(0, 1); // Bottom face

            return entryTime;
        }

        private bool RayIntersectsRect(Vector2 rayOrigin, Vector2 rayDir, Rectangle rect, out float entryTime, out float exitTime)
        {
            entryTime = float.MinValue;
            exitTime = float.MaxValue;

            // X axis
            if (rayDir.X != 0)
            {
                float t1 = (rect.Left - rayOrigin.X) / rayDir.X;
                float t2 = (rect.Right - rayOrigin.X) / rayDir.X;

                if (t1 > t2) { float temp = t1; t1 = t2; t2 = temp; } // Swap

                entryTime = Math.Max(entryTime, t1);
                exitTime = Math.Min(exitTime, t2);
            }
            else
            {
                // Ray parallel to X axis - check if within bounds
                if (rayOrigin.X < rect.Left || rayOrigin.X > rect.Right)
                    return false;
            }

            // Y axis
            if (rayDir.Y != 0)
            {
                float t1 = (rect.Top - rayOrigin.Y) / rayDir.Y;
                float t2 = (rect.Bottom - rayOrigin.Y) / rayDir.Y;

                if (t1 > t2) { float temp = t1; t1 = t2; t2 = temp; } // Swap

                entryTime = Math.Max(entryTime, t1);
                exitTime = Math.Min(exitTime, t2);
            }
            else
            {
                // Ray parallel to Y axis - check if within bounds
                if (rayOrigin.Y < rect.Top || rayOrigin.Y > rect.Bottom)
                    return false;
            }

            return entryTime <= exitTime && exitTime >= 0;
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
                    int value = level.TileData[row, col];
                    if (IsSolidTile(value))
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

        private void UpdateTileCollisionConfig(World world)
        {
            foreach (var entity in world.Query<LevelCollisionConfigComponent>())
            {
                var config = world.GetComponent<LevelCollisionConfigComponent>(entity);
                _solidValues = config.SolidValues;
                _waterValues = config.WaterValues;
                _waterIsSolid = config.WaterIsSolid;
                return;
            }

            _solidValues = null;
            _waterValues = null;
            _waterIsSolid = true;
        }

        private bool IsSolidTile(int value)
        {
            if (_solidValues == null)
                return value > 0;

            if (_waterValues != null && _waterValues.Contains(value))
                return _waterIsSolid;

            return _solidValues.Contains(value);
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

            Rectangle entityBounds = new Rectangle(
                (int)(position.Value.X + collider.Bounds.X),
                (int)(position.Value.Y + collider.Bounds.Y),
                collider.Bounds.Width,
                collider.Bounds.Height
            );

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
