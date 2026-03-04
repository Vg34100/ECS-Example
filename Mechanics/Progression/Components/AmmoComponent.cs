namespace ECS_Base.Mechanics.Progression.Components
{
    /// <summary>
    /// Tracks ammo counts.
    /// </summary>
    public struct AmmoComponent
    {
        public int Arrows;
        public int Bombs;

        public AmmoComponent(int arrows, int bombs)
        {
            Arrows = arrows;
            Bombs = bombs;
        }
    }
}
