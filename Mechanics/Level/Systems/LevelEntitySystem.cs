using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Gravity.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.LevelData;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ECS_Base.Mechanics.Level.Systems
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

        private void SpawnLevelEntities(World world, LevelData.Level level)
        {
            Console.WriteLine($"Spawning entities for level: {level.Identifier}");

            // Spawn player entities
            foreach (var playerData in level.Players)
            {
                // Convert level-relative position to world position
                float worldX = level.X + playerData.X;
                float worldY = level.Y + playerData.Y;

                var playerEntity = world.CreateEntity();
                world.AddComponent(playerEntity, new PositionComponent(worldX, worldY));
                world.AddComponent(playerEntity, new VelocityComponent());
                world.AddComponent(playerEntity, new PlayerComponent(moveSpeed: 100f, jumpForce: 250f));
                world.AddComponent(playerEntity, new InputComponent());
                world.AddComponent(playerEntity, new GravityComponent(gravityScale: 1.0f));
                world.AddComponent(playerEntity, new ColliderComponent(
                    new Rectangle(0, 0, playerData.Width, playerData.Height), // LOCAL bounds, not world position!
                    ColliderComponent.ColliderType.Dynamic
                ));
                world.AddComponent(playerEntity, new GroundedComponent(false));
                world.AddComponent(playerEntity, new ShapeComponent(
                    ShapeComponent.ShapeType.Rectangle,
                    Color.Blue,
                    new Vector2(playerData.Width, playerData.Height)
                ));
                world.AddComponent(playerEntity, new CameraTargetComponent { IsActive = true });

                // Add platformer feel components (optional, but makes the game feel much better!)
                world.AddComponent(playerEntity, new CoyoteTimeComponent(timeWindow: 0.15f));
                world.AddComponent(playerEntity, new JumpBufferComponent(bufferWindow: 0.15f));
                world.AddComponent(playerEntity, new VariableJumpComponent(
                    minJumpTime: 0.1f,
                    maxJumpTime: 0.4f,
                    jumpCutMultiplier: 0.5f
                ));
                world.AddComponent(playerEntity, new AirControlComponent(
                    airControlFactor: 0.8f,
                    airAcceleration: 0.5f
                ));

                Console.WriteLine($"Spawned player at world position: ({worldX}, {worldY})");
            }

            // Spawn enemies
            foreach (var enemyData in level.Enemies)
            {
                float worldX = level.X + enemyData.X;
                float worldY = level.Y + enemyData.Y;

                var enemyEntity = world.CreateEntity();
                world.AddComponent(enemyEntity, new PositionComponent(worldX, worldY));
                world.AddComponent(enemyEntity, new ShapeComponent(
                    ShapeComponent.ShapeType.Rectangle,
                    Color.Red,
                    new Vector2(enemyData.Width, enemyData.Height)
                ));

                Console.WriteLine($"Spawned enemy at world position: ({worldX}, {worldY})");
            }
        }

        public void ClearSpawnedLevels()
        {
            _spawnedLevels.Clear();
        }
    }
}
