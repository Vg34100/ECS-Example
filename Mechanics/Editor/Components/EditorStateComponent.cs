namespace ECS_Base.Mechanics.Editor.Components
{
    public enum EditorTool
    {
        TilePaint,
        TileErase,
        EntityPlace
    }

    public enum EditorEntityType
    {
        Player,
        Enemy,
        Pickup,
        Hazard,
        Path
    }

    /// <summary>
    /// Stores editor state and current selection.
    /// </summary>
    public struct EditorStateComponent
    {
        public bool IsEditing;
        public EditorTool Tool;
        public int SelectedTileValue;
        public EditorEntityType SelectedEntityType;
        public string SelectedEntityKind;
        public string ActiveLevelId;

        public EditorStateComponent(bool isEditing, EditorTool tool, int selectedTileValue, EditorEntityType entityType, string entityKind, string activeLevelId)
        {
            IsEditing = isEditing;
            Tool = tool;
            SelectedTileValue = selectedTileValue;
            SelectedEntityType = entityType;
            SelectedEntityKind = entityKind;
            ActiveLevelId = activeLevelId;
        }
    }
}
