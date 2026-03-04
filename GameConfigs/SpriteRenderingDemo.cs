using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Rendering.Systems;
using System;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Demo for sprite rendering system
    /// Tests: layers, rotation, scale, tint, opacity, flipping
    /// Uses programmatically generated textures (no external assets needed)
    /// </summary>
    public class SpriteRenderingDemo : IGameConfig
    {
        public string Name => "Sprite Rendering Demo";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Mark that we want sprite system (will be created in LoadContent with SpriteBatch)
            game.SpriteSystem = new SpriteSystem(null); // Placeholder, recreated in LoadContent

            // Add camera system
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
                lagFactor: 0.0f, // No lag for demo
                offset: Vector2.Zero,
                zoom: 1.0f,
                dampeningThreshold: 0f
            ));

            // Create test textures
            Texture2D redSquare = CreateColoredTexture(game.GraphicsDevice, 64, 64, Color.Red);
            Texture2D greenSquare = CreateColoredTexture(game.GraphicsDevice, 64, 64, Color.Green);
            Texture2D blueSquare = CreateColoredTexture(game.GraphicsDevice, 64, 64, Color.Blue);
            Texture2D yellowSquare = CreateColoredTexture(game.GraphicsDevice, 32, 32, Color.Yellow);
            Texture2D purpleSquare = CreateColoredTexture(game.GraphicsDevice, 48, 48, Color.Purple);

            // Test 1: Basic sprites at different positions
            CreateSprite(world, redSquare, new Vector2(100, 100), "Red - Layer 0");
            CreateSprite(world, greenSquare, new Vector2(200, 100), "Green - Layer 0");
            CreateSprite(world, blueSquare, new Vector2(300, 100), "Blue - Layer 0");

            // Test 2: Layering (overlapping sprites)
            var backSprite = CreateSprite(world, redSquare, new Vector2(150, 250), "Back Layer (-1)");
            var backSpriteComp = world.GetComponent<SpriteComponent>(backSprite);
            backSpriteComp.Layer = -1; // Behind
            world.AddComponent(backSprite, backSpriteComp);

            var frontSprite = CreateSprite(world, blueSquare, new Vector2(170, 270), "Front Layer (1)");
            var frontSpriteComp = world.GetComponent<SpriteComponent>(frontSprite);
            frontSpriteComp.Layer = 1; // In front
            world.AddComponent(frontSprite, frontSpriteComp);

            // Test 3: Rotation
            var rotatedSprite = CreateSprite(world, greenSquare, new Vector2(400, 200), "Rotated 45°");
            var rotatedComp = world.GetComponent<SpriteComponent>(rotatedSprite);
            rotatedComp.Rotation = MathHelper.ToRadians(45);
            world.AddComponent(rotatedSprite, rotatedComp);

            // Test 4: Scaling
            var scaledUpSprite = CreateSprite(world, yellowSquare, new Vector2(500, 150), "Scaled 2x");
            var scaledUpComp = world.GetComponent<SpriteComponent>(scaledUpSprite);
            scaledUpComp.Scale = new Vector2(2.0f, 2.0f);
            world.AddComponent(scaledUpSprite, scaledUpComp);

            var scaledDownSprite = CreateSprite(world, yellowSquare, new Vector2(600, 150), "Scaled 0.5x");
            var scaledDownComp = world.GetComponent<SpriteComponent>(scaledDownSprite);
            scaledDownComp.Scale = new Vector2(0.5f, 0.5f);
            world.AddComponent(scaledDownSprite, scaledDownComp);

            // Test 5: Tinting
            var tintedSprite = CreateSprite(world, redSquare, new Vector2(100, 400), "Tinted Cyan");
            var tintedComp = world.GetComponent<SpriteComponent>(tintedSprite);
            tintedComp.Tint = Color.Cyan;
            world.AddComponent(tintedSprite, tintedComp);

            // Test 6: Opacity
            var transparentSprite = CreateSprite(world, purpleSquare, new Vector2(250, 400), "50% Opacity");
            var transparentComp = world.GetComponent<SpriteComponent>(transparentSprite);
            transparentComp.Opacity = 0.5f;
            world.AddComponent(transparentSprite, transparentComp);

            // Test 7: Flip horizontal
            var flippedHSprite = CreateSprite(world, blueSquare, new Vector2(400, 400), "Flipped Horizontal");
            var flippedHComp = world.GetComponent<SpriteComponent>(flippedHSprite);
            flippedHComp.Effects = SpriteEffects.FlipHorizontally;
            world.AddComponent(flippedHSprite, flippedHComp);

            // Test 8: Flip vertical
            var flippedVSprite = CreateSprite(world, blueSquare, new Vector2(500, 400), "Flipped Vertical");
            var flippedVComp = world.GetComponent<SpriteComponent>(flippedVSprite);
            flippedVComp.Effects = SpriteEffects.FlipVertically;
            world.AddComponent(flippedVSprite, flippedVComp);

            // Test 9: Combined effects (rotation + scale + opacity)
            var complexSprite = CreateSprite(world, greenSquare, new Vector2(650, 350), "Complex");
            var complexComp = world.GetComponent<SpriteComponent>(complexSprite);
            complexComp.Rotation = MathHelper.ToRadians(30);
            complexComp.Scale = new Vector2(1.5f, 1.5f);
            complexComp.Opacity = 0.7f;
            complexComp.Layer = 2;
            world.AddComponent(complexSprite, complexComp);

            Console.WriteLine("=== Sprite Rendering Demo ===");
            Console.WriteLine("Testing: Layers, Rotation, Scale, Tint, Opacity, Flipping");
            Console.WriteLine("All sprites use programmatically generated textures");
        }

        private Entity CreateSprite(World world, Texture2D texture, Vector2 position, string label)
        {
            var entity = world.CreateEntity();
            world.AddComponent(entity, new PositionComponent(position.X, position.Y));
            world.AddComponent(entity, new SpriteComponent(texture));
            return entity;
        }

        /// <summary>
        /// Creates a simple solid color texture for testing
        /// </summary>
        private Texture2D CreateColoredTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = color;
            }
            texture.SetData(colorData);
            return texture;
        }
    }
}
