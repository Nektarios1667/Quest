using System.IO;
using System.IO.Compression;
using System.Text;
using static Quest.Editor.PopupFactory;

namespace Quest.Editor;
public enum EditorTool
{
    Tile,
    Decal,
    Biome,
}
public class EditorManager
{
    // Debug
    private static readonly Color[] colors = {
        Color.Purple, new(255, 128, 128), new(128, 255, 128), new(255, 255, 180), new(128, 255, 255),
        Color.Brown, Color.Gray, new(192, 128, 64), new(64, 128, 192), new(192, 192, 64),
        new(64, 192, 128), new(192, 64, 128), new(160, 80, 0), new(80, 160, 0), new(0, 160, 80),
        new(160, 0, 80), new(96, 96, 192), new(192, 96, 96), new(96, 192, 96), new(192, 192, 96)
    };
    private GameManager gameManager { get; }
    private LevelManager levelManager { get; }
    private LevelGenerator levelGenerator { get; }
    private StringBuilder debugSb { get; }
    private GraphicsDevice graphics { get; }
    private SpriteBatch spriteBatch { get; }
    private float delta { get; set; }
    private float cacheDelta { get; set; }
    private Dictionary<string, double> frameTimes { get; set; } = new();
    private Tile? mouseTile { get; set; }
    private Point mouseCoord { get; set; }
    private Point mouseSelection { get; set; }
    private Point mouseSelectionCoord { get; set; }
    private TileTypeID tileSelection { get; set; }
    private EditorTool currentTool { get; set; }
    private BiomeType biomeSelection { get; set; }
    private RenderTarget2D minimap { get; set; }
    private bool rebuildMinimap { get; set; } = true;
    private DecalType? previousDecal { get; set; } = null;
    private string world = "";
    public EditorManager(GraphicsDevice graphics, GameManager gameManager, LevelManager levelManager, LevelGenerator levelGenerator, SpriteBatch batch, StringBuilder debugSb)
    {
        this.graphics = graphics;
        this.gameManager = gameManager;
        this.levelManager = levelManager;
        this.levelGenerator = levelGenerator;
        this.debugSb = debugSb;
        this.spriteBatch = batch;
        cacheDelta = delta;
    }
    public void Update(TileTypeID material, BiomeType biome, EditorTool tool, float deltaTime, Tile? mouseTile, Point mouseCoord, Point mouseSelection, Point mouseSelectionCoord)
    {
        delta = deltaTime;
        this.mouseTile = mouseTile;
        this.mouseCoord = mouseCoord;
        this.mouseSelection = mouseSelection;
        this.mouseSelectionCoord = mouseSelectionCoord;
        currentTool = tool;
        tileSelection = material;
        biomeSelection = biome;

        if (rebuildMinimap) RebuildMiniMap();
    }
    public void DrawMiniMap()
    {
        DebugManager.StartBenchmark("DrawMinimap");

        // Frame
        gameManager.Batch.DrawRectangle(new(7, Constants.NativeResolution.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Draw minimap texture
        if (minimap != null)
            spriteBatch.Draw(minimap, new Rectangle(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10);
        spriteBatch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }

    public void RebuildMiniMap()
    {
        minimap = new RenderTarget2D(graphics, Constants.MapSize.X, Constants.MapSize.Y);
        graphics.SetRenderTarget(minimap);
        graphics.Clear(Color.Transparent);
        spriteBatch.Begin();

        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                Tile tile = gameManager.LevelManager.GetTile(new Point(x, y))!;
                spriteBatch.DrawPoint(new(x, y), Constants.MiniMapColors[(byte)tile.Type.ID]);
            }
        }

        spriteBatch.End();
        graphics.SetRenderTarget(null);
        rebuildMinimap = false;
    }
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 190, 0, 190, (int)boxHeight), Color.Black * 0.8f);

        debugSb.Clear();
        foreach (var kv in frameTimes)
        {
            debugSb.Append(kv.Key);
            debugSb.Append(": ");
            debugSb.AppendFormat("{0:0.0}ms", kv.Value);
            debugSb.Append('\n');
        }

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(Constants.NativeResolution.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        FillRectangle(spriteBatch, new(0, 0, 200, 200), Color.Black * 0.8f);

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nGameTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.GameTime);
        debugSb.Append("\nDayTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.DayTime);
        debugSb.Append("\nTotalTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.TotalTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(mouseTile == null ? "none" : mouseTile.Type);
        if (mouseTile != null)
        {
            debugSb.Append("\nMouse Tile: ");
            debugSb.AppendFormat("{0:0},{1:0}", mouseTile.Location.X, mouseTile.Location.Y);
        }
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Name);
        debugSb.Append("\nGUI: ");
        debugSb.Append(gameManager.UIManager.Gui.Widgets.Count);

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        // Background
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 320, Constants.NativeResolution.Y - frameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 310, Constants.NativeResolution.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in frameTimes)
        {
            spriteBatch.DrawString(Arial, process.Key, new Vector2(Constants.NativeResolution.X - Arial.MeasureString(process.Key).X - 5, Constants.NativeResolution.Y - 20 * c - 60), colors[c]);
            FillRectangle(spriteBatch, new Rectangle(Constants.NativeResolution.X - 310 + start, Constants.NativeResolution.Y - 40, (int)(process.Value / (cacheDelta * 1000) * 300), 25), colors[c]);
            start += (int)(process.Value / (cacheDelta * 1000)) * 300;
            c++;
        }
    }
    public void UpdateFrameTimes()
    {
        frameTimes.Clear();
        frameTimes = new(DebugManager.FrameTimes);
        cacheDelta = gameManager.DeltaTime;
    }
    public void EditTile()
    {
        Tile? tile = levelManager.GetTile(mouseSelectionCoord);
        // Stair
        if (tile is Stairs stairs)
        {
            var (success, values) = ShowInputForm("Stair Editor", [new("Level", null), new("Spawn X", IsByte), new("Spawn Y", IsByte)]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Stair edit failed.");
                return;
            }
            if (values[0].Contains('\\') || values[0].Contains('/'))
            {
                Logger.Error("Invalid level format. Stairs can not go to other worlds.");
                return;
            }

            // Level
            stairs.DestLevel = $"{world}\\{values[0]}";
            stairs.DestPosition = new(int.Parse(values[1]), int.Parse(values[2]));
        }
        // Door
        else if (tile is Door door)
        {
            var (success, values) = ShowInputForm("Door Editor", [new("Key", null)]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Stair edit failed.");
                return;
            }
            door.Key = values[0];
        }
        // Chest
        else if (tile is Chest chest)
        {
            var (success, values) = ShowInputForm("Chest Editor", [new("Loot File Name", null), new("Loot Type", null, ["Loot Preset", "Loot Table"])]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Chest edit failed.");
                return;
            }
            if (values[1] == "Loot Table")
                chest.RegenerateLoot(LootTable.ReadLootTable($"GameData\\Worlds\\{world}\\loot\\{values[0]}.qlt"));
            else if (values[1] == "Loot Preset")
                chest.RegenerateLoot(LootPreset.ReadLootPreset($"GameData\\Worlds\\{world}\\loot\\{values[0]}.qlp"));
            else
                Logger.Error("Chest edit failed.");
        }
        // Lamp
        else if (tile is Lamp lamp)
        {
            var (success, values) = ShowInputForm("Lamp Editor", [new("Color R", IsByte), new("Color G", IsByte), new("Color B", IsByte), new("Light Strength", IsByte), new("Light Radius", IsUInt16)]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Lamp edit failed.");
                return;
            }
            lamp.LightColor = new(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2]), byte.Parse(values[3]));
            lamp.LightRadius = ushort.Parse(values[4]);
        }
    }
    public void FloodFill()
    {
        if (currentTool == EditorTool.Tile) FloodFillTiles();
        else if (currentTool == EditorTool.Decal) { } // TODO
        else if (currentTool == EditorTool.Biome) FloodFillBiome();
    }
    public void FloodFillTiles()
    {
        // Fill with current material
        int count = 0;
        Tile tileBelow = GetTile(mouseCoord);
        if (tileBelow.Type.ID != tileSelection)
        {
            Queue<Tile> queue = new();
            HashSet<Point> visited = []; // Track visited tiles
            queue.Enqueue(tileBelow);
            count++;
            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();
                if (current.Type.ID == tileSelection || visited.Contains(current.Location)) continue; // Skip if already filled
                count++;
                SetTile(LevelManager.TileFromId((int)tileSelection, current.Location));
                visited.Add(current.Location); // Mark as visited
                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current.Location + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    Tile neighborTile = GetTile(neighborCoord);
                    if (neighborTile.Type == tileBelow.Type && neighborTile.Type.ID != tileSelection)
                    {
                        queue.Enqueue(neighborTile);
                    }
                }
            }
            Logger.Log($"Filled {count} tiles with '{tileSelection}' starting from {mouseCoord.X}, {mouseCoord.Y}.");
        }
    }
    public void FloodFillBiome()
    {
        // Fill with current material
        int count = 0;
        BiomeType? startBiome = levelManager.GetBiome(mouseCoord);
        if (startBiome != biomeSelection)
        {
            Queue<Point> queue = new();
            HashSet<Point> visited = []; // Track visited tiles
            queue.Enqueue(mouseCoord);
            count++;
            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                if (levelManager.GetBiome(current) == biomeSelection || visited.Contains(current)) continue; // Skip if already filled
                visited.Add(current); // Mark as visited
                count++;
                levelManager.Level.Biome[LevelManager.Flatten(current)] = biomeSelection;
                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    BiomeType? biome = levelManager.GetBiome(neighborCoord);
                    if (biome == startBiome && biome != biomeSelection)
                    {
                        queue.Enqueue(neighborCoord);
                    }
                }
            }
            Logger.Log($"Set {count} tiles to biome '{biomeSelection}' starting from {mouseCoord.X}, {mouseCoord.Y}.");
        }
    }
    public void SetSpawn()
    {
        levelManager.Level.Spawn = mouseSelectionCoord;
        Logger.Log($"Set level spawn to {mouseSelectionCoord.X}, {mouseSelectionCoord.Y}");
    }
    public void SetTint()
    {
        // Winforms
        var (success, values) = ShowInputForm("Tint Editor", [new("R", IsByte), new("G", IsByte), new("B", IsByte), new("A", IsByte)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to set tint.");
            return;
        }
        levelManager.Level.Tint = new Color(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2])) * (byte.Parse(values[3]) / 255f);
    }
    public void EditNPCs()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
            DeleteNPC();
        else // New
            NewNPC();
    }
    public void NewNPC()
    {
        // Check
        if (levelManager.Level.NPCs.Count >= 255)
        {
            Logger.Error("Maximum number of NPCs reached (255).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("NPC Editor", [new("Name", null), new("Dialog", null), new("Size [1-25.5]", IsScaleValue), new("Texture", null, Enum.GetNames(typeof(TextureID)))]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("NPC creation failed.");
            return;
        }

        // Create
        string name = values[0];
        string dialog = values[1];
        float scale = float.Parse(values[2]);
        if (scale <= 0 || scale > 25.5)
        {
            Logger.Warning("Scale must be between 1 and 25.5. Scale defaulted to 1.");
            scale = 1;
        }
        TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), values[3]);
        levelManager.Level.NPCs.Add(new NPC(gameManager.UIManager, texture, mouseSelectionCoord, name, dialog, Color.White, scale));
    }
    public void DeleteNPC()
    {
        foreach (NPC npc in levelManager.Level.NPCs)
        {
            if (npc.Location == mouseSelectionCoord)
            {
                levelManager.Level.NPCs.Remove(npc);
                Logger.Log($"Deleted NPC '{npc.Name}' @ {mouseSelectionCoord.X}, {mouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void EditDecals()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
            DeleteDecal();
        else if (InputManager.KeyDown(Keys.LeftAlt)) // Paste
            PasteDecal();
        else // New
            NewDecal();
    }
    public void NewDecal()
    {
        // Check
        if (levelManager.Level.Decals.Count >= 255)
        {
            Logger.Error("Maximum number of Decals reached (255).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("Decal Editor", [new("Decal", null, Constants.DecalTypeNames)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Decal creation failed.");
            return;
        }

        string name = values[0];
        DecalType decal = Enum.TryParse<DecalType>(name, true, out var dec) ? dec : DecalType.Torch;
        previousDecal = decal;
        levelManager.Level.Decals.Add(LevelManager.DecalFromId(decal, mouseSelectionCoord));
    }
    public void PasteDecal()
    {
        // Check
        if (previousDecal == null) return;
        if (levelManager.Level.Decals.Count >= 255)
        {
            Logger.Error("Maximum number of Decals reached (255).");
            return;
        }

        levelManager.Level.Decals.Add(LevelManager.DecalFromId(previousDecal.Value, mouseCoord));
    }
    public void DeleteDecal()
    {
        foreach (Decal decal in levelManager.Level.Decals)
        {
            if (decal.Location == mouseSelectionCoord)
            {
                levelManager.Level.Decals.Remove(decal);
                Logger.Log($"Deleted decal '{decal.Type}' @ {mouseSelectionCoord.X}, {mouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void EditLoot()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
            DeleteLoot();
        else // New
            NewLoot();
    }
    public void NewLoot()
    {
        // Check
        if (levelManager.Level.Loot.Count >= 255)
        {
            Logger.Error("Maximum number of Loot reached (255).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("Loot Editor", [new("Item", null, Constants.ItemTypeNames), new("Amount", IsByte)]);
        if (!success || values[1] == "0")
        {
            if (!PopupOpen) Logger.Error("Loot creation failed.");
            return;
        }

        string name = values[0];
        byte amount = byte.Parse(values[1]);
        levelManager.Level.Loot.Add(new Loot(name, amount, mouseSelection, gameManager.GameTime));
    }
    public void DeleteLoot()
    {
        foreach (Loot loot in levelManager.Level.Loot)
        {
            if (Vector2.DistanceSquared(loot.Location.ToVector2(), mouseSelection.ToVector2()) < 900)
            {
                levelManager.Level.Loot.Remove(loot);
                Logger.Log($"Deleted loot '{loot.Item}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                break;
            }
        }
    }
    public void SaveLevel()
    {
        // Winforms
        var (success, values) = ShowInputForm("Save Level", [new("Name", null)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to save level.");
            return;
        }

        // Parse
        if (values[0].Contains('\\') || values[0].Contains('/'))
        {
            Logger.Error($"Invalid level format. File will be outputted in world '{world}' as it can not be part of another world.");
            return;
        }
        Directory.CreateDirectory($"../../../GameData/Worlds/{world}");
        Directory.CreateDirectory($"../../../GameData/Worlds/{world}/levels");
        Directory.CreateDirectory($"../../../GameData/Worlds/{world}/loot");
        Directory.CreateDirectory($"../../../GameData/Worlds/{world}/saves");
        using FileStream fileStream = File.Create($"../../../GameData/Worlds/{world}/levels/{values[0]}.qlv");
        using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(gzipStream);

        // Write tint
        writer.Write(levelManager.Level.Tint.R);
        writer.Write(levelManager.Level.Tint.G);
        writer.Write(levelManager.Level.Tint.B);
        writer.Write(levelManager.Level.Tint.A);

        // Write spawn
        writer.Write(LevelEditor.IntToByte(levelManager.Level.Spawn.X));
        writer.Write(LevelEditor.IntToByte(levelManager.Level.Spawn.Y));

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            Tile tile = levelManager.Level.Tiles[i];
            // Write tile data
            writer.Write((byte)tile.Type.ID);
            // Extra properties
            if (tile is Stairs stairs)
            {
                // Write destination
                writer.Write(stairs.DestLevel);
                writer.Write(LevelEditor.IntToByte(stairs.DestPosition.X));
                writer.Write(LevelEditor.IntToByte(stairs.DestPosition.Y));
            }
            else if (tile is Door door)
            {
                // Write door key
                writer.Write(door.Key);
            }
            else if (tile is Chest chest)
            {
                // Write chest loot
                writer.Write(chest.LootGenerator.FileName);
            }
            else if (tile is Lamp lamp)
            {
                // Write lamp data
                writer.Write(lamp.LightColor.R);
                writer.Write(lamp.LightColor.G);
                writer.Write(lamp.LightColor.B);
                writer.Write(lamp.LightColor.A);
                writer.Write((ushort)lamp.LightRadius);
            }

        }

        // Biome
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
            writer.Write((byte)(int)levelManager.Level.Biome[i]);

        // NPCs
        writer.Write((byte)Math.Min(levelManager.Level.NPCs.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.NPCs.Count, 255); n++)
        {
            NPC npc = levelManager.Level.NPCs[n];
            // Write NPC data
            writer.Write(npc.Name);
            writer.Write(npc.Dialog);
            writer.Write(LevelEditor.IntToByte(npc.Location.X));
            writer.Write(LevelEditor.IntToByte(npc.Location.Y));
            writer.Write(LevelEditor.IntToByte((int)(npc.Scale * 10)));
            writer.Write(LevelEditor.IntToByte((int)npc.Texture));
        }

        // Floor loot
        writer.Write((byte)Math.Min(levelManager.Level.Loot.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.Loot.Count, 255); n++)
        {
            Loot loot = levelManager.Level.Loot[n];
            // Write loot data
            writer.Write(loot.Item);
            writer.Write(LevelEditor.IntToByte(loot.Amount));
            writer.Write((UInt16)loot.Location.X);
            writer.Write((UInt16)loot.Location.Y);
        }

        // Decals
        writer.Write((byte)Math.Min(levelManager.Level.Decals.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.Decals.Count, 255); n++)
        {
            Decal decal = levelManager.Level.Decals[n];
            // Write decal data
            writer.Write((byte)decal.Type);
            writer.Write(LevelEditor.IntToByte(decal.Location.X));
            writer.Write(LevelEditor.IntToByte(decal.Location.Y));
        }

        // Log
        Logger.Log($"Exported level to '{values[0]}.qlv'.");
    }
    public void GenerateLevel()
    {
        // Winforms
        var (success, values) = ShowInputForm("Generate Level", [new("Seed", IsInteger), new("Terrain", null, [.. levelGenerator.Terrains.Keys]), new("Structure Attempts", IsPositiveIntegerOrZero)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Level generation failed.");
            return;
        }

        // Generate
        levelGenerator.Seed = int.Parse(values[0]);
        levelGenerator.Terrain = levelGenerator.Terrains.GetValueOrDefault(values[1], levelGenerator.Terrain);
        Tile[] tiles = levelGenerator.GenerateLevel(Constants.MapSize, int.Parse(values[2]));

        Level current = levelManager.Level;
        Level level = new(current.Name, tiles, [], current.Spawn, current.NPCs, current.Loot, current.Decals, current.Enemies, current.Tint);

        levelManager.LoadLevelObject(gameManager, level);
        FlagRebuildMinimap();
    }
    public void OpenFile()
    {
        // Winforms
        var (success, values) = ShowInputForm("Open File", [new("File Name", null)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to open file.");
            return;
        }

        // Open
        string filename = values[0];
        if (!filename.Contains('/') && !filename.Contains('\\'))
        {
            Logger.Error("Invalid level name. Use format 'WorldName/LevelName'.");
            return;
        }
        world = filename.Split('\\', '/')[0];
        try
        {
            levelManager.ReadLevel(gameManager.UIManager, filename, reload: true);
            levelManager.LoadLevel(gameManager, filename);
            Logger.Log($"Opened level '{filename}'.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to open level '{filename}': {ex.Message}");
        }

        FlagRebuildMinimap();
    }
    public Tile GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
        return levelManager.Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public void SetTile(Tile tile)
    {
        levelManager.Level.Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
        FlagRebuildMinimap();
    }
    public void FlagRebuildMinimap() { rebuildMinimap = true; }
}
