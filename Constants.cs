using System.Linq;
namespace Quest;

public static class Constants
{
    // Debug
    public const bool COMMANDS = true;

    // Game
    public static int PlayerSpeed = 999; // normal is 300
    public const int MaxStack = 10;
    public const int DayLength = 600; // Length of a day in seconds

    // Screen
    public static readonly Point ScreenResolution = new(1280, 720); // Actual screen resolution
    public static readonly Point NativeResolution = new(1280, 720); // Game window resolution
    public static readonly Vector2 ScreenScale = ScreenResolution.ToVector2() / NativeResolution.ToVector2(); // Scale factor from native resolution to actual screen resolution
    public static readonly Rectangle WindowRect = new(Point.Zero, NativeResolution); // Game window rectangle
    public static readonly float CameraRigidity = .07f; // Camera smoothing weight
    public const int FPS = -1; // -1 = unlimited
    public const bool VSYNC = false;

    // Tile and map
    public static readonly Point TileSize = new(64, 64); // In-game tile size
    public static readonly Point TileHalfSize = new(TileSize.X / 2, TileSize.Y / 2); // Half of the in-game tile size
    public static readonly Point TilePixelSize = new(16, 16); // Native resolution of tile images
    public static readonly Point TileMapDim = new(4, 4); // Dimensions of the connected texture map
    public static readonly Point MapSize = new(256, 256); // Map size in tiles - can not be above 256x256 because of save file
    public static readonly Point HalfMapSize = new(MapSize.X / 2, MapSize.Y / 2); // Half of the map size in tiles
    public static readonly float TileSizeScale = TileSize.X / TilePixelSize.X; // Scale factor from tile pixel size to in-game tile size
    public static readonly Point[] NeighborTiles = [new(0, 1), new(1, 0), new(0, -1), new(-1, 0)];
    public static readonly Point[] DiagonalNeighborTiles = [new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)];
    public static readonly Point[] AllNeighborTiles = [.. NeighborTiles.Concat(DiagonalNeighborTiles)]; // All 8 neighbor tiles
    public static readonly Point Middle = new(NativeResolution.X / 2, NativeResolution.Y / 2); // Center of the screen
    public static readonly Point MiddleCoord = Middle / TileSize; // Center tile coordinate

    // Utility
    public static readonly Vector2 HalfVec = new(0.5f);
    public static readonly Point NegativePoint = new(-1);
    public static readonly Point OnePoint = new(1);
    public static readonly Point TwoPoint = new(2);
    public const float SQRT2 = 1.41421356237f;

    // Rendering and positioning
    public static readonly Rectangle ZeroSource = new(0, 0, TilePixelSize.X, TilePixelSize.Y); // Default tile source rect
    public static readonly Point[] PlayerCorners = [new(-20, -20), new(20, -20), new(-20, 0), new(20, 0)]; // Player bounding box corners, tl, tr, bl, br
    public static readonly Point PlayerBox = new(PlayerCorners[1].X - PlayerCorners[0].X, PlayerCorners[2].Y - PlayerCorners[1].Y);
    public static readonly Point MageSize = new(80, 80); // Size of the mage sprite in pixels
    public static readonly Point MageHalfSize = new(MageSize.X / 2, MageSize.Y / 2); // Half size of the mage sprite in pixels
    public static readonly Point MageDrawShift = new(MageHalfSize.X, 0); // Used for center aligning mages

    // Colors
    public static readonly Color NearBlack = new(85, 85, 85);
    public static readonly Color FocusBlue = new(174, 200, 209);
    public static readonly Color CottonCandy = new(242, 182, 240);
    public static readonly Color DarkenScreen = new(0, 0, 0, 188);
    public static readonly Color DebugPinkTint = new Color(255, 0, 255) * .5f;
    public static readonly Color DebugGreenTint = new Color(55, 255, 55) * .5f;
    public static readonly Color SemiTransparent = Color.White * .6f;

    // Enum string representations
    public static readonly string[] TileTypeNames = Enum.GetNames(typeof(TileTypeID));
    public static readonly string[] DecalTypeNames = Enum.GetNames(typeof(DecalType));
    public static readonly string[] ItemTypeNames = Enum.GetNames(typeof(ItemType));
    public static readonly string[] TextureIDNames = Enum.GetNames(typeof(TextureID));

    // Minimap pixel colors
    public static readonly Color[] MiniMapColors = [
        new(92, 221, 241), // Sky
        Color.Green, // Grass
        Color.Blue, // Water
        Color.DarkGray, // StoneWall
        new(117, 66, 13), // Stairs
        new(155, 155, 235), // Flooring
        Color.Yellow, // Sand
        new(48, 25, 0), // Dirt
        new(18, 18, 18), // Darkness
        new(41, 15, 0), // Door
        new(102, 60, 14), // WoodFlooring
        Color.Gray, // Stone
        Color.Tan, // Chest
        Color.LightGray, // ConcreteWall
        new(92, 53, 23), // WoodWall
        new(163, 89, 33), // Path
        Color.OrangeRed, // Lava
        new(118, 128, 133), // StoneTiles
        Color.Red, // RedTiles
        Color.Orange, // OrangeTiles
        Color.Yellow, // YellowTiles
        Color.Lime, // LimeTiles
        Color.DarkGreen, // GreenTiles
        Color.Cyan, // CyanTiles
        Color.Blue, // BlueTiles
        Color.Purple, // PurpleTiles
        Color.Pink, // PinkTiles
        Color.DarkGray, // BlackTiles
        Color.SaddleBrown, // BrownTiles
        new(168, 159, 151), // IronWall
        Color.White, // Snow
        new(107, 169, 197), // Ice
        new(230, 255, 230), // SnowyGrass
        new(247, 255, 199), // Lamp
        new(225, 180, 0), // Sandstone
        new(255, 255, 90), // SandstoneWall
        // MINIMAPCOLORS
    ];
}
