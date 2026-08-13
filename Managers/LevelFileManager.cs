using Quest.World;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IO = System.IO;

namespace Quest.Managers;

public static class LevelFileManager
{
    public static async Task<bool> ReadWorldAsync(GameManager gameManager, string filename, bool reload = false)
    {
        return await Task.Run(() =>
        {
            return ReadWorld(gameManager, filename, reload);
        });
    }
    public  static bool ReadWorld(GameManager gameManager, string folder, bool reload = false)
    {
        string[] levelFiles = Directory.GetFiles($"GameData/Worlds/{folder}/levels", "*.qlv");

        // Progress info
        gameManager.LevelManager.TasksComplete = 0;
        gameManager.LevelManager.TotalTasks = levelFiles.Length * 11 + 1; // All levels * 11 tasks per level + reading loot files
        MenuManager.SetCurrentlyLoading("Loading world...");

        if (!Directory.Exists($"GameData/Worlds/{folder}"))
        {
            Logger.Error($"World '{folder}' does not exist.");
            return false;
        }
        FileTools.CheckDirExists($"GameData/Worlds/{folder}/loot");
        FileTools.CheckDirExists($"GameData/Worlds/{folder}/levels");

        // Read loot tables and presets
        string[] qlp = Directory.GetFiles($"GameData/Worlds/{folder}/loot", "*.qlp");
        string[] qlt = Directory.GetFiles($"GameData/Worlds/{folder}/loot", "*.qlt");
        foreach (string file in qlp.Concat(qlt).Select(f => IO.Path.GetFileName(f)))
        {
            LootGeneratorHelper.Read(folder, file);
            Logger.System($"Loaded Loot file {file}.");
        }
        gameManager.LevelManager.TasksComplete++;

        // Read levels
        foreach (string file in levelFiles)
        {
            MenuManager.SetCurrentlyLoading($"Loading {IO.Path.GetFileNameWithoutExtension(file)}...");
            ReadLevel(gameManager, $"{folder}/{IO.Path.GetFileNameWithoutExtension(file)}", reload, multiTask: true);
            gameManager.LevelManager.TasksComplete++;
        }

        return true;
    }
    public static bool ReadLevel(GameManager gameManager, string filename, bool reload = false, bool multiTask = false)
    {
        if (!multiTask)
        {
            gameManager.LevelManager.TasksComplete = 0;
            gameManager.LevelManager.TotalTasks = 12;
        }

        var sw = new Stopwatch();
        sw.Start();
        // File checks
        filename = filename.Replace('\\', '/');
        LevelPath levelPath = new(filename);
        string path = $"GameData/Worlds/{levelPath.WorldName}/levels/{levelPath.LevelName}.qlv";

        if (levelPath.IsNull()) {
            Logger.Error($"Invalid file format '{filename}.'");
            return false;
        }
        if (!File.Exists(path))
        {
            Logger.Error($"Level file '{filename}' does not exist.");
            return false;
        }

        // Read metadata
        WorldMetadata meta = WorldMetadata.Null;
        var kvDict = SaveManager.ReadKeyValueFile($"Worlds/{levelPath.WorldName}/metadata");
        meta.Author = kvDict.GetValueOrDefault("Author", defaultValue: "Unknown");
        meta.Description = kvDict.GetValueOrDefault("Description", defaultValue: "None");
        gameManager.LevelManager.TasksComplete++;

        // Check if already read
        if (!reload && gameManager.LevelManager.Levels.Any(l => l.Path == filename)) return true;

        // Make buffers
        int totalTiles = Constants.MapSize.X * Constants.MapSize.Y;
        Tile[] tilesBuffer = new Tile[totalTiles];
        BiomeType[] biomeBuffer = new BiomeType[totalTiles];
        Point spawn = new();
        Color tint = new();
        List<Loot> lootBuffer = new();
        List<NPC> npcBuffer = new();
        List<Enemy> enemyBuffer = new();
        Dictionary<ByteCoord, Decal> decalBuffer = new();
        List<QuillScript> scriptBuffer = new();

        gameManager.LevelManager.TasksComplete++;

        // Context
        using FileStream fileStream = File.OpenRead(path);
        using BufferedStream buffer = new(fileStream, 128 * 1024);
        using GZipStream gzipStream = new(buffer, CompressionMode.Decompress);
        using BinaryReader reader = new(gzipStream);

        // Metadata
        byte[] magic = reader.ReadBytes(4);
        if (Encoding.ASCII.GetString(magic) != "QLVL")
        {
            Logger.Error($"invalid file format for file '{filename}'.");
            return false;
        }
        gameManager.LevelManager.TasksComplete++;

        // Reading
        string id = "";
        try
        {
            // Read sections
            while (true)
            {
                id = reader.ReadString();
                int length = reader.ReadInt32();


                if (id == "_EOF") break;

                byte[] data = reader.ReadBytes(length);

                using MemoryStream sectionStream = new MemoryStream(data);
                using BinaryReader sectionReader = new BinaryReader(sectionStream);

                // Section types
                switch (id)
                {
                    case "LEVL": ReadLevelSection(gameManager, sectionReader, ref tint, ref spawn); break;
                    case "TILE": ReadTileSection(gameManager, sectionReader, levelPath, tilesBuffer); break;
                    case "BIOM": ReadBiomeSection(gameManager, sectionReader, filename, totalTiles, biomeBuffer); break;
                    case "NPCS": ReadNPCSection(gameManager, sectionReader, npcBuffer); break;
                    case "LOOT": ReadLootSection(gameManager, sectionReader, lootBuffer); break;
                    case "DCAL": ReadDecalSection(gameManager, sectionReader, decalBuffer); break;
                    case "ENEM": ReadEnemySection(gameManager, sectionReader, enemyBuffer); break;
                    case "QSCR": ReadScriptSection(gameManager, sectionReader, levelPath, scriptBuffer); break;
                    default: Logger.Warning($"Unknown level section '{id}'"); break; // Unknown section - ignore it
                }
            }

            // Make and add the level
            Level created = new(filename, tilesBuffer, biomeBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, enemyBuffer, [], scriptBuffer, meta, tint);
            if (reload) gameManager.LevelManager.Levels.RemoveAll(l => l.Path == filename);
            gameManager.LevelManager.Levels.Add(created);
            sw.Stop();
            Logger.System($"Successfully read level '{filename}' in {sw.ElapsedMilliseconds:F0}ms.");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to read level file '{filename}' [{id}]: {ex}");
            return false;
        }

    }
    private static void ReadLevelSection(GameManager gameManager, BinaryReader reader, ref Color tint, ref Point spawn)
    {
        // Tint
        tint = reader.ReadColor();
        gameManager.LevelManager.TasksComplete++;

        // Spawn
        spawn = reader.ReadByteCoord().ToPoint();
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadTileSection(GameManager gameManager, BinaryReader reader, LevelPath levelPath, Tile[] tilesBuffer)
    {
        // Tiles
        for (int y = 0; y < Constants.MapSize.Y; y++)
            for (int x = 0; x < Constants.MapSize.X; x++)
                tilesBuffer[x + y * Constants.MapSize.X] = ReadTile(reader, levelPath, x, y);
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadBiomeSection(GameManager gameManager, BinaryReader reader, string filename, int totalTiles, BiomeType[] biomeBuffer)
    {
        // Biomes
        int read = reader.Read(MemoryMarshal.AsBytes(biomeBuffer.AsSpan()));
        if (read != totalTiles) Logger.Error($"Failed to read biome data for level '{filename}' - expected {totalTiles}B got {read}B.");
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadNPCSection(GameManager gameManager, BinaryReader reader, List<NPC> npcBuffer)
    {
        // NPCs
        ushort npcCount = reader.ReadUInt16();
        npcBuffer.Clear();
        npcBuffer.Capacity = npcCount;

        for (int n = 0; n < npcCount; n++)
            npcBuffer.Add(reader.ReadNPC());
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadLootSection(GameManager gameManager, BinaryReader reader, List<Loot> lootBuffer)
    {
        // Loot
        ushort lootCount = reader.ReadUInt16();
        lootBuffer.Clear();
        lootBuffer.Capacity = lootCount;

        for (int n = 0; n < lootCount; n++)
            lootBuffer.Add(reader.ReadLoot(gameManager));
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadDecalSection(GameManager gameManager, BinaryReader reader, Dictionary<ByteCoord, Decal> decalBuffer)
    {
        // Decals
        ushort decalCount = reader.ReadUInt16();
        decalBuffer.Clear();

        for (int n = 0; n < decalCount; n++)
        {
            Decal decal = reader.ReadDecal();
            decalBuffer[decal.Location] = decal;
        }
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadEnemySection(GameManager gameManager, BinaryReader reader, List<Enemy> enemyBuffer)
    {
        // Enemies
        ushort enemyCount = reader.ReadUInt16();
        enemyBuffer.Clear();
        enemyBuffer.Capacity = enemyCount;

        for (int e = 0; e < enemyCount; e++)
            enemyBuffer.Add(reader.ReadEnemy());
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadScriptSection(GameManager gameManager, BinaryReader reader, LevelPath levelPath, List<QuillScript> scriptBuffer)
    {
        // Scripts
        Directory.CreateDirectory($"GameData/Worlds/{levelPath.WorldName}/scripts");

        byte scriptCount = reader.ReadByte();
        scriptBuffer.Clear();
        scriptBuffer.Capacity = scriptCount;


        for (int s = 0; s < scriptCount; s++)
        {
            string name = reader.ReadString();
            string scriptPath = $"GameData/Worlds/{levelPath.WorldName}/scripts/{name}";
            string code = File.Exists(scriptPath) ? File.ReadAllText(scriptPath) : "// NUL";
            scriptBuffer.Add(new QuillScript(name, code));
        }

        gameManager.LevelManager.TasksComplete++;
    }
    private static Tile ReadTile(BinaryReader reader, LevelPath levelPath, int x, int y)
    {
        Point loc = new(x, y);

        // Type
        if (!Enum.TryParse(reader.ReadByte().ToString(), out TileTypeID type))
        {
            Logger.Error($"Invalid tile type at {x}, {y} in level file.");
            return new Sky(loc);
        }

        // Make generic tile
        Tile tile = Tile.TileFromId(type, loc, levelPath.LevelName);

        // Extra data
        if (tile is IHasLevelData data)
            data.ReadLevelData(reader, levelPath);

        return tile;
    }
}
