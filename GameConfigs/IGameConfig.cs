using ECS_Base.Mechanics.Core;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Interface for game configurations. Each configuration defines a different game setup.
    /// </summary>
    public interface IGameConfig
    {
        /// <summary>
        /// Name of this configuration
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Initialize the game world and systems for this configuration
        /// </summary>
        void Initialize(Game1 game, World world, SystemManager systemManager);
    }
}
