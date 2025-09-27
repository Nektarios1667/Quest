﻿using System.IO;

namespace Quest.Managers;

public class LootTableEntry
{
    public Item Item { get; }
    public int MinAmount { get; }
    public int MaxAmount { get; }
    public float Chance { get; }
    public LootTableEntry(Item item, int minAmount, int maxAmount, float chance)
    {
        Item = item;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Chance = chance;
    }
}

public interface ILootGenerator
{
    public string FileName { get; }
    public Inventory Generate(int width, int height, int seed = -1);
}

public class LootPreset : ILootGenerator
{
    public string FileName { get; }
    public static readonly LootPreset EmptyPreset = new(new Item?[0, 0], "_");
    public Item?[,] Preset { get; }
    public LootPreset(Item?[,] items, string filename)
    {
        Preset = items;
        FileName = filename;
    }
    public Inventory Generate(int width, int height, int seed = -1) => new(width, height, ArrayTools.Resize2DArray(Preset, width, height));
    public static LootPreset ReadLootPreset(string file)
    {
        // Check
        if (!file.EndsWith(".qlp"))
        {
            Logger.Error($"Failed to read preset '{file}'. Expected .qlp file.");
            return EmptyPreset;
        }
        if (!File.Exists(file))
        {
            Logger.Error($"File {file} not found.");
            return EmptyPreset;
        }

        // Read
        Item?[,] items;
        using (FileStream stream = File.Open(file, FileMode.Open))
        using (BinaryReader reader = new(stream))
        {
            // Header
            int width = reader.ReadByte();
            int height = reader.ReadByte();
            items = new Item?[width, height];
            // Ranges
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int id = (reader.ReadByte() - 1);
                    // Empty
                    if (id < 0)
                    {
                        items[x, y] = null;
                        continue;
                    }
                    // Item
                    ItemType item = (ItemType)id;
                    byte amount = reader.ReadByte();
                    items[x, y] = Item.ItemFromItemType(item!, 0);
                }
            }
        }
        return new(items, file);
    }
}

public class LootTable : ILootGenerator
{
    public string FileName { get; }
    public List<LootTableEntry> Entries { get; private set; }
    public LootTable(List<LootTableEntry> entries, string fileName)
    {
        Entries = entries;
        FileName = fileName;
    }
    public Inventory Generate(int width, int height, int seed = -1)
    {
        if (seed != -1)
            RandomManager.SetSeed(seed);

        Inventory inv = new(width, height);
        foreach (var table in Entries)
        {
            // Check full
            if (inv.IsFull()) break;
            // Chances
            if (RandomManager.RandomFloat() >= table.Chance) continue;

            // Get random empty slot
            Point dest;
            do
                dest = RandomManager.RandomPoint(Point.Zero, inv.Size);
            while (inv.GetItem(dest) != null);

            // Set item
            Item item = table.Item.ShallowCopy();
            item.Amount = Math.Clamp(RandomManager.RandomIntRange(table.MinAmount, table.MaxAmount + 1), 0, table.Item.MaxAmount);
            inv.SetSlot(dest, item);
        }
        return inv;
    }
    public void AddEntry(LootTableEntry entry)
    {
        Entries.Add(entry);
    }
    public void RemoveEntry(LootTableEntry entry)
    {
        Entries.Remove(entry);
    }

    public static LootTable ReadLootTable(string file)
    {
        // Check
        if (!file.EndsWith(".qlt"))
        {
            Logger.Error($"Failed to read preset '{file}'. Expected .qlt file.");
            return new([], "");
        }
        if (!File.Exists(file))
        {
            Logger.Error($"File {file} not found.");
            return new([], "");
        }

        // Read
        List<LootTableEntry> entries = new();
        using (FileStream stream = File.Open(file, FileMode.Open))
        using (BinaryReader reader = new(stream))
        {
            // Header
            int count = reader.ReadByte();
            // Ranges
            for (int i = 0; i < count; i++)
            {
                ItemType item = (ItemType)(reader.ReadByte() - 1);
                float chance = reader.ReadByte() / 100f;
                byte minAmount = reader.ReadByte();
                byte maxAmount = reader.ReadByte();
                entries.Add(new(Item.ItemFromItemType(item, 0), minAmount, maxAmount, chance));
            }
        }
        return new(entries, file);
    }
}

public static class LootGeneratorHelper
{
    public static ILootGenerator Read(string file)
    {
        if (file.EndsWith(".qlt"))
            return LootTable.ReadLootTable(file);
        else if (file.EndsWith(".qlp"))
            return LootPreset.ReadLootPreset(file);
        else 
            Logger.Error($"Failed to read loot generator '{file}'. Expected .qlt or .qlp file.");
        return LootPreset.EmptyPreset;
    }
}