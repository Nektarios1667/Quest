using Quest.World;
using SharpDX.Direct2D1.Effects;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Managers;

public class SaveManager
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
    // Save State changes
    private static readonly Dictionary<IHasState, LevelPath> savedStateTiles = [];
    private static readonly Dictionary<Chest, LevelPath> savedChests = [];
    private static readonly Dictionary<IContainer, LevelPath> savedContainers = [];
    private static Dictionary<LevelPath, Level> pathToLevel = [];

    public static LevelPath CurrentSave { get; set; } = new();
    public static void SaveStateTile(IHasState tile, LevelPath levelPath)
    {
        if (savedStateTiles.Count >= ushort.MaxValue)
        {
            Logger.Error($"Maximum state tile count reached {ushort.MaxValue}");
            return;
        }

        savedStateTiles[tile] = levelPath;
    }
    public static void UnsaveStateTile(IHasState tile)
    {
        savedStateTiles.Remove(tile);
    }
    public static void SaveChestGenerator(Chest chest, LevelPath levelPath)
    {
        if (savedChests.Count >= ushort.MaxValue)
        {
            Logger.Error($"Maximum chest count reached {ushort.MaxValue}");
            return;
        }

        savedChests[chest] = levelPath;
    }
    public static void SaveContainer(IContainer container, LevelPath levelPath)
    {
        // Don't allow Chest even though it is IContainer
        if (container is Chest)
        {
            Logger.Warning("Chest should not be saved as IContainer as it has seperate logic - use SaveChestGenerator instead");
            return;
        }

        // Add
        if (savedContainers.Count >= ushort.MaxValue)
        {
            Logger.Error($"Maximum container count reached {ushort.MaxValue}");
            return;
        }

        savedContainers[container] = levelPath;
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

        // Collect all level objects
        var allPaths = savedStateTiles.Values.Concat(savedChests.Values.Concat(savedContainers.Values)).Distinct();
        pathToLevel = allPaths.ToDictionary(p => p, p => gameManager.LevelManager.GetLevel(p));

        // Progress
        TasksComplete = 0;
        TotalTasks = 10;

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
            WriteSection(writer, "LOOT", WriteLootSection, gameManager, playerManager);
            WriteSection(writer, "TILE", WriteTilesSection, gameManager, playerManager);
            WriteSection(writer, "CHST", WriteChestsSection, gameManager, playerManager);
            WriteSection(writer, "CONT", WriteContainersSection, gameManager, playerManager);
            WriteSection(writer, "ENEM", WriteEnemiesSection, gameManager, playerManager);
            WriteSection(writer, "PROJ", WriteProjectilesSection, gameManager, playerManager);
            WriteSection(writer, "NPCS", WriteNPCsSection, gameManager, playerManager);
            WriteSection(writer, "INVT", WriteInventorySection, gameManager, playerManager);
            WriteSection(writer, "EFFX", WriteEffectsSection, gameManager, playerManager);
            WriteSection(writer, "WAYP", WriteWaypointsSection, gameManager, playerManager);
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
    private static void WriteSection(BinaryWriter writer, string id, Action<BinaryWriter, GameManager, PlayerManager> writeData, GameManager gameManager, PlayerManager playerManager)
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
        writer.Write(GameManager.DayTime);
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
        writer.Write((byte)playerManager.Hunger);
        writer.Write((byte)playerManager.MaxHunger);
        writer.Write(TimerManager.TryTimeLeft("PlayerHungerLoss") ?? -1f);   // float
        writer.Write(TimerManager.TryTimeLeft("PlayerNaturalRegen") ?? -1f); // float
        writer.Write(TimerManager.TryTimeLeft("PlayerStarvation") ?? -1f);   // float

        TasksComplete++;
    }
    private static void WriteLootSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Collect all loot
        var allLoot = gameManager.LevelManager.Levels
        .SelectMany(level => level.Loot
            .Take(ushort.MaxValue)
            .Select(loot => (loot, level)))
        .ToArray();

        // Loot
        writer.Write((ushort)allLoot.Length);
        foreach ((Loot loot, Level level) in allLoot)
        {
            writer.Write(level.UID);
            writer.Write((byte)(loot.Item.Type.TypeID + 1));
            writer.Write(loot.Item.Amount);
            writer.Write((ushort)loot.Position.X);
            writer.Write((ushort)loot.Position.Y);
        }
        TasksComplete++;
    }
    private static void WriteTilesSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        writer.Write((ushort)savedStateTiles.Count);
        foreach ((IHasState stateTile, LevelPath level) in savedStateTiles)
        {
            writer.Write(pathToLevel[level].UID);
            writer.Write(stateTile.UID);
            stateTile.WriteState(writer, gameManager);
        }
        TasksComplete++;
    }
    private static void WriteChestsSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        writer.Write((ushort)savedChests.Count);
        foreach ((Chest chest, LevelPath level) in savedChests)
        {
            writer.Write(pathToLevel[level].UID);
            WriteChestData(writer, chest);
        }
        TasksComplete++;
    }
    private static void WriteContainersSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        writer.Write((ushort)savedContainers.Count);
        foreach ((IContainer cont, LevelPath level) in savedContainers)
        {
            writer.Write(pathToLevel[level].UID);
            WriteContainerData(writer, cont);
        }
        TasksComplete++;
    }
    private static void WriteEnemiesSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Collect all enemies
        var allEnemies = gameManager.LevelManager.Levels
        .SelectMany(level => level.Enemies.Values
            .Take(ushort.MaxValue)
            .Select(enemy => (enemy, level)))
        .ToArray();

        // Enemy
        writer.Write((ushort)allEnemies.Length);
        foreach ((Enemy enemy, Level level) in allEnemies)
        {
            writer.Write(level.UID);
            WriteEnemyData(writer, enemy);
        }
        TasksComplete++;
    }
    private static void WriteProjectilesSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Collect all projectiles
        var allProjectiles = gameManager.LevelManager.Levels
        .SelectMany(level => level.Projectiles
            .Take(ushort.MaxValue)
            .Select(proj => (proj, level)))
        .ToArray();

        // Enemy
        writer.Write((ushort)allProjectiles.Length);
        foreach ((Projectile proj, Level level) in allProjectiles)
        {
            writer.Write(level.UID);
            WriteProjectileData(writer, proj);
        }
        TasksComplete++;
    }
    private static void WriteNPCsSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Collect all NPCs
        var allNPCs = gameManager.LevelManager.Levels
        .SelectMany(level => level.NPCs.Values
            .Take(ushort.MaxValue)
            .Select(npc => (npc, level)))
        .ToArray();

        // NPCs
        writer.Write((ushort)allNPCs.Length);
        foreach ((NPC npc, Level level) in allNPCs)
        {
            writer.Write(level.UID);
            writer.Write(npc.UID);
            writer.Write((byte)npc.ShopOptions.Count);

            foreach (var item in npc.ShopOptions)
                writer.Write(item.Stock);
        }
        TasksComplete++;
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
        byte effectsCount = (byte)Math.Clamp(playerManager.StatusEffects.Count, 0, 255);
        writer.Write(effectsCount);

        foreach (var kv in playerManager.StatusEffects.Take(effectsCount))
        {
            writer.Write((byte)kv.Key); // effect type - byte
            writer.Write(kv.Value);     // effect timer - float
        }

        TasksComplete++;
    }
    private static void WriteWaypointsSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager)
    {
        // Write WAYP
        // Collect
        var playerWaypoints = gameManager.LevelManager.Levels
        .SelectMany(level => level.Waypoints
            .Where(w => w.PlayerMade)
            .Take(255)
            .Select(point => (point, level)))
        .ToArray();

        byte waypointsCount = (byte)playerWaypoints.Length;
        
        writer.Write(waypointsCount);
        foreach ((Waypoint point, Level level) in playerWaypoints)
        {
            writer.Write(level.UID);
            writer.Write(point);
        }
    }
    private static void WriteEOFSection(BinaryWriter writer, GameManager gameManager, PlayerManager playerManager) { }
    #endregion
    public static async Task<bool> ReadGameState(GameManager gameManager, PlayerManager playerManager, LevelPath levelPath)
    {
        string file = $"GameData/Worlds/{levelPath.WorldName}/saves/{levelPath.LevelName}.qsv";
        if (!File.Exists(file))
        {
            Logger.Error($"Save file '{file}' does not exist.");
            return false;
        }
        CurrentSave = levelPath;
        WriteKeyValueFile("Persistent/continue", new() { { "save", levelPath.ToString() } });
        await LevelFileManager.ReadWorldAsync(gameManager, levelPath.WorldName, true);

        gameManager.LevelManager.TasksComplete = 0;
        gameManager.LevelManager.TotalTasks = 6;
        MenuManager.SetCurrentlyLoading("Loading save file...");

        // Level table - uid <--> levelName
        Dictionary<ushort, Level> levelTable = [];

        // Read sections
        string id = "";
        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            // Magic
            byte[] magic = reader.ReadBytes(4);
            if (Encoding.ASCII.GetString(magic) != "QSAV")
            {
                Logger.Error($"invalid file format for file '{levelPath}'.");
                return false;
            }

            // Arbitrary limit of 200
            for (int i = 0; i < 200; i++)
            {

                id = reader.ReadString();
                int length = reader.ReadInt32();

                if (id == "_EOF") break;

                byte[] data = reader.ReadBytes(length);

                using MemoryStream sectionStream = new MemoryStream(data);
                using BinaryReader sectionReader = new BinaryReader(sectionStream);

                // Section types
                try
                {
                    switch (id)
                    {
                        case "TABL": ReadTableSection(gameManager, sectionReader, levelTable, levelPath.WorldName); break;
                        case "WHTR": ReadWeatherSection(gameManager, sectionReader, levelTable); break;
                        case "CAMR": ReadCameraSection(gameManager, sectionReader, levelTable); break;
                        case "PLYR": ReadPlayerSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "LOOT": ReadLootSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "TILE": ReadTilesSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "CHST": ReadChestsSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "CONT": ReadContainersSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "ENEM": ReadEnemiesSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "PROJ": ReadProjectilesSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "NPCS": ReadNPCsSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "INVT": ReadInventorySection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "EFFX": ReadEffectsSection(gameManager, playerManager, sectionReader, levelTable); break;
                        case "WAYP": ReadWaypointsSection(gameManager, playerManager, sectionReader, levelTable); break;
                        default: Logger.Warning($"Unknown level section '{id}'"); break; // Unknown section - ignore it
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Save Error | {id}\n{e}");
                }
            }
        }

        gameManager.OverlayManager.Notification($"Save Loaded", Color.Cyan);
        Logger.System("Loaded game state from save.qsv.");
        return true;
    }
    #region ReadSections
    public static void ReadTableSection(GameManager gameManager, BinaryReader reader, Dictionary<ushort, Level> levelTable, string worldName)
    {
        ushort tableLength = reader.ReadUInt16();
        for (int t = 0; t < tableLength; t++)
        {
            ushort id = reader.ReadUInt16();
            string name = reader.ReadString();
            levelTable[id] = gameManager.LevelManager.GetLevel(new LevelPath(worldName, name));
        }

    }
    public static void ReadWeatherSection(GameManager gameManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Read weather data
        string level = reader.ReadString();
        gameManager.LevelManager.LoadLevel(gameManager, level);

        GameManager.DayTime = reader.ReadSingle();
        GameManager.GameTime = reader.ReadSingle();
        int weatherSeed = reader.ReadInt32();
        float lastWeather = reader.ReadSingle();
        gameManager.WeatherManager.SetWeatherPersistent(seed: weatherSeed, lastWeatherTime: lastWeather, lastTimeValue: GameManager.GameTime);
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadCameraSection(GameManager gameManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Read CameraManager data
        CameraManager.CameraDest = new(reader.ReadSingle(), reader.ReadSingle());
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(gameManager, 0); // In bounds check
        gameManager.LevelManager.TasksComplete++;
    }
    public static void ReadPlayerSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        playerManager.Health = reader.ReadByte();
        playerManager.MaxHealth = reader.ReadByte();
        gameManager.LevelManager.TasksComplete++;

        float hungerLossTimer = reader.ReadSingle();
        if (hungerLossTimer >= 0) TimerManager.SetTimer("PlayerHungerLoss", hungerLossTimer, null);

        float regenTimer = reader.ReadSingle();
        if (regenTimer >= 0) TimerManager.SetTimer("PlayerNaturalRegen", regenTimer, null);

        float starvationTimer = reader.ReadSingle();
        if (starvationTimer >= 0) TimerManager.SetTimer("PlayerStarvation", starvationTimer, null);
    }
    public static void ReadLootSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Loot
        ushort lootCount = reader.ReadUInt16();
        for (int l = 0; l < lootCount; l++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            byte typeID = (byte)(reader.ReadByte() - 1);
            byte amount = reader.ReadByte();
            Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
            level.Loot.Add(new Loot(new(ItemTypes.All[typeID], amount), location));
        }
    }
    public static void ReadTilesSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        ushort tileCount = reader.ReadUInt16();
        for (int t = 0; t < tileCount; t++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            ushort tileID = reader.ReadUInt16();
            if (level.Tiles[tileID] is IHasState stateTile)
                stateTile.ReadState(reader, gameManager);
        }
        TasksComplete++;
    }
    public static void ReadChestsSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        ushort chestsCount = reader.ReadUInt16();

        for (int c = 0; c < chestsCount; c++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];
            ReadChestData(reader, level, level.LevelPath);
        }
        TasksComplete++;
    }
    public static void ReadContainersSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        ushort containersCount = reader.ReadUInt16();

        for (int c = 0; c < containersCount; c++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];
            ReadContainerData(reader, level);
        }
        TasksComplete++;
    }
    public static void ReadEnemiesSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Enemy
        ushort enemiesSection = reader.ReadUInt16();
        for (int l = 0; l < enemiesSection; l++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            ReadEnemyData(reader, level);
        }

        TasksComplete++;
    }
    public static void ReadProjectilesSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Projectiles
        ushort projectilesCount = reader.ReadUInt16();
        for (int l = 0; l < projectilesCount; l++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            ReadProjectileData(gameManager, playerManager, reader, level);
        }

        TasksComplete++;
    }
    public static void ReadNPCsSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // NPCs
        ushort npcCount = reader.ReadUInt16();
        for (int n = 0; n < npcCount; n++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            ushort uid = reader.ReadUInt16();
            // Read stock amounts
            if (level.NPCs.TryGetValue(uid, out var npc))
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

        TasksComplete++;
    }
    public static void ReadInventorySection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
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
    public static void ReadEffectsSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Read Status Effects
        StatusManager.ClearAllStatusEffects(gameManager, playerManager);
        byte effectsCount = reader.ReadByte();
        for (int i = 0; i < effectsCount; i++)
        {
            StatusEffect effect = (StatusEffect)reader.ReadByte();
            float duration = reader.ReadSingle();
            StatusManager.AddStatusEffect(playerManager, effect, duration);
        }
        gameManager.LevelManager.TasksComplete++;
    }
    private static void ReadWaypointsSection(GameManager gameManager, PlayerManager playerManager, BinaryReader reader, Dictionary<ushort, Level> levelTable)
    {
        // Write WAYP
        Waypoint[] playerWaypoints = gameManager.LevelManager.Level.Waypoints.Where(w => w.PlayerMade).Take(255).ToArray();
        byte waypointsCount = reader.ReadByte();

        for (int w = 0; w < waypointsCount; w++)
        {
            ushort levelUID = reader.ReadUInt16();
            Level level = levelTable[levelUID];

            Waypoint point = reader.ReadWaypoint();
            level.AddWaypoint(point);
        }
    }
    #endregion
    private static void ClearSavedState()
    {
        savedStateTiles.Clear();
        savedChests.Clear();
    }
    #region WriteHelpers
    public static void WriteChestData(BinaryWriter writer, Chest chest)
    {
        writer.Write(chest.UID); // TileID - ushort
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
        writer.Write(TimerManager.TryTimeLeft($"EnemyAttack_{enemy.UID}") ?? -1f);
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
    public static void WriteItemData(BinaryWriter writer, ItemRef? item)
    {
        writer.Write((byte)(item == null ? 0 : item.Type.TypeID + 1));
        if (item != null)
        {
            writer.Write(item.Amount);
            writer.Write(item.CustomName ?? "");
        }
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
        float speed = reader.ReadSingle();
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
