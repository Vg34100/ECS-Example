using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.SaveLoad.Data
{
    /// <summary>
    /// Root save data structure
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string SaveName { get; set; }
        public DateTime SaveTime { get; set; }
        public int Version { get; set; }
        public PlayerSaveData Player { get; set; }
        public List<EntitySaveData> Entities { get; set; }

        public SaveData()
        {
            SaveName = "Quicksave";
            SaveTime = DateTime.Now;
            Version = 1;
            Player = new PlayerSaveData();
            Entities = new List<EntitySaveData>();
        }
    }

    /// <summary>
    /// Player-specific save data
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public Vector2 Position { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public List<ItemSaveData> Inventory { get; set; }
        public Dictionary<string, float> Stats { get; set; }

        public PlayerSaveData()
        {
            Position = Vector2.Zero;
            Health = 100f;
            MaxHealth = 100f;
            Inventory = new List<ItemSaveData>();
            Stats = new Dictionary<string, float>();
        }
    }

    /// <summary>
    /// Entity save data
    /// </summary>
    [Serializable]
    public class EntitySaveData
    {
        public string EntityType { get; set; }
        public Vector2 Position { get; set; }
        public Dictionary<string, object> Components { get; set; }

        public EntitySaveData()
        {
            EntityType = "Generic";
            Position = Vector2.Zero;
            Components = new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Item save data
    /// </summary>
    [Serializable]
    public class ItemSaveData
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int StackSize { get; set; }

        public ItemSaveData()
        {
            ItemId = string.Empty;
            ItemName = string.Empty;
            StackSize = 1;
        }

        public ItemSaveData(string id, string name, int stackSize)
        {
            ItemId = id;
            ItemName = name;
            StackSize = stackSize;
        }
    }
}
