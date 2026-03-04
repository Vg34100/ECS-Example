using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Camera.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    /// <summary>
    /// Renders rotated rectangles in world space.
    /// </summary>
    public class RotatedRectSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _pixel;
        private readonly CameraSystem _cameraSystem;

        public RotatedRectSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            foreach (var entity in world.Query<RotatedRectComponent>())
            {
                if (!world.TryGetComponent<RotatedRectComponent>(entity, out var rect))
                    continue;

                _spriteBatch.Draw(
                    _pixel,
                    rect.Center,
                    null,
                    rect.Color,
                    rect.Rotation,
                    new Vector2(0.5f, 0.5f),
                    rect.Size,
                    SpriteEffects.None,
                    0f
                );
            }

            _spriteBatch.End();
        }
    }
}
