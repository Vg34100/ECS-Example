using Microsoft.Xna.Framework;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Components;
using ECS_Base.Mechanics.Camera.Systems;
using ECS_Base.Mechanics.Audio.Systems;
using System;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Audio system demo
    /// NOTE: This demo shows the audio system structure but doesn't play actual sounds
    /// (requires .wav/.xnb sound files in Content folder)
    ///
    /// To add real audio:
    /// 1. Add .wav files to Content folder
    /// 2. Use Content Pipeline to build them
    /// 3. Load with: Content.Load<SoundEffect>("SoundName")
    /// 4. Create AudioComponent with the loaded sound
    /// </summary>
    public class AudioDemoSimple : IGameConfig
    {
        public string Name => "Audio Demo (Structure Only)";

        public void Initialize(Game1 game, World world, SystemManager systemManager)
        {
            // Add audio systems
            systemManager.AddSystem(new AudioSystem());
            systemManager.AddSystem(new MusicSystem());

            var cameraSystem = new CameraSystem(new Vector2(
                game.GraphicsDeviceManager.PreferredBackBufferWidth,
                game.GraphicsDeviceManager.PreferredBackBufferHeight
            ));
            systemManager.AddSystem(cameraSystem);
            game.CameraSystem = cameraSystem;

            // Create camera
            var cameraEntity = world.CreateEntity();
            world.AddComponent(cameraEntity, new CameraComponent(
                initialPosition: Vector2.Zero,
                lagFactor: 0.0f,
                offset: Vector2.Zero,
                zoom: 1.0f,
                dampeningThreshold: 0f
            ));

            Console.WriteLine("=== Audio Demo (Structure Only) ===");
            Console.WriteLine("Audio systems initialized successfully!");
            Console.WriteLine();
            Console.WriteLine("To add real audio:");
            Console.WriteLine("1. Add .wav files to Content folder");
            Console.WriteLine("2. Use MGCB Editor to build them");
            Console.WriteLine("3. Load with: Content.Load<SoundEffect>(\"name\")");
            Console.WriteLine("4. Create entity with AudioComponent");
            Console.WriteLine();
            Console.WriteLine("Example usage:");
            Console.WriteLine("  var sfx = Content.Load<SoundEffect>(\"Jump\");");
            Console.WriteLine("  var entity = world.CreateEntity();");
            Console.WriteLine("  world.AddComponent(entity, new AudioComponent(sfx));");
            Console.WriteLine();
            Console.WriteLine("Features:");
            Console.WriteLine("  - One-shot and looping sounds");
            Console.WriteLine("  - Volume, pitch, pan control");
            Console.WriteLine("  - 3D positional audio (distance-based volume)");
            Console.WriteLine("  - Background music with crossfading");
        }
    }
}
