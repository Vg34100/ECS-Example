// Systems/LevelCollisionSystem.cs
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Collections.Generic;

namespace ECS_Example.Systems
{
    public class LevelCollisionSystem
    {
        private const int TILE_SIZE = 16;
        private List<Rectangle> _collisionTiles = new List<Rectangle>();

        public void Update(World world)
        {
            // Clear previous collision tiles
            _collisionTiles.Clear();

            // Generate collision tiles from all active levels
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    GenerateCollisionTiles(levelComponent.LevelData);
                }
            }

            // Apply collision detection to dynamic entities
            ApplyCollisionToEntities(world);
        }

        private void GenerateCollisionTiles(ECS_Example.LevelData.Level level)
        {
            if (level.TileData == null) return;

            int rows = level.TileData.GetLength(0);
            int cols = level.TileData.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    // Any tile value > 0 creates collision
                    if (level.TileData[row, col] > 0)
                    {
                        Rectangle tileRect = new Rectangle(
                            level.X + (col * TILE_SIZE),
                            level.Y + (row * TILE_SIZE),
                            TILE_SIZE,
                            TILE_SIZE
                        );
                        _collisionTiles.Add(tileRect);
                    }
                }
            }
        }

        private void ApplyCollisionToEntities(World world)
        {
            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<Collider>(entity, out var collider) ||
                    collider.Type != Collider.ColliderType.Dynamic)
                    continue;

                if (!world.TryGetComponent<Position>(entity, out var position) ||
                    !world.TryGetComponent<Velocity>(entity, out var velocity))
                    continue;

                bool hasGravity = world.TryGetComponent<Gravity>(entity, out var gravity);

                Rectangle entityBounds = new Rectangle(
                    (int)position.Value.X,
                    (int)position.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                bool touchingGround = false;

                // Check collision against all tile collision rectangles
                foreach (var tileRect in _collisionTiles)
                {
                    if (entityBounds.Intersects(tileRect))
                    {
                        // Calculate penetration depth on each side
                        float leftPenetration = entityBounds.Right - tileRect.Left;
                        float rightPenetration = tileRect.Right - entityBounds.Left;
                        float topPenetration = entityBounds.Bottom - tileRect.Top;
                        float bottomPenetration = tileRect.Bottom - entityBounds.Top;

                        // Find minimum penetration
                        float minX = System.Math.Min(leftPenetration, rightPenetration);
                        float minY = System.Math.Min(topPenetration, bottomPenetration);

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
                                // Entity is above the tile (standing on it)
                                position.Value.Y -= topPenetration;
                                velocity.Value.Y = 0;
                                touchingGround = true;
                            }
                            else
                            {
                                // Entity hit from below (ceiling)
                                position.Value.Y += bottomPenetration;
                                velocity.Value.Y = 0;
                            }
                        }

                        world.AddComponent(entity, position);
                        world.AddComponent(entity, velocity);
                    }
                }

                // Update grounded state
                if (hasGravity)
                {
                    gravity.IsGrounded = touchingGround;
                    world.AddComponent(entity, gravity);
                }
            }
        }


        // Debug method to get collision tiles for rendering
        public List<Rectangle> GetCollisionTiles()
        {
            return new List<Rectangle>(_collisionTiles);
        }
    }
}