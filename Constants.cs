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
        public static readonly Xna.Vector2 TileSize = new(64, 64);
        public const int PlayerSpeed = 200;
        public static readonly Xna.Vector2 Window = new(1400, 900);
        public static readonly Xna.Vector2 Middle = new(Window.X / 2, Window.Y / 2);
        public static readonly Xna.Point MiddleCoord = (Middle / TileSize).ToPoint();
        public static readonly string[] TileNames = ["Sky", "Grass", "Water", "Wall", "Stairs"];
        public static readonly Xna.Point NegOne = new(-1, -1);
        public static readonly Xna.Point MapSize = new(256, 256); // PUTTING THIS HIGHER THAN 256x256 CAN CAUSE ISSUES WITH THE SAVES USING A BYTE FOR EACH DIMENSION
    }
}
