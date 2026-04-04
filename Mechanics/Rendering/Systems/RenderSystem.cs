using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using ECS_Base.Mechanics.Camera.Systems;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    public class RenderSystem
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;
        private CameraSystem _cameraSystem;

        public RenderSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, CameraSystem cameraSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;

            // Create a 1x1 white texture for drawing shapes
            _whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            // Draw entities
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<PositionComponent>(entity, out var position) &&
                    world.TryGetComponent<ShapeComponent>(entity, out var shape))
                {
                    DrawShape(position, shape);
                }
            }

            _spriteBatch.End();
        }

        private void DrawShape(PositionComponent position, ShapeComponent shape)
        {
            switch (shape.Type)
            {
                case ShapeComponent.ShapeType.Rectangle:
                    _spriteBatch.Draw(
                        _whiteTexture,
                        new Rectangle(
                            (int)position.Value.X,
                            (int)position.Value.Y,
                            (int)shape.Size.X,
                            (int)shape.Size.Y
                        ),
                        shape.Color
                    );
                    break;

                case ShapeComponent.ShapeType.Circle:
                    // For now, we'll draw circles as squares
                    _spriteBatch.Draw(
                        _whiteTexture,
                        new Rectangle(
                            (int)(position.Value.X - shape.Size.X / 2),
                            (int)(position.Value.Y - shape.Size.X / 2),
                            (int)shape.Size.X,
                            (int)shape.Size.X
                        ),
                        shape.Color
                    );
                    break;
            }
        }
    }
}
