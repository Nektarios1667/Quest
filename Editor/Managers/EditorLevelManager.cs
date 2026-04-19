using Quest.Editor.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
        var flags = LevelFeatures.Biomes | LevelFeatures.QuillScripts;
        writer.Write(Encoding.UTF8.GetBytes("QLVL")); // Magic number
        writer.Write((ushort)flags); // Flags

        // Write tint
        writer.Write(LevelManager.Level.Tint);

        // Write spawn
        writer.Write(new ByteCoord(LevelManager.Level.Spawn));

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            Tile tile = LevelManager.Level.Tiles[i];
            // Write tile data
            writer.Write((byte)tile.Type.ID);
            // Extra properties
            if (tile is Stairs stairs)
            {
                // Write destination
                writer.Write(stairs.DestLevel.Split('/', '\\')[^1]);
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
            for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
                writer.Write((byte)(int)LevelManager.Level.Biome[i]);

        // NPCs
        writer.Write((ushort)Math.Min(LevelManager.Level.NPCs.Count, ushort.MaxValue));
        for (int n = 0; n < Math.Min(LevelManager.Level.NPCs.Count, ushort.MaxValue); n++)
            writer.Write(LevelManager.Level.NPCs[n]);

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
        var (success, values) = ShowInputForm("Generate Level", [new("Seed", IsInteger), new("Terrain", null, [.. LevelGenerator.Terrains.Keys]), new("Structure Attempts", IsPositiveIntegerOrZero)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Level generation failed.");
            return;
        }

        // Generate
        LevelGenerator.Seed = int.Parse(values[0]);
        LevelGenerator.Terrain = LevelGenerator.Terrains.GetValueOrDefault(values[1], LevelGenerator.Terrain);
        Tile[] tiles = LevelGenerator.GenerateLevel(Constants.MapSize, int.Parse(values[2]));

        Level current = LevelManager.Level;
        Level level = new(current.Path, tiles, [], current.Spawn, current.NPCs, current.Loot, current.Decals, current.Enemies, current.Projectiles, [], current.Tint);

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
        GameManager.LevelManager.ReadLevel(GameManager, filename, reload: true);
        GameManager.LevelManager.LoadLevel(GameManager, filename);
        Logger.Log($"Opened level '{filename}'.");
    }
    public void NewLevel()
    {
        // Check save to continue
        if (!WarnSave()) return;

        // Make blank level
        Tile[] grassTiles = new Tile[256 * 256];
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) grassTiles[t] = new Grass(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
        LevelManager.LoadLevelObject(GameManager, new("NUL/NUL", grassTiles, [], new(128, 128), [], [], [], [], [], []));
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
