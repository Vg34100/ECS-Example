namespace ECS_Base.Mechanics.Editor.Components
{
    public struct EditorDragStateComponent
    {
        public bool IsDragging;
        public EditorEntityType EntityType;
        public string EntityIid;

        public EditorDragStateComponent(bool isDragging, EditorEntityType entityType, string entityIid)
        {
            IsDragging = isDragging;
            EntityType = entityType;
            EntityIid = entityIid;
        }
    }
}
