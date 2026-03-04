namespace ECS_Base.Mechanics.Inventory.Data
{
    /// <summary>
    /// Represents an item that can be stored in inventory
    /// </summary>
    public struct Item
    {
        /// <summary>Unique item identifier</summary>
        public string Id;

        /// <summary>Display name</summary>
        public string Name;

        /// <summary>Item type/category</summary>
        public ItemType Type;

        /// <summary>Current stack size</summary>
        public int StackSize;

        /// <summary>Maximum stack size</summary>
        public int MaxStackSize;

        /// <summary>Can this item be stacked?</summary>
        public readonly bool IsStackable => MaxStackSize > 1;

        /// <summary>Is this stack full?</summary>
        public readonly bool IsStackFull => StackSize >= MaxStackSize;

        /// <summary>Icon sprite ID (for rendering)</summary>
        public string IconId;

        /// <summary>Item value/price</summary>
        public int Value;

        public Item(string id, string name, ItemType type, int maxStackSize = 1)
        {
            Id = id;
            Name = name;
            Type = type;
            MaxStackSize = maxStackSize;
            StackSize = 1;
            IconId = string.Empty;
            Value = 0;
        }

        /// <summary>
        /// Try to add to this stack. Returns amount actually added.
        /// </summary>
        public int AddToStack(int amount)
        {
            int space = MaxStackSize - StackSize;
            int toAdd = System.Math.Min(amount, space);
            StackSize += toAdd;
            return toAdd;
        }

        /// <summary>
        /// Try to remove from this stack. Returns amount actually removed.
        /// </summary>
        public int RemoveFromStack(int amount)
        {
            int toRemove = System.Math.Min(amount, StackSize);
            StackSize -= toRemove;
            return toRemove;
        }
    }

    public enum ItemType
    {
        Consumable,
        Weapon,
        Armor,
        Material,
        Quest,
        Misc
    }
}
