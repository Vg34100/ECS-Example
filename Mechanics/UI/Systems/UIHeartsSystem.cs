using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.UI.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace ECS_Base.Mechanics.UI.Systems
{
    /// <summary>
    /// Renders hearts for health.
    /// </summary>
    public class UIHeartsSystem
    {
        private readonly Texture2D _pixel;

        // 8x6 heart mask
        private static readonly string[] HeartMask =
        {
            "01100110",
            "11111111",
            "11111111",
            "01111110",
            "00111100",
            "00011000"
        };

        public UIHeartsSystem(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(World world, SpriteBatch spriteBatch)
        {
            foreach (var entity in world.Query<UIHeartsComponent>())
            {
                if (!world.TryGetComponent<UIHeartsComponent>(entity, out var hearts))
                    continue;

                if (!TryGetStats(world, hearts.TargetEntityId, out var stats))
                    continue;

                DrawHearts(spriteBatch, hearts, stats);
            }
        }

        private void DrawHearts(SpriteBatch spriteBatch, UIHeartsComponent hearts, StatsComponent stats)
        {
            int unitsPerHeart = hearts.UnitsPerHeart <= 0 ? 2 : hearts.UnitsPerHeart;
            int maxHearts = (int)System.Math.Ceiling(stats.MaxHealth / unitsPerHeart);

            int maskWidth = HeartMask[0].Length;
            int maskHeight = HeartMask.Length;
            int pixelW = System.Math.Max(1, hearts.HeartSize / maskWidth);
            int pixelH = System.Math.Max(1, hearts.HeartSize / maskHeight);

            for (int i = 0; i < maxHearts; i++)
            {
                float heartValue = stats.Health - (i * unitsPerHeart);
                float fillPercent = MathHelper.Clamp(heartValue / unitsPerHeart, 0f, 1f);

                Vector2 heartPos = new Vector2(
                    hearts.Position.X + i * (hearts.HeartSize + hearts.HeartSpacing),
                    hearts.Position.Y
                );

                DrawHeart(spriteBatch, heartPos, hearts, fillPercent, pixelW, pixelH);
            }
        }

        private void DrawHeart(SpriteBatch spriteBatch, Vector2 pos, UIHeartsComponent hearts, float fillPercent, int pixelW, int pixelH)
        {
            int maskWidth = HeartMask[0].Length;
            int fillColumns = (int)System.Math.Ceiling(maskWidth * fillPercent);

            for (int y = 0; y < HeartMask.Length; y++)
            {
                for (int x = 0; x < HeartMask[y].Length; x++)
                {
                    if (HeartMask[y][x] != '1')
                        continue;

                    Color color = x < fillColumns ? hearts.FillColor : hearts.EmptyColor;
                    DrawPixel(spriteBatch, pos, x, y, pixelW, pixelH, color);
                }
            }

            if (hearts.OutlineColor.A > 0)
            {
                DrawOutline(spriteBatch, pos, hearts, pixelW, pixelH);
            }
        }

        private void DrawOutline(SpriteBatch spriteBatch, Vector2 pos, UIHeartsComponent hearts, int pixelW, int pixelH)
        {
            for (int y = 0; y < HeartMask.Length; y++)
            {
                for (int x = 0; x < HeartMask[y].Length; x++)
                {
                    if (HeartMask[y][x] != '1')
                        continue;

                    bool isEdge = IsEdgePixel(x, y);
                    if (!isEdge)
                        continue;

                    DrawPixel(spriteBatch, pos, x, y, pixelW, pixelH, hearts.OutlineColor);
                }
            }
        }

        private bool IsEdgePixel(int x, int y)
        {
            if (HeartMask[y][x] != '1')
                return false;

            // Check 4-neighbors for empty
            if (x > 0 && HeartMask[y][x - 1] == '0') return true;
            if (x < HeartMask[y].Length - 1 && HeartMask[y][x + 1] == '0') return true;
            if (y > 0 && HeartMask[y - 1][x] == '0') return true;
            if (y < HeartMask.Length - 1 && HeartMask[y + 1][x] == '0') return true;

            return false;
        }

        private void DrawPixel(SpriteBatch spriteBatch, Vector2 pos, int x, int y, int w, int h, Color color)
        {
            spriteBatch.Draw(
                _pixel,
                new Rectangle(
                    (int)pos.X + x * w,
                    (int)pos.Y + y * h,
                    w,
                    h
                ),
                color
            );
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
