using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Level.Components
{
    public struct PathComponent
    {
        public string TargetLevelId;
        public string TargetPathEntityId;
        public Rectangle TriggerArea;
        public bool IsActive;

        public PathComponent(string targetLevelId, string targetPathEntityId, Rectangle triggerArea, bool isActive = true)
        {
            TargetLevelId = targetLevelId;
            TargetPathEntityId = targetPathEntityId;
            TriggerArea = triggerArea;
            IsActive = isActive;
        }
    }
}
