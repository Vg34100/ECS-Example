using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Level.Systems;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Platformer.Systems;
using ECS_Base.Mechanics.Platformer.Components;
using ECS_Base.Mechanics.Stats.Systems;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Progression.Systems;
using ECS_Base.Mechanics.Combat.Systems;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Editor.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Platformer config using LDtk programmatic project.
    /// </summary>
    public class PlatformerProgrammaticConfig : IGameConfig
    {
        public string Name => "Platformer Programmatic";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            var physicsSystem = new PlatformerPhysicsSystem();
            systemManager.AddSystem(new PlatformDemoInputSystem(physicsSystem));
            systemManager.AddSystem(physicsSystem);
            systemManager.AddSystem(new KnockbackSystem());

            var statsSystem = new StatsSystem();
            systemManager.AddSystem(statsSystem);
            systemManager.AddSystem(new InvulnerabilitySystem());
            systemManager.AddSystem(new RespawnSystem());
            systemManager.AddSystem(new DamageFlashSystem());
            systemManager.AddSystem(new CollectibleSystem());
            systemManager.AddSystem(new PowerupSystem());
            systemManager.AddSystem(new FloatingSystem());

            var platformSystem = new PlatformSystem();
            systemManager.AddSystem(platformSystem);
            systemManager.AddSystem(new PatrolSystem());

            systemManager.AddSystem(new MovementSystem());

            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;
            systemManager.AddSystem(new ContactDamageSystem(statsSystem));

            systemManager.AddSystem(new LevelEntitySystem());
            systemManager.AddSystem(new EditorModeSystem());
            systemManager.AddSystem(new EditorCameraSystem());
            systemManager.AddSystem(new EditorTilePaintSystem());
            systemManager.AddSystem(new EditorEntitySystem());
            systemManager.AddSystem(new EditorSaveSystem());
            systemManager.AddSystem(new EditorPlaytestSystem(game));

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            var levelManagerSystem = new LevelManagerSystem { ProjectName = "ldtk-programmatic" };
            game.LevelManagerSystem = levelManagerSystem;

            systemManager.AddSystem(new CollisionDebugRenderSystem(
                game.GraphicsDevice.Viewport.Width > 0 ? new SpriteBatch(game.GraphicsDevice) : null,
                game.GraphicsDevice,
                cameraSystem,
                collisionSystem
            ));

            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.97f,
                offset: new Vector2(0, -50),
                zoom: 3.0f,
                dampeningThreshold: 5.0f
            ));

            var spawnConfig = world.CreateEntity();
            world.AddComponent(spawnConfig, new LevelSpawnConfigComponent(LevelSpawnMode.Platformer));

            var levelSelect = world.CreateEntity();
            world.AddComponent(levelSelect, new LevelSelectionConfigComponent(
                new System.Collections.Generic.HashSet<string> { "Level_0" }
            ));

            var editorState = world.CreateEntity();
            world.AddComponent(editorState, new EditorStateComponent(
                isEditing: false,
                tool: EditorTool.TilePaint,
                selectedTileValue: 1,
                entityType: EditorEntityType.Pickup,
                entityKind: "Coin",
                activeLevelId: "Level_0"
            ));

            var tilesetConfig = world.CreateEntity();
            world.AddComponent(tilesetConfig, new LevelTilesetConfigComponent(
                projectFileName: "ldtk-programmatic.ldtk",
                levelsRootRelative: "Levels",
                useIntGridRender: true
            ));

            var collisionConfig = world.CreateEntity();
            world.AddComponent(collisionConfig, new LevelCollisionConfigComponent(
                solidValues: new System.Collections.Generic.HashSet<int> { 1, 2, 3, 4, 5, 6, 13, 14, 15 },
                waterValues: new System.Collections.Generic.HashSet<int> { 12 },
                waterIsSolid: true,
                useAxisSeparation: true,
                useGridSnapForTiles: true
            ));

            var colorConfig = world.CreateEntity();
            world.AddComponent(colorConfig, new LevelTileColorConfigComponent(
                new System.Collections.Generic.Dictionary<int, Color>
                {
                    { 1, new Color(120, 80, 40) },   // Ground
                    { 2, new Color(90, 90, 90) },    // Rock
                    { 3, new Color(150, 90, 50) },   // Brick
                    { 4, new Color(200, 160, 60) },  // Question
                    { 5, new Color(60, 140, 70) },   // Pipe
                    { 6, new Color(200, 200, 200) }  // Spike
                },
                defaultColor: new Color(80, 80, 80)
            ));
        }
    }
}
