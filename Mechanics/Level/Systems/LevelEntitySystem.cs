using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Collision.Components;
using ECS_Base.Mechanics.PlayerController.Components;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Gravity.Components;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Combat.Components;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.UI.Components;
using ECS_Base.Mechanics.Progression.Components;
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
                        if (TryGetSpawnMode(world, out var mode))
                        {
                            SpawnLevelEntities(world, level, mode);
                        }
                        else
                        {
                            SpawnLegacyPlatformerEntities(world, level);
                        }
                        _spawnedLevels.Add(level.Identifier);
                    }
                }
            }
        }

        private bool TryGetSpawnMode(World world, out LevelSpawnMode mode)
        {
            foreach (var entity in world.Query<LevelSpawnConfigComponent>())
            {
                var config = world.GetComponent<LevelSpawnConfigComponent>(entity);
                mode = config.Mode;
                return true;
            }

            mode = LevelSpawnMode.Platformer;
            return false;
        }

        private void SpawnLegacyPlatformerEntities(World world, LevelData.Level level)
        {
            Console.WriteLine($"Spawning entities for level: {level.Identifier}");

            foreach (var playerData in level.Players)
            {
                float worldX = level.X + playerData.X;
                float worldY = level.Y + playerData.Y;

                var playerEntity = world.CreateEntity();
                world.AddComponent(playerEntity, new PositionComponent(worldX, worldY));
                world.AddComponent(playerEntity, new VelocityComponent());
                world.AddComponent(playerEntity, new PlayerComponent(moveSpeed: 100f, jumpForce: 250f));
                world.AddComponent(playerEntity, new InputComponent());
                world.AddComponent(playerEntity, new GravityComponent(gravityScale: 1.0f));
                world.AddComponent(playerEntity, new ColliderComponent(
                    new Rectangle(0, 0, playerData.Width, playerData.Height),
                    ColliderComponent.ColliderType.Dynamic
                ));
                world.AddComponent(playerEntity, new GroundedComponent(false));
                world.AddComponent(playerEntity, new ShapeComponent(
                    ShapeComponent.ShapeType.Rectangle,
                    Color.Blue,
                    new Vector2(playerData.Width, playerData.Height)
                ));
                world.AddComponent(playerEntity, new CameraTargetComponent { IsActive = true });

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

        private void SpawnLevelEntities(World world, LevelData.Level level, LevelSpawnMode mode)
        {
            Console.WriteLine($"Spawning entities for level: {level.Identifier}");

            // Spawn player entities
            foreach (var playerData in level.Players)
            {
                // Convert level-relative position to world position
                float worldX = level.X + playerData.X;
                float worldY = level.Y + playerData.Y;

                if (mode == LevelSpawnMode.Topdown)
                {
                    SpawnTopdownPlayer(world, worldX, worldY);
                }
                else
                {
                    SpawnPlatformerPlayer(world, worldX, worldY, playerData.Width, playerData.Height);
                }

                Console.WriteLine($"Spawned player at world position: ({worldX}, {worldY})");
            }

            // Spawn enemies
            foreach (var enemyData in level.Enemies)
            {
                float worldX = level.X + enemyData.X;
                float worldY = level.Y + enemyData.Y;

                if (mode == LevelSpawnMode.Topdown)
                {
                    SpawnTopdownEnemy(world, worldX, worldY, enemyData);
                }
                else
                {
                    SpawnPlatformerEnemy(world, worldX, worldY, enemyData.Width, enemyData.Height, enemyData);
                }

                Console.WriteLine($"Spawned enemy at world position: ({worldX}, {worldY})");
            }

            foreach (var pickupData in level.Pickups)
            {
                float worldX = level.X + pickupData.X;
                float worldY = level.Y + pickupData.Y;

                if (mode == LevelSpawnMode.Topdown)
                {
                    SpawnTopdownPickup(world, new Vector2(worldX, worldY), pickupData);
                }
                else
                {
                    SpawnPlatformerPickup(world, new Vector2(worldX, worldY), pickupData);
                }
            }

            foreach (var hazardData in level.Hazards)
            {
                float worldX = level.X + hazardData.X;
                float worldY = level.Y + hazardData.Y;

                if (mode == LevelSpawnMode.Topdown)
                {
                    SpawnTopdownHazard(world, new Vector2(worldX, worldY), hazardData);
                }
                else
                {
                    SpawnPlatformerHazard(world, new Vector2(worldX, worldY), hazardData);
                }
            }
        }

        private void SpawnPlatformerPlayer(World world, float worldX, float worldY, int width, int height)
        {
            var playerEntity = world.CreateEntity();
            world.AddComponent(playerEntity, new PositionComponent(worldX, worldY));
            world.AddComponent(playerEntity, new VelocityComponent());
            var physics = new PlatformerPhysicsComponent(320f, 1200f);
            physics.MaxAirJumps = 1;
            world.AddComponent(playerEntity, physics);

            world.AddComponent(playerEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(12, 20)));

            world.AddComponent(playerEntity, new ColliderComponent(
                new Rectangle(0, 0, 12, 20),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(playerEntity, new GroundedComponent(false));
            world.AddComponent(playerEntity, new CameraTargetComponent { IsActive = true });

            world.AddComponent(playerEntity, new PlatformDemoPlayerComponent());
            world.AddComponent(playerEntity, new PlayerComponent());

            world.AddComponent(playerEntity, new StatsComponent(maxHealth: 3, attack: 10f, defense: 0f, speed: 1f));
            world.AddComponent(playerEntity, new InvulnerabilityOnHitComponent(duration: 1.0f));
            world.AddComponent(playerEntity, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.4f));
            world.AddComponent(playerEntity, new RespawnComponent(new Vector2(worldX, worldY), delay: 2.5f));
            world.AddComponent(playerEntity, new CurrencyComponent());

            CreatePlatformerUI(world, playerEntity);
        }

        private void SpawnPlatformerEnemy(World world, float worldX, float worldY, int width, int height, LevelData.LevelEntity data)
        {
            var kind = GetKind(data);
            if (string.Equals(kind, "Goomba", System.StringComparison.OrdinalIgnoreCase))
            {
                SpawnGoomba(world, new Vector2(worldX, worldY), worldX - 80, worldX + 80);
                return;
            }

            var enemyEntity = world.CreateEntity();
            world.AddComponent(enemyEntity, new PositionComponent(worldX, worldY));
            world.AddComponent(enemyEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Red,
                new Vector2(width, height)
            ));
        }

        private void SpawnTopdownPlayer(World world, float worldX, float worldY)
        {
            var playerEntity = world.CreateEntity();
            world.AddComponent(playerEntity, new PositionComponent(worldX, worldY));
            world.AddComponent(playerEntity, new VelocityComponent());
            world.AddComponent(playerEntity, new TopDownMovementComponent(moveSpeed: 100f));
            world.AddComponent(playerEntity, new InputComponent());
            world.AddComponent(playerEntity, new ColliderComponent(
                new Rectangle(0, 0, 12, 16),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(playerEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Blue,
                new Vector2(12, 16)
            ));
            world.AddComponent(playerEntity, new PlayerComponent(moveSpeed: 100f, jumpForce: 0f));
            world.AddComponent(playerEntity, new StatsComponent(maxHealth: 6, attack: 10f, defense: 0f, speed: 1f));
            world.AddComponent(playerEntity, new InvulnerabilityOnHitComponent(duration: 0.8f));
            world.AddComponent(playerEntity, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.35f));
            world.AddComponent(playerEntity, new RespawnComponent(new Vector2(worldX, worldY), delay: 2.0f));
            world.AddComponent(playerEntity, new CurrencyComponent());
            world.AddComponent(playerEntity, new AmmoComponent(arrows: 10, bombs: 3));
            world.AddComponent(playerEntity, new MeleeWeaponComponent(damage: 2f, range: 36f, attackSpeed: 0.4f));
            world.AddComponent(playerEntity, new ShieldComponent(damageMultiplier: 0f));
            world.AddComponent(playerEntity, new CameraTargetComponent { IsActive = true });

            CreateTopdownUIAndIndicators(world, playerEntity);
        }

        private void SpawnTopdownEnemy(World world, float worldX, float worldY, LevelData.LevelEntity data)
        {
            var enemyEntity = world.CreateEntity();
            world.AddComponent(enemyEntity, new PositionComponent(worldX, worldY));
            world.AddComponent(enemyEntity, new VelocityComponent());
            world.AddComponent(enemyEntity, new ColliderComponent(
                new Rectangle(0, 0, 12, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(enemyEntity, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                Color.Red,
                new Vector2(12, 12)
            ));
            var kind = GetKind(data);
            bool canShoot = string.Equals(kind, "Shooter", System.StringComparison.OrdinalIgnoreCase);
            world.AddComponent(enemyEntity, new EnemyComponent(
                chaseRange: 300f,
                stopDistance: 28f,
                chaseSpeed: 60f,
                separationRadius: 20f,
                canShoot: canShoot
            ));
            world.AddComponent(enemyEntity, new StatsComponent(maxHealth: 2, attack: 1f, defense: 0f, speed: 1f));
            world.AddComponent(enemyEntity, new DamageFlashOnHitComponent(flashColor: Color.White, duration: 0.25f));
            world.AddComponent(enemyEntity, new ContactDamageComponent(damage: 1f));
        }

        private void SpawnPlatformerPickup(World world, Vector2 position, LevelData.LevelEntity data)
        {
            var kind = GetKind(data);
            if (string.Equals(kind, "Coin", System.StringComparison.OrdinalIgnoreCase))
            {
                SpawnCoin(world, position);
            }
            else if (string.Equals(kind, "Star", System.StringComparison.OrdinalIgnoreCase))
            {
                SpawnStar(world, position);
            }
            else if (string.Equals(kind, "Mushroom", System.StringComparison.OrdinalIgnoreCase))
            {
                SpawnMushroom(world, position);
            }
        }

        private void SpawnTopdownPickup(World world, Vector2 position, LevelData.LevelEntity data)
        {
            var kind = GetKind(data);
            if (string.Equals(kind, "Rupee", System.StringComparison.OrdinalIgnoreCase))
            {
                var rupee = world.CreateEntity();
                world.AddComponent(rupee, new PositionComponent { Value = position });
                world.AddComponent(rupee, new CollectibleComponent(CollectibleType.Rupee, amount: 1, scoreValue: 10));
                world.AddComponent(rupee, new ColliderComponent(new Rectangle(0, 0, 12, 12), ColliderComponent.ColliderType.Dynamic));
                world.AddComponent(rupee, new WorldIconComponent(UIIconType.Rupee, Color.LawnGreen, 2));
                world.AddComponent(rupee, new FloatingComponent(position, amplitude: 2.0f, speed: 1.8f));
            }
            else if (string.Equals(kind, "Heart", System.StringComparison.OrdinalIgnoreCase))
            {
                var heart = world.CreateEntity();
                world.AddComponent(heart, new PositionComponent { Value = position });
                world.AddComponent(heart, new PowerupComponent(PowerupType.Heal, amount: 1f));
                world.AddComponent(heart, new ColliderComponent(new Rectangle(0, 0, 12, 12), ColliderComponent.ColliderType.Dynamic));
                world.AddComponent(heart, new WorldIconComponent(UIIconType.Star, Color.Pink, 2));
                world.AddComponent(heart, new FloatingComponent(position, amplitude: 2.0f, speed: 1.6f));
            }
            else if (string.Equals(kind, "Arrow", System.StringComparison.OrdinalIgnoreCase))
            {
                var arrow = world.CreateEntity();
                world.AddComponent(arrow, new PositionComponent { Value = position });
                world.AddComponent(arrow, new CollectibleComponent(CollectibleType.Arrow, amount: 5, scoreValue: 0));
                world.AddComponent(arrow, new ColliderComponent(new Rectangle(0, 0, 12, 12), ColliderComponent.ColliderType.Dynamic));
                world.AddComponent(arrow, new WorldIconComponent(UIIconType.Arrow, Color.White, 2));
                world.AddComponent(arrow, new FloatingComponent(position, amplitude: 2.0f, speed: 1.6f));
            }
            else if (string.Equals(kind, "Bomb", System.StringComparison.OrdinalIgnoreCase))
            {
                var bomb = world.CreateEntity();
                world.AddComponent(bomb, new PositionComponent { Value = position });
                world.AddComponent(bomb, new CollectibleComponent(CollectibleType.Bomb, amount: 1, scoreValue: 0));
                world.AddComponent(bomb, new ColliderComponent(new Rectangle(0, 0, 12, 12), ColliderComponent.ColliderType.Dynamic));
                world.AddComponent(bomb, new WorldIconComponent(UIIconType.Bomb, Color.DarkGray, 2));
                world.AddComponent(bomb, new FloatingComponent(position, amplitude: 2.0f, speed: 1.6f));
            }
        }

        private void SpawnPlatformerHazard(World world, Vector2 position, LevelData.LevelEntity data)
        {
            var kind = GetKind(data);
            if (!string.Equals(kind, "Spike", System.StringComparison.OrdinalIgnoreCase))
                return;

            var damage = world.CreateEntity();
            world.AddComponent(damage, new PositionComponent { Value = position });
            world.AddComponent(damage, new ColliderComponent(new Rectangle(0, 0, 16, 16), ColliderComponent.ColliderType.Dynamic));
            world.AddComponent(damage, new ContactDamageComponent(damage: 1));
        }

        private void SpawnTopdownHazard(World world, Vector2 position, LevelData.LevelEntity data)
        {
            // no-op for now
        }

        private string GetKind(LevelData.LevelEntity data)
        {
            if (data.CustomFields != null && data.CustomFields.TryGetValue("Kind", out var value))
            {
                return value?.ToString();
            }
            return null;
        }

        private void SpawnCoin(World world, Vector2 position)
        {
            var coin = world.CreateEntity();
            world.AddComponent(coin, new PositionComponent { Value = position });
            world.AddComponent(coin, new CollectibleComponent(CollectibleType.Coin, amount: 1, scoreValue: 100));
            world.AddComponent(coin, new ColliderComponent(
                new Rectangle(0, 0, 12, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(coin, new WorldIconComponent(UIIconType.Coin, Color.Gold, 2));
            world.AddComponent(coin, new FloatingComponent(position, amplitude: 2.5f, speed: 2.2f));
        }

        private void SpawnStar(World world, Vector2 position)
        {
            var star = world.CreateEntity();
            world.AddComponent(star, new PositionComponent { Value = position });
            world.AddComponent(star, new CollectibleComponent(CollectibleType.Star, amount: 1, scoreValue: 1000));
            world.AddComponent(star, new ColliderComponent(
                new Rectangle(0, 0, 12, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(star, new WorldIconComponent(UIIconType.Star, Color.Yellow, 3));
            world.AddComponent(star, new FloatingComponent(position, amplitude: 3.0f, speed: 1.8f));
        }

        private void SpawnGoomba(World world, Vector2 position, float minX, float maxX)
        {
            var enemy = world.CreateEntity();
            world.AddComponent(enemy, new PositionComponent { Value = position });
            world.AddComponent(enemy, new VelocityComponent());
            world.AddComponent(enemy, new PatrolComponent(minX, maxX, speed: 40f, direction: -1));
            world.AddComponent(enemy, new PlatformerPhysicsComponent(500f, 1200f));
            world.AddComponent(enemy, new GroundedComponent(false));
            world.AddComponent(enemy, new ColliderComponent(
                new Rectangle(0, 0, 14, 14),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(enemy, new ShapeComponent(
                ShapeComponent.ShapeType.Rectangle,
                new Color(155, 80, 30),
                new Vector2(14, 14)
            ));
            world.AddComponent(enemy, new ContactDamageComponent(damage: 1));
            world.AddComponent(enemy, new StatsComponent(maxHealth: 1, attack: 1f, defense: 0f, speed: 1f));
        }

        private void SpawnMushroom(World world, Vector2 position)
        {
            var powerup = world.CreateEntity();
            world.AddComponent(powerup, new PositionComponent { Value = position });
            world.AddComponent(powerup, new ColliderComponent(
                new Rectangle(0, 0, 12, 12),
                ColliderComponent.ColliderType.Dynamic
            ));
            world.AddComponent(powerup, new WorldIconComponent(UIIconType.Mushroom, new Color(220, 40, 40), 3));
            world.AddComponent(powerup, new FloatingComponent(position, amplitude: 2.5f, speed: 2.0f));
            world.AddComponent(powerup, new PowerupComponent(PowerupType.MaxHealthUp, amount: 1f));
        }

        private void CreatePlatformerUI(World world, Entity player)
        {
            var ui = world.CreateEntity();
            world.AddComponent(ui, new UISegmentedBarComponent(
                position: new Vector2(16, 16),
                targetEntityId: player.Id,
                segments: 3
            ));
            if (world.TryGetComponent<UISegmentedBarComponent>(ui, out var bar))
            {
                bar.BaseSegments = 3;
                bar.BonusFillColor = new Color(220, 60, 60);
                world.AddComponent(ui, bar);
            }

            var coinsText = world.CreateEntity();
            world.AddComponent(coinsText, new UITextComponent(null, "0", new Vector2(36, 32)));
            world.AddComponent(coinsText, new UICounterComponent(player.Id, CounterType.Coins, ""));
            world.AddComponent(coinsText, new UIIconComponent(new Vector2(16, 32), UIIconType.Coin, Color.Gold, 2));

            var starsText = world.CreateEntity();
            world.AddComponent(starsText, new UITextComponent(null, "0", new Vector2(40, 50)));
            world.AddComponent(starsText, new UICounterComponent(player.Id, CounterType.Stars, ""));
            world.AddComponent(starsText, new UIIconComponent(new Vector2(16, 48), UIIconType.Star, Color.Yellow, 3));

            var scoreText = world.CreateEntity();
            world.AddComponent(scoreText, new UITextComponent(null, "Score: 0", new Vector2(16, 64)));
            world.AddComponent(scoreText, new UICounterComponent(player.Id, CounterType.Score, "Score: "));
        }

        private void CreateTopdownUIAndIndicators(World world, Entity player)
        {
            var ui = world.CreateEntity();
            world.AddComponent(ui, new UIHeartsComponent(
                position: new Vector2(48, 16),
                targetEntityId: player.Id,
                unitsPerHeart: 2,
                heartSize: 16,
                heartSpacing: 4
            ));

            var rupeesText = world.CreateEntity();
            world.AddComponent(rupeesText, new UITextComponent(null, "Rupees: 0", new Vector2(16, 36)));
            world.AddComponent(rupeesText, new UICounterComponent(player.Id, CounterType.Rupees, "Rupees: "));
            world.AddComponent(rupeesText, new UIIconComponent(new Vector2(16, 36), UIIconType.Rupee, Color.LawnGreen, 2));

            var arrowsText = world.CreateEntity();
            world.AddComponent(arrowsText, new UITextComponent(null, "0", new Vector2(36, 56)));
            world.AddComponent(arrowsText, new UICounterComponent(player.Id, CounterType.Arrows, ""));
            world.AddComponent(arrowsText, new UIIconComponent(new Vector2(16, 56), UIIconType.Arrow, Color.White, 2));

            var bombsText = world.CreateEntity();
            world.AddComponent(bombsText, new UITextComponent(null, "0", new Vector2(36, 72)));
            world.AddComponent(bombsText, new UICounterComponent(player.Id, CounterType.Bombs, ""));
            world.AddComponent(bombsText, new UIIconComponent(new Vector2(16, 72), UIIconType.Bomb, Color.Black, 2));

            var swordIcon = world.CreateEntity();
            world.AddComponent(swordIcon, new UIIconComponent(new Vector2(200, 16), UIIconType.Star, Color.White, 2));
            world.AddComponent(swordIcon, new UIActionIndicatorComponent(player.Id, ActionType.Sword, Color.White, new Color(80, 80, 80)));

            var shieldIcon = world.CreateEntity();
            world.AddComponent(shieldIcon, new UIIconComponent(new Vector2(220, 16), UIIconType.Star, Color.White, 2));
            world.AddComponent(shieldIcon, new UIActionIndicatorComponent(player.Id, ActionType.Shield, Color.Cyan, new Color(80, 80, 80)));

            var bowIcon = world.CreateEntity();
            world.AddComponent(bowIcon, new UIIconComponent(new Vector2(240, 16), UIIconType.Arrow, Color.White, 2));
            world.AddComponent(bowIcon, new UIActionIndicatorComponent(player.Id, ActionType.Bow, Color.Yellow, new Color(80, 80, 80)));

            var facing = world.CreateEntity();
            world.AddComponent(facing, new PositionComponent(0, 0));
            world.AddComponent(facing, new Rendering.Components.WorldIconComponent(UIIconType.Arrow, Color.White, 1));
            world.AddComponent(facing, new FacingIndicatorComponent(player.Id, distance: 16f, offset: Vector2.Zero));

            var shield = world.CreateEntity();
            world.AddComponent(shield, new PositionComponent(0, 0));
            world.AddComponent(shield, new ShapeComponent(ShapeComponent.ShapeType.Rectangle, Color.Transparent, new Vector2(28, 8)));
            world.AddComponent(shield, new ShieldVisualComponent(player.Id, new Vector2(28, 8), distance: 12f, color: new Color(120, 200, 255, 160)));

            var bow = world.CreateEntity();
            world.AddComponent(bow, new PositionComponent(0, 0));
            world.AddComponent(bow, new ShapeComponent(ShapeComponent.ShapeType.Rectangle, Color.Transparent, new Vector2(12, 4)));
            world.AddComponent(bow, new BowVisualComponent(player.Id, new Vector2(12, 4), distance: 10f, timeRemaining: 0f, color: new Color(255, 220, 120, 180)));
        }

        public void ClearSpawnedLevels()
        {
            _spawnedLevels.Clear();
        }
    }
}
