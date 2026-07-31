using Quest.World;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quest.Managers;

[Flags]
public enum LevelFeatures : ushort
{
    None = 0,
    Biomes = 1,
    QuillScripts = 2,
}
public enum GameState
{
    MainMenu,
    Settings,
    Credits,
    LevelSelect,
    Loading,
    Game,
    Editor,
}
public enum OverlayState
{
    None,
    Container,
    Pause,
    Typing,
    Finished,
    Death,
}
public enum Mood
{
    Calm,
    Dark,
    Epic,
}
public static class StateManager
{
    // Loading
    public static event Action<float>? LoadingProgressed;
    public static int TotalTasks { get; private set; }
    private static int _tasksComplete;
    public static int TasksComplete
    {
        get { return _tasksComplete; }
        set { _tasksComplete = value; LoadingProgressed?.Invoke(LoadingProgress); }
    }
    public static float LoadingProgress => TotalTasks <= 0 ? 0 : (float)TasksComplete / TotalTasks;
    // States
    public static Action<GameState>? OnStateChanged;
    public static Action<OverlayState>? OnOverlayStateChanged;
    public static bool IsPlayingState => State == GameState.Game || State == GameState.Editor;
    private static GameState _state = GameState.MainMenu;
    public static GameState State
    {
        get => _state;
        set
        {
            PreviousState = _state;
            OnStateChanged?.Invoke(value);
            _state = value;
        }
    }
    private static GameState PreviousState { get; set; } = GameState.MainMenu;
    private static OverlayState _overlaystate = OverlayState.None;
    public static OverlayState OverlayState
    {
        get => _overlaystate;
        set
        {
            PreviousOverlayState = _overlaystate;
            OnOverlayStateChanged?.Invoke(value);
            _overlaystate = value;
        }
    }
    private static OverlayState PreviousOverlayState { get; set; } = OverlayState.None;
    public static Mood Mood { get; set; } = Mood.Calm;
    public static LevelPath CurrentSave { get; set; } = new();
    // Save State changes
    private static readonly Dictionary<string, HashSet<ushort>> openedDoors = [];
    private static readonly Dictionary<string, HashSet<Chest>> chests = [];
    private static readonly Dictionary<string, HashSet<IContainer>> containers = [];
    public static void RevertGameState()
    {
        State = PreviousState;
    }
    public static void RevertOverlayState()
    {
        OverlayState = PreviousOverlayState;
    }
    public static void SaveDoorOpened(ushort idx, string level)
    {
        if (openedDoors.TryGetValue(level, out var levelDoors))
            levelDoors.Add(idx);
        else
            openedDoors[level] = [idx];
    }
    public static void SaveChestGenerator(Chest chest, string level)
    {
        if (chests.TryGetValue(level, out var levelChests))
            levelChests.Add(chest);
        else
            chests[level] = [chest];
    }
    public static void SaveContainer(IContainer container, string level)
    {
        // Don't allow Chest even though it is IContainer
        if (container is Chest)
        {
            Logger.Warning("Chest should not be saved as IContainer as it has seperate logic - use SaveChestGenerator instead");
            return;
        }

        // Add
        if (containers.TryGetValue(level, out var levelContainers))
            levelContainers.Add(container);
        else
            containers[level] = [container];
    }
    public static async void SaveGameStateAsync(GameManager gameManager, PlayerManager playerMaanger)
    {
        await Task.Run(() =>
        {
            SaveGameState(gameManager, playerMaanger);
        });
    }
    public static void SaveGameState(GameManager gameManager, PlayerManager playerManager)
    {
        // Continue save
        WriteKeyValueFile("Persistent/continue", new() { { "save", CurrentSave.ToString() } });
        string worldName = gameManager.LevelManager.Level.WorldName;
        byte[] data;

        // All of the levels with extra data
        string[] levels = new[] {
                chests.Keys,
                openedDoors.Keys,
                gameManager.LevelManager.Levels.Where(l => l.WorldName == worldName &&
                (l.Loot.Count > 0 || l.Enemies.Count > 0 || l.Projectiles.Count > 0 || l.NPCs.Count > 0))
            .Select(l => l.LevelName),
            }.SelectMany(x => x).Distinct().Take(255).ToArray();

        // Progress
        TasksComplete = 0;
        TotalTasks = 5 + levels.Length * 7; // Saving level data has 7 tasks each

        // Context
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write magic
            byte[] magic = Encoding.ASCII.GetBytes("QSAV");
            writer.Write(magic);

            // Write sections
            WriteSection(writer, "TABL", WriteTableSection, gameManager, playerManager);
            WriteSection(writer, "WHTR", WriteWeatherSection, gameManager, playerManager);
            WriteSection(writer, "CAMR", WriteCameraSection, gameManager, playerManager);
            WriteSection(writer, "PLYR", WritePlayerSection, gameManager, playerManager);
            WriteSection(writer, "LEVL", WriteLevelsSection, gameManager, playerManager);
            WriteSection(writer, "INVT", WriteInventorySection, gameManager, playerManager);
            WriteSection(writer, "EFFX", WriteEffectsSection, gameManager, playerManager);
            WriteSection(writer, "_EOF", WriteEOFSection, gameManager, playerManager);

            writer.Flush();
            data = ms.ToArray();
        }
        Logger.System("Saved game state.");

        // Write
        using (var fs = new FileStream($"GameData/Worlds/{CurrentSave.WorldName}/saves/{CurrentSave.LevelName}.qsv", FileMode.Create, FileAccess.Write))
            fs.Write(data, 0, data.Length);
        if (Constants.DEVMODE)
            File.Copy($"GameData/Worlds/{CurrentSave.WorldName}/saves/{CurrentSave.LevelName}.qsv", $"../../../GameData/Worlds/{CurrentSave.WorldName}/saves/{CurrentSave.LevelName}.qsv", true);

        gameManager.OverlayManager.Notification($"Game Saved", Color.Cyan);
        Logger.System($"Saved game state to '{CurrentSave.LevelName}.qsv'.");
    }
    #region WriteSections
    public static void WriteSection(BinaryWriter writer, string id, Action<BinaryWriter, GameManager, PlayerManager> writeData, GameManager gameManager, PlayerManager playerManager)
    {
        using MemoryStream tempStream = new MemoryStream();
        using BinaryWriter tempWriter = new BinaryWriter(tempStream);

        // Write the section normally
        writeData(tempWriter, gameManager, playerManager);

        tempWriter.Flush();

        // Get byte count
        byte[] data = tempStream.ToArray();

        // Write 4 char section header
        writer.Write(id);
        writer.Write(data.Length);

        // Write section data
        writer.Write(data);
    }
    private static void WriteTableSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write level uid table
        writer.Write((ushort)gameManager.LevelManager.Levels.Count); // ushort
        foreach (Level level in gameManager.LevelManager.Levels.OrderBy(l => l.UID))
        {
            writer.Write(level.UID); // ushort
            writer.Write(level.LevelName); // string
        }
    }
    private static void WriteWeatherSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write WHTR data
        writer.Write(gameManager.LevelManager.Level.Path);
        writer.Write(gameManager.DayTime);
        writer.Write(GameManager.GameTime);
        writer.Write(gameManager.WeatherManager.WeatherSeed);
        writer.Write(gameManager.WeatherManager.LastWeather);

        TasksComplete++;
    }
    private static void WriteCameraSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write CAMR data
        writer.Write(CameraManager.CameraDest.X);
        writer.Write(CameraManager.CameraDest.Y);

        TasksComplete++;
    }
    private static void WritePlayerSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write PLYR data
        writer.Write((byte)playerManager.Health);
        writer.Write((byte)playerManager.MaxHealth);

        TasksComplete++;
    }
    private static void WriteLevelsSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        string worldName = gameManager.LevelManager.Level.WorldName;

        string[] levels = new[]
        {
        chests.Keys,
        openedDoors.Keys,
        gameManager.LevelManager.Levels
            .Where(l => l.WorldName == worldName &&
                (l.Loot.Count > 0 ||
                 l.Enemies.Count > 0 ||
                 l.Projectiles.Count > 0 ||
                 l.NPCs.Count > 0))
            .Select(l => l.LevelName)

    }.SelectMany(x => x)
         .Distinct()
         .Take(255)
         .ToArray();


        // Write LEVL data
        writer.Write((byte)levels.Length);
        foreach (string level in levels)
        {
            writer.Write(level);
            Level levelObj = gameManager.LevelManager.GetLevel($"{worldName}/{level}");

            // Loot
            writer.Write((ushort)levelObj.Loot.Count);
            foreach (var loot in levelObj.Loot)
            {
                writer.Write((byte)(loot.Item.Type.TypeID + 1));
                writer.Write(loot.Item.Amount);
                writer.Write((ushort)loot.Position.X);
                writer.Write((ushort)loot.Position.Y);
            }
            TasksComplete++;


            // Doors
            if (openedDoors.TryGetValue(level, out var levelDoors))
            {
                writer.Write((ushort)levelDoors.Count);

                foreach (ushort door in levelDoors)
                    writer.Write(door);
            }
            else
                writer.Write((ushort)0);
            TasksComplete++;


            // Chests
            if (chests.TryGetValue(level, out var levelChests))
            {
                writer.Write((ushort)levelChests.Count);
                foreach (Chest chest in levelChests)
                    WriteChestData(writer, chest);
            }
            else
                writer.Write((ushort)0);
            TasksComplete++;


            // Containers
            if (containers.TryGetValue(level, out var levelContainers))
            {
                writer.Write((ushort)levelContainers.Count);
                foreach (IContainer cont in levelContainers)
                    WriteContainerData(writer, cont);
            }
            else
                writer.Write((ushort)0);
            TasksComplete++;

            // Enemies
            writer.Write((ushort)levelObj.Enemies.Count);
            foreach (var enemy in levelObj.Enemies.Values)
                WriteEnemyData(writer, enemy);
            TasksComplete++;

            // Projectiles
            writer.Write((ushort)levelObj.Projectiles.Count);
            foreach (var proj in levelObj.Projectiles)
                WriteProjectileData(writer, proj);
            TasksComplete++;

            // NPCs
            writer.Write((ushort)levelObj.NPCs.Count);
            foreach (var npc in levelObj.NPCs.Values)
            {
                writer.Write(npc.UID);
                writer.Write((byte)npc.ShopOptions.Count);

                foreach (var item in npc.ShopOptions)
                    writer.Write(item.Stock);
            }
            TasksComplete++;
        }
    }
    private static void WriteInventorySection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write INVT data
        var inventory = playerManager.Inventory;
        writer.Write((byte)inventory.Items.Length);

        for (int i = 0; i < inventory.Items.Length; i++)
            WriteItemData(writer, inventory.Items[i]);

        TasksComplete++;
    }
    private static void WriteEffectsSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write EFFX
        byte effectsCount = (byte)Math.Clamp(playerManager.StatusManager.GetStatusEffectsCount(), 0, 255);
        writer.Write(effectsCount);

        foreach (var kv in playerManager.StatusManager.GetStatusEffects().Take(effectsCount))
        {
            writer.Write((byte)kv.Key); // effect type - byte
            writer.Write(kv.Value);     // effect timer - float
        }

        TasksComplete++;
    }
    private static void WriteEOFSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager) { }
    #endregion
    public static async Task<bool> ReadGameState(GameManager gameManager, PlayerManager playerManager, string save)
    {
        LevelPath levelPath = new(save);
        string file = $"GameData/Worlds/{levelPath.WorldName}/saves/{levelPath.LevelName}.qsv";
        if (!File.Exists(file))
        {
            Logger.Error($"Save file '{file}' does not exist.");
            return false;
        }
        CurrentSave = levelPath;
        WriteKeyValueFile("Persistent/continue", new() { { "save", save } });
        await LevelFileManager.ReadWorldAsync(gameManager, levelPath.WorldName, true);

        gameManager.LevelManager.TasksComplete = 0;
        gameManager.LevelManager.TotalTasks = 6;
        MenuManager.SetCurrentlyLoading("Loading save file...");

        // Level table - uid <--> levelName
        Dictionary<ushort, string> levelTable = new Dictionary<ushort, string>();

        // Read sections
        string id;
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            // Magic
            byte[] magic = reader.ReadBytes(4);
            if (Encoding.ASCII.GetString(magic) != "QSAV")
            {
                Logger.Error($"invalid file format for file '{save}'.");
                return false;
            }

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
                    case "TABL": ReadTableSection(gameManager, sectionReader, levelTable); break;
                    case "WHTR": ReadWeatherSection(gameManager, sectionReader); break;
                    case "CAMR": ReadCameraSection(gameManager, sectionReader); break;
                    case "PLYR": ReadPlayerSection(gameManager, playerManager, sectionReader); break;
                    case "LEVL": ReadLevelSection(gameManager, playerManager, levelPath, sectionReader); break;
                    case "INVT": ReadInventorySection(gameManager, playerManager, sectionReader); break;
                    case "EFFX": ReadEffectsSection(gameManager, playerManager, sectionReader); break;
                    default: Logger.Warning($"Unknown level section '{id}'"); break; // Unknown section - ignore it
                }
            }
        }

        gameManager.OverlayManager.Notification($"Save Loaded", Color.Cyan);
        Logger.System("Loaded game state from save.qsv.");
        return true;
    }
    #region ReadSections
    public static void ReadTableSection(GameManager gameManager, BinaryReader reader, Dictionary<ushort, string> levelTable)
    {
        ushort tableLength = reader.ReadUInt16();
        for (int t = 0; t < tableLength; t++)
            levelTable[reader.ReadUInt16()] = reader.ReadString();
    }
    public static void ReadWeatherSection(GameManager gameManager, BinaryReader reader)
    {
        // Read weather data
        string level = reader.ReadString();
        gameManager.LevelManager.LoadLevel(gameManager, level);

        gameManager.DayTime = reader.ReadSingle();
        GameManager.GameTime = reader.ReadSingle();
        int weatherSeed = reader.ReadInt32();
        float lastWeather = reader.ReadSingle();
        gameManager.WeatherManager.SetWeatherPersistent(seed: weatherSeed, lastWeatherTime: lastWeather, lastTimeValue: GameManager.GameTime);
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadCameraSection(GameManager gameManager, BinaryReader reader)
    {
        // Read CameraManager data
        CameraManager.CameraDest = new(reader.ReadSingle(), reader.ReadSingle());
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(0); // In bounds check
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadPlayerSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader)
    {
        playerManager.Health = reader.ReadByte();
        playerManager.MaxHealth = reader.ReadByte();
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadLevelSection(GameManager gameManager, PlayerManager playerManager, LevelPath levelPath, BinaryReader reader)
    {
        // Levels
        byte levelCount = reader.ReadByte();
        for (int lc = 0; lc < levelCount; lc++)
        {
            string lvl = $"{levelPath.WorldName}/{reader.ReadString()}";
            Level current = gameManager.LevelManager.GetLevel(lvl);
            // Loot
            ushort lootCount = reader.ReadUInt16();
            for (int l = 0; l < lootCount; l++)
            {
                byte typeID = (byte)(reader.ReadByte() - 1);
                byte amount = reader.ReadByte();
                Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
                current.Loot.Add(new Loot(new(ItemTypes.All[typeID], amount), location, 0f));
            }

            // Doors
            ushort doorsCount = reader.ReadUInt16();
            for (int d = 0; d < doorsCount; d++)
                if (current.Tiles[reader.ReadUInt16()] is Door door)
                    //Console.WriteLine();
                    door.Open(gameManager);

            // Chests
            ushort chestCount = reader.ReadUInt16();
            for (int c = 0; c < chestCount; c++)
                ReadChestData(reader, current, levelPath);

            // Containers
            ushort containerCount = reader.ReadUInt16();
            for (int o = 0; o < containerCount; o++)
                ReadContainerData(reader, current);

            // Enemies
            ushort enemyCount = reader.ReadUInt16();
            for (int e = 0; e < enemyCount; e++)
                ReadEnemyData(reader, current);

            // Projectiles
            ushort projectileCount = reader.ReadUInt16();
            for (int p = 0; p < projectileCount; p++)
                ReadProjectileData(gameManager, playerManager, reader, current);

            // NPCs
            ushort npcCount = reader.ReadUInt16();
            for (int n = 0; n < npcCount; n++)
            {
                ushort uid = reader.ReadUInt16();
                // Read stock amounts
                if (current.NPCs.TryGetValue(uid, out var npc))
                {
                    byte shopCount = reader.ReadByte();
                    for (int s = 0; s < shopCount; s++)
                    {
                        byte stock = reader.ReadByte();
                        npc.ShopOptions[s].Stock = stock;
                    }
                }
                // Read and discard stock amounts if NPC not found
                else
                {
                    byte shopCount = reader.ReadByte();
                    reader.ReadBytes(shopCount);
                    Logger.Error($"NPC with UID {uid} not found in level.");
                }
            }
        }
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadInventorySection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader)
    {
        // Read Inventory data
        byte invLength = reader.ReadByte();
        for (int i = 0; i < invLength; i++)
        {
            var item = ReadItemData(reader);
            playerManager.Inventory.SetSlot(i, item);
        }
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadEffectsSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader)
    {
        // Read Status Effects
        byte effectsCount = reader.ReadByte();
        for (int i = 0; i < effectsCount; i++)
        {
            StatusEffect effect = (StatusEffect)reader.ReadByte();
            float duration = reader.ReadSingle();
            playerManager.StatusManager.AddStatusEffect(playerManager, effect, duration);
        }
        gameManager.LevelManager.TasksComplete++;
    }
    #endregion
    public static void ClearSavedState()
    {
        openedDoors.Clear();
        chests.Clear();
    }
    #region WriteHelpers
    public static void WriteChestData(BinaryWriter writer, Chest chest)
    {
        writer.Write(chest.TileID); // TileID - ushort
        writer.Write(chest.Generated); // IsGenerated - bool
        if (chest.Generated)
        {
            writer.Write((byte)chest.Container.Items!.Length);
            foreach (Item? item in chest.Container.Items!)
                WriteItemData(writer, item);
        }
        else
        {
            writer.Write(chest.Seed); // int (4 bytes)
            writer.Write(chest.LootGenerator.FileName.Split('\\', '/')[^1]);
        }
    }
    public static void WriteContainerData(BinaryWriter writer, IContainer container)
    {
        // Idx
        writer.Write((ushort)(container.Location.Y * Constants.MapSize.X + container.Location.X));
        // Amount of items
        byte amount = (byte)Math.Clamp(container.Container.Items.Length, 0, 255);
        writer.Write(amount);
        // Write items
        for (int i = 0; i < amount; i++)
            WriteItemData(writer, container.Container.Items[i]);
    }
    public static void WriteEnemyData(BinaryWriter writer, Enemy enemy)
    {
        writer.Write(enemy.UID);
        writer.Write((ushort)Math.Clamp(enemy.Health, ushort.MinValue, ushort.MaxValue));
        writer.Write((ushort)enemy.Position.X);
        writer.Write((ushort)enemy.Position.Y);
        writer.Write((float)TimerManager.TryTimeLeft($"EnemyAttack_{enemy.UID}"));
    }
    public static void WriteProjectileData(BinaryWriter writer, Projectile proj)
    {
        writer.Write(proj.OwnerUID);
        writer.Write((ushort)proj.Position.X);
        writer.Write((ushort)proj.Position.Y);
        writer.Write(proj.Direction);
        writer.Write((ushort)proj.Texture);
        writer.Write(proj.Damage);
        writer.Write(proj.Speed);
        writer.Write((byte)proj.Size.X);
        writer.Write((byte)proj.Size.Y);
    }
    public static void WriteItemData(BinaryWriter writer, ItemRef? itemRef)
    {
        Item? item = itemRef == null ? null : new(itemRef);
        WriteItemData(writer, item);
        item?.Dispose();
    }
    public static void WriteItemData(BinaryWriter writer, Item? item)
    {
        writer.Write((byte)(item == null ? 0 : item.Type.TypeID + 1));
        if (item != null)
        {
            writer.Write(item.Amount);
            writer.Write(item.CustomName ?? "");
        }
    }
    #endregion
    #region ReadHelpers
    public static Item? ReadItemData(BinaryReader reader)
    {
        int id = reader.ReadByte();
        if (id == 0) return null;
        ItemTypeID itemType = (ItemTypeID)(id - 1);
        byte amount = reader.ReadByte();
        string customName = reader.ReadString();
        return Item.Create(itemType, amount, customName == "" ? null : customName);
    }
    public static void ReadContainerData(BinaryReader reader, Level current)
    {
        // Idx
        ushort idx = reader.ReadUInt16();

        // Read items
        byte amount = reader.ReadByte();
        Item?[] items = new Item?[amount];
        for (int i = 0; i < items.Length; i++)
            items[i] = ReadItemData(reader);

        // Apply buffer to IContainer
        if (idx < Constants.MapSize.X * Constants.MapSize.Y && current.Tiles[idx] is IContainer cont)
            cont.Container.SetItems(items);
        else
            Logger.Error($"Tile {idx} is not an IContainer");
    }
    public static void ReadChestData(BinaryReader reader, Level current, LevelPath levelPath)
    {
        int idx = reader.ReadUInt16(); // TileID
        bool isGenerated = reader.ReadBoolean(); // IsGenerated
        if (idx < Constants.MapSize.X * Constants.MapSize.Y && current.Tiles[idx] is Chest chest)
        {
            if (isGenerated)
            {
                chest.SetEmpty();
                byte chestSize = reader.ReadByte();
                for (int s = 0; s < chestSize; s++)
                    chest.Container!.Items[s] = ReadItemData(reader);
            }
            else
            {
                chest.SetSeed(reader.ReadInt32());
                chest.RegenerateLoot(LootGeneratorHelper.Read(levelPath.WorldName, reader.ReadString()));
            }
        }
        else
        {
            Logger.Error($"Tile at index {idx} is not a chest.");

            // Chew up next bytes
            if (isGenerated)
                for (int s = 0; s < Chest.Size.X * Chest.Size.Y; s++)
                    ReadItemData(reader);
            else
            {
                reader.ReadInt32();
                reader.ReadString();
            }
        }
    }
    public static void ReadEnemyData(BinaryReader reader, Level current)
    {
        ushort uid = reader.ReadUInt16();
        ushort health = reader.ReadUInt16();
        Vector2 position = new(reader.ReadUInt16(), reader.ReadUInt16());
        float attackTimer = reader.ReadSingle();
        Enemy? enemy = current.Enemies.TryGetValue(uid, out var e) ? e : null;
        if (enemy != null)
        {
            enemy.Health = health;
            enemy.Position = position;
            TimerManager.GetTimer($"EnemyAttack_{enemy.UID}").Left = attackTimer;
        }
        else
            Logger.Error($"Enemy with UID {uid} not found in level.");
    }
    public static void ReadProjectileData(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Level current)
    {
        // Data
        ushort ownerUID = reader.ReadUInt16();
        Vector2 position = new(reader.ReadUInt16(), reader.ReadUInt16());
        float direction = reader.ReadSingle();
        TextureID tex = (TextureID)reader.ReadUInt16();
        ushort damage = reader.ReadUInt16();
        ushort speed = reader.ReadUInt16();
        Point size = new(reader.ReadByte(), reader.ReadByte());

        Projectile proj = new(gameManager, ownerUID, position, direction, tex, damage, speed, size);
        gameManager.LevelManager.Level.Projectiles.Add(proj);
    }
    #endregion
    public static Dictionary<string, string> ReadKeyValueFile(string path)
    {
        // Check if file exists
        Directory.CreateDirectory("GameData/");
        if (!File.Exists($"GameData/{path}.qkv"))
        {
            Logger.Error($"Quest Key Value file '{path}.qkv' not found in GameData/.");
            return [];
        }

        // Read key-value pairs from file
        try
        {
            Dictionary<string, string> data = [];
            using (var fs = new FileStream($"GameData/{path}.qkv", FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {

                uint pairs = reader.ReadUInt32();
                for (int p = 0; p < pairs; p++)
                {
                    string key = reader.ReadString();
                    string value = reader.ReadString();
                    data[key] = value;
                }
            }
            return data;
        }
        catch
        {
            return [];
        }
    }
    public static void WriteKeyValueFile(string path, Dictionary<string, string> data)
    {
        // Write key-value pairs to file
        using (var fs = new FileStream($"GameData/{path}.qkv", FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write((uint)data.Count);
            foreach (var pair in data)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }
        // Copy back to source code
        if (Constants.DEVMODE)
            File.Copy($"GameData/{path}.qkv", $"../../../GameData/{path}.qkv", true);
    }
}
