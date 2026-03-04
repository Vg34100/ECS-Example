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

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Simple animation demo - shows multiple characters with different animations
    /// Auto-plays to demonstrate looping, speed, and one-shot animations
    /// </summary>
    public class AnimationDemoSimple : IGameConfig
    {
        public string Name => "Animation Demo (Simple)";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Mark that we want sprite system
            game.SpriteSystem = new SpriteSystem(null);

            // Add animation system
            systemManager.AddSystem(new AnimationSystem());

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create camera entity
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: new Vector2(640, 360),
                lagFactor: 0.0f,
                offset: Vector2.Zero,
                zoom: 1.0f,
                dampeningThreshold: 0f
            ));

            // Create a fake sprite sheet (4x4 grid, each frame is 32x32)
            Texture2D spriteSheet = CreateSpriteSheet(game.GraphicsDevice, 128, 128, 32);

            // Create animation data
            var animations = CreateAnimations();

            // Create multiple characters showing different animations
            CreateAnimatedCharacter(world, spriteSheet, animations, new Vector2(200, 300), "Idle", 1.0f);
            CreateAnimatedCharacter(world, spriteSheet, animations, new Vector2(400, 300), "Walk", 1.0f);
            CreateAnimatedCharacter(world, spriteSheet, animations, new Vector2(600, 300), "Run", 1.0f);
            CreateAnimatedCharacter(world, spriteSheet, animations, new Vector2(800, 300), "Walk", 2.0f); // 2x speed
            CreateAnimatedCharacter(world, spriteSheet, animations, new Vector2(1000, 300), "Idle", 0.5f); // 0.5x speed

            Console.WriteLine("=== Animation Demo (Simple) ===");
            Console.WriteLine("Character 1 (x:200): Idle animation (slow)");
            Console.WriteLine("Character 2 (x:400): Walk animation (normal)");
            Console.WriteLine("Character 3 (x:600): Run animation (fast)");
            Console.WriteLine("Character 4 (x:800): Walk animation (2x speed)");
            Console.WriteLine("Character 5 (x:1000): Idle animation (0.5x speed)");
        }

        private void CreateAnimatedCharacter(World world, Texture2D spriteSheet,
            Dictionary<string, AnimationData> animations, Vector2 position,
            string startAnimation, float speed)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new PositionComponent(position.X, position.Y));
            world.AddComponent(entity, new SpriteComponent(spriteSheet));

            var animComponent = new AnimationComponent(animations);
            animComponent.SpeedMultiplier = speed;
            animComponent.Play(startAnimation);
            world.AddComponent(entity, animComponent);
        }

        private Dictionary<string, AnimationData> CreateAnimations()
        {
            var animations = new Dictionary<string, AnimationData>();

            // Idle animation - slow, 4 frames (row 0)
            var idle = new AnimationData("Idle", loop: true);
            for (int i = 0; i < 4; i++)
            {
                idle.AddFrame(new Rectangle(i * 32, 0, 32, 32), 0.3f);
            }
            animations["Idle"] = idle;

            // Walk animation - medium speed, 4 frames (row 1)
            var walk = new AnimationData("Walk", loop: true);
            for (int i = 0; i < 4; i++)
            {
                walk.AddFrame(new Rectangle(i * 32, 32, 32, 32), 0.15f);
            }
            animations["Walk"] = walk;

            // Run animation - fast, 4 frames (row 2)
            var run = new AnimationData("Run", loop: true);
            for (int i = 0; i < 4; i++)
            {
                run.AddFrame(new Rectangle(i * 32, 64, 32, 32), 0.08f);
            }
            animations["Run"] = run;

            return animations;
        }

        private Texture2D CreateSpriteSheet(GraphicsDevice graphicsDevice, int width, int height, int frameSize)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];

            // Define colors for different frames
            Color[] frameColors = new Color[]
            {
                // Row 0 (Idle) - Blue tones
                Color.Blue, Color.CornflowerBlue, Color.DodgerBlue, Color.SkyBlue,
                // Row 1 (Walk) - Green tones
                Color.Green, Color.LimeGreen, Color.ForestGreen, Color.YellowGreen,
                // Row 2 (Run) - Red tones
                Color.Red, Color.OrangeRed, Color.Crimson, Color.Tomato,
                // Row 3 (Jump) - Purple tones
                Color.Purple, Color.Magenta, Color.Violet, Color.Orchid
            };

            // Fill sprite sheet with colored frames
            int framesPerRow = width / frameSize;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int frameX = x / frameSize;
                    int frameY = y / frameSize;
                    int frameIndex = (frameY * framesPerRow) + frameX;

                    if (frameIndex < frameColors.Length)
                    {
                        colorData[y * width + x] = frameColors[frameIndex];
                    }
                    else
                    {
                        colorData[y * width + x] = Color.Black;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }
    }
}
