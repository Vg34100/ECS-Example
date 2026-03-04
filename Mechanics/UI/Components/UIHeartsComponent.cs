using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Draws a row of hearts for health (Zelda-style).
    /// </summary>
    public struct UIHeartsComponent
    {
        /// <summary>Top-left position in screen space</summary>
        public Vector2 Position;

        /// <summary>Target entity ID that owns StatsComponent</summary>
        public int TargetEntityId;

        /// <summary>Health units per heart (2 = half-hearts)</summary>
        public int UnitsPerHeart;

        /// <summary>Pixel size of a heart (width and height)</summary>
        public int HeartSize;

        /// <summary>Pixels between hearts</summary>
        public int HeartSpacing;

        /// <summary>Color for filled heart</summary>
        public Color FillColor;

        /// <summary>Color for empty heart</summary>
        public Color EmptyColor;

        /// <summary>Color for heart outline</summary>
        public Color OutlineColor;

        public UIHeartsComponent(Vector2 position, int targetEntityId, int unitsPerHeart = 2, int heartSize = 16, int heartSpacing = 4)
        {
            Position = position;
            TargetEntityId = targetEntityId;
            UnitsPerHeart = unitsPerHeart;
            HeartSize = heartSize;
            HeartSpacing = heartSpacing;
            FillColor = new Color(220, 40, 40);
            EmptyColor = new Color(40, 40, 40);
            OutlineColor = new Color(10, 10, 10);
        }
    }
}
