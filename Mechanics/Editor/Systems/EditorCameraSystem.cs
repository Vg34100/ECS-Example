using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Editor.Components;
using ECS_Base.Mechanics.Camera.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace ECS_Base.Mechanics.Editor.Systems
{
    /// <summary>
    /// Allows camera panning in editor mode.
    /// </summary>
    public class EditorCameraSystem : IEditorSystem
    {
        public void Update(World world, float deltaTime)
        {
            var stateEntity = world.Query<EditorStateComponent>().FirstOrDefault();
            if (stateEntity == null)
                return;

            var state = world.GetComponent<EditorStateComponent>(stateEntity);
            if (!state.IsEditing)
                return;

            var cameraEntity = world.GetEntities().FirstOrDefault(e => world.TryGetComponent<CameraComponent>(e, out var cam) && cam.IsActive);
            if (cameraEntity == null || !world.TryGetComponent<CameraComponent>(cameraEntity, out var camera))
                return;

            var keyboard = Keyboard.GetState();
            float speed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift) ? 600f : 300f;

            Vector2 move = Vector2.Zero;
            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) move.X -= 1;
            if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) move.X += 1;
            if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)) move.Y -= 1;
            if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) move.Y += 1;

            if (move.LengthSquared() > 0)
            {
                move.Normalize();
                camera.Position += move * speed * deltaTime;
                camera.TargetPosition = camera.Position;
                camera.Velocity = Vector2.Zero;
                world.AddComponent(cameraEntity, camera);
            }
        }
    }
}
