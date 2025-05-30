// Systems/LevelCollisionSystem.cs - Fixed corner collision bugs
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Example.Systems
{
    public class LevelCollisionSystem
    {
        private const int TILE_SIZE = 16;
        private const float CONTACT_THRESHOLD = 2.0f; // Distance to consider "touching"
        private List<Rectangle> _collisionTiles = new List<Rectangle>();
        private DebugSystem _debugSystem;

        public LevelCollisionSystem(DebugSystem debugSystem = null)
        {
            _debugSystem = debugSystem;
        }

        public void Update(World world)
        {
            // Reset collision flags at start of frame
            _debugSystem?.ResetCollisionFlags();

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
                bool isPlayer = world.TryGetComponent<PlayerController>(entity, out _);

                Rectangle entityBounds = new Rectangle(
                    (int)position.Value.X,
                    (int)position.Value.Y,
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );

                bool touchingGround = false;
                bool touchingLeft = false;
                bool touchingRight = false;
                bool touchingTop = false;

                // Collect all colliding tiles and their collision info
                var collisions = new List<CollisionInfo>();
                foreach (var tileRect in _collisionTiles)
                {
                    if (entityBounds.Intersects(tileRect))
                    {
                        collisions.Add(new CollisionInfo(tileRect, entityBounds));
                    }
                }

                // Sort collisions by priority: vertical first (ground/ceiling), then horizontal (walls)
                // This prevents corner collision artifacts
                var verticalCollisions = collisions.Where(c => c.ResolveVertically).OrderBy(c => c.VerticalPenetration);
                var horizontalCollisions = collisions.Where(c => !c.ResolveVertically).OrderBy(c => c.HorizontalPenetration);

                // Resolve vertical collisions first (more important for platformers)
                foreach (var collision in verticalCollisions)
                {
                    ResolveVerticalCollision(ref position, ref velocity, collision);

                    // Update bounds after each resolution
                    entityBounds = new Rectangle(
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
                    if (entityBounds.Intersects(collision.TileRect))
                    {
                        ResolveHorizontalCollision(ref position, ref velocity, entityBounds, collision.TileRect);

                        // Update bounds after each resolution
                        entityBounds = new Rectangle(
                            (int)position.Value.X,
                            (int)position.Value.Y,
                            collider.Bounds.Width,
                            collider.Bounds.Height
                        );
                    }
                }

                // Second pass: Check for resting contact (near-touching)
                foreach (var tileRect in _collisionTiles)
                {
                    CheckRestingContact(entityBounds, tileRect, ref touchingGround, ref touchingLeft, ref touchingRight, ref touchingTop);
                }

                // Special case: If touching ground and velocity is very small downward, zero it out
                if (touchingGround && velocity.Value.Y > -10 && velocity.Value.Y < 50)
                {
                    velocity.Value.Y = 0;
                }

                // Special case: If touching walls and moving into them slowly, zero horizontal velocity
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

                // Update debug collision info for player
                if (isPlayer && _debugSystem != null)
                {
                    _debugSystem.PlayerTouchingLeft = touchingLeft;
                    _debugSystem.PlayerTouchingRight = touchingRight;
                    _debugSystem.PlayerTouchingTop = touchingTop;
                    _debugSystem.PlayerTouchingBottom = touchingGround;
                }
            }
        }

        private void ResolveVerticalCollision(ref Position position, ref Velocity velocity, CollisionInfo collision)
        {
            if (collision.TopPenetration < collision.BottomPenetration)
            {
                // Entity is penetrating from below, push it up (standing on platform)
                position.Value.Y -= collision.TopPenetration;
                // Only zero Y velocity if moving downward
                if (velocity.Value.Y > 0)
                    velocity.Value.Y = 0;
            }
            else
            {
                // Entity is penetrating from above, push it down (hit ceiling)
                position.Value.Y += collision.BottomPenetration;
                // Only zero Y velocity if moving upward
                if (velocity.Value.Y < 0)
                    velocity.Value.Y = 0;
            }
        }

        private void ResolveHorizontalCollision(ref Position position, ref Velocity velocity, Rectangle entityBounds, Rectangle tileRect)
        {
            float leftPenetration = entityBounds.Right - tileRect.Left;
            float rightPenetration = tileRect.Right - entityBounds.Left;

            if (leftPenetration < rightPenetration)
            {
                // Entity is penetrating from the right, push it left
                position.Value.X -= leftPenetration;
            }
            else
            {
                // Entity is penetrating from the left, push it right
                position.Value.X += rightPenetration;
            }
            velocity.Value.X = 0;
        }

        private void CheckRestingContact(Rectangle bounds, Rectangle tileRect,
                                       ref bool touchingGround, ref bool touchingLeft,
                                       ref bool touchingRight, ref bool touchingTop)
        {
            // Bottom contact (grounded) - more precise check
            if (bounds.Bottom >= tileRect.Top - CONTACT_THRESHOLD &&
                bounds.Bottom <= tileRect.Top + CONTACT_THRESHOLD &&
                bounds.Right > tileRect.Left + 1 &&  // Small margin to avoid corner artifacts
                bounds.Left < tileRect.Right - 1)
            {
                touchingGround = true;
            }

            // Top contact (ceiling) - more precise check
            if (bounds.Top >= tileRect.Bottom - CONTACT_THRESHOLD &&
                bounds.Top <= tileRect.Bottom + CONTACT_THRESHOLD &&
                bounds.Right > tileRect.Left + 1 &&
                bounds.Left < tileRect.Right - 1)
            {
                touchingTop = true;
            }

            // Left contact (wall on left side)
            if (bounds.Left >= tileRect.Right - CONTACT_THRESHOLD &&
                bounds.Left <= tileRect.Right + CONTACT_THRESHOLD &&
                bounds.Bottom > tileRect.Top + 1 &&
                bounds.Top < tileRect.Bottom - 1)
            {
                touchingLeft = true;
            }

            // Right contact (wall on right side)
            if (bounds.Right >= tileRect.Left - CONTACT_THRESHOLD &&
                bounds.Right <= tileRect.Left + CONTACT_THRESHOLD &&
                bounds.Bottom > tileRect.Top + 1 &&
                bounds.Top < tileRect.Bottom - 1)
            {
                touchingRight = true;
            }
        }

        // Helper class to organize collision data
        private class CollisionInfo
        {
            public Rectangle TileRect { get; }
            public float LeftPenetration { get; }
            public float RightPenetration { get; }
            public float TopPenetration { get; }
            public float BottomPenetration { get; }
            public float HorizontalPenetration => System.Math.Min(LeftPenetration, RightPenetration);
            public float VerticalPenetration => System.Math.Min(TopPenetration, BottomPenetration);
            public bool ResolveVertically => VerticalPenetration < HorizontalPenetration;

            public CollisionInfo(Rectangle tileRect, Rectangle entityBounds)
            {
                TileRect = tileRect;
                LeftPenetration = entityBounds.Right - tileRect.Left;
                RightPenetration = tileRect.Right - entityBounds.Left;
                TopPenetration = entityBounds.Bottom - tileRect.Top;
                BottomPenetration = tileRect.Bottom - entityBounds.Top;
            }
        }

        // Debug method to get collision tiles for rendering
        public List<Rectangle> GetCollisionTiles()
        {
            return new List<Rectangle>(_collisionTiles);
        }
    }
}