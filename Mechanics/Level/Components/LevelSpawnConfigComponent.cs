namespace ECS_Base.Mechanics.Level.Components
{
    public enum LevelSpawnMode
    {
        Platformer,
        Topdown
    }

    public struct LevelSpawnConfigComponent
    {
        public LevelSpawnMode Mode;

        public LevelSpawnConfigComponent(LevelSpawnMode mode)
        {
            Mode = mode;
        }
    }
}
