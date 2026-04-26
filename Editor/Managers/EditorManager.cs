using System.Diagnostics;
using System.IO;
using System.Linq;
using static Quest.Editor.PopupFactory;

namespace Quest.Editor.Managers;
public enum EditorTool : byte
{
    Tile,
    Decal,
    Biome,
}
public class EditorManager
{
    // Static
    public static Tile? MouseTile { get; private set; }
    public static Point MouseCoord { get; private set; }
    // Settings
    public bool ShowBiomeMarkers = true;
    // Helper
    public string[] ItemsOptionsWNone = ["NONE", .. Constants.ItemTypeNames];
    // Public
    public GameManager GameManager { get; private set; }
    public LevelManager LevelManager => GameManager.LevelManager;
    public Level CurrentLevel => GameManager.LevelManager.Level;
    // Private
    private Point MouseSelection;
    private Point MouseSelectionCoord;
    private TileTypeID TileSelection;
    private EditorTool CurrentTool;
    private BiomeType BiomeSelection;

    private DecalType? PreviousDecal = null;
    public EditorManager(GameManager gameManager)
    {
        GameManager = gameManager;
    }
    public void Update(TileTypeID material, BiomeType biome, EditorTool tool, float deltaTime, Tile? mouseTile, Point mouseCoord, Point mouseSelection, Point mouseSelectionCoord)
    {
        MouseTile = mouseTile;
        MouseCoord = mouseCoord;
        MouseSelection = mouseSelection;
        MouseSelectionCoord = mouseSelectionCoord;
        CurrentTool = tool;
        TileSelection = material;
        BiomeSelection = biome;
    }

    public void EditTile()
    {
        Tile? tile = LevelManager.GetTile(MouseSelectionCoord);
        // Stair
        if (tile is Stairs stairs)
        {
            var (success, values) = ShowInputForm("Stair Editor", [
                new("Level", null, placeholder: stairs.DestLevel),
                new("Spawn X", IsByte, placeholder: stairs.Dest.X.ToString()),
                new("Spawn Y", IsByte, placeholder: stairs.Dest.Y.ToString())]);
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
            stairs.DestLevel = $"{CurrentLevel.WorldName}/{values[0]}";
            stairs.Dest = new(byte.Parse(values[1]), byte.Parse(values[2]));
        }
        // Door
        else if (tile is Door door)
        {
            var (success, values) = ShowInputForm("Door Editor", [
                new("Key", null, ItemsOptionsWNone, door.Key == null ? "NONE" : door.Key.Name.ToString()),
                new("Amount", IsByte, placeholder: door.Key == null ? "0" : door.Key.Amount.ToString()),
                new("Consume Key", null, ["True", "False"], door.ConsumeKey.ToString())]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Stair edit failed.");
                return;
            }
            door.Key = values[0].Equals("none", StringComparison.CurrentCultureIgnoreCase) ? null : new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[0])], byte.Parse(values[1]));
            if (door.Key?.Amount <= 0)
                door.Key = null;
            door.ConsumeKey = bool.Parse(values[2]);
        }
        // Chest
        else if (tile is Chest chest)
        {
            var (success, values) = ShowInputForm("Chest Editor", [
                new("Loot File Name", null, placeholder: chest.LootGenerator.FileName.Split('.')[0]),
                new("Loot Type", null, ["Loot Preset", "Loot Table"], chest.LootGenerator.FileName.Split('.')[1] == "qlt" ? "Loot Table" : "Loot Preset"),
                new("Key", null, ItemsOptionsWNone, chest.Key == null ? "NONE" : chest.Key.Name.ToString()),
                new("Amount", IsByte, placeholder: chest.Key == null ? "0" : chest.Key.Amount.ToString()),
                new("Consume Key", null, ["True", "False"], chest.ConsumeKey.ToString())]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Chest edit failed.");
                return;
            }
            // Loot
            if (values[1] == "Loot Table")
                chest.RegenerateLoot(LootTable.ReadLootTable(CurrentLevel.WorldName, $"{values[0]}.qlt"));
            else if (values[1] == "Loot Preset")
                chest.RegenerateLoot(LootPreset.ReadLootPreset(CurrentLevel.WorldName, $"{values[0]}.qlp"));
            else
                Logger.Error("Chest loot edit failed.");
            // Key
            chest.Key = values[2].Equals("none", StringComparison.CurrentCultureIgnoreCase) ? null : new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[2])], byte.Parse(values[3]));
            if (chest.Key?.Amount <= 0)
                chest.Key = null;
            chest.ConsumeKey = bool.Parse(values[4]);
        }
        // Lamp
        else if (tile is Lamp lamp)
        {
            var (success, values) = ShowInputForm("Lamp Editor", [new("Light Radius", IsByte, placeholder: lamp.LightRadius.ToString())]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Lamp edit failed.");
                return;
            }
            lamp.LightRadius = byte.Parse(values[4]);
        }
        // Display case
        else if (tile is DisplayCase displayCase)
        {
            var (success, values) = ShowInputForm("Lamp Editor", [
                new("Item", null, ItemsOptionsWNone, displayCase.Container.Items[0] == null ? "NONE" : displayCase.Container.Items[0]!.Name.ToString()),
                new("Amount", IsNonZeroByte, placeholder: displayCase.Container.Items[0]?.Amount.ToString())]);
            if (!success)
            {
                if (!PopupOpen) Logger.Error("Display case edit failed.");
                return;
            }
            displayCase.Container.Items[0] = new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[0])], byte.Parse(values[1]));
        }
    }
    public void FloodFill()
    {
        if (CurrentTool == EditorTool.Tile) FloodFillTiles();
        else if (CurrentTool == EditorTool.Decal) { }
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
                        queue.Enqueue(neighborTile);
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
                        queue.Enqueue(neighborCoord);
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
        if (LevelManager.Level.NPCs.Count >= ushort.MaxValue)
        {
            Logger.Error("Maximum number of NPCs reached (65,535).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("NPC Editor", [
            new("Name", null),
            new("Dialog", null),
            new("Size [0.1-25.5]", IsScaleValue),
            new("Texture", null, [.. CharacterTextures.Select(t => t.ToString())]),
            new("Shop Option 1", null),
            new("Shop Option 2", null),
            new("Shop Option 3", null),
            new("Shop Option 4", null),
            new("Shop Option 5", null)
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("NPC creation failed.");
            return;
        }

        // Create
        string name = values[0];
        string dialog = values[1];
        float scale = float.Parse(values[2]);

        // Shop options parsing
        List<ShopOption> shopOptions = [];
        foreach (string option in values[4].Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(option)) continue;
            try
            {
                shopOptions.Add(ShopOption.ParseText(option));
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse shop option '{option}': {ex.Message}");
                return;
            }
        }

        if (scale < 0.1 || scale > 25.5)
        {
            Logger.Warning("Scale must be between 0.1 and 25.5. Scale defaulted to 1.");
            scale = 1;
        }
        TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), values[3]);
        NPC npc = new NPC(texture, MouseSelectionCoord, name, dialog, Color.White, scale);
        foreach (ShopOption option in shopOptions)
            npc.AddShopOption(option);
        LevelManager.Level.NPCs.Add(npc);
    }
    public void DeleteNPC()
    {
        foreach (NPC npc in LevelManager.Level.NPCs)
        {
            if (npc.Position == MouseSelectionCoord)
            {
                LevelManager.Level.NPCs.Remove(npc);
                Logger.Log($"Deleted NPC '{npc.Name}' @ {MouseSelectionCoord.X}, {MouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void EditNPC()
    {
        // Grab NPC
        NPC? editing = null;
        foreach (NPC npc in LevelManager.Level.NPCs)
        {
            if (npc.Position == MouseSelectionCoord)
            {
                editing = npc;
                break;
            }
        }
        if (editing == null) return;

        // Remake
        var (success, values) = ShowInputForm("NPC Editor", [
            new("Name", null, placeholder: editing.Name),
            new("Dialog", null, placeholder: editing.Dialog),
            new("Size [0.1-25.5]", IsScaleValue, placeholder: editing.Scale.ToString()),
            new("Texture", null, [.. CharacterTextures.Select(t => t.ToString())], placeholder: editing.Texture.ToString()),
            new("Shop Option 1", null, placeholder: editing.ShopOptions.ElementAtOrDefault(0)?.ToString() ?? ""),
            new("Shop Option 2", null, placeholder: editing.ShopOptions.ElementAtOrDefault(1)?.ToString() ?? ""),
            new("Shop Option 3", null, placeholder: editing.ShopOptions.ElementAtOrDefault(2)?.ToString() ?? ""),
            new("Shop Option 4", null, placeholder: editing.ShopOptions.ElementAtOrDefault(3)?.ToString() ?? ""),
            new("Shop Option 5", null, placeholder: editing.ShopOptions.ElementAtOrDefault(4)?.ToString() ?? "")
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("NPC creation failed.");
            return;
        }

        // Create
        editing.Name = values[0];
        editing.Dialog = values[1];
        float scale = float.Parse(values[2]);
        if (scale < 0.1 || scale > 25.5)
        {
            Logger.Warning("Scale must be between 0.1 and 25.5. Scale defaulted to 1.");
            scale = 1;
        }
        editing.Scale = scale;
        editing.Texture = (TextureID)Enum.Parse(typeof(TextureID), values[3]);

        // Shop options parsing
        editing.ShopOptions.Clear();
        foreach (string line in values[4].Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                editing.AddShopOption(ShopOption.ParseText(line));
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse shop option '{line}': {ex.Message}");
                return;
            }
        }
    }
    public void NewEnemy()
    {
        // Check
        if (LevelManager.Level.Enemies.Count >= ushort.MaxValue)
        {
            Logger.Error("Maximum number of Enemies reached (65,535).");
            return;
        }

        // Winforms
        var (success, values) = ShowInputForm("Enemy Editor", [
            new("Health", IsUInt16),
            new("Damage", IsUInt16),
            new("Attack Speed", IsPositiveFloat),
            new("Defense", IsUInt16),
            new("Speed", IsUInt16),
            new("Projectile Speed", IsUInt16),
            new("View Range", IsUInt16),
            new("Attack Range", IsUInt16),
            new("Texture", null, [.. CharacterTextures.Select(t => t.ToString())]),
            new("Projectile Texture", null, [.. ProjectileTextures.Select(t => t.ToString())])
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("NPC creation failed.");
            return;
        }

        // Create
        TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), values[8]);
        TextureID projTexture = (TextureID)Enum.Parse(typeof(TextureID), values[9]);
        Enemy enemy = new(
            MouseSelection.ToVector2(),
            ushort.Parse(values[0]),
            ushort.Parse(values[1]),
            float.Parse(values[2]),
            ushort.Parse(values[3]),
            ushort.Parse(values[4]),
            ushort.Parse(values[5]),
            ushort.Parse(values[6]),
            ushort.Parse(values[7]),
            texture,
            projTexture
        );
        LevelManager.Level.Enemies[enemy.UID] = enemy;
    }
    public void DeleteEnemy()
    {
        foreach (Enemy enemy in LevelManager.Level.Enemies.Values)
        {
            if (Vector2.DistanceSquared(enemy.Position, MouseSelection.ToVector2()) < 50 * 50)
            {
                LevelManager.Level.Enemies.Remove(enemy.UID);
                Logger.Log($"Deleted Enemy @ {MouseSelection.X}, {MouseSelection.Y}.");
                break;
            }
        }
    }
    public void EditEnemy()
    {
        // Grab NPC
        Enemy? editing = null;
        foreach (Enemy enemy in LevelManager.Level.Enemies.Values)
        {
            if (Vector2.DistanceSquared(enemy.Position, MouseSelection.ToVector2()) < 50 * 50)
            {
                editing = enemy;
                break;
            }
        }
        if (editing == null) return;

        // Remake
        var (success, values) = ShowInputForm("Enemy Editor", [
            new("Health", IsUInt16, placeholder: editing.Health.ToString()),
            new("Damage", IsUInt16, placeholder: editing.Damage.ToString()),
            new("Attack Speed", IsPositiveFloat, placeholder: editing.AttackSpeed.ToString()),
            new("Defense", IsUInt16, placeholder: editing.Defense.ToString()),
            new("Speed", IsUInt16, placeholder: editing.Speed.ToString()),
            new("Projectile Speed", IsUInt16, placeholder: editing.ProjectileSpeed.ToString()),
            new("View Range", IsUInt16, placeholder: editing.ViewRange.ToString()),
            new("Attack Range", IsUInt16, placeholder: editing.AttackRange.ToString()),
            new("Texture", null, [.. CharacterTextures.Select(t => t.ToString())], placeholder: editing.Texture.ToString()),
            new("Projectile Texture", null, [.. ProjectileTextures.Select(t => t.ToString())], placeholder: editing.ProjectileTexture.ToString())
        ]);
        if (!success)
        {
            if (!PopupOpen) Logger.Error("NPC creation failed.");
            return;
        }

        // Create
        TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), values[8]);
        TextureID projTexture = (TextureID)Enum.Parse(typeof(TextureID), values[9]);
        editing.Health = ushort.Parse(values[0]);
        editing.Damage = ushort.Parse(values[1]);
        editing.AttackSpeed = float.Parse(values[2]);
        editing.Defense = ushort.Parse(values[3]);
        editing.Speed = ushort.Parse(values[4]);
        editing.ProjectileSpeed = ushort.Parse(values[5]);
        editing.ViewRange = ushort.Parse(values[6]);
        editing.AttackRange = ushort.Parse(values[7]);
        editing.Texture = texture;
        editing.ProjectileTexture = projTexture;
    }
    public void NewDecal()
    {
        // Check
        if (LevelManager.Level.Decals.Count >= ushort.MaxValue)
        {
            Logger.Error("Maximum number of Decals reached (65,535).");
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
        LevelManager.Level.Decals[MouseSelectionCoord.ToByteCoord()] = LevelManager.DecalFromId(decal, MouseSelectionCoord);
    }
    public void DeleteDecal()
    {
        foreach (Decal decal in LevelManager.Level.Decals.Values)
        {
            if (decal.Location == MouseSelectionCoord)
            {
                LevelManager.Level.Decals.Remove(decal.Location);
                Logger.Log($"Deleted decal '{decal.Type}' @ {MouseSelectionCoord.X}, {MouseSelectionCoord.Y}.");
                break;
            }
        }
    }
    public void NewLoot()
    {
        // Check
        if (LevelManager.Level.Loot.Count >= ushort.MaxValue)
        {
            Logger.Error("Maximum number of Loot reached (65,535).");
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
            if (Vector2.DistanceSquared(loot.Position.ToVector2(), MouseSelection.ToVector2()) < 900)
            {
                LevelManager.Level.Loot.Remove(loot);
                Logger.Log($"Deleted loot '{loot.Item}' @ {MouseCoord.X}, {MouseCoord.Y}.");
                break;
            }
        }
    }
    public void DeleteScript()
    {
        var (success, values) = ShowInputForm("Delete Script", [new("Script Name", dropdownOptions: [.. LevelManager.Level.Scripts.Select(s => s.Name)])]);
        if (!success || string.IsNullOrWhiteSpace(values[0]))
        {
            if (!PopupOpen) Logger.Error("Script deletion failed.");
            return;
        }

        string name = values[0];
        LevelManager.Level.Scripts.RemoveAll(s => s.Name == name);
    }
    public void NewScript()
    {
        var (success, values) = ShowInputForm("New Script", [new("Script File", null)]);
        if (!success || string.IsNullOrWhiteSpace(values[0]))
        {
            if (!PopupOpen) Logger.Error("Script creation failed.");
            return;
        }
        string path = $"GameData/Worlds/{CurrentLevel.WorldName}/scripts/{values[0]}";
        if (!File.Exists(path))
        {
            Logger.Error($"Source file '{path}' not found.");
            return;
        }
        if (LevelManager.Level.Scripts.Any(s => s.Name == values[0]))
        {
            Logger.Error($"A script with the name '{values[0]}' already exists.");
            return;
        }
        string sourceCode = File.ReadAllText(path);
        LevelManager.Level.Scripts.Add(new QuillScript(values[0], sourceCode));
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
    }
}
