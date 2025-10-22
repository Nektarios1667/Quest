using SharpDX.XAPO.Fx;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Quest.Managers;
public class LevelManager
{
    public List<ILootGenerator> LootGenerators = new();
    public List<Level> Levels { get; private set; }
    public Level Level { get; private set; }
    public Color SkyColor { get; set; }
    public Color WeatherColor { get; set; }
    public event Action<string>? LevelLoaded;
    public static readonly Point lootStackOffset = new(4, 4);
    public LevelManager()
    {
        // Empty
        Levels = [];
        Tile[] skyTiles = new Tile[256 * 256];
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) skyTiles[t] = new Sky(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
        Level = new("", skyTiles, [], new(128, 128), [], [], [], []);
    }
    public void Update(GameManager gameManager)
    {
        if (!StateManager.IsGameState) return;

        // Entities
        foreach (NPC npc in Level.NPCs) npc.Update(gameManager);
        foreach (Enemy enemy in Level.Enemies) enemy.Update(gameManager);

        // SkyTint
        UpdateSky(gameManager);


        // Dynamic lighting
        foreach (Loot loot in Level.Loot)
            if (loot.Item == "Lantern")
            {
                Point loc = loot.Location - CameraManager.Camera.ToPoint() + Constants.Middle + TextureManager.Metadata[loot.Texture].Size;
                LightingManager.SetLight($"Loot_{loot.UID}", loc, 2, Color.Transparent);
            }
    }
    public void UpdateSky(GameManager gameManager)
    {
        // Custom tint
        if (Level.Tint != Color.Transparent)
        {
            SkyColor = Level.Tint;
            return;
        }

        // Calculate sky colors from weather, biome, and time
        BiomeType? currentBiome = GetBiome(CameraManager.TileCoord);
        float blend = StateManager.WeatherIntensity(gameManager.GameTime);
        SkyColor = ColorTools.GetSkyColor(gameManager.DayTime) * 0.9f;

        if (currentBiome == null || currentBiome == BiomeType.Indoors || blend == 0) WeatherColor = Color.Transparent;
        else {
            switch (currentBiome)
            {
                case BiomeType.Temperate: WeatherColor = Color.MediumBlue; break;
                case BiomeType.Snowy: WeatherColor = Color.White; break;
                case BiomeType.Desert: WeatherColor = Color.OrangeRed; break;
                case BiomeType.Ocean: WeatherColor = Color.Blue; break;
            }
        }
        WeatherColor *= blend;
    }
    public void Draw(GameManager gameManager)
    {
        if (!StateManager.IsGameState) return;

        DrawTiles(gameManager);
        DrawDecals(gameManager);
        DrawLoot(gameManager);
        DrawCharacters(gameManager);
    }
    public void DrawTiles(GameManager gameManager)
    {
        // Tiles
        DebugManager.StartBenchmark("TileDraws");
        if (Level.Tiles == null || Level.Tiles.Length == 0) return;

        // Get bounds
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;

        // Iterate
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                GetTile(x, y)?.Draw(gameManager);
            }
        }
        DebugManager.EndBenchmark("TileDraws");
    }
    public void DrawDecals(GameManager gameManager)
    {
        // Draw each decal
        DebugManager.StartBenchmark("DecalDraws");
        foreach (Decal decal in Level.Decals)
            decal.Draw(gameManager);
        DebugManager.EndBenchmark("DecalDraws");
    }
    public void DrawLoot(GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawLoot");
        // Draw each
        for (int l = 0; l < Level.Loot.Count; l++)
        {
            Loot loot = Level.Loot[l];
            Point pos = loot.Location - CameraManager.Camera.ToPoint() + Constants.Middle;
            pos.Y += (int)(Math.Sin((gameManager.GameTime - loot.Birth) * 2 % (Math.PI * 2)) * 6); // Bob up and down
            DrawTexture(gameManager.Batch, loot.Texture, pos, scale: 2);
            // Draw stacks if multiple
            if (loot.Amount > 1)
                DrawTexture(gameManager.Batch, loot.Texture, pos + lootStackOffset, scale: 2);
            if (loot.Amount > 2)
                DrawTexture(gameManager.Batch, loot.Texture, pos + lootStackOffset + lootStackOffset, scale: 2);
            // Draw hitbox if enabled
            if (DebugManager.DrawHitboxes)
                FillRectangle(gameManager.Batch, new(pos, new(32)), Constants.DebugPinkTint);
        }
        DebugManager.EndBenchmark("DrawLoot");
    }
    public void DrawCharacters(GameManager gameManager)
    {
        DebugManager.StartBenchmark("CharacterDraws");
        foreach (NPC npc in Level.NPCs) npc.Draw(gameManager);
        foreach (Enemy enemy in Level.Enemies) enemy.Draw(gameManager);
        DebugManager.EndBenchmark("CharacterDraws");
    }
    public bool LoadLevel(GameManager gameManager, int levelIndex)
    {
        // Check index
        if (levelIndex < -Levels.Count || levelIndex >= Levels.Count)
        {
            Logger.Error("Invalid level index.");
            return false;
        }

        // Close dialogs
        if (Level != null)
        {
            foreach (NPC npc in Level.NPCs)
            {
                npc.DialogBox.IsVisible = false;
                npc.DialogBox.Displayed = "";
            }
        }

        // Load the level data
        if (levelIndex < 0) levelIndex = Levels.Count - Math.Abs(levelIndex);
        Level = Levels[levelIndex];

        // MiniMap
        gameManager.UIManager.RefreshMiniMap();
        LevelLoaded?.Invoke(Level.Name);

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
        return true;
    }
    public bool LoadLevel(GameManager gameManager, string name)
    {
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == name)
            {
                LoadLevel(gameManager, l);
                return true; ;
            }
        }
        // If not found throw an error
        Logger.Error($"Level '{name}' not found in stored levels.");
        return false;
    }
    public bool LoadLevelObject(GameManager gameManager, Level level)
    {
        // Close dialogs
        if (Level != null)
        {
            foreach (NPC npc in Level.NPCs)
            {
                npc.DialogBox.IsVisible = false;
                npc.DialogBox.Displayed = "";
            }
        }

        // Load the level data
        Level = level;

        // MiniMap
        gameManager.UIManager.RefreshMiniMap();

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
        Logger.System($"Loaded level '{level.Name}'.");
        return true;
    }
    public bool UnloadLevel(int levelIndex)
    {
        // Check index
        if (levelIndex < 0 || levelIndex >= Levels.Count)
        {
            Logger.Error($"Invalid level index {levelIndex}.");
            return false;
        }
        // Unload the level
        string name = Levels[levelIndex].Name;
        if (Level == Levels[levelIndex])
            Level = new("", [], [], new Point(128, 128), [], [], [], []);

        Levels.RemoveAt(levelIndex);
        Logger.System($"Unloaded level '{name}'.");
        return true;
    }
    public bool UnloadLevel(string levelName)
    {
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == levelName)
            {
                UnloadLevel(l);
                return true;
            }
        }
        
        Logger.Error($"Level '{levelName}' not found in stored levels.");
        return false;
    }
    public bool ReadWorld(OverlayManager uIManager, string folder, bool reload = false)
    {
        if (!Directory.Exists($"GameData\\Worlds\\{folder}"))
        {
            Logger.Error($"World folder '{folder}' does not exist.");
            return false;
        }

        // Read loot tables and presets
        string[] qlp = Directory.GetFiles($"GameData\\Worlds\\{folder}\\loot", "*.qlp");
        string[] qlt = Directory.GetFiles($"GameData\\Worlds\\{folder}\\loot", "*.qlt");
        foreach (string file in qlp.Concat(qlt).ToArray())
        {
            if (file.EndsWith(".qlt"))
                LootGenerators.Add(LootTable.ReadLootTable(file));
            else if (file.EndsWith(".qlp"))
                LootGenerators.Add(LootPreset.ReadLootPreset(file));
            Logger.System($"Loaded Loot file {file}.");
        }

        // Read levels
        string[] qlv = Directory.GetFiles($"GameData\\Worlds\\{folder}\\levels", "*.qlv");
        foreach (string file in qlv)
            ReadLevel(uIManager, $"{folder}\\{System.IO.Path.GetFileNameWithoutExtension(file)}", reload);
        return true;
    }
    public bool ReadLevel(OverlayManager uiManager, string filename, bool reload = false)
    {
        // File checks
        string[] splitPath = filename.Split('\\', '/');
        if (splitPath.Length != 2)
        {
            Logger.Error($"Invalid file format '{filename}.'");
            return false;
        }
        string path = $"GameData\\Worlds\\{splitPath[0]}\\levels\\{splitPath[1]}.qlv";
        if (!File.Exists(path))
        {
            Logger.Error($"Level file '{filename}' does not exist.");
            return false;
        }

        // Check if already read
        if (!reload)
            foreach (Level level in Levels)
                if (level.Name == filename)
                    return true;

        // Make buffers
        Tile[] tilesBuffer = new Tile[Constants.MapSize.X * Constants.MapSize.Y];
        BiomeType[] biomeBuffer = new BiomeType[Constants.MapSize.X * Constants.MapSize.Y];
        List<NPC> npcBuffer = [];
        List<Loot> lootBuffer = [];
        List<Decal> decalBuffer = [];

        // Context
        using FileStream fileStream = File.OpenRead(path);
        using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
        using BinaryReader reader = new(gzipStream);

        // Tint
        Color tint = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        // Spawn
        Point spawn = new(reader.ReadByte(), reader.ReadByte());

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            // Read tile data
            int type = reader.ReadByte();
            // Check if valid tile type
            if (type < 0 || type >= Enum.GetValues(typeof(TileType)).Length)
                throw new ArgumentException($"Invalid tile type {type} @ {i % Constants.MapSize.X}, {i / Constants.MapSize.X} in level file.");
            // Extra properties
            Tile tile;
            Point loc = new(i % Constants.MapSize.X, i / Constants.MapSize.X);
            // Stairs
            if (type == (int)TileType.Stairs)
                tile = new Stairs(loc, reader.ReadString(), new(reader.ReadByte(), reader.ReadByte()));
            // Doors
            else if (type == (int)TileType.Door)
                tile = new Door(loc, reader.ReadString());
            // Chests
            else if (type == (int)TileType.Chest)
            {
                string lootGenFile = reader.ReadString();
                ILootGenerator? lootGen = null;
                if (lootGenFile.EndsWith(".qlt"))
                    lootGen = LootTable.ReadLootTable(lootGenFile);
                else if (lootGenFile.EndsWith(".qlp"))
                    lootGen = LootPreset.ReadLootPreset(lootGenFile);
                if (lootGen == null)
                {
                    Logger.Error($"Invalid loot generator file '{lootGenFile}' for chest at {loc}.");
                    tile = new Chest(loc, LootPreset.EmptyPreset);
                }
                else
                    tile = new Chest(loc, lootGen);
            }
            // Lamps
            else if (type == (int)TileType.Lamp)
            {
                Color lampTint = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                int lampRadius = reader.ReadUInt16();
                tile = new Lamp(loc, lampTint, lampRadius);
            }
            else // Regular tile
                tile = TileFromId(type, loc);
            int idx = tile.Location.X + tile.Location.Y * Constants.MapSize.X;
            tilesBuffer[idx] = tile;
        }

        // Biomes
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
            biomeBuffer[i] = (BiomeType)reader.ReadByte();

        // NPCs
        int npcCount = reader.ReadByte();
        for (int n = 0; n < npcCount; n++)
        {
            string name = reader.ReadString();
            string dialog = reader.ReadString();
            Point location = new(reader.ReadByte(), reader.ReadByte());
            byte scale = reader.ReadByte();
            int texId = reader.ReadByte();
            TextureID texture = (TextureID)texId;
            npcBuffer.Add(new NPC(uiManager, texture, location, name, dialog, Color.White, scale / 10f));
        }

        // Loot
        byte lootCount = reader.ReadByte();
        for (int n = 0; n < lootCount; n++)
        {
            string name = reader.ReadString();
            byte amount = reader.ReadByte();
            Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
            lootBuffer.Add(new Loot(name, amount, location, 0f));
        }

        // Decals
        byte decalCount = reader.ReadByte();
        for (int n = 0; n < decalCount; n++)
        {
            int type = reader.ReadByte();
            Point location = new(reader.ReadByte(), reader.ReadByte());
            decalBuffer.Add(DecalFromId((DecalType)type, location));
        }

        // Make and add the level
        Level created = new(filename, tilesBuffer, biomeBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, [], tint);
        if (reload && Levels.Contains(Level))
            Levels[Levels.IndexOf(Level)] = created;
        else
            Levels.Add(created);
        Logger.System($"Successfully read level '{filename}'.");
        return true;
    }
    public static Tile TileFromId(int id, Point location)
    {
        // Create a tile from an id
        TileType type = (TileType)id;
        return type switch
        {
            TileType.Sky => new Sky(location),
            TileType.Grass => new Grass(location),
            TileType.Water => new Water(location),
            TileType.StoneWall => new StoneWall(location),
            TileType.Stairs => new Stairs(location, "", Constants.MiddleCoord),
            TileType.Flooring => new Flooring(location),
            TileType.Sand => new Sand(location),
            TileType.Dirt => new Dirt(location),
            TileType.Darkness => new Darkness(location),
            TileType.WoodFlooring => new WoodFlooring(location),
            TileType.Stone => new Stone(location),
            TileType.Door => new Door(location, ""),
            TileType.Chest => new Chest(location, LootPreset.EmptyPreset),
            TileType.ConcreteWall => new ConcreteWall(location),
            TileType.WoodWall => new WoodWall(location),
            TileType.Path => new Tiles.Path(location),
            TileType.Lava => new Lava(location),
            TileType.StoneTiles => new StoneTiles(location),
            TileType.RedTiles => new RedTiles(location),
            TileType.OrangeTiles => new OrangeTiles(location),
            TileType.YellowTiles => new YellowTiles(location),
            TileType.LimeTiles => new LimeTiles(location),
            TileType.GreenTiles => new GreenTiles(location),
            TileType.CyanTiles => new CyanTiles(location),
            TileType.BlueTiles => new BlueTiles(location),
            TileType.PurpleTiles => new PurpleTiles(location),
            TileType.PinkTiles => new PinkTiles(location),
            TileType.BlackTiles => new BlackTiles(location),
            TileType.BrownTiles => new BrownTiles(location),
            TileType.IronWall => new IronWall(location),
            TileType.Snow => new Snow(location),
            TileType.Ice => new Ice(location),
            TileType.SnowyGrass => new SnowyGrass(location),
            TileType.Lamp => new Lamp(location),
            // TILEFROMID INSERT
            _ => throw new ArgumentException($"Unknown TileFromId TileType '{id}'.")
        };
    }
    public static Tile TileFromId(TileType id, Point location) => TileFromId((int)id, location);
    public static Decal DecalFromId(DecalType id, Point location)
    {
        // Create a decal from an id
        Decal? dec = id switch
        {
            DecalType.Torch => new Torch(location),
            DecalType.BlueTorch => new BlueTorch(location),
            DecalType.WaterPuddle => new WaterPuddle(location),
            DecalType.BloodPuddle => new BloodPuddle(location),
            DecalType.Footprint => new Footprint(location),
            DecalType.Pebbles => new Pebbles(location),
            DecalType.Bush1 => new Bush1(location),
            DecalType.Bush2 => new Bush2(location),
            DecalType.Bush3 => new Bush3(location),
            DecalType.SnowyBush1 => new SnowyBush1(location),
            DecalType.SnowyBush2 => new SnowyBush2(location),
            DecalType.SnowyBush3 => new SnowyBush3(location),
            // DECALFROMID INSERT
            _ => null,
        };
        if (dec == null)
        {
            Logger.Error($"Unknown DecalFromId DecalType '{id}'.");
            return new Torch(location);
        }
        return dec;
    }
    public int TileConnectionsMask(Tile tile)
    {
        int mask = 0;
        int x = tile.Location.X;
        int y = tile.Location.Y;

        Tile? left = GetTile(x - 1, y);
        Tile? right = GetTile(x + 1, y);
        Tile? up = GetTile(x, y - 1);
        Tile? down = GetTile(x, y + 1);

        if (left == null || left.Type == tile.Type || (tile.IsWall && left.IsWall)) mask |= 1; // left
        if (down == null || down.Type == tile.Type || (tile.IsWall && down.IsWall)) mask |= 2; // down
        if (right == null || right.Type == tile.Type || (tile.IsWall && right.IsWall)) mask |= 4; // right
        if (up == null || up.Type == tile.Type || (tile.IsWall && up.IsWall)) mask |= 8; // up

        return mask;
    }
    public int BiomeConnectionsMask(Point loc)
    {
        int mask = 0;
        int x = loc.X;
        int y = loc.Y;

        BiomeType? center = GetBiome(x, y);
        BiomeType? left = GetBiome(x - 1, y);
        BiomeType? right = GetBiome(x + 1, y);
        BiomeType? up = GetBiome(x, y - 1);
        BiomeType? down = GetBiome(x, y + 1);

        if (center == null) return mask;
        if (left == null || left == center) mask |= 1; // left
        if (down == null || down == center) mask |= 2; // down
        if (right == null || right == center) mask |= 4; // right
        if (up == null || up == center) mask |= 8; // up

        return mask;
    }
    public Rectangle TileTextureSource(Tile tile)
    {

        int mask = TileConnectionsMask(tile);

        int srcX = (mask % Constants.TileMapDim.X) * Constants.TilePixelSize.X;
        int srcY = (mask / Constants.TileMapDim.X) * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
    }
    public Rectangle BiomeTextureSource(Point loc)
    {

        int mask = BiomeConnectionsMask(loc);

        int srcX = (mask % Constants.TileMapDim.X) * Constants.TilePixelSize.X;
        int srcY = (mask / Constants.TileMapDim.X) * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
    }
    public BiomeType? GetBiome(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Level.Biome[coord.X + coord.Y * Constants.MapSize.X];
    }
    public BiomeType? GetBiome(int x, int y)
    {
        if (x < 0 || x >= Constants.MapSize.X || y < 0 || y >= Constants.MapSize.Y)
            return null;
        return Level.Biome[x + y * Constants.MapSize.X];
    }
    public Tile? GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public Tile? GetTile(int x, int y)
    {
        if (x < 0 || x >= Constants.MapSize.X || y < 0 || y >= Constants.MapSize.Y)
            return null;
        return Level.Tiles[x + y * Constants.MapSize.X];
    }
    public void DropLoot(GameManager gameManager, Loot loot)
    {
        Level.Loot.Add(loot);
        gameManager.UIManager.LootNotifications.AddNotification($"-{loot.DisplayName}");
    }
    public static Point TileCoord(Point loc) => new(loc.X / Constants.TileSize.X, loc.Y / Constants.TileSize.Y);
    public static Point TileCoord(Vector2 loc) => new((int)(loc.X / Constants.TileSize.X), (int)(loc.Y / Constants.TileSize.Y));
    public static Vector2 WorldCoord(Point tileCoord) => new(tileCoord.X * Constants.TileSize.X, tileCoord.Y * Constants.TileSize.Y);
    public static Vector2 WorldCoord(Vector2 tileCoord) => new(tileCoord.X * Constants.TileSize.X, tileCoord.Y * Constants.TileSize.Y);
    public static int Flatten(Point point) => point.X + point.Y * Constants.MapSize.X;
}
