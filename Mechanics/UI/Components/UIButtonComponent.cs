using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Component for clickable UI buttons
    /// </summary>
    public struct UIButtonComponent
    {
        /// <summary>Button bounds in screen space</summary>
        public Rectangle Bounds;

        /// <summary>Button label text</summary>
        public string Text;

        /// <summary>Font for button text (optional)</summary>
        public SpriteFont Font;

        /// <summary>Callback when button is clicked</summary>
        public Action OnClick;

        /// <summary>Normal state color</summary>
        public Color NormalColor;

        /// <summary>Hover state color</summary>
        public Color HoverColor;

        /// <summary>Pressed state color</summary>
        public Color PressedColor;

        /// <summary>Text color</summary>
        public Color TextColor;

        /// <summary>Current button state</summary>
        public UIButtonState State;

        /// <summary>Is the button currently enabled?</summary>
        public bool IsEnabled;

        public UIButtonComponent(Rectangle bounds, string text, Action onClick)
        {
            Bounds = bounds;
            Text = text;
            OnClick = onClick;
            Font = null;
            NormalColor = new Color(60, 60, 60);
            HoverColor = new Color(80, 80, 80);
            PressedColor = new Color(40, 40, 40);
            TextColor = Color.White;
            State = UIButtonState.Normal;
            IsEnabled = true;
        }
    }

    public enum UIButtonState
    {
        Normal,
        Hovered,
        Pressed
    }
}
