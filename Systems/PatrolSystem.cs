// Systems/PatrolSystem.cs - Refined with smaller, more appropriate detection areas
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System;
using ECS_Example.Entities;
using System.Collections.Generic;

namespace ECS_Example.Systems
{
    public class PatrolSystem
    {
        private const int TILE_SIZE = 16;
        private const float WALL_CHECK_DISTANCE = 4f; // Small but safe distance to avoid getting stuck
        private const float EDGE_CHECK_AHEAD = 6f; // How far ahead to check for edge
        private List<Rectangle> _collisionTiles = new List<Rectangle>();

        public void Update(World world)
        {
            // Get current collision tiles from active levels
            UpdateCollisionTiles(world);

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

                // Check for wall collision ahead
                if (CheckWallAhead(bounds, patrol.FacingRight))
                {
                    shouldTurn = true;
                }

                // If avoiding falls, check for edge ahead
                if (patrol.AvoidFalling && !shouldTurn)
                {
                    if (CheckForEdgeAhead(bounds, patrol.FacingRight))
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

        private void UpdateCollisionTiles(World world)
        {
            _collisionTiles.Clear();

            // Get collision tiles from all active levels
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    GenerateCollisionTiles(levelComponent.LevelData);
                }
            }
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

        private bool CheckWallAhead(Rectangle entityBounds, bool facingRight)
        {
            // Create a smaller check rectangle much closer to the entity
            Rectangle checkRect;

            if (facingRight)
            {
                checkRect = new Rectangle(
                    entityBounds.Right, // Start right at the entity edge
                    entityBounds.Top + 2, // Small margin from top
                    (int)WALL_CHECK_DISTANCE, // Very small distance ahead (2px)
                    entityBounds.Height - 4 // Small margin from bottom
                );
            }
            else
            {
                checkRect = new Rectangle(
                    entityBounds.Left - (int)WALL_CHECK_DISTANCE, // Very close to left edge
                    entityBounds.Top + 2,
                    (int)WALL_CHECK_DISTANCE,
                    entityBounds.Height - 4
                );
            }

            // Check if any collision tiles intersect with our check rectangle
            foreach (var tileRect in _collisionTiles)
            {
                if (checkRect.Intersects(tileRect))
                {
                    return true; // Wall detected
                }
            }

            return false;
        }

        private bool CheckForEdgeAhead(Rectangle entityBounds, bool facingRight)
        {
            // Make edge detection proportional to actual entity size from collider
            int checkWidth = Math.Max(3, entityBounds.Width / 4); // At least 3px, or 1/4 of actual entity width
            int checkDepth = Math.Max(4, entityBounds.Height / 3); // At least 4px, or 1/3 of actual entity height

            Rectangle checkRect;

            if (facingRight)
            {
                checkRect = new Rectangle(
                    entityBounds.Right + (int)EDGE_CHECK_AHEAD - (checkWidth / 2), // Check ahead, centered
                    entityBounds.Bottom, // Start at bottom of entity
                    checkWidth, // Width based on actual entity size
                    checkDepth // Depth based on actual entity size
                );
            }
            else
            {
                checkRect = new Rectangle(
                    entityBounds.Left - (int)EDGE_CHECK_AHEAD - (checkWidth / 2),
                    entityBounds.Bottom,
                    checkWidth,
                    checkDepth
                );
            }

            // Check if there's ground below the next step
            foreach (var tileRect in _collisionTiles)
            {
                if (checkRect.Intersects(tileRect))
                {
                    return false; // Ground found - no edge
                }
            }

            return true; // No ground found - it's an edge!
        }

        // Getter methods for debug visualization
        public float GetWallCheckDistance() => WALL_CHECK_DISTANCE;
        public float GetEdgeCheckAhead() => EDGE_CHECK_AHEAD;

        public Rectangle GetWallCheckRect(Rectangle entityBounds, bool facingRight)
        {
            if (facingRight)
            {
                return new Rectangle(
                    entityBounds.Right,
                    entityBounds.Top + 2,
                    (int)WALL_CHECK_DISTANCE,
                    entityBounds.Height - 4
                );
            }
            else
            {
                return new Rectangle(
                    entityBounds.Left - (int)WALL_CHECK_DISTANCE,
                    entityBounds.Top + 2,
                    (int)WALL_CHECK_DISTANCE,
                    entityBounds.Height - 4
                );
            }
        }

        public Rectangle GetEdgeCheckRect(Rectangle entityBounds, bool facingRight)
        {
            int checkWidth = Math.Max(3, entityBounds.Width / 4);
            int checkDepth = Math.Max(4, entityBounds.Height / 3);

            if (facingRight)
            {
                return new Rectangle(
                    entityBounds.Right + (int)EDGE_CHECK_AHEAD - (checkWidth / 2),
                    entityBounds.Bottom,
                    checkWidth,
                    checkDepth
                );
            }
            else
            {
                return new Rectangle(
                    entityBounds.Left - (int)EDGE_CHECK_AHEAD - (checkWidth / 2),
                    entityBounds.Bottom,
                    checkWidth,
                    checkDepth
                );
            }
        }
    }
}