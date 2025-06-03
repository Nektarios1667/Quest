using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.MediaFoundation.DirectX;
using Xna = Microsoft.Xna.Framework;
namespace Quest
{
    public static class Constants
    {
        // Tile and map
        public static readonly Xna.Vector2 TileSize = new(64, 64); // In-game tile size
        public static readonly Xna.Vector2 TilePixelSize = new(16, 16); // Native resolution of tile images
        public static readonly Xna.Point TileMapDim = new(4, 4); // Dimensions of the connected texture map
        public static readonly Xna.Point MapSize = new(256, 256); // Map size in tiles - can not be above 256x256 because of save file
        public static readonly Xna.Vector2 TileSizeScale = TileSize / TilePixelSize; // Scale factor from tile pixel size to in-game tile size

        // Screen
        public static readonly Xna.Vector2 Window = new(1400, 900); // Game window resolution
        public static readonly Xna.Vector2 Middle = new(Window.X / 2, Window.Y / 2); // Center of the screen
        public static readonly Xna.Point MiddleCoord = (Middle / TileSize).ToPoint(); // Center tile coordinate
        public static readonly float CameraRigidity = .1f; // Camera smoothing weight

        // Game
        public static readonly string[] TileNames = ["Sky", "Grass", "Water", "StoneWall", "Stairs", "Flooring", "Sand", "Dirt"];
        public static readonly string[] ItemNames = ["Sword", "Pickaxe"];
        public const int PlayerSpeed = 200;
        public const int MaxStack = 10;

        // Rendering and positioning
        public static readonly Xna.Rectangle ZeroSource = new(0, 0, (int)TilePixelSize.X, (int)TilePixelSize.Y); // Default tile source rect
        public static readonly Xna.Vector2[] PlayerCorners = [new(-24, -10), new(24, -10), new(-24, 24), new(24, 24)]; // Player bounding box corners

        // Utility
        public static readonly Xna.Point NegOne = new(-1, -1); // Flipping vector

        // Colors
        public static readonly Xna.Color NearBlack = new(85, 85, 85);
        public static readonly Xna.Color FocusBlue = new(174, 200, 209);
        public static readonly Xna.Color CottonCandy = new(242, 182, 240);

        // Texture default sizes
        public static readonly Xna.Point SlotTextureSize = new(80, 80); // Default size for item slots
        public static readonly Xna.Point ItemTextureSize = new(60, 60); // Default size for items
    }
}
