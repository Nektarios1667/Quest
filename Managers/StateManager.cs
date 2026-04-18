using Quest.Items;
using Quest.Tiles;
using Quest.Utilities;
using ScottPlot.Colormaps;
using SharpDX.Direct3D9;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Policy;

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
    // Weather
    public static readonly FastNoiseLite WeatherNoise = new((int)(DateTime.Now.Ticks ^ (DateTime.Now.Ticks >> 32)));
    private static int _weatherSeed = Environment.TickCount;
    public static int WeatherSeed { get => _weatherSeed; set { _weatherSeed = value; WeatherNoise.SetSeed(value); } }
    public const float weatherThreshold = 0.65f;
    private static float lastWeather = 0f;
    private static float lastTime = -1f;
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
    static StateManager()
    {
        WeatherNoise.SetSeed(WeatherSeed);
        WeatherNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        WeatherNoise.SetFrequency(0.005f);
        WeatherNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        WeatherNoise.SetFractalOctaves(3);
    }
    public static void SetWeatherPersistent(int seed = -1, float lastWeatherTime = 0f, float lastTimeValue = -1f)
    {
        if (seed != -1)
            WeatherNoise.SetSeed(seed);
        lastWeather = lastWeatherTime;
        lastTime = lastTimeValue;
    }
    public static void RevertGameState()
    {
        State = PreviousState;
    }
    public static void RevertOverlayState()
    {
        OverlayState = PreviousOverlayState;
    }
    public static float WeatherValue(float time)
    {
        float val = WeatherNoise.GetNoise(time, 0) * 0.5f + 0.5f;
        val = 1f / (1 + (float)Math.Pow(MathHelper.E, -8 * (val - 0.5f)));
        float delta = lastTime == -1 ? 0 : (time - lastTime);

        // Weather buildup
        val += WeatherBoost(time);
        if (val >= weatherThreshold)
        {
            lastWeather += Math.Min(12 * delta * (val - weatherThreshold) / (1 - weatherThreshold), time - lastWeather);
        }

        lastTime = time;

        return val;
    }
    public static float WeatherBoost(float time)
    {
        if (time - lastWeather > 600) return Math.Min((time - lastWeather - 600) / 1800f, 0.1f);
        return 0;
    }
    private static float NoiseToIntensity(float noise) => Math.Min((float)Math.Sqrt(Math.Max(noise - weatherThreshold, 0) / (1 - weatherThreshold)), 0.8f);
    public static float WeatherIntensity(float time) => NoiseToIntensity(WeatherValue(time));
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
    public static void SaveGameState(GameManager gameManager, PlayerManager playerManager)
    {
        WriteKeyValueFile("continue", new() { { "save", CurrentSave.ToString() } });
        string worldName = gameManager.LevelManager.Level.WorldName;
        byte[] data;

        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write GameManager data
            writer.Write(gameManager.LevelManager.Level.Path);
            writer.Write(gameManager.DayTime);
            writer.Write(GameManager.GameTime);
            writer.Write(WeatherSeed);
            writer.Write(lastWeather);
            // Write CameraManager data
            writer.Write(CameraManager.CameraDest.X);
            writer.Write(CameraManager.CameraDest.Y);
            // Write PlayerManager data
            writer.Write((byte)gameManager.OverlayManager.HealthBar.CurrentValue);
            writer.Write((byte)gameManager.OverlayManager.HealthBar.MaxValue);
            // Level specific data
            // All of the levels with extra data
            string[] levels = new[] {
                chests.Keys,
                openedDoors.Keys,
                gameManager.LevelManager.Levels.Where(l => l.WorldName == worldName && l.Loot.Count > 0).Select(l => l.LevelName)
            }.SelectMany(x => x).Distinct().Take(255).ToArray();

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
                // Doors
                if (openedDoors.TryGetValue(level, out var levelDoors))
                {
                    writer.Write((ushort)levelDoors.Count);
                    foreach (ushort door in levelDoors)
                        writer.Write(door);
                }
                else
                    writer.Write((ushort)0);
                // Chests
                if (chests.TryGetValue(level, out var levelChests))
                {
                    writer.Write((ushort)levelChests.Count);
                    foreach (Chest chest in levelChests)
                        WriteChestData(writer, chest);
                }
                else
                    writer.Write((ushort)0);
                // Containers
                if (containers.TryGetValue(level, out var levelContainers))
                {
                    writer.Write((ushort)levelContainers.Count);
                    foreach (IContainer cont in levelContainers)
                        WriteContainerData(writer, cont);
                }
                else
                    writer.Write((ushort)0);
            }

            // Write Inventory data
            var inventory = playerManager.Inventory;
            writer.Write((byte)inventory.Items.Length);
            for (int i = 0; i < inventory.Items.Length; i++)
                    WriteItemData(writer, inventory.Items[i]);
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
    public static bool ReadGameState(GameManager gameManager, PlayerManager playerManager, string save)
    {
        LevelPath levelPath = new(save);
        string file = $"GameData/Worlds/{levelPath.WorldName}/saves/{levelPath.LevelName}.qsv";
        if (!File.Exists(file))
        {
            Logger.Error($"Save file '{file}' does not exist.");
            return false;
        }
        CurrentSave = levelPath;
        WriteKeyValueFile("continue", new() { { "save", save } });
        gameManager.LevelManager.ReadWorld(gameManager, levelPath.WorldName, true);

        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(fs))
        {
            // Read GameManager data
            string level = reader.ReadString();
            gameManager.LevelManager.LoadLevel(gameManager, level);

            gameManager.DayTime = reader.ReadSingle();
            GameManager.GameTime = reader.ReadSingle();
            WeatherSeed = reader.ReadInt32();
            lastWeather = reader.ReadSingle();
            SetWeatherPersistent(lastWeatherTime: lastWeather, lastTimeValue: GameManager.GameTime);
            // Read CameraManager data
            CameraManager.CameraDest = new(reader.ReadSingle(), reader.ReadSingle());
            CameraManager.Camera = CameraManager.CameraDest;
            CameraManager.Update(0); // In bounds check
            // Read PlayerManager data
            gameManager.OverlayManager.HealthBar.CurrentValue = reader.ReadByte();
            gameManager.OverlayManager.HealthBar.MaxValue = reader.ReadByte();
            // Read LevelManager data
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
                        door.Open(gameManager);
                // Chests
                ushort chestCount = reader.ReadUInt16();
                for (int c = 0; c < chestCount; c++)
                    ReadChestData(reader, current, levelPath);
                // Containers
                ushort containerCount = reader.ReadUInt16();
                for (int o = 0; o < containerCount; o++)
                    ReadContainerData(reader, current);
            }

            // Read Inventory data
            byte invLength = reader.ReadByte();
            for (int i = 0; i < invLength; i++)
            {
                var item = ReadItemData(reader);
                playerManager.Inventory.SetSlot(i, item);
            }
        }

        gameManager.OverlayManager.Notification($"Save Loaded", Color.Cyan);
        Logger.System("Loaded game state from save.qsv.");
        return true;
    }
    public static void ClearSavedState()
    {
        openedDoors.Clear();
        chests.Clear();
    }
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
    public static Item? ReadItemData(BinaryReader reader)
    {
        int id = reader.ReadByte();
        if (id == 0) return null;
        ItemTypeID itemType = (ItemTypeID)(id - 1);
        byte amount = reader.ReadByte();
        string customName = reader.ReadString();
        return Item.Create(itemType, amount, customName == "" ? null : customName);
    }
    public static Dictionary<string, string> ReadKeyValueFile(string name)
    {
        // Check if file exists
        Directory.CreateDirectory("GameData/Persistent");
        if (!File.Exists($"GameData/Persistent/{name}.qkv"))
        {
            Logger.Error("Quest Key Value file does not exist.");
            return [];
        }

        // Read key-value pairs from file
        try
        {
            Dictionary<string, string> data = [];
            using (var fs = new FileStream($"GameData/Persistent/{name}.qkv", FileMode.Open, FileAccess.Read))
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
    public static void WriteKeyValueFile(string name, Dictionary<string, string> data)
    {
        // Write key-value pairs to file
        using (var fs = new FileStream($"GameData/Persistent/{name}.qkv", FileMode.Create, FileAccess.Write))
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
            File.Copy($"GameData/Persistent/{name}.qkv", $"../../../GameData/Persistent/{name}.qkv", true);
    }
}
