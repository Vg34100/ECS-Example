using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.Platformer.Components;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ECS_Base.Mechanics.Collision.Systems
{
    /// <summary>
    /// AABB collision - resolves on axis with smallest penetration
    /// This prevents the teleporting bug when landing on platforms
    /// </summary>
    public class CollisionSystem
    {
        private const int TILE_SIZE = 16;
        private List<(Rectangle bounds, Entity entity)> _solidObjects = new List<(Rectangle, Entity)>();
        private HashSet<int> _solidValues = null;
        private HashSet<int> _waterValues = null;
        private bool _waterIsSolid = true;
        private bool _useAxisSeparation = false;
        private bool _useGridSnapForTiles = false;
        private readonly List<ECS_Base.LevelData.Level> _activeLevels = new List<ECS_Base.LevelData.Level>();

        public void Update(World world, float deltaTime)
        {
            // Collect all solid objects
            _solidObjects.Clear();
            UpdateTileCollisionConfig(world);
            _activeLevels.Clear();

            // Collect tiles from levels
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    _activeLevels.Add(levelComponent.LevelData);
                    if (!_useGridSnapForTiles)
                    {
                        CollectTilesFromLevel(levelComponent.LevelData);
                    }
                }
            }

            // Collect static platforms
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<ColliderComponent>(entity, out var collider) &&
                    collider.Type == ColliderComponent.ColliderType.Static &&
                    world.TryGetComponent<PositionComponent>(entity, out var position))
                {
                    var bounds = new Rectangle(
                        (int)position.Value.X + collider.Bounds.X,
                        (int)position.Value.Y + collider.Bounds.Y,
                        collider.Bounds.Width,
                        collider.Bounds.Height
                    );
                    _solidObjects.Add((bounds, entity));  // Store entity reference
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

                if (_useAxisSeparation)
                {
                    ResolveAxisSeparated(ref position, ref velocity, collider, deltaTime);
                }
                else
                {
                    position.Value += velocity.Value * deltaTime;

                    Rectangle entityBounds = GetWorldBounds(position, collider);

                    float smallestPenetration = float.MaxValue;
                    Rectangle? collisionToResolve = null;
                    bool resolveX = false;
                    bool pushLeft = false;
                    bool pushUp = false;

                    const float MIN_PENETRATION_THRESHOLD = 0.1f;

                    foreach ((Rectangle solidBounds, Entity solidEntity) in _solidObjects)
                    {
                        if (entityBounds.Intersects(solidBounds))
                        {
                            if (solidEntity != null &&
                                world.TryGetComponent<PlatformComponent>(solidEntity, out var platform) &&
                                platform.OneWay)
                            {
                                if (world.TryGetComponent<PositionComponent>(solidEntity, out var platformPos))
                                {
                                    float platformTop = platformPos.Value.Y - platform.Height * 0.5f;
                                    bool canPass = velocity.Value.Y < 0 || position.Value.Y > platformTop;
                                    if (canPass)
                                    {
                                        continue;
                                    }
                                }
                            }

                            float leftPen = entityBounds.Right - solidBounds.Left;
                            float rightPen = solidBounds.Right - entityBounds.Left;
                            float topPen = entityBounds.Bottom - solidBounds.Top;
                            float bottomPen = solidBounds.Bottom - entityBounds.Top;

                            float xPen = Math.Min(leftPen, rightPen);
                            float yPen = Math.Min(topPen, bottomPen);

                            float minPen = Math.Min(xPen, yPen);

                            if (minPen >= MIN_PENETRATION_THRESHOLD && minPen < smallestPenetration)
                            {
                                smallestPenetration = minPen;
                                collisionToResolve = solidBounds;
                                resolveX = (xPen < yPen);
                                pushLeft = (leftPen < rightPen);
                                pushUp = (topPen < bottomPen);
                            }
                        }
                    }

                    if (collisionToResolve.HasValue)
                    {
                        var solid = collisionToResolve.Value;

                        if (resolveX)
                        {
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
                    int value = level.TileData[row, col];
                    if (IsSolidTile(value))
                    {
                        var bounds = new Rectangle(
                            level.X + (col * TILE_SIZE),
                            level.Y + (row * TILE_SIZE),
                            TILE_SIZE,
                            TILE_SIZE
                        );
                        _solidObjects.Add((bounds, null));  // Tiles don't have entities
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
                _useAxisSeparation = config.UseAxisSeparation;
                _useGridSnapForTiles = config.UseGridSnapForTiles;
                return;
            }

            _solidValues = null;
            _waterValues = null;
            _waterIsSolid = true;
            _useAxisSeparation = false;
            _useGridSnapForTiles = false;
        }

        private bool IsSolidTile(int value)
        {
            if (_solidValues == null)
                return value > 0;

            if (_waterValues != null && _waterValues.Contains(value))
                return _waterIsSolid;

            return _solidValues.Contains(value);
        }

        private void ResolveAxisSeparated(ref PositionComponent position, ref VelocityComponent velocity, ColliderComponent collider, float deltaTime)
        {
            if (_useGridSnapForTiles)
            {
                ResolveAxisSeparatedGridTiles(ref position, ref velocity, collider, deltaTime);
            }
            else
            {
                ResolveAxisSeparatedSolids(ref position, ref velocity, collider, deltaTime);
            }
        }

        private void ResolveAxisSeparatedSolids(ref PositionComponent position, ref VelocityComponent velocity, ColliderComponent collider, float deltaTime)
        {
            position.Value.X += velocity.Value.X * deltaTime;
            Rectangle boundsX = GetWorldBounds(position, collider);
            if (velocity.Value.X > 0)
            {
                float minX = position.Value.X;
                foreach ((Rectangle solidBounds, Entity _) in _solidObjects)
                {
                    if (!boundsX.Intersects(solidBounds))
                        continue;
                    float candidate = solidBounds.Left - collider.Bounds.Width - collider.Bounds.X;
                    if (candidate < minX)
                        minX = candidate;
                }
                if (minX != position.Value.X)
                {
                    position.Value.X = minX;
                    velocity.Value.X = 0;
                }
            }
            else if (velocity.Value.X < 0)
            {
                float maxX = position.Value.X;
                foreach ((Rectangle solidBounds, Entity _) in _solidObjects)
                {
                    if (!boundsX.Intersects(solidBounds))
                        continue;
                    float candidate = solidBounds.Right - collider.Bounds.X;
                    if (candidate > maxX)
                        maxX = candidate;
                }
                if (maxX != position.Value.X)
                {
                    position.Value.X = maxX;
                    velocity.Value.X = 0;
                }
            }

            position.Value.Y += velocity.Value.Y * deltaTime;
            Rectangle boundsY = GetWorldBounds(position, collider);
            if (velocity.Value.Y > 0)
            {
                float minY = position.Value.Y;
                foreach ((Rectangle solidBounds, Entity _) in _solidObjects)
                {
                    if (!boundsY.Intersects(solidBounds))
                        continue;
                    float candidate = solidBounds.Top - collider.Bounds.Height - collider.Bounds.Y;
                    if (candidate < minY)
                        minY = candidate;
                }
                if (minY != position.Value.Y)
                {
                    position.Value.Y = minY;
                    velocity.Value.Y = 0;
                }
            }
            else if (velocity.Value.Y < 0)
            {
                float maxY = position.Value.Y;
                foreach ((Rectangle solidBounds, Entity _) in _solidObjects)
                {
                    if (!boundsY.Intersects(solidBounds))
                        continue;
                    float candidate = solidBounds.Bottom - collider.Bounds.Y;
                    if (candidate > maxY)
                        maxY = candidate;
                }
                if (maxY != position.Value.Y)
                {
                    position.Value.Y = maxY;
                    velocity.Value.Y = 0;
                }
            }
        }

        private void ResolveAxisSeparatedGridTiles(ref PositionComponent position, ref VelocityComponent velocity, ColliderComponent collider, float deltaTime)
        {
            ResolveAxisSeparatedSolids(ref position, ref velocity, collider, 0f);

            if (_activeLevels.Count == 0)
            {
                position.Value += velocity.Value * deltaTime;
                return;
            }

            // X axis against tiles
            position.Value.X += velocity.Value.X * deltaTime;
            Rectangle boundsX = GetWorldBounds(position, collider);
            if (velocity.Value.X > 0)
            {
                float minX = position.Value.X;
                foreach (var level in _activeLevels)
                {
                    if (!TryGetTileCollisionX(level, boundsX, collider, velocity.Value.X, out var candidate))
                        continue;
                    if (candidate < minX)
                        minX = candidate;
                }
                if (minX != position.Value.X)
                {
                    position.Value.X = minX;
                    velocity.Value.X = 0;
                }
            }
            else if (velocity.Value.X < 0)
            {
                float maxX = position.Value.X;
                foreach (var level in _activeLevels)
                {
                    if (!TryGetTileCollisionX(level, boundsX, collider, velocity.Value.X, out var candidate))
                        continue;
                    if (candidate > maxX)
                        maxX = candidate;
                }
                if (maxX != position.Value.X)
                {
                    position.Value.X = maxX;
                    velocity.Value.X = 0;
                }
            }

            // Y axis against tiles
            position.Value.Y += velocity.Value.Y * deltaTime;
            Rectangle boundsY = GetWorldBounds(position, collider);
            if (velocity.Value.Y > 0)
            {
                float minY = position.Value.Y;
                foreach (var level in _activeLevels)
                {
                    if (!TryGetTileCollisionY(level, boundsY, collider, velocity.Value.Y, out var candidate))
                        continue;
                    if (candidate < minY)
                        minY = candidate;
                }
                if (minY != position.Value.Y)
                {
                    position.Value.Y = minY;
                    velocity.Value.Y = 0;
                }
            }
            else if (velocity.Value.Y < 0)
            {
                float maxY = position.Value.Y;
                foreach (var level in _activeLevels)
                {
                    if (!TryGetTileCollisionY(level, boundsY, collider, velocity.Value.Y, out var candidate))
                        continue;
                    if (candidate > maxY)
                        maxY = candidate;
                }
                if (maxY != position.Value.Y)
                {
                    position.Value.Y = maxY;
                    velocity.Value.Y = 0;
                }
            }
        }

        private bool TryGetTileCollisionX(ECS_Base.LevelData.Level level, Rectangle bounds, ColliderComponent collider, float velX, out float resolveX)
        {
            resolveX = 0f;
            if (level.TileData == null)
                return false;

            const int SKIN = 2;
            int top = bounds.Top + SKIN;
            int bottom = bounds.Bottom - 1 - SKIN;
            if (bottom < top)
            {
                top = bounds.Top;
                bottom = bounds.Bottom - 1;
            }

            int rowStart = (top - level.Y) / TILE_SIZE;
            int rowEnd = (bottom - level.Y) / TILE_SIZE;

            if (rowEnd < 0 || rowStart >= level.TileData.GetLength(0))
                return false;

            rowStart = Math.Max(0, rowStart);
            rowEnd = Math.Min(level.TileData.GetLength(0) - 1, rowEnd);

            if (velX > 0)
            {
                int col = (bounds.Right - 1 - level.X) / TILE_SIZE;
                if (col < 0 || col >= level.TileData.GetLength(1))
                    return false;

                for (int r = rowStart; r <= rowEnd; r++)
                {
                    if (IsSolidTile(level.TileData[r, col]))
                    {
                        resolveX = level.X + col * TILE_SIZE - collider.Bounds.Width - collider.Bounds.X;
                        return true;
                    }
                }
            }
            else if (velX < 0)
            {
                int col = (bounds.Left - level.X) / TILE_SIZE;
                if (col < 0 || col >= level.TileData.GetLength(1))
                    return false;

                for (int r = rowStart; r <= rowEnd; r++)
                {
                    if (IsSolidTile(level.TileData[r, col]))
                    {
                        resolveX = level.X + (col + 1) * TILE_SIZE - collider.Bounds.X;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryGetTileCollisionY(ECS_Base.LevelData.Level level, Rectangle bounds, ColliderComponent collider, float velY, out float resolveY)
        {
            resolveY = 0f;
            if (level.TileData == null)
                return false;

            const int SKIN = 2;
            int left = bounds.Left + SKIN;
            int right = bounds.Right - 1 - SKIN;
            if (right < left)
            {
                left = bounds.Left;
                right = bounds.Right - 1;
            }

            int colStart = (left - level.X) / TILE_SIZE;
            int colEnd = (right - level.X) / TILE_SIZE;

            if (colEnd < 0 || colStart >= level.TileData.GetLength(1))
                return false;

            colStart = Math.Max(0, colStart);
            colEnd = Math.Min(level.TileData.GetLength(1) - 1, colEnd);

            if (velY > 0)
            {
                int row = (bounds.Bottom - 1 - level.Y) / TILE_SIZE;
                if (row < 0 || row >= level.TileData.GetLength(0))
                    return false;

                for (int c = colStart; c <= colEnd; c++)
                {
                    if (IsSolidTile(level.TileData[row, c]))
                    {
                        resolveY = level.Y + row * TILE_SIZE - collider.Bounds.Height - collider.Bounds.Y;
                        return true;
                    }
                }
            }
            else if (velY < 0)
            {
                int row = (bounds.Top - level.Y) / TILE_SIZE;
                if (row < 0 || row >= level.TileData.GetLength(0))
                    return false;

                for (int c = colStart; c <= colEnd; c++)
                {
                    if (IsSolidTile(level.TileData[row, c]))
                    {
                        resolveY = level.Y + (row + 1) * TILE_SIZE - collider.Bounds.Y;
                        return true;
                    }
                }
            }

            return false;
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

            foreach (var (solidBounds, _) in _solidObjects)
            {
                if (groundCheck.Intersects(solidBounds))
                    grounded.IsGrounded = true;
                if (leftCheck.Intersects(solidBounds))
                    grounded.TouchingLeft = true;
                if (rightCheck.Intersects(solidBounds))
                    grounded.TouchingRight = true;
                if (topCheck.Intersects(solidBounds))
                    grounded.TouchingTop = true;
            }

            if (_useGridSnapForTiles && _activeLevels.Count > 0)
            {
                if (TileIntersectsRect(groundCheck))
                    grounded.IsGrounded = true;
                if (TileIntersectsRect(leftCheck))
                    grounded.TouchingLeft = true;
                if (TileIntersectsRect(rightCheck))
                    grounded.TouchingRight = true;
                if (TileIntersectsRect(topCheck))
                    grounded.TouchingTop = true;
            }

            world.AddComponent(entity, grounded);
        }

        private bool TileIntersectsRect(Rectangle rect)
        {
            foreach (var level in _activeLevels)
            {
                if (level.TileData == null)
                    continue;

                int colStart = (rect.Left - level.X) / TILE_SIZE;
                int colEnd = (rect.Right - 1 - level.X) / TILE_SIZE;
                int rowStart = (rect.Top - level.Y) / TILE_SIZE;
                int rowEnd = (rect.Bottom - 1 - level.Y) / TILE_SIZE;

                if (colEnd < 0 || rowEnd < 0 || rowStart >= level.TileData.GetLength(0) || colStart >= level.TileData.GetLength(1))
                    continue;

                colStart = Math.Max(0, colStart);
                rowStart = Math.Max(0, rowStart);
                colEnd = Math.Min(level.TileData.GetLength(1) - 1, colEnd);
                rowEnd = Math.Min(level.TileData.GetLength(0) - 1, rowEnd);

                for (int r = rowStart; r <= rowEnd; r++)
                {
                    for (int c = colStart; c <= colEnd; c++)
                    {
                        if (IsSolidTile(level.TileData[r, c]))
                            return true;
                    }
                }
            }

            return false;
        }

        public List<Rectangle> GetCollisionTiles()
        {
            return _solidObjects.Select(x => x.bounds).ToList();
        }
    }
}
