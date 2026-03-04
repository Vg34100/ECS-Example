using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Renders segmented bars (e.g., Mario-style health/energy).
    /// </summary>
    public class UISegmentedBarSystem
    {
        private readonly Texture2D _pixel;

        public UISegmentedBarSystem(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world, SpriteBatch spriteBatch)
        {
            foreach (var entity in world.Query<UISegmentedBarComponent>())
            {
                if (!world.TryGetComponent<UISegmentedBarComponent>(entity, out var bar))
                    continue;

                if (!TryGetStats(world, bar.TargetEntityId, out var stats))
                    continue;

                DrawBar(spriteBatch, bar, stats);
            }
        }

        private void DrawBar(SpriteBatch spriteBatch, UISegmentedBarComponent bar, StatsComponent stats)
        {
            int baseSegments = bar.BaseSegments > 0 ? bar.BaseSegments : (bar.Segments <= 0 ? 3 : bar.Segments);
            int bonusSegments = stats.MaxHealth > baseSegments
                ? (int)System.Math.Ceiling(stats.MaxHealth - baseSegments)
                : 0;
            int segments = baseSegments + bonusSegments;
            float segmentValue = stats.MaxHealth > 0 ? stats.MaxHealth / segments : 1f;

            for (int i = 0; i < segments; i++)
            {
                Vector2 segPos = new Vector2(
                    bar.Position.X + i * (bar.SegmentSize.X + bar.SegmentSpacing),
                    bar.Position.Y
                );

                var bounds = new Rectangle(
                    (int)segPos.X,
                    (int)segPos.Y,
                    bar.SegmentSize.X,
                    bar.SegmentSize.Y
                );

                // Empty background
                spriteBatch.Draw(_pixel, bounds, bar.EmptyColor);

                // Filled portion
                float segmentHealth = stats.Health - (i * segmentValue);
                float fillPercent = MathHelper.Clamp(segmentHealth / segmentValue, 0f, 1f);

                if (fillPercent > 0f)
                {
                    var fillRect = new Rectangle(
                        bounds.X,
                        bounds.Y,
                        (int)(bounds.Width * fillPercent),
                        bounds.Height
                    );
                    var fillColor = i < baseSegments ? bar.FillColor : bar.BonusFillColor;
                    spriteBatch.Draw(_pixel, fillRect, fillColor);
                }

                // Border
                if (bar.BorderColor.A > 0 && bar.BorderThickness > 0)
                {
                    DrawBorder(spriteBatch, bounds, bar.BorderColor, bar.BorderThickness);
                }
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
        {
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            spriteBatch.Draw(_pixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }

        private bool TryGetStats(World world, int entityId, out StatsComponent stats)
        {
            stats = default;
            var target = world.GetEntities().FirstOrDefault(e => e.Id == entityId);
            if (target == null)
                return false;

            return world.TryGetComponent(target, out stats);
        }
    }
}
