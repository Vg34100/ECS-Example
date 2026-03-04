using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using System.Linq;

namespace ECS_Base.Mechanics.Rendering.Systems
{
    /// <summary>
    /// Renders entities with SpriteComponent
    /// Supports layering, rotation, scaling, tinting, and sprite sheets
    /// Works alongside existing ShapeComponent rendering
    /// </summary>
    public class SpriteSystem
    {
        private SpriteBatch _spriteBatch;
        private Matrix _viewMatrix;

        public SpriteSystem(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _viewMatrix = Matrix.Identity;
        }

        public void SetViewMatrix(Matrix viewMatrix)
        {
            _viewMatrix = viewMatrix;
        }

        public void Draw(World world)
        {
            // Collect all entities with sprites and sort by layer
            var spriteEntities = world.Query<PositionComponent, SpriteComponent>()
                .Select(entity => new
                {
                    Entity = entity,
                    Position = world.GetComponent<PositionComponent>(entity),
                    Sprite = world.GetComponent<SpriteComponent>(entity)
                })
                .OrderBy(e => e.Sprite.Layer)
                .ToList();

            // Begin sprite batch with camera transform
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp,
                transformMatrix: _viewMatrix
            );

            // Draw all sprites
            foreach (var entityData in spriteEntities)
            {
                var position = entityData.Position.Value;
                var sprite = entityData.Sprite;

                // Calculate final color with opacity
                Color finalColor = sprite.Tint * sprite.Opacity;

                // Draw sprite
                _spriteBatch.Draw(
                    texture: sprite.Texture,
                    position: position,
                    sourceRectangle: sprite.SourceRectangle,
                    color: finalColor,
                    rotation: sprite.Rotation,
                    origin: sprite.Origin,
                    scale: sprite.Scale,
                    effects: sprite.Effects,
                    layerDepth: 0f
                );
            }

            _spriteBatch.End();
        }
    }
}
