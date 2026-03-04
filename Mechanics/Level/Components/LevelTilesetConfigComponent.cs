using System.Collections.Generic;

namespace ECS_Base.Mechanics.Level.Components
{
    /// <summary>
    /// Configures tileset-based rendering for LDtk intgrid levels.
    /// </summary>
    public struct LevelTilesetConfigComponent
    {
        public string ProjectFileName;
        public string LevelsRootRelative;
        public bool UseIntGridRender;

        public LevelTilesetConfigComponent(string projectFileName, string levelsRootRelative = "Levels", bool useIntGridRender = false)
        {
            ProjectFileName = projectFileName;
            LevelsRootRelative = levelsRootRelative;
            UseIntGridRender = useIntGridRender;
        }
    }
}
