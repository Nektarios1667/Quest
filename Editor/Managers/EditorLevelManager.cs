using Quest.Editor.Generator;
using Quest.World;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static Quest.Editor.PopupFactory;

namespace Quest.Editor.Managers;

public class EditorLevelManager
{
    public GameManager GameManager { get; private set; }
    public LevelManager LevelManager => GameManager.LevelManager;
    public LevelGenerator LevelGenerator { get; private set; }
    public EditorLevelManager(GameManager gameManager, LevelGenerator levelGenerator)
    {
        GameManager = gameManager;
        LevelGenerator = levelGenerator;
    }
    public void ResaveLevel(LevelPath levelPath)
    {
        OpenLevel(levelPath.Path);
        SaveLevel(levelPath);
        Logger.Log($"Resaved level '{levelPath.Path}'");
    }
    public void ResaveWorld(string world)
    {
        // Get files
        string prefix = Constants.DEVMODE ? "../../../" : "";
        string[] levels = [.. Directory.GetFiles($"{prefix}GameData/Worlds/{world}/levels").Where(f => f.EndsWith(".qlv"))];

        // Resave all
        foreach (var level in levels)
        {
            string formattedLevel = $"{world}/{System.IO.Path.GetFileNameWithoutExtension(level)}";
            ResaveLevel(new(formattedLevel));
        }
    }
    public void SaveLevelDialog()
    {
        // Winforms
        if (LevelManager.Level.LevelPath.IsNull())
            SaveLevelAs();
        else
            SaveLevel(LevelManager.Level.LevelPath);
    }
    public void SaveLevelAs()
    {
        // Winforms
        var (success, values) = ShowInputForm("Save As", [new("World", null), new("Level", null)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to save level.");
            return;
        }
        LevelPath path = new(values[0], values[1]);
        SaveLevel(path);
        LevelManager.Level.Rename(path);
    }
    public void SaveLevel(LevelPath path)
    {
        string prefix = Constants.DEVMODE ? "../../../" : "";

        Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/levels");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/loot");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/saves");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/scripts");
        using FileStream fileStream = File.Create($"{prefix}GameData/Worlds/{path.WorldName}/levels/{path.LevelName}.qlv");
        using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(gzipStream);

        // Metadata
        var flags = LevelFeatures.Biomes | LevelFeatures.QuillScripts | LevelFeatures.CustomSize;
        writer.Write(Encoding.UTF8.GetBytes("QLVL")); // Magic number
        writer.Write((ushort)flags); // Flags

        // Write tint
        writer.Write(LevelManager.Level.Tint);

        // Write spawn
        writer.Write(new ByteCoord(LevelManager.Level.Spawn));

        // Size
        if (flags.HasFlag(LevelFeatures.CustomSize))
            writer.Write(LevelManager.Level.Size);

        // Tiles
        for (int i = 0; i < LevelManager.MapSize.X * LevelManager.MapSize.Y; i++)
        {
            Tile tile = LevelManager.Level.Tiles[i];
            // Write tile data
            writer.Write((byte)tile.Type.ID);
            // Extra properties
            if (tile is Stairs stairs)
            {
                // Write destination
                writer.Write(stairs.DestLevel.LevelName);
                writer.Write(stairs.Dest);
            }
            else if (tile is Door door)
            {
                // Write door key
                StateManager.WriteItemData(writer, door.Key);
                writer.Write(door.ConsumeKey);
            }
            else if (tile is Chest chest)
            {
                writer.Write(chest.LootGenerator);
                StateManager.WriteItemData(writer, chest.Key);
                writer.Write(chest.ConsumeKey);
            }
            else if (tile is Lamp lamp)
                writer.Write(lamp.LightRadius);
            else if (tile is DisplayCase displayCase)
                StateManager.WriteItemData(writer, displayCase.Container.Items[0]);
        }

        // Biome
        if (flags.HasFlag(LevelFeatures.Biomes))
            for (int i = 0; i < LevelManager.MapSize.X * LevelManager.MapSize.Y; i++)
                writer.Write((byte)LevelManager.Level.Biome[i]);

        // NPCs
        writer.Write((ushort)Math.Min(LevelManager.Level.NPCs.Count, ushort.MaxValue));
        NPC[] npcs = [.. LevelManager.Level.NPCs.Values];
        for (int n = 0; n < Math.Min(npcs.Length, ushort.MaxValue); n++)
            writer.Write(npcs[n]);

        // Floor loot
        writer.Write((ushort)Math.Min(LevelManager.Level.Loot.Count, ushort.MaxValue));
        for (int n = 0; n < Math.Min(LevelManager.Level.Loot.Count, ushort.MaxValue); n++)
            writer.Write(LevelManager.Level.Loot[n]);

        // Decals
        writer.Write((ushort)Math.Min(LevelManager.Level.Decals.Count, ushort.MaxValue));
        Decal[] decals = [.. LevelManager.Level.Decals.Values];
        for (int n = 0; n < Math.Min(decals.Length, ushort.MaxValue); n++)
            writer.Write(decals[n]);

        // Enemies
        writer.Write((ushort)Math.Min(LevelManager.Level.Enemies.Count, ushort.MaxValue));
        Enemy[] enemies = [.. LevelManager.Level.Enemies.Values];
        for (int n = 0; n < Math.Min(enemies.Length, ushort.MaxValue); n++)
            writer.Write(enemies[n]);

        // Scripts
        if (flags.HasFlag(LevelFeatures.QuillScripts))
        {
            writer.Write((byte)LevelManager.Level.Scripts.Count);
            for (int s = 0; s < LevelManager.Level.Scripts.Count; s++)
            {
                QuillScript script = LevelManager.Level.Scripts[s];
                writer.Write(script.Name);
            }
        }

        // Log
        Logger.Log($"Exported level to '{path}.qlv'.");
    }
    public void GenerateLevel()
    {
        // Winforms
        var (success, values) = ShowInputForm("Generate Level", [
                new("Width", IsPositiveInteger),
                new("Height", IsPositiveInteger),
                new("Seed", IsInteger),
                new("Terrain", null, [.. LevelGenerator.Terrains.Keys]),
                new("Structure Attempts", IsPositiveIntegerOrZero),
            ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Level generation failed.");
            return;
        }

        // Generate
        LevelGenerator.Size = new(int.Parse(values[0]), int.Parse(values[1]));
        LevelGenerator.Seed = int.Parse(values[2]);
        LevelGenerator.Terrain = LevelGenerator.Terrains.GetValueOrDefault(values[3], LevelGenerator.Terrain);
        Tile[] tiles = LevelGenerator.GenerateLevel(LevelGenerator.Size, int.Parse(values[4]));

        Level current = LevelManager.Level;
        Level level = new(current.LevelPath, LevelGenerator.Size, tiles, new BiomeType[LevelGenerator.Size.X * LevelGenerator.Size.Y], current.Spawn, [.. current.NPCs.Values], current.Loot, current.Decals, [.. current.Enemies.Values], current.Projectiles, [], current.Tint);

        LevelManager.LoadLevelObject(GameManager, level);
    }
    public void OpenLevelDialog()
    {
        // Winforms
        var (success, values) = ShowInputForm("Open Level", [new("Level", null)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to open file.");
            return;
        }
        OpenLevel(values[0]);
    }
    public void OpenLevel(string filename)
    {
        // Open
        if (!filename.Contains('/') && !filename.Contains('\\'))
        {
            Logger.Error("Invalid level name. Use format 'WorldName/LevelName'.");
            return;
        }
        // Autocomplete 'world/' to 'world/world'
        if (filename.EndsWith('/'))
            filename = $"{filename[..^1]}/{filename[..^1]}";

        GameManager.LevelManager.ReadLevel(GameManager, filename, reload: true);
        GameManager.LevelManager.LoadLevel(GameManager, filename);

        Logger.Log($"Opened level '{filename}'.");
    }
    public void NewLevel()
    {
        // Check save to continue
        if (!WarnSave()) return;

        // Winforms
        var (success, values) = ShowInputForm("New Level", [
            new("World", null),
            new("Level", null),
            new("Width", IsPositiveInteger),
            new("Height", IsPositiveInteger)
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to open file.");
            return;
        }

        Level level = LevelManager.GetEmptyLevel(values[0], values[1], int.Parse(values[2]), int.Parse(values[3]));
        LevelManager.LoadLevelObject(GameManager, level);
    }
    public bool WarnSave()
    {
        var result = System.Windows.Forms.MessageBox.Show(
            "Do you want to save level before closing?",
            "Unsaved Changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning
        );
        switch (result)
        {
            case DialogResult.Yes:
                SaveLevelDialog();
                return true;
            case DialogResult.No:
                return true;
            case DialogResult.Cancel:
                return false;
            default:
                return false;
        }
    }
}
