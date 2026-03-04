using System.Collections.Generic;

namespace ECS_Base.Mechanics.Level.Components
{
    /// <summary>
    /// Filters which levels are active by identifier.
    /// </summary>
    public struct LevelSelectionConfigComponent
    {
        public HashSet<string> AllowedIdentifiers;

        public LevelSelectionConfigComponent(HashSet<string> allowedIdentifiers)
        {
            AllowedIdentifiers = allowedIdentifiers;
        }
    }
}
