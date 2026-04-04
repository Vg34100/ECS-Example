using System.Collections.Generic;

namespace ECS_Base.Mechanics.Editor.Components
{
    public struct EditorHotbarComponent
    {
        public int MaxItems;
        public int SelectedIndex;
        public List<EditorPaletteItem> Items;

        public EditorHotbarComponent(int maxItems)
        {
            MaxItems = maxItems;
            SelectedIndex = 0;
            Items = new List<EditorPaletteItem>(maxItems);
        }
    }
}
