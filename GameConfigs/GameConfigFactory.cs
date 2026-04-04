using System;

namespace ECS_Base.GameConfigs
{
    /// <summary>
    /// Factory for creating game configurations based on command line arguments
    /// </summary>
    public static class GameConfigFactory
    {
        /// <summary>
        /// Get a game configuration based on the config name
        /// </summary>
        /// <param name="configName">Name of the configuration (blank, platformer, etc.)</param>
        /// <returns>The game configuration instance</returns>
        public static IGameConfig GetConfig(string configName)
        {
            return configName?.ToLower() switch
            {
                "blank" => new BlankScreenConfig(),
                "platformer" => new PlatformerConfig(),
                "simple" => new SimplePlatformerTest(),
                "swept" => new SweptCollisionDemo(),
                "topdown" => new TopDownShooterDemo(),
                "sprite" => new SpriteRenderingDemo(),
                "animation" => new AnimationDemoSimple(),
                "audio" => new AudioDemoSimple(),
                "tests" => new SystemTestsDemo(),
                "platform" => new PlatformDemo(),
                "platformer-prog" => new PlatformerProgrammaticConfig(),
                "topdown-prog" => new TopDownProgrammaticConfig(),
                null => new SimplePlatformerTest(), // Default to simple test
                _ => new SimplePlatformerTest() // Default to simple test for unknown configs
            };
        }

        /// <summary>
        /// Parse command line arguments to get the config name
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>The configuration name, or null for default</returns>
        public static string ParseConfigFromArgs(string[] args)
        {
            if (args.Length > 0)
            {
                return args[0];
            }
            return null;
        }
    }
}
