// Systems/LevelEntitySystem.cs - Updated Update method to avoid collection modification
using ECS_Example.Components;
using ECS_Example.LevelData;
using ECS_Example.Factories;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ECS_Example.Systems
{
    public class LevelEntitySystem
    {
        private HashSet<string> _spawnedLevels = new HashSet<string>();

        public void Update(World world)
        {
            // Create a copy of the entities list to avoid modification during enumeration
            var entities = world.GetEntities().ToList();

            foreach (var entity in entities)
            {
                if (world.TryGetComponent<LevelComponent>(entity, out var levelComponent) &&
                    levelComponent.IsActive)
                {
                    var level = levelComponent.LevelData;

                    // Only spawn entities once per level
                    if (!_spawnedLevels.Contains(level.Identifier))
                    {
                        SpawnLevelEntities(world, level);
                        _spawnedLevels.Add(level.Identifier);
                    }
                }
            }
        }

        private void SpawnLevelEntities(World world, Level level)
        {
            Console.WriteLine($"Spawning entities for level: {level.Identifier}");

            // Spawn players
            foreach (var playerData in level.Players)
            {
                var playerEntity = PlayerFactory.CreatePlayer(world,
                    new Vector2(level.X + playerData.X, level.Y + playerData.Y));
                Console.WriteLine($"Spawned player at ({level.X + playerData.X}, {level.Y + playerData.Y})");
            }

            // Spawn enemies
            foreach (var enemyData in level.Enemies)
            {
                var enemyEntity = EnemyFactory.CreatePatrolEnemy(world,
                    new Vector2(level.X + enemyData.X, level.Y + enemyData.Y),
                    10, // speed
                    true, // avoid falling
                    Color.Orange, // color
                    1); // health
                Console.WriteLine($"Spawned enemy at ({level.X + enemyData.X}, {level.Y + enemyData.Y})");
            }

            // Spawn path entities (for future teleportation system)
            foreach (var pathData in level.Paths)
            {
                SpawnPathEntity(world, level, pathData);
            }
        }

        private void SpawnPathEntity(World world, Level level, PathEntity pathData)
        {
            var pathEntity = world.CreateEntity();

            // Position the path at its level-relative coordinates
            world.AddComponent(pathEntity, new Position(level.X + pathData.X, level.Y + pathData.Y));

            // Create a visual representation (invisible for now, but useful for debugging)
            world.AddComponent(pathEntity, new Shape(
                Shape.ShapeType.Rectangle,
                Color.Purple * 0.3f, // Semi-transparent purple
                new Vector2(pathData.Width, pathData.Height)
            ));

            // Add path component (we'll need to create this)
            world.AddComponent(pathEntity, new PathComponent(
                pathData.NextDoorLevelIid,
                pathData.NextDoorEntityIid,
                new Rectangle(0, 0, pathData.Width, pathData.Height)
            ));

            Console.WriteLine($"Spawned path at ({level.X + pathData.X}, {level.Y + pathData.Y}) -> Level: {pathData.NextDoorLevelIid}");
        }

        public void ClearSpawnedLevels()
        {
            _spawnedLevels.Clear();
        }
    }
}