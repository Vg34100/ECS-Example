using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Component for progress bars (health bars, loading bars, etc.)
    /// </summary>
    public struct UIProgressBarComponent
    {
        /// <summary>Position and size of the progress bar</summary>
        public Rectangle Bounds;

        /// <summary>Current value</summary>
        public float CurrentValue;

        /// <summary>Maximum value</summary>
        public float MaxValue;

        /// <summary>Background color</summary>
        public Color BackgroundColor;

        /// <summary>Foreground/fill color</summary>
        public Color ForegroundColor;

        /// <summary>Border color (Color.Transparent for no border)</summary>
        public Color BorderColor;

        /// <summary>Border thickness in pixels</summary>
        public int BorderThickness;

        /// <summary>Direction the bar fills</summary>
        public FillDirection Direction;

        /// <summary>Gets the fill percentage (0.0 to 1.0)</summary>
        public readonly float FillPercentage => MaxValue > 0 ? MathHelper.Clamp(CurrentValue / MaxValue, 0f, 1f) : 0f;

        public UIProgressBarComponent(Rectangle bounds, float currentValue, float maxValue)
        {
            Bounds = bounds;
            CurrentValue = currentValue;
            MaxValue = maxValue;
            BackgroundColor = new Color(40, 40, 40);
            ForegroundColor = new Color(0, 200, 0); // Green
            BorderColor = Color.White;
            BorderThickness = 1;
            Direction = FillDirection.LeftToRight;
        }
    }

    public enum FillDirection
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom
    }
}
