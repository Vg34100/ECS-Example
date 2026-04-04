using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.SaveLoad.Data;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Stats.Components;
using ECS_Base.Mechanics.Inventory.Components;
using ECS_Base.Mechanics.Inventory.Data;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ECS_Base.Mechanics.SaveLoad.Systems
{
    /// <summary>
    /// System for saving and loading game state
    /// </summary>
    public class SaveLoadSystem
    {
        private readonly string _saveDirectory;

        public SaveLoadSystem(string saveDirectory = "Saves")
        {
            _saveDirectory = saveDirectory;

            // Create save directory if it doesn't exist
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }

        /// <summary>
        /// Save the current game state
        /// </summary>
        public bool SaveGame(World world, Entity playerEntity, string saveName)
        {
            try
            {
                var saveData = new SaveData
                {
                    SaveName = saveName,
                    SaveTime = DateTime.Now
                };

                // Save player data
                if (world.TryGetComponent<PositionComponent>(playerEntity, out var playerPos))
                {
                    saveData.Player.Position = playerPos.Value;
                }

                if (world.TryGetComponent<StatsComponent>(playerEntity, out var playerStats))
                {
                    saveData.Player.Health = playerStats.Health;
                    saveData.Player.MaxHealth = playerStats.MaxHealth;
                    saveData.Player.Stats["Attack"] = playerStats.Attack;
                    saveData.Player.Stats["Defense"] = playerStats.Defense;
                    saveData.Player.Stats["Speed"] = playerStats.Speed;
                }

                if (world.TryGetComponent<InventoryComponent>(playerEntity, out var inventory))
                {
                    var items = inventory.Items.Where(i => i.HasValue).Select(i => i.Value);
                    foreach (var item in items)
                    {
                        saveData.Player.Inventory.Add(new ItemSaveData(item.Id, item.Name, item.StackSize));
                    }
                }

                // Serialize and save
                string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                string filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load a saved game state
        /// </summary>
        public SaveData LoadGame(string saveName)
        {
            try
            {
                string filePath = Path.Combine(_saveDirectory, $"{saveName}.json");

                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Apply loaded save data to world
        /// </summary>
        public void ApplyLoadedData(World world, Entity playerEntity, SaveData saveData)
        {
            if (saveData == null)
                return;

            // Restore player position
            if (world.TryGetComponent<PositionComponent>(playerEntity, out var playerPos))
            {
                playerPos.Value = saveData.Player.Position;
                world.AddComponent(playerEntity, playerPos);
            }

            // Restore player stats
            if (world.TryGetComponent<StatsComponent>(playerEntity, out var playerStats))
            {
                playerStats.Health = saveData.Player.Health;
                playerStats.MaxHealth = saveData.Player.MaxHealth;

                if (saveData.Player.Stats.TryGetValue("Attack", out float attack))
                    playerStats.Attack = attack;
                if (saveData.Player.Stats.TryGetValue("Defense", out float defense))
                    playerStats.Defense = defense;
                if (saveData.Player.Stats.TryGetValue("Speed", out float speed))
                    playerStats.Speed = speed;

                world.AddComponent(playerEntity, playerStats);
            }

            // Restore inventory
            if (world.TryGetComponent<InventoryComponent>(playerEntity, out var inventory))
            {
                // Clear existing inventory
                for (int i = 0; i < inventory.Items.Count; i++)
                {
                    inventory.Items[i] = null;
                }

                // Add saved items
                for (int i = 0; i < saveData.Player.Inventory.Count && i < inventory.MaxSlots; i++)
                {
                    var itemData = saveData.Player.Inventory[i];
                    var item = new Item(itemData.ItemId, itemData.ItemName, ItemType.Misc);
                    item.StackSize = itemData.StackSize;
                    inventory.Items[i] = item;
                }

                world.AddComponent(playerEntity, inventory);
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public bool SaveExists(string saveName)
        {
            string filePath = Path.Combine(_saveDirectory, $"{saveName}.json");
            return File.Exists(filePath);
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        public bool DeleteSave(string saveName)
        {
            try
            {
                string filePath = Path.Combine(_saveDirectory, $"{saveName}.json");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all available save files
        /// </summary>
        public string[] GetAvailableSaves()
        {
            if (!Directory.Exists(_saveDirectory))
                return Array.Empty<string>();

            return Directory.GetFiles(_saveDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();
        }
    }
}
