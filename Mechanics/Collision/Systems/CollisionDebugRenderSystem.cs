using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Camera.Systems;

namespace ECS_Base.Mechanics.Collision.Systems
{
    /// <summary>
    /// Debug system to visualize collision boxes
    /// </summary>
    public class CollisionDebugRenderSystem
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;
        private CameraSystem _cameraSystem;
        private CollisionSystem _collisionSystem;

        public CollisionDebugRenderSystem(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice,
                                         CameraSystem cameraSystem, CollisionSystem collisionSystem)
        {
            _spriteBatch = spriteBatch;
            _cameraSystem = cameraSystem;
            _collisionSystem = collisionSystem;

            // Create a 1x1 white texture for drawing
            _whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });
        }

        public void Draw(World world)
        {
            var viewMatrix = _cameraSystem.GetViewMatrix(world);
            _spriteBatch.Begin(transformMatrix: viewMatrix, samplerState: SamplerState.PointClamp);

            // Draw collision boxes as red outlines
            foreach (var collisionBox in _collisionSystem.GetCollisionTiles())
            {
                // Draw outline (top, bottom, left, right)
                DrawRectangleOutline(collisionBox, Color.Red * 0.5f, 1);
            }

            _spriteBatch.End();
        }

        private void DrawRectangleOutline(Rectangle rect, Color color, int thickness)
        {
            // Top
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            _spriteBatch.Draw(_whiteTexture, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
