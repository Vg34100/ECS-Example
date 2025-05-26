// Systems/RenderSystem.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Example.Components;
using ECS_Example.Entities;

namespace ECS_Example.Systems
{
    public class RenderSystem
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _whiteTexture;
        private bool _debug = true;
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
            _spriteBatch.Begin(transformMatrix: viewMatrix);

            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<Position>(entity, out var position) &&
                    world.TryGetComponent<Shape>(entity, out var shape))
                {
                    // Check if entity should be visible
                    bool shouldDraw = true;
                    if (world.TryGetComponent<FlashEffect>(entity, out var flash))
                    {
                        shouldDraw = flash.IsVisible;
                    }

                    if (shouldDraw)
                    {

                        DrawShape(position, shape);

                        // DEBUG: Draw collider bounds in red
                        if (_debug && world.TryGetComponent<Collider>(entity, out var collider))
                        {

                            _spriteBatch.Draw(
                                _whiteTexture,
                                new Rectangle(
                                    (int)position.Value.X + collider.Bounds.X,
                                    (int)position.Value.Y + collider.Bounds.Y,
                                    collider.Bounds.Width,
                                    1  // Top edge
                                ),
                                Color.Red * 0.5f
                            );
                            _spriteBatch.Draw(
                                _whiteTexture,
                                new Rectangle(
                                    (int)position.Value.X + collider.Bounds.X,
                                    (int)position.Value.Y + collider.Bounds.Y + collider.Bounds.Height - 1,
                                    collider.Bounds.Width,
                                    1  // Bottom edge
                                ),
                                Color.Red * 0.5f
                            );
                        }

                        // DEBUG: Draw attack hitboxes
                        if (_debug && world.TryGetComponent<Attack>(entity, out var attack) && attack.IsAttacking)
                        {
                            // Get entity center for better positioning
                            if (world.TryGetComponent<Collider>(entity, out var entityCollider))
                            {
                                float entityCenterX = position.Value.X + (entityCollider.Bounds.Width / 2);
                                float entityCenterY = position.Value.Y + (entityCollider.Bounds.Height / 2);

                                // Calculate hitbox position
                                float hitboxX;
                                if (attack.IsFacingRight)
                                {
                                    // For right-facing, position starts at the right edge of player
                                    hitboxX = position.Value.X + entityCollider.Bounds.Width;
                                }
                                else
                                {
                                    // For left-facing, position ends at the left edge of player
                                    hitboxX = position.Value.X - attack.HitboxSize.X;
                                }

                                // Center the hitbox vertically on the player
                                float hitboxY = entityCenterY - (attack.HitboxSize.Y / 2);

                                _spriteBatch.Draw(
                                    _whiteTexture,
                                    new Rectangle(
                                        (int)hitboxX,
                                        (int)hitboxY,
                                        (int)attack.HitboxSize.X,
                                        (int)attack.HitboxSize.Y
                                    ),
                                    Color.Red * 0.5f
                                );
                            }
                        }


                    }


                }
            }




            _spriteBatch.End();
        }

        private void DrawShape(Position position, Shape shape)
        {
            switch (shape.Type)
            {
                case Shape.ShapeType.Rectangle:
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

                case Shape.ShapeType.Circle:
                    // For now, we'll draw circles as squares
                    // (proper circle drawing requires more complex code)
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