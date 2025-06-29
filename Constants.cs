using Xna = Microsoft.Xna.Framework;
namespace Quest;

public static class Constants
{
    // Debug
    public static bool COLLISION_DEBUG = true; // Enable collision debug mode
    public static bool TEXT_INFO = true; // Show text info on screen
    public static bool FRAME_INFO = true; // Show frame info on screen
    public static bool LOG_INFO = true; // Log game info
    public static bool FRAME_BAR = true; // Frame time graphical bar
    public static bool DRAW_HITBOXES = true; // Draw hitboxes for entities
    public static bool COMMANDS = true; // Allow console commands

    // Tile and map
    public static readonly Point TileSize = new(64, 64); // In-game tile size
    public static readonly Point TilePixelSize = new(16, 16); // Native resolution of tile images
    public static readonly Point TileMapDim = new(4, 4); // Dimensions of the connected texture map
    public static readonly Point MapSize = new(256, 256); // Map size in tiles - can not be above 256x256 because of save file
    public static readonly Point HalfMapSize = new(MapSize.X / 2, MapSize.Y / 2); // Half of the map size in tiles
    public static readonly float TileSizeScale = TileSize.X / TilePixelSize.X; // Scale factor from tile pixel size to in-game tile size
    public static readonly List<Point> NeighborTiles = [new(0, 1), new(1, 0), new(0, -1), new(-1, 0)];

    // Screen
    public static readonly Point Window = new(1400, 900); // Game window resolution
    public static readonly Point Middle = new(Window.X / 2, Window.Y / 2); // Center of the screen
    public static readonly Point MiddleCoord = Middle / TileSize; // Center tile coordinate
    public static readonly float CameraRigidity = .07f; // Camera smoothing weight
    public const bool VSYNC = false;

    // Utility
    public static readonly Vector2 HalfVec = new(0.5f, 0.5f);
    public static readonly Point NegativePoint = new(-1, -1);
    public static readonly Point OnePoint = new(1, 1);
    public static readonly Point TwoPoint = new(2, 2);

    // Game
    public static readonly string[] TileNames = Enum.GetNames(typeof(TileType));
    public static int PlayerSpeed = 200;
    public const int MaxStack = 10;

    // Rendering and positioning
    public static readonly Rectangle ZeroSource = new(0, 0, TilePixelSize.X, TilePixelSize.Y); // Default tile source rect
    public static readonly Point[] PlayerCorners = [new(-20, -20), new(20, -20), new(-20, 0), new(20, 0)]; // Player bounding box corners, tl, tr, bl, br
    public static readonly Point PlayerBox = new(PlayerCorners[1].X - PlayerCorners[0].X, PlayerCorners[2].Y - PlayerCorners[1].Y);
    public static readonly Point MageSize = new(80, 80); // Size of the mage sprite in pixels
    public static readonly Point MageHalfSize = new(40, 40); // Half size of the mage sprite in pixels
    public static readonly Point MageDrawShift = new(MageHalfSize.X, 0); // Used for center aligning mages

    // Colors
    public static readonly Color NearBlack = new(85, 85, 85);
    public static readonly Color FocusBlue = new(174, 200, 209);
    public static readonly Color CottonCandy = new(242, 182, 240);
    public static readonly Color DarkenScreen = new(0, 0, 0, 188);
    public static readonly Color DebugPinkTint = new Color(255, 0, 255) * .5f;
    public static readonly Color DebugGreenTint = new Color(55, 255, 55) * .5f;

    // Minimap pixel colors
    public static readonly Color[] MiniMapColors = [
        new(92, 221, 241), // Sky
        Color.Green, // Grass
        Color.Blue, // Water
        Color.DarkGray, // StoneWall
        new(117, 66, 13), // Stairs
        new(155, 155, 195), // Flooring
        Color.Yellow, // Sand
        new(48, 25, 0), // Dirt
        new(18, 18, 18), // Darkness
        new(41, 15, 0), // Door
        new(102, 60, 14), // WoodPlanks
        Color.Gray, // Stone
    ];
}
