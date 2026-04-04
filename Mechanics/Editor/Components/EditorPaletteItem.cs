using ECS_Base.Mechanics.UI.Components;

namespace ECS_Base.Mechanics.Editor.Components
{
    public struct EditorPaletteItem
    {
        public bool IsTile;
        public int TileValue;
        public EditorEntityType EntityType;
        public string EntityKind;
        public string Label;
        public UIIconType Icon;
        public bool UseIcon;

        public static EditorPaletteItem Tile(int value, string label)
        {
            return new EditorPaletteItem
            {
                IsTile = true,
                TileValue = value,
                Label = label,
                UseIcon = false
            };
        }

        public static EditorPaletteItem Entity(EditorEntityType type, string kind, string label, UIIconType icon = UIIconType.Coin, bool useIcon = false)
        {
            return new EditorPaletteItem
            {
                IsTile = false,
                EntityType = type,
                EntityKind = kind ?? string.Empty,
                Label = label,
                Icon = icon,
                UseIcon = useIcon
            };
        }

        public bool Equals(EditorPaletteItem other)
        {
            return IsTile == other.IsTile &&
                   TileValue == other.TileValue &&
                   EntityType == other.EntityType &&
                   EntityKind == other.EntityKind &&
                   Label == other.Label;
        }
    }
}
