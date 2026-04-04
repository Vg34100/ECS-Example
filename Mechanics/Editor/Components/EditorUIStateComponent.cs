namespace ECS_Base.Mechanics.Editor.Components
{
    public struct EditorUIStateComponent
    {
        public bool ShowPalette;
        public bool IsHoveringUI;
        public int ToggleCooldownFrames;
        public bool ShowGrid;
        public float SaveToastTime;

        public EditorUIStateComponent(bool showPalette)
        {
            ShowPalette = showPalette;
            IsHoveringUI = false;
            ToggleCooldownFrames = 0;
            ShowGrid = true;
            SaveToastTime = 0f;
        }
    }
}
