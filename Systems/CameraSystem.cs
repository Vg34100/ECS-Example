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
                camera.TargetPosition = targetPos.Value - _screenCenter + camera.Offset;

                // Apply lag/smoothing using linear interpolation
                Vector2 difference = camera.TargetPosition - camera.Position;
                camera.Position += difference * (1.0f - camera.LagFactor);

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
                return Matrix.CreateTranslation(-camera.Position.X, -camera.Position.Y, 0);
            }

            return Matrix.Identity;
        }
    }
}