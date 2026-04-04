using ECS_Base.LevelData;

namespace ECS_Base.Mechanics.Level.Components
{
    public struct LevelComponent
    {
        public LevelData.Level LevelData;
        public bool IsActive;

        public LevelComponent(LevelData.Level levelData, bool isActive = true)
        {
            LevelData = levelData;
            IsActive = isActive;
        }
    }
}
