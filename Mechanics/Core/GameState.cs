namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Defines the various states a game can be in.
    /// Add custom states as needed for your game.
    /// </summary>
    public enum GameState
    {
        /// <summary>Main menu or title screen</summary>
        MainMenu,

        /// <summary>Active gameplay</summary>
        Playing,

        /// <summary>Game is paused</summary>
        Paused,

        /// <summary>Game over screen</summary>
        GameOver,

        /// <summary>Transitioning between levels</summary>
        LevelTransition,

        /// <summary>Inventory/equipment screen</summary>
        Inventory,

        /// <summary>Dialogue or cutscene</summary>
        Dialogue,

        /// <summary>Loading screen</summary>
        Loading,

        /// <summary>Settings/options menu</summary>
        Settings
    }
}
