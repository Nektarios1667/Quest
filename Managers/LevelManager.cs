using Quest.Enemies;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Quest.Managers;
public class LevelManager
{
    public List<ILootGenerator> LootGenerators = new();
    public List<Level> Levels { get; private set; }
    public Level Level { get; private set; }
    public Color SkyLight { get; private set; }

    public static readonly Point lootStackOffset = new(4, 4);
    public LevelManager()
    {
        // Empty
        Levels = [];
        Tile[] skyTiles = new Tile[256 * 256];
        for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++) skyTiles[t] = new Sky(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
        Level = new("", skyTiles, new(128, 128), [], [], [], []);

        // Read loot tables and presets
        string[] qlp = Directory.GetFiles("World\\Loot", "*.qlp");
        string[] qlt = Directory.GetFiles("World\\Loot", "*.qlt");
        foreach (string file in qlp.Concat(qlt).ToArray())
        {
            if (file.EndsWith(".qlt"))
                LootGenerators.Add(LootTable.ReadLootTable(file));
            else if (file.EndsWith(".qlp"))
                LootGenerators.Add(LootPreset.ReadLootPreset(file));
            Logger.System($"Loaded Loot file {file}.");
        }
    }
    public void Update(GameManager gameManager)
    {
        // Entities
        foreach (NPC npc in Level.NPCs) npc.Update(gameManager);
        foreach (Enemy enemy in Level.Enemies) enemy.Update(gameManager);

        // SkyTint
        SkyLight = (Level.Tint != Color.Transparent ? Level.Tint : ColorTools.GetSkyColor(gameManager.DayTime)) * .8f;

        // Dynamic lighting
        foreach (Loot loot in Level.Loot)
            if (loot.Item == "Lantern")
            {
                Point loc = loot.Location - CameraManager.Camera.ToPoint() + Constants.Middle + TextureManager.Metadata[loot.Texture].Size;
                LightingManager.SetLight($"Loot_{loot.UID}", loc, 100, Color.Transparent, 0.6f);
            }
    }
    public void Draw(GameManager gameManager)
    {
        if (StateManager.State == GameState.Game || StateManager.State == GameState.Editor)
        {
            DrawTiles(gameManager);
            DrawDecals(gameManager);
            DrawLoot(gameManager);
            DrawCharacters(gameManager);
        }
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
    public void LoadLevel(GameManager gameManager, int levelIndex)
    {
        // Check index
        if (levelIndex < -Levels.Count || levelIndex >= Levels.Count)
            throw new ArgumentOutOfRangeException(nameof(levelIndex), "Invalid level index.");

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

        // Spawn
        CameraManager.CameraDest = (Level.Spawn * Constants.TileSize).ToVector2();
        CameraManager.Camera = CameraManager.CameraDest;
    }
    public void LoadLevel(GameManager gameManager, string levelName)
    {
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == levelName)
            {
                LoadLevel(gameManager, l);
                return;
            }
        }
        // If not found throw an error
        Logger.Error($"Level '{levelName}' not found in stored levels. Make sure the level file has been read before loading.", false);
    }
    public void LoadLevelObject(GameManager gameManager, Level level)
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
    }
    public void UnloadLevel(int levelIndex)
    {
        // Check index
        if (levelIndex < 0 || levelIndex >= Levels.Count)
            throw new ArgumentOutOfRangeException(nameof(levelIndex), "Invalid level index.");
        // Unload the level
        string name = Levels[levelIndex].Name;
        if (Level == Levels[levelIndex])
            Level = new("null", [], new Point(128, 128), [], [], [], []);
        Levels.RemoveAt(levelIndex);
        Logger.System($"Unloaded level '{name}'.");
    }
    public void UnloadLevel(string levelName)
    {
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == levelName)
            {
                UnloadLevel(l);
                return;
            }
        }
        // If not found throw an error
        Logger.Error($"Level '{levelName}' not found in stored levels. Make sure the level file has been read before unloading.", true);
    }
    public void ReadLevel(UIManager uiManager, string filename)
    {
        // Check if already read
        foreach (Level level in Levels)
            if (level.Name == filename)
                return;

        // Make buffers
        Tile[] tilesBuffer = new Tile[Constants.MapSize.X * Constants.MapSize.Y];
        List<NPC> npcBuffer = [];
        List<Loot> lootBuffer = [];
        List<Decal> decalBuffer = [];

        // Context
        using FileStream fileStream = File.OpenRead($"World/Levels/{filename}.qlv");
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
            if (type == (int)TileType.Stairs)
                tile = new Stairs(loc, reader.ReadString(), new(reader.ReadByte(), reader.ReadByte()));
            else if (type == (int)TileType.Door)
                tile = new Door(loc, reader.ReadString());
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
                } else
                    tile = new Chest(loc, lootGen);
            }
            else // Regular tile
                tile = TileFromId(type, loc);
            int idx = tile.Location.X + tile.Location.Y * Constants.MapSize.X;
            tilesBuffer[idx] = tile;
        }

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
        Level created = new(filename, tilesBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, [], tint);
        Levels.Add(created);
        Logger.System($"Successfully read level '{filename}'.");
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
            TileType.WoodPlanks => new WoodPlanks(location),
            TileType.Stone => new Stone(location),
            TileType.Door => new Door(location, ""),
            TileType.Chest => new Chest(location, LootPreset.EmptyPreset),
            TileType.ConcreteWall => new ConcreteWall(location),
            TileType.WoodWall => new WoodWall(location),
            TileType.Path => new Tiles.Path(location),
            _ => throw new ArgumentException($"Unknown TileFromId TileType '{id}'.")
        };
    }
    public static Tile TileFromId(TileType id, Point location) => TileFromId((int)id, location);
    public static Decal DecalFromId(DecalType id, Point location)
    {
        // Create a decal from an id
        return id switch
        {
            DecalType.Torch => new Decals.Torch(location),
            DecalType.BlueTorch => new BlueTorch(location),
            DecalType.WaterPuddle => new WaterPuddle(location),
            DecalType.BloodPuddle => new BloodPuddle(location),
            DecalType.Footprint => new Footprint(location),
            _ => throw new ArgumentException($"Unknown DecalFromId DecalType '{id}'.")
        };
    }
    public int TileConnectionsMask(Tile tile)
    {
        int mask = 0;
        int x = tile.Location.X;
        int y = tile.Location.Y;

        Tile? left = GetTile(x - 1, y);
        Tile? right = GetTile(x + 1, y);
        Tile? down = GetTile(x, y + 1);
        Tile? up = GetTile(x, y - 1);

        if (left?.Type == tile.Type) mask |= 1; // left
        if (down?.Type == tile.Type) mask |= 2; // down
        if (right?.Type == tile.Type) mask |= 4; // right
        if (up?.Type == tile.Type) mask |= 8; // up

        return mask;
    }
    public Rectangle TileTextureSource(Tile tile)
    {

        int mask = TileConnectionsMask(tile);

        int srcX = (mask % Constants.TileMapDim.X) * Constants.TilePixelSize.X;
        int srcY = (mask / Constants.TileMapDim.X) * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
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
