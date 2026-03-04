using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Animation.Components;
using ECS_Base.Mechanics.Animation.Data;
using ECS_Base.Mechanics.Animation.Systems;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Visual test demo that verifies system functionality
    /// Runs automated tests and displays PASS/FAIL results
    /// </summary>
    public class SystemTestsDemo : IGameConfig
    {
        public string Name => "System Tests (Visual)";

        private float _testTimer = 0f;
        private int _currentTest = 0;
        private List<string> _testResults = new List<string>();

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            game.SpriteSystem = new SpriteSystem(null);
            systemManager.AddSystem(new AnimationSystem());

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.0f,
                offset: Vector2.Zero,
                zoom: 1.0f,
                dampeningThreshold: 0f
            ));

            Console.WriteLine("=== System Tests (Visual) ===");
            Console.WriteLine("Running automated system tests...");
            Console.WriteLine();

            RunAllTests(world);
        }

        private void RunAllTests(World world)
        {
            int passed = 0;
            int failed = 0;

            // Test 1: World creates unique entities
            try
            {
                var e1 = world.CreateEntity();
                var e2 = world.CreateEntity();
                if (e1.Id != e2.Id)
                {
                    LogPass("World creates unique entity IDs");
                    passed++;
                }
                else
                {
                    LogFail("World creates unique entity IDs", "IDs were the same");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                LogFail("World creates unique entity IDs", ex.Message);
                failed++;
            }

            // Test 2: Components can be added and retrieved
            try
            {
                var entity = world.CreateEntity();
                var pos = new PositionComponent(10, 20);
                world.AddComponent(entity, pos);
                var retrieved = world.GetComponent<PositionComponent>(entity);

                if (retrieved.Value.X == 10 && retrieved.Value.Y == 20)
                {
                    LogPass("Components can be added and retrieved");
                    passed++;
                }
                else
                {
                    LogFail("Components can be added and retrieved", "Values don't match");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                LogFail("Components can be added and retrieved", ex.Message);
                failed++;
            }

            // Test 3: Query returns correct entities
            try
            {
                var e1 = world.CreateEntity();
                var e2 = world.CreateEntity();
                var e3 = world.CreateEntity();

                world.AddComponent(e1, new PositionComponent(1, 1));
                world.AddComponent(e2, new PositionComponent(2, 2));
                // e3 has no position

                var results = world.Query<PositionComponent>();

                if (results.Count() == 2)
                {
                    LogPass("Query returns entities with components");
                    passed++;
                }
                else
                {
                    LogFail("Query returns entities with components", $"Expected 2, got {results.Count()}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                LogFail("Query returns entities with components", ex.Message);
                failed++;
            }

            // Test 4: Animation system advances frames
            try
            {
                var system = new AnimationSystem();
                var entity = world.CreateEntity();

                var animations = new Dictionary<string, AnimationData>();
                var testAnim = new AnimationData("Test", loop: true);
                testAnim.AddFrame(new Rectangle(0, 0, 32, 32), 0.1f);
                testAnim.AddFrame(new Rectangle(32, 0, 32, 32), 0.1f);
                animations["Test"] = testAnim;

                world.AddComponent(entity, new PositionComponent(0, 0));
                world.AddComponent(entity, new SpriteComponent(null!)
                {
                    SourceRectangle = new Rectangle(0, 0, 32, 32)
                });

                var animComp = new AnimationComponent(animations);
                animComp.Play("Test");
                world.AddComponent(entity, animComp);

                system.Update(world, 0.15f);

                var updated = world.GetComponent<AnimationComponent>(entity);
                if (updated.CurrentFrame == 1)
                {
                    LogPass("Animation system advances frames");
                    passed++;
                }
                else
                {
                    LogFail("Animation system advances frames", $"Expected frame 1, got {updated.CurrentFrame}");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                LogFail("Animation system advances frames", ex.Message);
                failed++;
            }

            // Summary
            Console.WriteLine();
            Console.WriteLine("=== Test Summary ===");
            Console.WriteLine($"PASSED: {passed}");
            Console.WriteLine($"FAILED: {failed}");
            Console.WriteLine($"TOTAL:  {passed + failed}");
            Console.WriteLine();

            if (failed == 0)
            {
                Console.WriteLine("*** ALL TESTS PASSED ***");
            }
            else
            {
                Console.WriteLine($"*** {failed} TEST(S) FAILED ***");
            }
        }

        private void LogPass(string testName)
        {
            string result = $"[PASS] {testName}";
            _testResults.Add(result);
            Console.WriteLine(result);
        }

        private void LogFail(string testName, string reason)
        {
            string result = $"[FAIL] {testName}: {reason}";
            _testResults.Add(result);
            Console.WriteLine(result);
        }
    }
}
