using System.IO;

namespace Quest.Editor;
public static class CodeGenerator
{
    private const string tileCodeTemplate = "namespace Quest.Tiles;\r\n\r\npublic class $name : Tile\r\n{\r\n    public $name(Point location) : base(location)\r\n    {\r\n        IsWalkable = $iswalkable;\r\n    }\r\n}\r\n";
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
        ReloadSource();
        // CLI
        Console.WriteLine(">>");
        string? resp = Console.ReadLine()?.ToLower();
        if (resp == null || resp == "") return;

        // Tile
        if (resp == "tile")
            WriteTileCode();
        else if (resp == "decal")
            WriteDecalCode();
        else if (resp == "item")
            WriteItemCode();
        else if (resp == "exit")
            return;
    }
    public static void ReloadSource()
    {
        textureManagerSource = File.ReadAllText($"{sourceDirectory}\\Managers\\TextureManager.cs");
        levelManagerSource = File.ReadAllText($"{sourceDirectory}\\Managers\\LevelManager.cs");
        constantsSource = File.ReadAllText($"{sourceDirectory}\\Constants.cs");
        tileSource = File.ReadAllText($"{sourceDirectory}\\Tiles\\Tile.cs");
        decalSource = File.ReadAllText($"{sourceDirectory}\\Decals\\Decal.cs");
        itemSource = File.ReadAllText($"{sourceDirectory}\\Items\\Item.cs");
    }
    public static void WriteTileCode()
    {
        string? name = Ask("Tile name: ");
        bool isWalkable = Ask("Is Walkable [y/n]: ")?.ToLower() == "y";
        string? color = Ask("Tile Minimap Color [new(r, g, b)/Color.someColor]: ");
        if (color == null) return;
        if (name == null) return;
        Directory.CreateDirectory("_generated");

        // Source code
        string classSource = tileCodeTemplate.Replace("$name", name).Replace("$iswalkable", isWalkable ? "true" : "false");
        File.WriteAllText($"_generated\\{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // TILES ENUM INSERT", $"        {name},\r\n        // TILES ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Tiles/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(4, 4), \"tile\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // TILES INSERT", $"        {loadSource}        // TILES INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // TILES METADATA INSERT", $"        {metadataSource}        // TILES METADATA INSERT");
        File.WriteAllText("_generated\\TextureManager.cs", newTextureManagerSource);

        // TileType enum in Tile.cs
        string newTileSource = tileSource.Replace("    // TILES", $"    {name},\r\n    // TILES");
        File.WriteAllText("_generated\\Tile.cs", newTileSource);

        // TileFromID in LevelManager.cs
        string newLevelManagerSource = levelManagerSource.Replace("            // TILEFROMID INSERT", $"            TileType.{name} => new {name}(location),\r\n            // TILEFROMID INSERT");
        File.WriteAllText("_generated\\LevelManager.cs", newLevelManagerSource);

        // Minimap color
        string newConstantsSource = constantsSource.Replace("        // MINIMAPCOLORS", $"        {color}, // {name}\r\n        // MINIMAPCOLORS");
        File.WriteAllText("_generated\\Constants.cs", newConstantsSource);
    }
    public static void WriteDecalCode()
    {
        string? name = Ask("Decal name: ");
        if (name == null) return;
        Directory.CreateDirectory("_generated");

        // Source code
        string classSource = decalCodeTemplate.Replace("$name", name);
        File.WriteAllText($"_generated\\{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // DECALS ENUM INSERT", $"        {name},\r\n        // DECALS ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Decals/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(1, 1), \"decal\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // DECALS INSERT", $"        {loadSource}        // DECALS INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // DECALS METADATA INSERT", $"        {metadataSource}        // DECALS METADATA INSERT");
        File.WriteAllText("_generated\\TextureManager.cs", newTextureManagerSource);

        // DecalType enum in Decal.cs
        string newDecalSource = decalSource.Replace("    // DECALS", $"    {name},\r\n    // DECALS");
        File.WriteAllText("_generated\\Decal.cs", newDecalSource);

        // DecalFromID in LevelManager.cs
        string newLevelManagerSource = levelManagerSource.Replace("            // DECALFROMID INSERT", $"            DecalType.{name} => new {name}(location),\r\n            // DECALFROMID INSERT");
        File.WriteAllText("_generated\\LevelManager.cs", newLevelManagerSource);
    }
    public static void WriteItemCode()
    {
        string? name = Ask("Item name:");
        string? maxAmount = Ask("Max item amount:");
        string? description = Ask("Item description: ");

        if (name == null || maxAmount == null || description == null) return;
        Directory.CreateDirectory("_generated");

        // Source code
        string classSource = itemCodeTemplate.Replace("$name", name).Replace("$maxamount", maxAmount).Replace("$description", description);
        File.WriteAllText($"_generated\\{name}.cs", classSource);

        // TextureManager TextureID enum
        string newTextureManagerSource = textureManagerSource.Replace("        // ITEMS ENUM INSERT", $"        {name},\r\n        // ITEMS ENUM INSERT");

        // TextureManager Load and Metadata
        string loadSource = $"Textures[TextureID.{name}] = content.Load<Texture2D>(\"Images/Items/{name}\");\r\n";
        string metadataSource = $"Metadata[TextureID.{name}] = new(Textures[TextureID.{name}].Bounds.Size, new(1, 1), \"item\");\r\n";
        newTextureManagerSource = newTextureManagerSource.Replace("        // ITEMS INSERT", $"        {loadSource}        // ITEMS INSERT");
        newTextureManagerSource = newTextureManagerSource.Replace("        // ITEMS METADATA INSERT", $"        {metadataSource}        // ITEMS METADATA INSERT");
        File.WriteAllText("_generated\\TextureManager.cs", newTextureManagerSource);

        // ItemType enum in item.cs
        string newItemSource = itemSource.Replace("    // ITEMS", $"    {name},\r\n    // ITEMS");
        File.WriteAllText("_generated\\Item.cs", newItemSource);
    }
    public static string? Ask(string question)
    {
        Console.WriteLine(question);
        return Console.ReadLine();
    }
}
