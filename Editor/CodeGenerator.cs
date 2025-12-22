using MonoGame.Extended.ECS;
using ScottPlot;
using ScottPlot.TickGenerators;
using System.IO;
using System.Linq;

namespace Quest.Editor;
public static class CodeGenerator
{
    private const string tileCodeTemplate = "namespace Quest.Tiles;\r\n\r\npublic class $name : Tile\r\n{\r\n    public $name(Point location) : base(location, TileTypes.$name) { }\r\n}\r\n";
    private const string decalCodeTemplate = "namespace Quest.Decals;\r\npublic class $name(Point location) : Decal(location) {}\r\n";
    private const string itemCodeTemplate = "namespace Quest.Items;\r\npublic class $name : Item\r\n{\r\n    public $name(int amount) : base(amount)\r\n    {\r\n        MaxAmount = $maxamount;\r\n        Description = \"$description\";\r\n    }\r\n}\r\n";

    private static string textureManagerSource = "";
    private static string levelManagerSource = "";
    private static string constantsSource = "";
    private static string tileSource = "";
    private static string decalSource = "";
    private static string itemSource = "";
    private static string sourceDirectory = "";
    public static void Run(string sourceDir)
    {
        sourceDirectory = sourceDir;
        while (true)
        {
            ReloadSource();
            // CLI
            Console.WriteLine("[t]ile, [d]ecal, [i]tem, [l]oot table, or [w]eather test: ");
            string? resp = Console.ReadLine()?.ToLower();
            if (resp == null || resp == "") continue;

            // Tile
            if (resp == "t" || resp == "tile")
                WriteTileCode();
            else if (resp == "d" || resp == "decal")
                WriteDecalCode();
            else if (resp == "i" || resp == "item")
                WriteItemCode();
            else if (resp == "l" || resp == "loot table")
                WriteLootTable();
            else if (resp == "w" || resp == "weather noise")
                TestWeatherNoise();
            else if (resp == "exit" || resp == "quit")
                return;
            else
                Console.WriteLine("Unknown response");
        }
    }
    public static void TestWeatherNoise()
    {
        const int seconds = 3600;

        float[] values = new float[seconds];
        int[] times = new int[seconds];
        float[] boost = new float[seconds];
        float[] intensity = new float[seconds];

        int offset = DateTime.Now.Millisecond;
        StateManager.SetWeatherPersistent(lastTimeValue: offset);
        for (int t = 0; t < seconds; t++)
        {
            values[t] = StateManager.WeatherNoiseValue(t + offset);
            times[t] = t;
            boost[t] = StateManager.WeatherBoost(t + offset);
            intensity[t] = StateManager.WeatherIntensity(t + offset);
        }

        float weatherPercent = values.Where(f => f >= StateManager.weatherThreshold).Count() / (float)seconds;
        float lightPercent = values.Where(f => f >= StateManager.weatherThreshold && f < StateManager.weatherThreshold + 0.1f).Count() / (float)seconds;
        float heavyPercent = values.Where(f => f >= 1 - StateManager.weatherThreshold / 2).Count() / (float)seconds;

        Console.WriteLine($"Weather: {weatherPercent * 100:0.0}%\n  Light: {lightPercent * 100:0.0}%\n  Heavy: {heavyPercent * 100:0.0}%");

        Plot plot = new();
        plot.Axes.Bottom.TickGenerator = new NumericFixedInterval(120);

        var intensityVal = plot.Add.Scatter(times, intensity, color: Colors.Orange);
        intensityVal.MarkerShape = MarkerShape.None;
        intensityVal.LineWidth = 2;

        var boostVal = plot.Add.Scatter(times, boost, color: Colors.Green);
        boostVal.MarkerShape = MarkerShape.None;
        boostVal.LineWidth = 2;

        var weatherVal = plot.Add.Scatter(times, values, color: Colors.Blue);
        weatherVal.MarkerShape = MarkerShape.None;
        weatherVal.LineWidth = 3;

        var lightLine = plot.Add.HorizontalLine(StateManager.weatherThreshold, pattern: LinePattern.Dotted);
        lightLine.Color = Colors.Orange;
        lightLine.LineWidth = 2;

        var heavyLine = plot.Add.HorizontalLine(1 - StateManager.weatherThreshold / 2, pattern: LinePattern.Dotted);
        heavyLine.Color = Colors.Red;
        heavyLine.LineWidth = 2;

        var info = plot.Add.Text($"Weather: {weatherPercent * 100:0.0}% Light: {lightPercent * 100:0.0}% Heavy: {heavyPercent * 100:0.0}%", 1, 0);
        info.LabelFontSize = 16;
        info.LabelFontColor = Colors.Green;

        plot.SavePng("weather.png", 1200, 400);

    }
    public static void ReloadSource()
    {
        textureManagerSource = File.ReadAllText($"{sourceDirectory}/Managers/TextureManager.cs");
        levelManagerSource = File.ReadAllText($"{sourceDirectory}/Managers/LevelManager.cs");
        constantsSource = File.ReadAllText($"{sourceDirectory}/Constants.cs");
        tileSource = File.ReadAllText($"{sourceDirectory}/Tiles/Tile.cs");
        decalSource = File.ReadAllText($"{sourceDirectory}/Decals/Decal.cs");
        itemSource = File.ReadAllText($"{sourceDirectory}/Items/Item.cs");
    }
    public static void WriteTileCode()
    {
        string? name = Ask("Tile name: ");
        bool isWalkable = Ask("Is Walkable [y/n]: ")?.ToLower() == "y";
        bool isWall = !isWalkable && Ask("Is Wall [y/n]:")?.ToLower() == "y";
        string? color = Ask("Tile Minimap Color [new(r, g, b)/Color.someColor]: ");
        if (color == null) return;
        if (name == null) return;

        // Source code
        string classSource = tileCodeTemplate.Replace("$name", name).Replace("$iswalkable", isWalkable ? "true" : "false").Replace("$iswall", isWall ? "        IsWall = true;\r\n" : "");
        File.WriteAllText($"{sourceDirectory}/Tiles/{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // TILES ENUM INSERT", $"        {name},\r\n        // TILES ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Tiles/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(4, 4), \"tile\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // TILES INSERT", $"        {loadSource}        // TILES INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // TILES METADATA INSERT", $"        {metadataSource}        // TILES METADATA INSERT");
        File.WriteAllText($"{sourceDirectory}/Managers/TextureManager.cs", newTextureManagerSource);

        // TileTypeID enum in Tile.cs
        string newTileSource = tileSource.Replace("    // TILES ID", $"    {name},\r\n    // TILES ID");
        // TileType variable in TileTypes class in Tile.cs
        newTileSource = newTileSource.Replace("        // TILES REGISTER", $"        new(TileTypeID.{name}, TextureID.{name}, {isWalkable.ToString().ToLower()}, {isWall.ToString().ToLower()}),\r\n        // TILES REGISTER");
        File.WriteAllText($"{sourceDirectory}/Tiles/Tile.cs", newTileSource);

        // TileFromID in LevelManager.cs
        string newLevelManagerSource = levelManagerSource.Replace("            // TILEFROMID INSERT", $"            TileTypeID.{name} => new {name}(location),\r\n            // TILEFROMID INSERT");
        File.WriteAllText($"{sourceDirectory}/Managers/LevelManager.cs", newLevelManagerSource);

        // Minimap color
        string newConstantsSource = constantsSource.Replace("        // MINIMAPCOLORS", $"        {color}, // {name}\r\n        // MINIMAPCOLORS");
        File.WriteAllText($"{sourceDirectory}/Constants.cs", newConstantsSource);
    }
    public static void WriteDecalCode()
    {
        string? name = Ask("Decal name: ");
        if (name == null) return;

        // Source code
        string classSource = decalCodeTemplate.Replace("$name", name);
        File.WriteAllText($"{sourceDirectory}/Decals/{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // DECALS ENUM INSERT", $"        {name},\r\n        // DECALS ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Decals/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(1, 1), \"decal\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // DECALS INSERT", $"        {loadSource}        // DECALS INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // DECALS METADATA INSERT", $"        {metadataSource}        // DECALS METADATA INSERT");
        File.WriteAllText($"{sourceDirectory}/Managers/TextureManager.cs", newTextureManagerSource);

        // DecalType enum in Decal.cs
        string newDecalSource = decalSource.Replace("    // DECALS", $"    {name},\r\n    // DECALS");
        File.WriteAllText($"{sourceDirectory}/Decals/Decal.cs", newDecalSource);

        // DecalFromID in LevelManager.cs
        string newLevelManagerSource = levelManagerSource.Replace("            // DECALFROMID INSERT", $"            DecalType.{name} => new {name}(location),\r\n            // DECALFROMID INSERT");
        File.WriteAllText($"{sourceDirectory}/Managers/LevelManager.cs", newLevelManagerSource);
    }
    public static void WriteItemCode()
    {
        string? name = Ask("Item name:");
        string? maxAmount = Ask("Max item amount:");
        string? description = Ask("Item description: ");

        if (name == null || maxAmount == null || description == null) return;

        // Source code
        string classSource = itemCodeTemplate.Replace("$name", name).Replace("$maxamount", maxAmount).Replace("$description", description);
        File.WriteAllText($"{sourceDirectory}/Items/{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // ITEMS ENUM INSERT", $"        {name},\r\n        // ITEMS ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Items/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(1, 1), \"item\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // ITEMS INSERT", $"        {loadSource}        // ITEMS INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // ITEMS METADATA INSERT", $"        {metadataSource}        // ITEMS METADATA INSERT");
        File.WriteAllText($"{sourceDirectory}/Managers/TextureManager.cs", newTextureManagerSource);

        // ItemType enum in item.cs
        string newItemSource = itemSource.Replace("    // ITEMS", $"    {name},\r\n    // ITEMS");
        newItemSource = newItemSource.Replace("    // ITEMS REGISTER", $"    public static readonly ItemType {name} = new(ItemTypeID.{name}, \"{description}\"{(maxAmount == "10" ? "" : $", {maxAmount}")});\r\n    // ITEMS REGISTER");
        File.WriteAllText($"{sourceDirectory}/Items/Item.cs", newItemSource);
    }
    public static void WriteLootTable()
    {
        byte[] data;
        byte entries = 0;
        Console.WriteLine("Note: ID starts at 1\nEnter blank when done.");
        string? tableName = Ask("Table name: ");
        if (tableName == null || tableName == "") return;

        string? world;
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            // Add items
            writer.Write((byte)0);
            string? itemResp = "";
            do
            {
                int item;
                byte percentChance;
                byte min, max;

                // Item
                itemResp = Ask("Item name or ID: ");
                if (itemResp == null || itemResp == "") break;
                item = ParseItemEnumOrInt(itemResp, offset: 1);
                if (item == -1) continue;
                writer.Write((byte)item);

                // Chance
                percentChance = Logger.InputByte("Percent chance: ", 0);
                if (percentChance == 0)
                {
                    Console.WriteLine("Bad percentage.");
                    break;
                }
                writer.Write(percentChance);

                // Min/max
                min = Logger.InputByte("Min amount: ", 0);
                max = Logger.InputByte("Max amount: ", 0);
                writer.Write(min);
                writer.Write(max);
                entries++;

            } while (itemResp != "" && entries < 255);
            do
                world = Ask("World to output to: ");
            while (world == null || world == "" || !Directory.Exists($"{sourceDirectory}/GameData/Worlds/{world}"));

            // Write to data
            data = ms.ToArray();
            data[0] = entries;
        }

        // Write
       Directory.CreateDirectory($"{sourceDirectory}/GameData/Worlds/{world}/loot");
        File.WriteAllBytes($"{sourceDirectory}/GameData/Worlds/{world}/loot/{tableName}.qlt", data);
    }
    public static int ParseItemEnumOrInt(string input, int offset = 0)
    {
        // Try parse as int
        if (int.TryParse(input, out int intValue))
        {
            if (Enum.IsDefined(typeof(ItemTypeID), intValue - offset))
                return intValue;
            else
            {
                Logger.Error($"'{intValue - offset}' is not a valid value of enum ItemType.");
                return -1;
            }
        }

        // Try parse as enum name
        if (Enum.TryParse(input, true, out ItemTypeID enumValue))
            return (int)enumValue + offset;

        Logger.Error($"'{input}' is not a valid name or value of enum ItemType.");
        return -1;
    }

    public static string? Ask(string question)
    {
        Console.WriteLine(question);
        return Console.ReadLine();
    }
}
