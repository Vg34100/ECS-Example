using System.Collections.Generic;

namespace ECS_Base.Mechanics.Level.Components
{
    /// <summary>
    /// Configures which intgrid values are treated as solid.
    /// </summary>
    public struct LevelCollisionConfigComponent
    {
        public HashSet<int> SolidValues;
        public HashSet<int> WaterValues;
        public bool WaterIsSolid;
        public bool UseAxisSeparation;
        public bool UseGridSnapForTiles;

        public LevelCollisionConfigComponent(HashSet<int> solidValues, HashSet<int> waterValues, bool waterIsSolid, bool useAxisSeparation = false, bool useGridSnapForTiles = false)
        {
            SolidValues = solidValues;
            WaterValues = waterValues;
            WaterIsSolid = waterIsSolid;
            UseAxisSeparation = useAxisSeparation;
            UseGridSnapForTiles = useGridSnapForTiles;
        }
    }
}
