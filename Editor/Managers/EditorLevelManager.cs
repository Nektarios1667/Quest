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
            SaveLevel(LevelManager.Level.LevelPath, LevelManager.Level.Metadata);
    }
    public void SaveLevelAs()
    {
        // Winforms
        var (success, values) = ShowInputForm("Save As", [
            new("World", null),
            new("Level", null),
            new("Author", null),
            new("Description", null),
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to save level.");
            return;
        }
        LevelPath path = new(values[0], values[1]);
        SaveLevel(path, new WorldMetadata(values[2], values[3]));
        LevelManager.Level.Rename(path);
    }
    public void SaveLevel(LevelPath path, WorldMetadata? metadata = null)
    {
        metadata = metadata ?? LevelManager.Level.Metadata;

        // Create folders
        Directory.CreateDirectory($"GameData/Worlds/{path.WorldName}");
        Directory.CreateDirectory($"GameData/Worlds/{path.WorldName}/levels");
        Directory.CreateDirectory($"GameData/Worlds/{path.WorldName}/loot");
        Directory.CreateDirectory($"GameData/Worlds/{path.WorldName}/saves");
        Directory.CreateDirectory($"GameData/Worlds/{path.WorldName}/scripts");
        if (Constants.DEVMODE)
        {
            const string prefix = "../../../";
            Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}");
            Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/levels");
            Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/loot");
            Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/saves");
            Directory.CreateDirectory($"{prefix}GameData/Worlds/{path.WorldName}/scripts");
        }


        // Write metadata
        StateManager.WriteKeyValueFile($"Worlds/{path.WorldName}/metadata", metadata.ToDict());

        // Write metadata
        StateManager.WriteKeyValueFile($"Worlds/{path.WorldName}/metadata", metadata.ToDict());

        // Context
        using FileStream fileStream = File.Create($"GameData/Worlds/{path.WorldName}/levels/{path.LevelName}.qlv");
        using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(gzipStream);

        // Write magic
        byte[] magic = Encoding.ASCII.GetBytes("QLVL");
        writer.Write(magic);

        // Write sections
        WriteSection(writer, "LEVL", WriteLevelSection);
        WriteSection(writer, "TILE", WriteTileSection);
        WriteSection(writer, "BIOM", WriteBiomeSection);
        WriteSection(writer, "NPCS", WriteNPCSection);
        WriteSection(writer, "LOOT", WriteLootSection);
        WriteSection(writer, "DCAL", WriteDecalSection);
        WriteSection(writer, "ENEM", WriteEnemySection);
        WriteSection(writer, "QSCR", WriteScriptSection);
        WriteSection(writer, "_EOF", WriteEOFSection);

        writer.Dispose();
        if (Constants.DEVMODE)
            File.Copy($"GameData/Worlds/{path.WorldName}/levels/{path.LevelName}.qlv", $"../../../GameData/Worlds/{path.WorldName}/levels/{path.LevelName}.qlv", true);

        // Log
        Logger.Log($"Exported level to '{path}.qlv'.");
    }
    private static void WriteSection(BinaryWriter writer, string id, Action<BinaryWriter> writeData)
    {
        using MemoryStream tempStream = new MemoryStream();
        using BinaryWriter tempWriter = new BinaryWriter(tempStream);

        // Write the section normally
        writeData(tempWriter);

        tempWriter.Flush();

        // Get byte count
        byte[] data = tempStream.ToArray();

        // Write 4 char section header
        writer.Write(id);
        writer.Write(data.Length);

        // Write section data
        writer.Write(data);
    }
    private void WriteLevelSection(BinaryWriter writer)
    {
        // Write tint
        writer.Write(LevelManager.Level.Tint);

        // Write spawn
        writer.Write(new ByteCoord(LevelManager.Level.Spawn));
    }
    private void WriteTileSection(BinaryWriter writer)
    {
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
                writer.Write(stairs.DestLevel.LevelName ?? "");
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
    }
    private void WriteBiomeSection(BinaryWriter writer)
    {
        // Biome
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
            writer.Write((byte)(int)LevelManager.Level.Biome[i]);
    }
    private void WriteNPCSection(BinaryWriter writer)
    {
        // NPCs
        writer.Write((ushort)Math.Min(LevelManager.Level.NPCs.Count, ushort.MaxValue));
        NPC[] npcs = [.. LevelManager.Level.NPCs.Values];
        for (int n = 0; n < Math.Min(npcs.Length, ushort.MaxValue); n++)
            writer.Write(npcs[n]);
    }
    private void WriteLootSection(BinaryWriter writer)
    {
        // Floor loot
        writer.Write((ushort)Math.Min(LevelManager.Level.Loot.Count, ushort.MaxValue));
        for (int n = 0; n < Math.Min(LevelManager.Level.Loot.Count, ushort.MaxValue); n++)
            writer.Write(LevelManager.Level.Loot[n]);
    }
    private void WriteDecalSection(BinaryWriter writer)
    {
        // Decals
        writer.Write((ushort)Math.Min(LevelManager.Level.Decals.Count, ushort.MaxValue));
        Decal[] decals = [.. LevelManager.Level.Decals.Values];
        for (int n = 0; n < Math.Min(decals.Length, ushort.MaxValue); n++)
            writer.Write(decals[n]);
    }
    private void WriteEnemySection(BinaryWriter writer)
    {
        // Enemies
        writer.Write((ushort)Math.Min(LevelManager.Level.Enemies.Count, ushort.MaxValue));
        Enemy[] enemies = [.. LevelManager.Level.Enemies.Values];
        for (int n = 0; n < Math.Min(enemies.Length, ushort.MaxValue); n++)
            writer.Write(enemies[n]);
    }
    public void WriteScriptSection(BinaryWriter writer)
    {
        // Scripts
        writer.Write((byte)LevelManager.Level.Scripts.Count);
        for (int s = 0; s < LevelManager.Level.Scripts.Count; s++)
        {
            QuillScript script = LevelManager.Level.Scripts[s];
            writer.Write(script.Name);
        }
    }
    public void WriteEOFSection(BinaryWriter writer) { }
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
        Level level = new(current.Path, tiles, [], current.Spawn, [.. current.NPCs.Values], current.Loot, current.Decals, [.. current.Enemies.Values], current.Projectiles, [], current.Metadata, current.Tint);

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

        // Make blank level
        Tile[] grassTiles = new Tile[256 * 256];
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) grassTiles[t] = new Grass(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
        LevelManager.LoadLevelObject(GameManager, new("NUL/NUL", grassTiles, [], new(128, 128), [], [], [], [], [], [], WorldMetadata.Null));
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
