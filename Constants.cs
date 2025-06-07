using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;
namespace Quest
{
    public static class Constants
    {
        // Debug
        public static readonly bool COLLISION_DEBUG = true; // Enable collision debug mode
        public static readonly bool TEXT_INFO = true; // Show text info on screen
        public static readonly bool FRAME_INFO = true; // Show frame info on screen
        public static readonly bool LOG_INFO = true; // Log game info
        public static readonly bool FRAME_BAR = true; // Frame time graphical bar
        public static readonly bool DRAW_HITBOXES = true; // Draw hitboxes for entities

        // Tile and map
        public static readonly Xna.Vector2 TileSize = new(64, 64); // In-game tile size
        public static readonly Xna.Vector2 TilePixelSize = new(16, 16); // Native resolution of tile images
        public static readonly Xna.Point TileMapDim = new(4, 4); // Dimensions of the connected texture map
        public static readonly Xna.Point MapSize = new(256, 256); // Map size in tiles - can not be above 256x256 because of save file
        public static readonly Xna.Vector2 TileSizeScale = TileSize / TilePixelSize; // Scale factor from tile pixel size to in-game tile size
        public static readonly List<Xna.Point> NeighborTiles = [new(0, 1), new(1, 0), new(0, -1), new(-1, 0)];

        // Screen
        public static readonly Xna.Vector2 Window = new(1400, 900); // Game window resolution
        public static readonly Xna.Vector2 Middle = new(Window.X / 2, Window.Y / 2); // Center of the screen
        public static readonly Xna.Point MiddleCoord = (Middle / TileSize).ToPoint(); // Center tile coordinate
        public static readonly float CameraRigidity = .07f; // Camera smoothing weight
        public const bool VSYNC = false;

        // Game
        public static readonly string[] TileNames = ["Sky", "Grass", "Water", "StoneWall", "Stairs", "Flooring", "Sand", "Dirt"];
        public const int PlayerSpeed = 200;
        public const int MaxStack = 10;

        // Rendering and positioning
        public static readonly Xna.Rectangle ZeroSource = new(0, 0, (int)TilePixelSize.X, (int)TilePixelSize.Y); // Default tile source rect
        public static readonly Xna.Vector2[] PlayerCorners = [new(-20, -20), new(20, -20), new(-20, 0), new(20, 0)]; // Player bounding box corners, tl, tr, bl, br
        public static readonly Xna.Vector2 PlayerBox = new(PlayerCorners[1].X - PlayerCorners[0].X, PlayerCorners[2].Y - PlayerCorners[1].Y);
        public static readonly Xna.Point MageSize = new(80, 80); // Size of the mage sprite in pixels
        public static readonly Xna.Point MageHalfSize = new(20, 40); // Half size of the mage sprite in pixels
        public static readonly Xna.Vector2 MageDrawShift = new(MageHalfSize.X, 0); // Used for center aligning mages

        // Utility
        public static readonly Xna.Vector2 HalfVec = new(0.5f, 0.5f);
        public static readonly Xna.Point NegOne = new(-1, -1); // Flipping vector

        // Colors
        public static readonly Xna.Color NearBlack = new(85, 85, 85);
        public static readonly Xna.Color FocusBlue = new(174, 200, 209);
        public static readonly Xna.Color CottonCandy = new(242, 182, 240);
        public static readonly Xna.Color DarkenScreen = new(0, 0, 0, 188);
        public static readonly Xna.Color DebugPinkTint = new Xna.Color(255, 0, 255) * .4f;
        public static readonly Xna.Color DebugGreenTint = new Xna.Color(55, 255, 55) * .4f;

        // Minimap pixel colors
        public static readonly Xna.Color[] MiniMapColors = [
            new(92, 221, 241), // Sky
            Xna.Color.Green, // Grass
            Xna.Color.Blue, // Water
            Xna.Color.Gray, // StoneWall
            new(117, 66, 13), // Stairs
            new(155, 155, 195), // Flooring
            Xna.Color.Yellow, // Sand
            new(48, 25, 0) // Dirt
        ];
    }
}
