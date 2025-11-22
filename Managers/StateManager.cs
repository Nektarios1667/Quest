using System.IO;

namespace Quest.Managers;
public enum GameState
{
    MainMenu,
    Settings,
    Credits,
    LevelSelect,
    Game,
    Editor,
    Death,
}
public enum OverlayState
{
    None,
    Container,
    Pause,
}
public enum Mood
{
    Calm,
    Dark,
    Epic,
}

public enum Weather
{
    Clear,
    Light,
    Heavy,
}
public static class StateManager
{
    // Weather controls
    private const float mu = .1f; // Max boost
    private const float zeta = 2; // Rain boost depletion
    private const float alpha = 300; // Rain boost acivation time
    // Weather
    public static readonly FastNoiseLite WeatherNoise = new((int)(DateTime.Now.Ticks ^ (DateTime.Now.Ticks >> 32)));
    private static int _weatherSeed = Environment.TickCount;
    public static int WeatherSeed { get => _weatherSeed; set { _weatherSeed = value; WeatherNoise.SetSeed(value); } }
    public const float weatherThreshold = 0.65f;
    private static float lastWeather = 0f;
    private static float lastTime = -1f;
    // States
    public static bool IsGameState => State == GameState.Game || State == GameState.Editor;
    private static GameState _state = GameState.MainMenu;
    public static GameState State
    {
        get => _state;
        set
        {
            PreviousState = _state;
            _state = value;
        }
    }
    public static GameState PreviousState { get; private set; } = GameState.MainMenu;
    public static OverlayState OverlayState { get; set; } = OverlayState.None;
    public static Mood Mood { get; set; } = Mood.Calm;
    public static string CurrentSave { get; set; } = "";
    public static string ContinueSave { get; set; } = "";
    // Save State changes
    private static readonly HashSet<int> openedDoors = [];
    private static readonly HashSet<Chest> chests = [];
    static StateManager()
    {
        WeatherNoise.SetSeed(WeatherSeed);
        WeatherNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        WeatherNoise.SetFrequency(0.005f);
        WeatherNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        WeatherNoise.SetFractalOctaves(3);

        var continuePersist = ReadKeyValueFile("continue");
        if (continuePersist.TryGetValue("lastSave", out string? value))
            ContinueSave = value;
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
    public static float WeatherNoiseValue(float time)
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
    public static float NoiseToIntensity(float noise) => Math.Min((float)Math.Sqrt(Math.Max(noise - weatherThreshold, 0) / (1 - weatherThreshold)), 0.8f);
    public static float WeatherIntensity(float time) => NoiseToIntensity(WeatherNoiseValue(time));
    public static void SaveDoorOpened(int idx)
    {
        openedDoors.Add(idx);
    }
    public static void SaveChestGenerator(Chest chest)
    {
        chests.Add(chest);
    }
    public static void SaveGameState(GameManager gameManager, PlayerManager playerManager, string saveName)
    {
        byte[] data;

        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Write GameManager data
            writer.Write(gameManager.LevelManager.Level.Name);
            writer.Write(gameManager.DayTime);
            writer.Write(gameManager.GameTime);
            writer.Write(WeatherSeed);
            writer.Write(lastWeather);
            // Write CameraManager data
            writer.Write(CameraManager.CameraDest.X);
            writer.Write(CameraManager.CameraDest.Y);
            // Write PlayerManager data
            writer.Write((byte)gameManager.UIManager.HealthBar.CurrentValue);
            writer.Write((byte)gameManager.UIManager.HealthBar.MaxValue);
            // Write LevelManager data
            // Loot
            writer.Write((byte)gameManager.LevelManager.Level.Loot.Count);
            foreach (var loot in gameManager.LevelManager.Level.Loot)
            {
                writer.Write((byte)((byte)Enum.Parse(typeof(ItemType), loot.Item, true) + 1));
                writer.Write((byte)loot.Amount);
                writer.Write((ushort)loot.Location.X);
                writer.Write((ushort)loot.Location.Y);
            }
            // Chests
            writer.Write((ushort)chests.Count);
            writer.Write((byte)Chest.ChestSize.X);
            writer.Write((byte)Chest.ChestSize.Y);
            foreach (var chest in chests)
            {
                writer.Write(chest.TileID); // TileID
                writer.Write(chest.Generated); // IsGenerated
                if (chest.Generated)
                    for (int y = 0; y < chest.Inventory!.Items.GetLength(1); y++)
                        for (int x = 0; x < chest.Inventory!.Items.GetLength(0); x++)
                            WriteItemData(writer, chest.Inventory.Items[x, y]);
                else
                {
                    writer.Write(chest.Seed);
                    writer.Write(chest.LootGenerator.FileName);
                }
            }
            // Doors
            writer.Write((ushort)openedDoors.Count);
            foreach (var idx in openedDoors)
            {
                writer.Write((ushort)idx);
            }
            // Write Inventory data
            var inventory = playerManager.Inventory;
            for (int y = 0; y < inventory.Items.GetLength(1); y++)
                for (int x = 0; x < inventory.Items.GetLength(0); x++)
                    WriteItemData(writer, inventory.Items[x, y]);
            writer.Flush();
            data = ms.ToArray();
        }
        Logger.System("Saved game state.");

        // Write
        string world = gameManager.LevelManager.Level.World;
        using (var fs = new FileStream($"..\\..\\..\\GameData\\Worlds\\{world}\\saves\\{saveName}.qsv", FileMode.Create, FileAccess.Write))
            fs.Write(data, 0, data.Length);
        File.Copy($"..\\..\\..\\GameData\\Worlds\\{world}\\saves\\{saveName}.qsv", $"GameData\\Worlds\\{world}\\saves\\{saveName}.qsv", true);

        gameManager.UIManager.LootNotifications.AddNotification($"Game Saved", Color.Cyan);
        Logger.System($"Saved game state to '{saveName}.qsv'.");
    }
    public static bool ReadGameState(GameManager gameManager, PlayerManager playerManager, string save)
    {
        var path = StringTools.ParseLevelPath(save);
        string file = $"GameData\\Worlds\\{path.world}\\saves\\{path.level}.qsv";
        if (!File.Exists(file))
        {
            Logger.Error($"Save file '{file}' does not exist.");
            return false;
        }
        CurrentSave = save;

        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        //using (var gzip = new GZipStream(fs, CompressionMode.Decompress))
        using (var reader = new BinaryReader(fs))
        {
            // Read GameManager data
            string level = reader.ReadString();
            gameManager.LevelManager.ReadLevel(gameManager.UIManager, level, reload: true);
            gameManager.LevelManager.LoadLevel(gameManager, level);

            gameManager.DayTime = reader.ReadSingle();
            gameManager.GameTime = reader.ReadSingle();
            WeatherSeed = reader.ReadInt32();
            lastWeather = reader.ReadSingle();
            SetWeatherPersistent(lastWeatherTime:lastWeather, lastTimeValue: gameManager.GameTime);
            // Read CameraManager data
            CameraManager.CameraDest = new(reader.ReadSingle(), reader.ReadSingle());
            CameraManager.Camera = CameraManager.CameraDest;
            // Read PlayerManager data
            gameManager.UIManager.HealthBar.CurrentValue = reader.ReadByte();
            gameManager.UIManager.HealthBar.MaxValue = reader.ReadByte();
            // Read LevelManager data
            // Loot
            byte lootCount = reader.ReadByte();
            for (int l = 0; l < lootCount; l++)
            {
                string name = ((ItemType)reader.ReadByte() - 1).ToString();
                byte amount = reader.ReadByte();
                Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
                gameManager.LevelManager.Level.Loot.Add(new Loot(name, amount, location, 0f));
            }
            // Chests
            ushort chestCount = reader.ReadUInt16();
            byte chestWidth = reader.ReadByte();
            byte chestHeight = reader.ReadByte();
            for (int c = 0; c < chestCount; c++)
            {
                int idx = reader.ReadUInt16(); // TileID
                bool isGenerated = reader.ReadBoolean(); // IsGenerated
                if (gameManager.LevelManager.Level.Tiles[idx] is Chest chest)
                    if (isGenerated)
                    {
                        chest.SetGenerated(true);
                        for (int s = 0; s < chestWidth * chestHeight; s++)
                            chest.Inventory!.SetSlot(s, ReadItemData(reader));
                    }
                    else
                    {
                        chest.SetSeed(reader.ReadInt32());
                        chest.RegenerateLoot(LootGeneratorHelper.Read(reader.ReadString()));
                    }
                else
                {
                    Logger.Error($"Tile at index {idx} is not a chest.");

                    // Chew up next bytes
                    if (isGenerated)
                        for (int s = 0; s < chestWidth * chestHeight; s++)
                            ReadItemData(reader);
                    else
                    {
                        reader.ReadInt32();
                        reader.ReadString();
                    }
                }
            }
            // Doors
            ushort doorCount = reader.ReadUInt16();
            for (int d = 0; d < doorCount; d++)
            {
                int idx = reader.ReadUInt16();
                if (gameManager.LevelManager.Level.Tiles[idx] is Door door)
                    door.Open();
                else
                    Logger.Error($"Tile at index {idx} is not a door.");
            }
            // Read Inventory data
            for (int s = 0; s < 24; s++)
            {
                var item = ReadItemData(reader);
                playerManager.Inventory.SetSlot(s, item);
            }
        }

        gameManager.UIManager.LootNotifications.AddNotification($"Save Loaded", Color.Cyan);
        Logger.System("Loaded game state from save.qsv.");
        return true;
    }
    public static void ClearSavedState()
    {
        openedDoors.Clear();
        chests.Clear();
    }
    public static void WriteItemData(BinaryWriter writer, Item? item)
    {
        writer.Write((byte)(item == null ? 0 : (byte)Enum.Parse(typeof(ItemType), item.Name, true) + 1));
        if (item != null)
            writer.Write((byte)item.Amount);
    }
    public static Item? ReadItemData(BinaryReader reader)
    {
        int id = reader.ReadByte();
        if (id == 0) return null;
        ItemType itemType = (ItemType)(id - 1);
        byte amount = reader.ReadByte();
        return Item.ItemFromItemType(itemType, amount);
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
    public static void WriteKeyValueFile(string name, Dictionary<string, string> data)
    {
        // Write key-value pairs to file
        using (var fs = new FileStream($"..\\..\\..\\GameData\\Persistent\\{name}.qkv", FileMode.Create, FileAccess.Write))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write((uint)data.Count);
            foreach (var pair in data)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }
    }
}
