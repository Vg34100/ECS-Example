// Components/LevelComponent.cs
using ECS_Example.LevelData;

namespace ECS_Example.Components
{
    public struct LevelComponent
    {
        public Level LevelData;
        public bool IsActive;

        public LevelComponent(Level levelData, bool isActive = true)
        {
            LevelData = levelData;
            IsActive = isActive;
        }
    }
}