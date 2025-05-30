// Systems/RenderSystem.cs - Updated with accurate patrol debug visualization
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Example.Components;
using ECS_Example.Entities;
using System.Collections.Generic;
using System;

namespace ECS_Example.Systems
{
    public class RenderSystem
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;
        private CameraSystem _cameraSystem;
        private DebugSystem _debugSystem;
        private PatrolSystem _patrolSystem; // Reference to get accurate debug rectangles

        public RenderSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, CameraSystem cameraSystem, DebugSystem debugSystem = null)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
            _debugSystem = debugSystem;

            // Create a 1x1 white texture for drawing shapes
            _whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });
        }

        // Method to set patrol system reference
        public void SetPatrolSystem(PatrolSystem patrolSystem)
        {
            _patrolSystem = patrolSystem;
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            // Draw tile collision debug if enabled
            if (_debugSystem?.ShowTileCollisions == true)
            {
                DrawTileCollisions(world);
            }

            // Draw entities
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Position>(entity, out var position) &&
                    world.TryGetComponent<Shape>(entity, out var shape))
                {
                    // Check if entity should be visible
                    bool shouldDraw = true;
                    if (world.TryGetComponent<FlashEffect>(entity, out var flash))
                    {
                        shouldDraw = flash.IsVisible;
                    }

                    if (shouldDraw)
                    {
                        DrawShape(position, shape);

                        // Draw entity collision bounds if debug enabled
                        if (_debugSystem?.ShowEntityCollisions == true && world.TryGetComponent<Collider>(entity, out var collider))
                        {
                            DrawColliderBounds(position, collider, Color.Red);
                        }

                        // Draw player-specific debug info
                        if (_debugSystem?.ShowCollisionInfo == true && world.TryGetComponent<PlayerController>(entity, out _))
                        {
                            DrawPlayerCollisionDebug(world, entity, position);
                        }

                        // Draw patrol debug info
                        if (_debugSystem?.ShowPatrolDebug == true && world.TryGetComponent<Patrol>(entity, out var patrol))
                        {
                            DrawPatrolDebug(world, entity, position, patrol);
                        }

                        // Draw attack hitboxes for debugging
                        if (_debugSystem?.ShowEntityCollisions == true && world.TryGetComponent<Attack>(entity, out var attack) && attack.IsAttacking)
                        {
                            DrawAttackHitbox(position, attack, world.GetComponent<Collider>(entity));
                        }
                    }
                }
            }

            _spriteBatch.End();
        }

        private void DrawPatrolDebug(World world, Entity entity, Position position, Patrol patrol)
        {
            if (!world.TryGetComponent<Collider>(entity, out var collider))
                return;

            Rectangle entityBounds = new Rectangle(
                (int)position.Value.X,
                (int)position.Value.Y,
                collider.Bounds.Width,
                collider.Bounds.Height
            );

            // Draw direction indicator
            Color directionColor = patrol.FacingRight ? Color.Green : Color.Blue;
            Vector2 directionStart = new Vector2(entityBounds.Center.X, entityBounds.Center.Y);

            // Draw direction line (as a rectangle since we don't have line drawing)
            Rectangle directionRect = new Rectangle(
                patrol.FacingRight ? entityBounds.Center.X : entityBounds.Center.X - 16,
                entityBounds.Center.Y - 1,
                16, 2
            );
            _spriteBatch.Draw(_whiteTexture, directionRect, directionColor);

            // Use PatrolSystem methods to get accurate check rectangles
            if (_patrolSystem != null)
            {
                // Draw wall check area (yellow)
                Rectangle wallCheckRect = _patrolSystem.GetWallCheckRect(entityBounds, patrol.FacingRight);
                DrawRectangleOutline(wallCheckRect, Color.Yellow, 1);

                // Draw edge check area (purple) - only if avoiding falls
                if (patrol.AvoidFalling)
                {
                    Rectangle edgeCheckRect = _patrolSystem.GetEdgeCheckRect(entityBounds, patrol.FacingRight);
                    DrawRectangleOutline(edgeCheckRect, Color.Purple, 1);
                }
            }
            else
            {
                // Fallback if patrol system reference not set - use simplified visualization
                DrawFallbackPatrolDebug(entityBounds, patrol);
            }
        }

        private void DrawFallbackPatrolDebug(Rectangle entityBounds, Patrol patrol)
        {
            // Simplified fallback visualization
            const float WALL_CHECK_DISTANCE = 4f;
            const float EDGE_CHECK_AHEAD = 6f;

            // Draw wall check area
            Rectangle wallCheckRect;
            if (patrol.FacingRight)
            {
                wallCheckRect = new Rectangle(
                    entityBounds.Right,
                    entityBounds.Top + 2,
                    (int)WALL_CHECK_DISTANCE,
                    entityBounds.Height - 4
                );
            }
            else
            {
                wallCheckRect = new Rectangle(
                    entityBounds.Left - (int)WALL_CHECK_DISTANCE,
                    entityBounds.Top + 2,
                    (int)WALL_CHECK_DISTANCE,
                    entityBounds.Height - 4
                );
            }
            DrawRectangleOutline(wallCheckRect, Color.Yellow, 1);

            // Draw edge check area (if avoiding falls)
            if (patrol.AvoidFalling)
            {
                int checkWidth = Math.Max(3, entityBounds.Width / 4);
                int checkDepth = Math.Max(4, entityBounds.Height / 3);

                Rectangle edgeCheckRect;
                if (patrol.FacingRight)
                {
                    edgeCheckRect = new Rectangle(
                        entityBounds.Right + (int)EDGE_CHECK_AHEAD - (checkWidth / 2),
                        entityBounds.Bottom,
                        checkWidth,
                        checkDepth
                    );
                }
                else
                {
                    edgeCheckRect = new Rectangle(
                        entityBounds.Left - (int)EDGE_CHECK_AHEAD - (checkWidth / 2),
                        entityBounds.Bottom,
                        checkWidth,
                        checkDepth
                    );
                }
                DrawRectangleOutline(edgeCheckRect, Color.Purple, 1);
            }
        }

        private void DrawTileCollisions(World world)
        {
            // Draw tile collision boxes from all active levels
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    DrawLevelTileCollisions(levelComponent.LevelData);
                }
            }
        }

        private void DrawLevelTileCollisions(ECS_Example.LevelData.Level level)
        {
            if (level.TileData == null) return;

            const int TILE_SIZE = 16;
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

                        // Draw tile collision outline
                        DrawRectangleOutline(tileRect, Color.Cyan, 1);
                    }
                }
            }
        }

        private void DrawPlayerCollisionDebug(World world, Entity player, Position position)
        {
            if (!world.TryGetComponent<Collider>(player, out var collider))
                return;

            Rectangle bounds = new Rectangle(
                (int)position.Value.X,
                (int)position.Value.Y,
                collider.Bounds.Width,
                collider.Bounds.Height
            );

            // Draw different colored edges based on collision state
            if (_debugSystem != null)
            {
                // Left edge
                Color leftColor = _debugSystem.PlayerTouchingLeft ? Color.Red : Color.Green;
                _spriteBatch.Draw(_whiteTexture, new Rectangle(bounds.Left - 1, bounds.Top, 2, bounds.Height), leftColor);

                // Right edge
                Color rightColor = _debugSystem.PlayerTouchingRight ? Color.Red : Color.Green;
                _spriteBatch.Draw(_whiteTexture, new Rectangle(bounds.Right - 1, bounds.Top, 2, bounds.Height), rightColor);

                // Top edge
                Color topColor = _debugSystem.PlayerTouchingTop ? Color.Red : Color.Green;
                _spriteBatch.Draw(_whiteTexture, new Rectangle(bounds.Left, bounds.Top - 1, bounds.Width, 2), topColor);

                // Bottom edge
                Color bottomColor = _debugSystem.PlayerTouchingBottom ? Color.Red : Color.Green;
                _spriteBatch.Draw(_whiteTexture, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 2), bottomColor);
            }
        }

        private void DrawColliderBounds(Position position, Collider collider, Color color)
        {
            Rectangle bounds = new Rectangle(
                (int)position.Value.X + collider.Bounds.X,
                (int)position.Value.Y + collider.Bounds.Y,
                collider.Bounds.Width,
                collider.Bounds.Height
            );

            DrawRectangleOutline(bounds, color, 1);
        }

        private void DrawAttackHitbox(Position position, Attack attack, Collider entityCollider)
        {
            // Get entity center for better positioning
            float entityCenterX = position.Value.X + (entityCollider.Bounds.Width / 2);
            float entityCenterY = position.Value.Y + (entityCollider.Bounds.Height / 2);

            // Calculate hitbox position
            float hitboxX;
            if (attack.IsFacingRight)
            {
                hitboxX = position.Value.X + entityCollider.Bounds.Width;
            }
            else
            {
                hitboxX = position.Value.X - attack.HitboxSize.X;
            }

            float hitboxY = entityCenterY - (attack.HitboxSize.Y / 2);

            Rectangle attackRect = new Rectangle(
                (int)hitboxX,
                (int)hitboxY,
                (int)attack.HitboxSize.X,
                (int)attack.HitboxSize.Y
            );

            _spriteBatch.Draw(_whiteTexture, attackRect, Color.Red * 0.5f);
            DrawRectangleOutline(attackRect, Color.Red, 2);
        }

        private void DrawRectangleOutline(Rectangle rect, Color color, int thickness)
        {
            // Top
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        private void DrawShape(Position position, Shape shape)
        {
            switch (shape.Type)
            {
                case Shape.ShapeType.Rectangle:
                    _spriteBatch.Draw(
                        _whiteTexture,
                        new Rectangle(
                            (int)position.Value.X,
                            (int)position.Value.Y,
                            (int)shape.Size.X,
                            (int)shape.Size.Y
                        ),
                        shape.Color
                    );
                    break;

                case Shape.ShapeType.Circle:
                    // For now, we'll draw circles as squares
                    _spriteBatch.Draw(
                        _whiteTexture,
                        new Rectangle(
                            (int)(position.Value.X - shape.Size.X / 2),
                            (int)(position.Value.Y - shape.Size.X / 2),
                            (int)shape.Size.X,
                            (int)shape.Size.X
                        ),
                        shape.Color
                    );
                    break;
            }
        }
    }
}