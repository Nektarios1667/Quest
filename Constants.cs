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
        public const int PlayerSpeed = 150;
        public static readonly Xna.Vector2 Window = new(1400, 900);
        public static readonly Xna.Vector2 Middle = new Xna.Vector2(Constants.Window.X / 2, Constants.Window.Y / 2);
        public static readonly string[] TileNames = ["Water", "Grass", "Wall", "Stairs"];
    }
}
