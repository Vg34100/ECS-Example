using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Movement.Systems;
using ECS_Base.Mechanics.Level.Systems;
using ECS_Base.Mechanics.Level.Components;
using ECS_Base.Mechanics.Collision.Systems;
using ECS_Base.Mechanics.Input.Systems;
using ECS_Base.Mechanics.Combat.Systems;
using ECS_Base.Mechanics.Stats.Systems;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Progression.Systems;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Editor.Systems;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Top-down config using LDtk programmatic project.
    /// </summary>
    public class TopDownProgrammaticConfig : IGameConfig
    {
        public string Name => "Top-Down Programmatic";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            systemManager.AddSystem(new InputSystem());
            systemManager.AddSystem(new TopDownMovementSystem());
            systemManager.AddSystem(new EnemyAISystem());
            var statsSystem = new StatsSystem();
            systemManager.AddSystem(statsSystem);
            var meleeSystem = new MeleeCombatSystem(statsSystem);
            systemManager.AddSystem(new SwordSystem(meleeSystem));
            systemManager.AddSystem(meleeSystem);
            systemManager.AddSystem(new ShieldSystem());
            systemManager.AddSystem(new ShieldVisualSystem());
            systemManager.AddSystem(new BowVisualSystem());
            systemManager.AddSystem(new BombSystem(statsSystem));
            systemManager.AddSystem(new ProjectileSystem());
            systemManager.AddSystem(new KnockbackSystem());
            systemManager.AddSystem(new MovementSystem());
            systemManager.AddSystem(new FloatingSystem());
            systemManager.AddSystem(new ExplosionSystem());
            systemManager.AddSystem(new FacingIndicatorSystem());

            var collisionSystem = new CollisionSystem();
            systemManager.AddSystem(collisionSystem);
            game.CollisionSystem = collisionSystem;
            systemManager.AddSystem(new SweptCollisionSystem());

            systemManager.AddSystem(new InvulnerabilitySystem());
            systemManager.AddSystem(new RespawnSystem());
            systemManager.AddSystem(new DamageFlashSystem());
            systemManager.AddSystem(new CombatSystem(statsSystem));
            systemManager.AddSystem(new CollectibleSystem());
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

            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.90f,
                offset: Vector2.Zero,
                zoom: 2.5f,
                dampeningThreshold: 5.0f
            ));

            var spawnConfig = world.CreateEntity();
            world.AddComponent(spawnConfig, new LevelSpawnConfigComponent(LevelSpawnMode.Topdown));

            var levelSelect = world.CreateEntity();
            world.AddComponent(levelSelect, new LevelSelectionConfigComponent(
                new System.Collections.Generic.HashSet<string> { "Level_1" }
            ));

            var editorState = world.CreateEntity();
            world.AddComponent(editorState, new EditorStateComponent(
                isEditing: false,
                tool: EditorTool.TilePaint,
                selectedTileValue: 10,
                entityType: EditorEntityType.Pickup,
                entityKind: "Rupee",
                activeLevelId: "Level_1"
            ));

            var tilesetConfig = world.CreateEntity();
            world.AddComponent(tilesetConfig, new LevelTilesetConfigComponent(
                projectFileName: "ldtk-programmatic.ldtk",
                levelsRootRelative: "Levels",
                useIntGridRender: true
            ));

            var collisionConfig = world.CreateEntity();
            world.AddComponent(collisionConfig, new LevelCollisionConfigComponent(
                solidValues: new System.Collections.Generic.HashSet<int> { 1, 2, 3, 4, 5, 6, 13, 14 },
                waterValues: new System.Collections.Generic.HashSet<int> { 12 },
                waterIsSolid: true,
                useAxisSeparation: true,
                useGridSnapForTiles: true
            ));

            var colorConfig = world.CreateEntity();
            world.AddComponent(colorConfig, new LevelTileColorConfigComponent(
                new System.Collections.Generic.Dictionary<int, Color>
                {
                    { 10, new Color(60, 140, 70) },  // Grass
                    { 11, new Color(130, 90, 50) },  // Dirt
                    { 12, new Color(40, 90, 170) },  // Water
                    { 13, new Color(90, 90, 90) },   // Cliff
                    { 14, new Color(30, 110, 40) },  // Tree
                    { 15, new Color(140, 100, 60) }  // Bridge
                },
                defaultColor: new Color(60, 120, 60)
            ));
        }
    }
}
