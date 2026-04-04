
using ECS_Base.GameConfigs;
using System;
using System.IO;

// Write debug info to a file we can actually see
var debugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt");
File.WriteAllText(debugPath, $"Args count: {args.Length}\n");
for (int i = 0; i < args.Length; i++)
{
    File.AppendAllText(debugPath, $"Arg[{i}]: {args[i]}\n");
}

// Parse command line arguments to determine which configuration to use
var configName = GameConfigFactory.ParseConfigFromArgs(args);
var config = GameConfigFactory.GetConfig(configName);

File.AppendAllText(debugPath, $"Config name parsed: {configName}\n");
File.AppendAllText(debugPath, $"Config created: {config.Name}\n");

System.Console.WriteLine($"Starting with configuration: {config.Name}");

using var game = new ECS_Base.Game1(config);
game.Run();
