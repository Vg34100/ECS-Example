using System.Collections.Generic;

namespace ECS_Base.Mechanics.Editor.Components
{
    public struct EditorPaletteComponent
    {
        public List<EditorPaletteItem> Items;

        public EditorPaletteComponent(List<EditorPaletteItem> items)
        {
            Items = items;
        }
    }
}
