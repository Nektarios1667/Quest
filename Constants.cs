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
        public static readonly Xna.Vector2 TileSize = new(64, 64); // Tiles are 64x64 pixels in the game world
        public static readonly Xna.Vector2 TilePixelSize = new(16, 16); // Tile images are 16x16 and get later scaled
        public const int PlayerSpeed = 200;
        public static readonly Xna.Vector2 Window = new(1400, 900);
        public static readonly Xna.Vector2 Middle = new(Window.X / 2, Window.Y / 2);
        public static readonly Xna.Point MiddleCoord = (Middle / TileSize).ToPoint();
        public static readonly string[] TileNames = ["Sky", "Grass", "Water", "StoneWall", "Stairs", "Flooring", "Sand", "Dirt"];
        public static readonly Xna.Point NegOne = new(-1, -1);
        public static readonly Xna.Point MapSize = new(256, 256); // PUTTING THIS HIGHER THAN 256x256 CAN CAUSE ISSUES WITH THE SAVES USING A BYTE FOR EACH DIMENSION
        public static readonly float CameraRigidity = 0.1f; // Camera lerping weight: 1 = instant, 0.1 = quick smooth
        public static readonly Xna.Point TileMapDim = new(4, 4);
        public static readonly Xna.Rectangle ZeroSource = new(0, 0, (int)TilePixelSize.X, (int)TilePixelSize.Y);
        public static readonly Xna.Vector2[] PlayerCorners = [new(-24, -10), new(24, -10), new(-24, 24), new(24, 24)];
    }
}
