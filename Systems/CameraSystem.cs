// Systems/CameraSystem.cs
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using System.Linq;

namespace ECS_Example.Systems
{
    public class CameraSystem
    {
        private Vector2 _screenCenter;

        public CameraSystem(Vector2 screenSize)
        {
            _screenCenter = screenSize / 2;
        }

        public void Update(World world, float deltaTime)
        {
            // Find the camera entity
            var cameraEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<Camera>(e, out var cam) && cam.IsActive);

            if (cameraEntity == null)
                return;

            if (!world.TryGetComponent<Camera>(cameraEntity, out var camera))
                return;

            // Find the target entity (player)
            var targetEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<CameraTarget>(e, out var target) && target.IsActive);

            if (targetEntity != null && world.TryGetComponent<Position>(targetEntity, out var targetPos))
            {
                // Calculate target camera position (center the target on screen)
                camera.TargetPosition = targetPos.Value - (_screenCenter / camera.Zoom) + (camera.Offset / camera.Zoom);

                // Calculate distance to target
                Vector2 difference = camera.TargetPosition - camera.Position;
                float distance = difference.Length();

                // Only move camera if beyond dampening threshold
                if (distance > camera.DampeningThreshold)
                {
                    // Smooth velocity-based movement
                    Vector2 desiredVelocity = difference * (1.0f - camera.LagFactor) * 60.0f; // Scale by 60 for frame rate independence

                    // Apply some velocity dampening to reduce jitter
                    camera.Velocity = Vector2.Lerp(camera.Velocity, desiredVelocity, 0.8f * deltaTime * 60.0f);

                    // Apply velocity to position
                    camera.Position += camera.Velocity * deltaTime;
                }
                else
                {
                    // Gradually reduce velocity when close to target
                    camera.Velocity = Vector2.Lerp(camera.Velocity, Vector2.Zero, 0.9f * deltaTime * 60.0f);
                    camera.Position += camera.Velocity * deltaTime;
                }

                // Update the camera component
                world.AddComponent(cameraEntity, camera);
            }
        }

        public Matrix GetViewMatrix(World world)
        {
            // Find the active camera
            var cameraEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<Camera>(e, out var cam) && cam.IsActive);

            if (cameraEntity != null && world.TryGetComponent<Camera>(cameraEntity, out var camera))
            {
                return Matrix.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0) *
                       Matrix.CreateScale(camera.Zoom, camera.Zoom, 1.0f);
            }

            return Matrix.Identity;
        }
    }
}