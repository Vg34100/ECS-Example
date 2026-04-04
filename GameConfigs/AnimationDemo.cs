using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using ECS_Base.Mechanics.Animation.Components;
using ECS_Base.Mechanics.Animation.Data;
using ECS_Base.Mechanics.Animation.Systems;
using ECS_Base.Mechanics.Input.Components;
using ECS_Base.Mechanics.Input.Systems;
using System;
using System.Collections.Generic;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Demo for animation system
    /// Tests: Frame animations, looping, one-shot, speed control, animation switching
    /// Uses a programmatically generated sprite sheet
    /// Controls:
    ///   1-4: Switch animations (Idle, Walk, Run, Jump)
    ///   Space: Toggle play/pause
    ///   Up/Down: Adjust animation speed
    /// </summary>
    public class AnimationDemo : IGameConfig
    {
        public string Name => "Animation Demo";

        private Entity _characterEntity;
        private KeyboardState _previousKeyboard;

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Mark that we want sprite system
            game.SpriteSystem = new SpriteSystem(null);

            // Add systems
            systemManager.AddSystem(new InputSystem());
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
                initialPosition: new Vector2(400, 300),
                lagFactor: 0.0f,
                offset: Vector2.Zero,
                zoom: 2.0f,
                dampeningThreshold: 0f
            ));

            // Create a fake sprite sheet (4x4 grid, each frame is 32x32)
            Texture2D spriteSheet = CreateSpriteSheet(game.GraphicsDevice, 128, 128, 32);

            // Create animation data
            var animations = CreateAnimations();

            // Create animated character
            _characterEntity = world.CreateEntity();
            world.AddComponent(_characterEntity, new PositionComponent(400, 300));
            world.AddComponent(_characterEntity, new SpriteComponent(spriteSheet));

            var animComponent = new AnimationComponent(animations);
            animComponent.Play("Idle"); // Start with idle animation
            world.AddComponent(_characterEntity, animComponent);

            // Create instruction text (we'll just use console for now)
            Console.WriteLine("=== Animation Demo ===");
            Console.WriteLine("Controls:");
            Console.WriteLine("  1: Play 'Idle' animation (4 frames, slow)");
            Console.WriteLine("  2: Play 'Walk' animation (4 frames, medium)");
            Console.WriteLine("  3: Play 'Run' animation (4 frames, fast)");
            Console.WriteLine("  4: Play 'Jump' animation (2 frames, one-shot)");
            Console.WriteLine("  Space: Toggle play/pause");
            Console.WriteLine("  Up/Down: Adjust animation speed");
        }

        private Dictionary<string, AnimationData> CreateAnimations()
        {
            var animations = new Dictionary<string, AnimationData>();

            // Idle animation - slow, 4 frames (row 0)
            var idle = new AnimationData("Idle", loop: true);
            for (int i = 0; i < 4; i++)
            {
                idle.AddFrame(new Rectangle(i * 32, 0, 32, 32), 0.3f); // 0.3s per frame
            }
            animations["Idle"] = idle;

            // Walk animation - medium speed, 4 frames (row 1)
            var walk = new AnimationData("Walk", loop: true);
            for (int i = 0; i < 4; i++)
            {
                walk.AddFrame(new Rectangle(i * 32, 32, 32, 32), 0.15f); // 0.15s per frame
            }
            animations["Walk"] = walk;

            // Run animation - fast, 4 frames (row 2)
            var run = new AnimationData("Run", loop: true);
            for (int i = 0; i < 4; i++)
            {
                run.AddFrame(new Rectangle(i * 32, 64, 32, 32), 0.08f); // 0.08s per frame
            }
            animations["Run"] = run;

            // Jump animation - one-shot (no loop), 2 frames (row 3)
            var jump = new AnimationData("Jump", loop: false);
            jump.AddFrame(new Rectangle(0, 96, 32, 32), 0.2f);
            jump.AddFrame(new Rectangle(32, 96, 32, 32), 0.2f);
            animations["Jump"] = jump;

            return animations;
        }

        /// <summary>
        /// Creates a test sprite sheet with colored frames in a grid
        /// Each frame has a different color for visual distinction
        /// </summary>
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

        // Note: This would ideally be in a custom system, but for demo simplicity we're handling input here
        // In a real game, you'd want a dedicated InputHandlerSystem
        public void HandleInput(World world, KeyboardState currentKeyboard)
        {
            if (!world.TryGetComponent(_characterEntity, out AnimationComponent animation))
                return;

            // Switch animations
            if (WasKeyJustPressed(Keys.D1, currentKeyboard))
            {
                animation.Play("Idle");
                Console.WriteLine("Playing: Idle");
            }
            else if (WasKeyJustPressed(Keys.D2, currentKeyboard))
            {
                animation.Play("Walk");
                Console.WriteLine("Playing: Walk");
            }
            else if (WasKeyJustPressed(Keys.D3, currentKeyboard))
            {
                animation.Play("Run");
                Console.WriteLine("Playing: Run");
            }
            else if (WasKeyJustPressed(Keys.D4, currentKeyboard))
            {
                animation.Play("Jump", loop: false);
                Console.WriteLine("Playing: Jump (one-shot)");
            }

            // Toggle play/pause
            if (WasKeyJustPressed(Keys.Space, currentKeyboard))
            {
                if (animation.IsPlaying)
                {
                    animation.Stop();
                    Console.WriteLine("Animation paused");
                }
                else
                {
                    animation.IsPlaying = true;
                    Console.WriteLine("Animation resumed");
                }
            }

            // Adjust speed
            if (WasKeyJustPressed(Keys.Up, currentKeyboard))
            {
                animation.SpeedMultiplier += 0.25f;
                Console.WriteLine($"Speed: {animation.SpeedMultiplier}x");
            }
            else if (WasKeyJustPressed(Keys.Down, currentKeyboard))
            {
                animation.SpeedMultiplier = Math.Max(0.25f, animation.SpeedMultiplier - 0.25f);
                Console.WriteLine($"Speed: {animation.SpeedMultiplier}x");
            }

            world.AddComponent(_characterEntity, animation);
            _previousKeyboard = currentKeyboard;
        }

        private bool WasKeyJustPressed(Keys key, KeyboardState currentKeyboard)
        {
            return currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
        }
    }
}
