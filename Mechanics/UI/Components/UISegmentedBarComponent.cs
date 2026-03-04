using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Segmented bar (e.g., Mario-style health/energy).
    /// </summary>
    public struct UISegmentedBarComponent
    {
        /// <summary>Top-left position in screen space</summary>
        public Vector2 Position;

        /// <summary>Target entity ID that owns StatsComponent</summary>
        public int TargetEntityId;

        /// <summary>Number of segments</summary>
        public int Segments;

        /// <summary>Base segments (drawn in base color). Extra segments use BonusColor.</summary>
        public int BaseSegments;

        /// <summary>Pixel size of each segment</summary>
        public Point SegmentSize;

        /// <summary>Pixels between segments</summary>
        public int SegmentSpacing;

        /// <summary>Filled segment color</summary>
        public Color FillColor;

        /// <summary>Filled bonus segment color</summary>
        public Color BonusFillColor;

        /// <summary>Empty segment color</summary>
        public Color EmptyColor;

        /// <summary>Border color</summary>
        public Color BorderColor;

        /// <summary>Border thickness in pixels</summary>
        public int BorderThickness;

        public UISegmentedBarComponent(Vector2 position, int targetEntityId, int segments = 3)
        {
            Position = position;
            TargetEntityId = targetEntityId;
            Segments = segments;
            BaseSegments = segments;
            SegmentSize = new Point(18, 6);
            SegmentSpacing = 4;
            FillColor = new Color(0, 220, 140);
            BonusFillColor = new Color(220, 60, 60);
            EmptyColor = new Color(40, 40, 40);
            BorderColor = new Color(10, 10, 10);
            BorderThickness = 1;
        }
    }
}
