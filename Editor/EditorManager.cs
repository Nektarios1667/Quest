using System.IO;
using System.IO.Compression;
using System.Text;
namespace Quest.Editor;
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
    private SpriteBatch spriteBatch { get; }
    private float delta { get; set; }
    private float cacheDelta { get; set; }
    private Dictionary<string, double> frameTimes { get; set; } = new();
    private Tile? mouseTile { get; set; }
    private Point mouseCoord { get; set; }
    private TileType Material { get; set; }
    public EditorManager(GameManager gameManager, LevelManager levelManager, LevelGenerator levelGenerator, SpriteBatch batch, StringBuilder debugSb)
    {
        this.gameManager = gameManager;
        this.levelManager = levelManager;
        this.levelGenerator = levelGenerator;
        this.debugSb = debugSb;
        this.spriteBatch = batch;
        cacheDelta = delta;
    }
    public void Update(TileType material, float deltaTime, Tile? mouseTile, Point mouseCoord)
    {
        delta = deltaTime;
        this.mouseTile = mouseTile;
        this.mouseCoord = mouseCoord;
        Material = material;
    }
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        spriteBatch.FillRectangle(new(Constants.Window.X - 190, 0, 190, boxHeight), Color.Black * 0.8f);

        debugSb.Clear();
        foreach (var kv in frameTimes)
        {
            debugSb.Append(kv.Key);
            debugSb.Append(": ");
            debugSb.AppendFormat("{0:0.0}ms", kv.Value);
            debugSb.Append('\n');
        }

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(Constants.Window.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        spriteBatch.FillRectangle(new(0, 0, 200, 180), Color.Black * 0.8f);

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.TotalTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(mouseTile == null ? "none" : mouseTile.Type);
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Name);
        debugSb.Append("\nInventory: ");
        debugSb.Append(gameManager.Inventory.Opened);
        debugSb.Append("\nGUI: ");
        debugSb.Append(gameManager.UIManager.Gui.Widgets.Count);

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        // Background
        spriteBatch.FillRectangle(new(Constants.Window.X - 320, Constants.Window.Y - frameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        spriteBatch.FillRectangle(new(Constants.Window.X - 310, Constants.Window.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in frameTimes)
        {
            spriteBatch.DrawString(Arial, process.Key, new Vector2(Constants.Window.X - Arial.MeasureString(process.Key).X - 5, Constants.Window.Y - 20 * c - 60), colors[c]);
            spriteBatch.FillRectangle(new Rectangle(Constants.Window.X - 310 + start, Constants.Window.Y - 40, (int)(process.Value / (cacheDelta * 1000) * 300), 25), colors[c]);
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
    public static void EditTile(Tile tile)
    {
        if (tile is Stairs stairs)
        {
            // Destination level
            Logger.Print("__Editing Stairs__");
            string resp = Logger.Input($"Dest level [{stairs.DestLevel}]: ");
            if (resp != "") stairs.DestLevel = resp;

            // Destination position
            resp = Logger.Input($"Dest position [{stairs.DestPosition.X}, {stairs.DestPosition.Y}]: ");
            if (resp != "")
            {
                string[] parts = resp.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                    if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                        stairs.DestPosition = new(x, y);
                    else
                        Logger.Error($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
                else
                    Logger.Error("Invalid position format - use 'x,y'.");
            }
        }
        else if (tile is Door door)
        {
            Logger.Print("__Editing Door__");
            string name = Logger.Input($"Key name [{door.Key}]: ");
            door.Key = name;
        }
    }
    public void FloodFill()
    {
        // Fill with current material
        int count = 0;
        Tile tileBelow = GetTile(mouseCoord);
        if (tileBelow.Type != Material)
        {
            Queue<Tile> queue = new();
            HashSet<Point> visited = []; // Track visited tiles
            queue.Enqueue(tileBelow);
            count++;
            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();
                if (current.Type == Material || visited.Contains(current.Location)) continue; // Skip if already filled
                count++;
                SetTile(LevelManager.TileFromId((int)Material, current.Location));
                visited.Add(current.Location); // Mark as visited
                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current.Location + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    Tile neighborTile = GetTile(neighborCoord);
                    if (neighborTile.Type == tileBelow.Type && neighborTile.Type != Material)
                    {
                        queue.Enqueue(neighborTile);
                    }
                }
            }
            Logger.Log($"Filled {count} tiles with '{Material}' starting from {mouseCoord.X}, {mouseCoord.Y}.");
        }
    }
    public void SetSpawn()
    {
        // Destination position
        string resp = Logger.Input($"Spawn position [{levelManager.Level.Spawn.X}, {levelManager.Level.Spawn.Y}]: ");
        if (resp != "")
        {
            string[] parts = resp.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                    levelManager.Level.Spawn = new(x, y);
                else
                    Logger.Error($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
            else
                Logger.Error("Invalid position format - use 'x,y'.");
        }
    }
    public void SetTint()
    {
        Color current = levelManager.Level.Tint;
        Color tint = Logger.InputColor($"Tint (R,G,B,A) [{current.R},{current.G},{current.B},{current.A}]: ", Color.Transparent);
        levelManager.Level.Tint = tint;
    }
    public void EditNPCs()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (NPC npc in levelManager.Level.NPCs)
            {
                if (npc.Location == mouseCoord)
                {
                    levelManager.Level.NPCs.Remove(npc);
                    Logger.Log($"Deleted NPC '{npc.Name}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__NPC__");
            string name = Logger.Input("Name: ");
            string dialog = Logger.Input("Dialog: ");
            int scale = Logger.InputInt("Size: ", fallback: 1);
            if (scale <= 0 || scale > 25.5)
            {
                Logger.Warning("Scale must be between 1 and 25.5- setting to 1");
                scale = 1;
            }
            TextureID texture = Logger.InputTexture("Texture: ", fallback: TextureID.PurpleWizard);
            levelManager.Level.NPCs.Add(new NPC(gameManager.UIManager, texture, mouseCoord, name, dialog, Color.White, scale));
        }
    }
    public void EditDecals()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Decal decal in levelManager.Level.Decals)
            {
                if (decal.Location == mouseCoord)
                {
                    levelManager.Level.Decals.Remove(decal);
                    Logger.Log($"Deleted decal  @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__Decal__");
            string name = Logger.Input("Decal: ");
            int decal = (int)(Enum.TryParse<DecalType>(name, true, out var dec) ? dec : DecalType.Torch);
            levelManager.Level.Decals.Add(LevelManager.DecalFromId(decal, mouseCoord));
        }
    }
    public void EditLoot()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Loot loot in levelManager.Level.Loot)
            {
                if (loot.Location == mouseCoord)
                {
                    levelManager.Level.Loot.Remove(loot);
                    Logger.Log($"Deleted loot '{loot.DisplayName}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__Loot__");
            string name = Logger.Input("Name: ");
            byte amount = Logger.InputByte("Amount: ", fallback: 1);
            levelManager.Level.Loot.Add(new Loot(name, amount, InputManager.MousePosition + CameraManager.Camera.ToPoint() - Constants.Middle, gameManager.TotalTime));
        }
    }
    public void SaveLevel()
    {
        // Input
        string name = Logger.Input("Export file name: ");

        // Parse
        Directory.CreateDirectory("..\\..\\..\\Levels");
        using FileStream fileStream = File.Create($"..\\..\\..\\Levels/{name}.qlv");
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
            writer.Write(LevelEditor.IntToByte((int)tile.Type));
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
        }

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
    }
    public void GenerateLevel()
    {
        Logger.Print("__Generate Level__");
        levelGenerator.Seed = Logger.InputInt("Seed: ", fallback: levelGenerator.Seed);
        levelGenerator.Terrain = levelGenerator.Terrains.TryGetValue(Logger.Input("Preset: "), out var preset) ? preset : levelGenerator.Terrain;
        Tile[] tiles = levelGenerator.GenerateLevel(Constants.MapSize, 20);
        Level level = new("generated", tiles, Constants.HalfMapSize, [], [], [], []);
        levelManager.LoadLevelObject(gameManager, level);
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
    }
}
