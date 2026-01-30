using Quest.Managers;
using Quest.Utilities;
using SharpDX.Direct3D9;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using static Quest.Editor.PopupFactory;
using IO = System.IO;

namespace Quest.Editor;
public enum EditorTool : byte
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
    // Settings
    public bool ShowBiomeMarkers = true;
    // Private
    private GameManager GameManager { get; }
    private LevelManager LevelManager { get; }
    private LevelGenerator LevelGenerator { get; }
    private StringBuilder DebugSb { get; }
    private GraphicsDevice Graphics { get; }
    private SpriteBatch SpriteBatch { get; }
    private float Delta;
    private float CacheDelta;
    private Dictionary<string, double> FrameTimes = [];
    private Tile? MouseTile;
    private Point MouseCoord;
    private Point MouseSelection;
    private Point MouseSelectionCoord;
    private TileTypeID TileSelection;
    private EditorTool CurrentTool;
    private BiomeType BiomeSelection;
    private RenderTarget2D Minimap = null!;
    private bool RebuildMinimap = true;
    private DecalType? PreviousDecal = null;
    private string world = "";
    public EditorManager(GraphicsDevice graphics, GameManager gameManager, LevelManager levelManager, LevelGenerator levelGenerator, SpriteBatch batch, StringBuilder debugSb)
    {
        Graphics = graphics;
        GameManager = gameManager;
        LevelManager = levelManager;
        LevelGenerator = levelGenerator;
        DebugSb = debugSb;
        SpriteBatch = batch;
        CacheDelta = Delta;
    }
    public void Update(TileTypeID material, BiomeType biome, EditorTool tool, float deltaTime, Tile? mouseTile, Point mouseCoord, Point mouseSelection, Point mouseSelectionCoord)
    {
        Delta = deltaTime;
        MouseTile = mouseTile;
        MouseCoord = mouseCoord;
        MouseSelection = mouseSelection;
        MouseSelectionCoord = mouseSelectionCoord;
        CurrentTool = tool;
        TileSelection = material;
        BiomeSelection = biome;

        if (RebuildMinimap) RebuildMiniMap();
    }
    public void DrawBiomes()
    {
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                Point loc = new(x, y);
                Point dest = loc * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
                BiomeType? biome = LevelManager.GetBiome(loc);
                Color color = biome == null ? Color.Magenta : Biome.Colors[(int)biome];
                SpriteBatch.Draw(Textures[TextureID.TileOutline], dest.ToVector2(), LevelManager.BiomeTextureSource(loc), color, 0, Vector2.Zero, Constants.TileSizeScale, SpriteEffects.None, 1.0f);
            }
        }
    }
    public void DrawMiniMap()
    {
        DebugManager.StartBenchmark("DrawMinimap");

        // Frame
        GameManager.Batch.DrawRectangle(new(7, Constants.NativeResolution.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Draw minimap texture
        if (Minimap != null)
            SpriteBatch.Draw(Minimap, new Rectangle(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10);
        SpriteBatch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }
    public void RebuildMiniMap()
    {
        Minimap = new RenderTarget2D(Graphics, Constants.MapSize.X, Constants.MapSize.Y);
        Graphics.SetRenderTarget(Minimap);
        Graphics.Clear(Color.Transparent);
        SpriteBatch.Begin();

        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                Tile tile = GameManager.LevelManager.GetTile(new Point(x, y))!;
                SpriteBatch.DrawPoint(new(x, y), Constants.MiniMapColors[(byte)tile.Type.ID]);
            }
        }

        SpriteBatch.End();
        Graphics.SetRenderTarget(null);
        RebuildMinimap = false;
    }
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        FillRectangle(SpriteBatch, new(Constants.NativeResolution.X - 190, 0, 190, (int)boxHeight), Color.Black * 0.8f);

        DebugSb.Clear();
        foreach (var kv in FrameTimes)
        {
            DebugSb.Append(kv.Key);
            DebugSb.Append(": ");
            DebugSb.AppendFormat("{0:0.0}ms", kv.Value);
            DebugSb.Append('\n');
        }

        SpriteBatch.DrawString(Arial, DebugSb.ToString(), new Vector2(Constants.NativeResolution.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        FillRectangle(SpriteBatch, new(0, 0, 200, 200), Color.Black * 0.8f);

        DebugSb.Clear();
        DebugSb.Append("FPS: ");
        DebugSb.AppendFormat("{0:0.0}", CacheDelta != 0 ? 1f / CacheDelta : 0);
        DebugSb.Append("\nGameTime: ");
        DebugSb.AppendFormat("{0:0.00}", GameManager.GameTime);
        DebugSb.Append("\nDayTime: ");
        DebugSb.AppendFormat("{0:0.00}", GameManager.DayTime);
        DebugSb.Append("\nTotalTime: ");
        DebugSb.AppendFormat("{0:0.00}", GameManager.TotalTime);
        DebugSb.Append("\nCamera: ");
        DebugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        DebugSb.Append("\nTile Below: ");
        DebugSb.Append(MouseTile == null ? "none" : MouseTile.Type.Texture);
        if (MouseTile != null)
        {
            DebugSb.Append("\nMouse Tile: ");
            DebugSb.AppendFormat("{0:0},{1:0}", MouseTile.X, MouseTile.Y);
        }
        DebugSb.Append("\nCoord: ");
        DebugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        DebugSb.Append("\nLevel: ");
        DebugSb.Append(LevelManager.Level?.Name);
        DebugSb.Append("\nGUI: ");
        DebugSb.Append(GameManager.UIManager.Gui.Widgets.Count);

        SpriteBatch.DrawString(Arial, DebugSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        // Background
        FillRectangle(SpriteBatch, new(Constants.NativeResolution.X - 320, Constants.NativeResolution.Y - FrameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        FillRectangle(SpriteBatch, new(Constants.NativeResolution.X - 310, Constants.NativeResolution.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in FrameTimes)
        {
            SpriteBatch.DrawString(Arial, process.Key, new Vector2(Constants.NativeResolution.X - Arial.MeasureString(process.Key).X - 5, Constants.NativeResolution.Y - 20 * c - 60), colors[c]);
            FillRectangle(SpriteBatch, new Rectangle(Constants.NativeResolution.X - 310 + start, Constants.NativeResolution.Y - 40, (int)(process.Value / (CacheDelta * 1000) * 300), 25), colors[c]);
            start += (int)(process.Value / (CacheDelta * 1000)) * 300;
            c++;
        }
    }
    public void UpdateFrameTimes()
    {
        FrameTimes.Clear();
        FrameTimes = new(DebugManager.FrameTimes);
        CacheDelta = GameManager.DeltaTime;
    }
    public void EditTile()
    {
        Tile? tile = LevelManager.GetTile(MouseSelectionCoord);
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
            stairs.DestLevel = $"{world}/{values[0]}";
            stairs.Dest = new(byte.Parse(values[1]), byte.Parse(values[2]));
        }
        // Door
        else if (tile is Door door)
        {
            var (success, values) = ShowInputForm("Door Editor", [new("Key", null, Enum.GetNames(typeof(ItemTypeID))), new("Amount", IsByte), new("Consume Key", null, ["true", "false"])]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Stair edit failed.");
                return;
            }
            door.Key = new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[0])], byte.Parse(values[1]));
            door.ConsumeKey = bool.Parse(values[2]);
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
                chest.RegenerateLoot(LootTable.ReadLootTable(world, $"{values[0]}.qlt"));
            else if (values[1] == "Loot Preset")
                chest.RegenerateLoot(LootPreset.ReadLootPreset(world, $"{values[0]}.qlp"));
            else
                Logger.Error("Chest edit failed.");
        }
        // Lamp
        else if (tile is Lamp lamp)
        {
            var (success, values) = ShowInputForm("Lamp Editor", [new("Light Radius", IsByte)]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Lamp edit failed.");
                return;
            }
            lamp.LightRadius = byte.Parse(values[4]);
        }
    }
    public void FloodFill()
    {
        if (CurrentTool == EditorTool.Tile) FloodFillTiles();
        else if (CurrentTool == EditorTool.Decal) { } // TODO
        else if (CurrentTool == EditorTool.Biome) FloodFillBiome();
    }
    public void FloodFillTiles()
    {
        // Fill with current material
        var sw = Stopwatch.StartNew();
        int count = 0;
        Tile tileBelow = GetTile(MouseCoord);
        if (tileBelow.Type.ID != TileSelection)
        {
            Queue<Tile> queue = new();
            HashSet<ByteCoord> visited = []; // Track visited tiles
            queue.Enqueue(tileBelow);
            count++;
            
            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();
                if (current.Type.ID == TileSelection || visited.Contains(current.Location)) continue; // Skip if already filled
                count++;
                SetTile(new Tile(current.Location, TileSelection));
                visited.Add(current.Location); // Mark as visited

                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current.Location + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    Tile neighborTile = GetTile(neighborCoord);
                    if (neighborTile.Type == tileBelow.Type && neighborTile.Type.ID != TileSelection)
                    {
                        queue.Enqueue(neighborTile);
                    }
                }
            }
            sw.Stop();
            Logger.Log($"Filled {count} tiles with '{TileSelection}' starting from {MouseCoord.X}, {MouseCoord.Y} in {sw.ElapsedMilliseconds:F1}ms.");
        }
    }

    public void FloodFillBiome()
    {
        // Fill with current material
        int count = 0;
        BiomeType? startBiome = LevelManager.GetBiome(MouseCoord);
        if (startBiome != BiomeSelection)
        {
            Queue<Point> queue = new();
            HashSet<Point> visited = []; // Track visited tiles
            queue.Enqueue(MouseCoord);
            count++;
            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();
                if (LevelManager.GetBiome(current) == BiomeSelection || visited.Contains(current)) continue; // Skip if already filled
                visited.Add(current); // Mark as visited
                count++;
                LevelManager.Level.Biome[LevelManager.Flatten(current)] = BiomeSelection;
                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    BiomeType? biome = LevelManager.GetBiome(neighborCoord);
                    if (biome == startBiome && biome != BiomeSelection)
                    {
                        queue.Enqueue(neighborCoord);
                    }
                }
            }
            Logger.Log($"Set {count} tiles to biome '{BiomeSelection}' starting from {MouseCoord.X}, {MouseCoord.Y}.");
        }
    }
    public void SetSpawn()
    {
        LevelManager.Level.Spawn = MouseSelectionCoord;
        Logger.Log($"Set level spawn to {MouseSelectionCoord.X}, {MouseSelectionCoord.Y}");
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
        LevelManager.Level.Tint = new Color(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2])) * (byte.Parse(values[3]) / 255f);
    }
    public void NewNPC()
    {
        // Check
        if (LevelManager.Level.NPCs.Count >= 255)
        {
            Logger.Error("Maximum number of NPCs reached (255).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("NPC Editor", [new("Name", null), new("Dialog", null), new("Size [1-25.5]", IsScaleValue), new("Texture", null, [.. CharacterTextures.Select(t => t.ToString())])]);
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
        LevelManager.Level.NPCs.Add(new NPC(GameManager.UIManager, texture, MouseSelectionCoord, name, dialog, Color.White, scale));
    }
    public void DeleteNPC()
    {
        foreach (NPC npc in LevelManager.Level.NPCs)
        {
            if (npc.Location == MouseSelectionCoord)
            {
                LevelManager.Level.NPCs.Remove(npc);
                Logger.Log($"Deleted NPC '{npc.Name}' @ {MouseSelectionCoord.X}, {MouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void NewDecal()
    {
        // Check
        if (LevelManager.Level.Decals.Count >= 255)
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
        PreviousDecal = decal;
        LevelManager.Level.Decals.Add(LevelManager.DecalFromId(decal, MouseSelectionCoord));
    }
    public void PasteDecal()
    {
        // Check
        if (PreviousDecal == null) return;
        if (LevelManager.Level.Decals.Count >= 255)
        {
            Logger.Error("Maximum number of Decals reached (255).");
            return;
        }

        LevelManager.Level.Decals.Add(LevelManager.DecalFromId(PreviousDecal.Value, MouseCoord));
    }
    public void DeleteDecal()
    {
        foreach (Decal decal in LevelManager.Level.Decals)
        {
            if (decal.Location == MouseSelectionCoord)
            {
                LevelManager.Level.Decals.Remove(decal);
                Logger.Log($"Deleted decal '{decal.Type}' @ {MouseSelectionCoord.X}, {MouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void NewLoot()
    {
        // Check
        if (LevelManager.Level.Loot.Count >= 255)
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

        ItemType name = ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[0])];
        byte amount = byte.Parse(values[1]);
        LevelManager.Level.Loot.Add(new Loot(new ItemRef(name, amount), MouseSelection, GameManager.GameTime));
    }
    public void DeleteLoot()
    {
        foreach (Loot loot in LevelManager.Level.Loot)
        {
            if (Vector2.DistanceSquared(loot.Location.ToVector2(), MouseSelection.ToVector2()) < 900)
            {
                LevelManager.Level.Loot.Remove(loot);
                Logger.Log($"Deleted loot '{loot.Item}' @ {MouseCoord.X}, {MouseCoord.Y}.");
                break;
            }
        }
    }
    public void EditScripts()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
            DeleteScript();
        else // New
            NewScript();
    }
    public void DeleteScript()
    {
        var (success, values) = ShowInputForm("Delete Script", [new("Script Name", dropdownOptions: [.. LevelManager.Level.Scripts.Select(s => s.ScriptName)])]);
        if (!success || string.IsNullOrWhiteSpace(values[0]))
        {
            if (!PopupOpen) Logger.Error("Script deletion failed.");
            return;
        }

        string name = values[0];
        LevelManager.Level.Scripts.RemoveAll(s => s.ScriptName == name);
    }
    public void NewScript()
    {
        var (success, values) = ShowInputForm("New Script", [new("Script File", null)]);
        if (!success || string.IsNullOrWhiteSpace(values[0]))
        {
            if (!PopupOpen) Logger.Error("Script creation failed.");
            return;
        }
        string path = $"GameData/Worlds/{world}/scripts/{values[0]}";
        if (!File.Exists(path))
        {
            Logger.Error($"Source file '{path}' not found.");
            return;
        }
        if (LevelManager.Level.Scripts.Any(s => s.ScriptName == values[0]))
        {
            Logger.Error($"A script with the name '{values[0]}' already exists.");
            return;
        }
        string sourceCode = File.ReadAllText(path);
        LevelManager.Level.Scripts.Add(new QuillScript(values[0], sourceCode));
    }
    public void ResaveLevel(LevelPath levelPath)
    {
        OpenLevel(levelPath.Path);
        SaveLevel(levelPath.LevelName, levelPath.WorldName);
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
            string formattedLevel = $"{world}/{IO.Path.GetFileNameWithoutExtension(level)}";
            ResaveLevel(new(formattedLevel));
        }
    }
    public void SaveLevelDialog()
    {
        // Winforms
        var (success, values) = ShowInputForm("Save Level", [new("Name", null)]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("Failed to save level.");
            return;
        }
        SaveLevel(values[0]);
    }

    public void SaveLevel(string name, string? currentWorld = null)
    {
        // Parse
        if (world == "" || world == "NUL_WORLD")
        {
            world = "new_world";
            name = name.Split('\\', '/')[0]; // Remove world
        }
        else if (name.Contains('\\') || name.Contains('/'))
        {
            Logger.Error($"Invalid level format. File has to be in the same world.");
            return;
        }
        if (currentWorld != null)
            world = currentWorld;
        string prefix = Constants.DEVMODE ? "../../../" : "";

        Directory.CreateDirectory($"{prefix}GameData/Worlds/{world}");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{world}/levels");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{world}/loot");
        Directory.CreateDirectory($"{prefix}GameData/Worlds/{world}/saves");
        using FileStream fileStream = File.Create($"{prefix}GameData/Worlds/{world}/levels/{name}.qlv");
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
                writer.Write(door.Key == null ? "" : door.Key.Name);
                if (door.Key == null) continue;
                writer.Write(door.Key.Amount);
                writer.Write(door.ConsumeKey);
            }
            else if (tile is Chest chest)
                writer.Write(chest.LootGenerator);
            else if (tile is Lamp lamp)
                writer.Write(lamp.LightRadius);
        }

        // Biome
        if (flags.HasFlag(LevelFeatures.Biomes))
            for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
                writer.Write((byte)(int)LevelManager.Level.Biome[i]);

        // NPCs
        writer.Write((byte)Math.Min(LevelManager.Level.NPCs.Count, 255));
        for (int n = 0; n < Math.Min(LevelManager.Level.NPCs.Count, 255); n++)
            writer.Write(LevelManager.Level.NPCs[n]);

        // Floor loot
        writer.Write((byte)Math.Min(LevelManager.Level.Loot.Count, 255));
        for (int n = 0; n < Math.Min(LevelManager.Level.Loot.Count, 255); n++)
            writer.Write(LevelManager.Level.Loot[n]);

        // Decals
        writer.Write((byte)Math.Min(LevelManager.Level.Decals.Count, 255));
        for (int n = 0; n < Math.Min(LevelManager.Level.Decals.Count, 255); n++)
            writer.Write(LevelManager.Level.Decals[n]);

        // Scripts
        if (flags.HasFlag(LevelFeatures.QuillScripts))
        {
            writer.Write((byte)LevelManager.Level.Scripts.Count);
            for (int s = 0; s < LevelManager.Level.Scripts.Count; s++)
            {
                QuillScript script = LevelManager.Level.Scripts[s];
                writer.Write(script.ScriptName);
            }
        }

        // Log
        Logger.Log($"Exported level to '{name}.qlv'.");
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
        Level level = new(current.Name, tiles, [], current.Spawn, current.NPCs, current.Loot, current.Decals, current.Enemies, [], current.Tint);

        LevelManager.LoadLevelObject(GameManager, level);
        FlagRebuildMinimap();
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
        world = filename.Split('\\', '/')[0];
        LevelManager.ReadLevel(GameManager, filename, reload: true);
        LevelManager.LoadLevel(GameManager, filename);
        Logger.Log($"Opened level '{filename}'.");

        FlagRebuildMinimap();
    }
    public Tile GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
        return LevelManager.Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public void SetTile(Tile tile)
    {
        LevelManager.Level.Tiles[tile.X + tile.Y * Constants.MapSize.X] = tile;
        FlagRebuildMinimap();
    }
    public void FlagRebuildMinimap() { RebuildMinimap = true; }
}
