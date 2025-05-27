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
        public static readonly Xna.Vector3 TileSize = new(64, 32, 32);
        public static readonly Xna.Vector2 TileSize2D = new(TileSize.X, TileSize.Y);
        public const int PlayerSpeed = 150;
        public static readonly Xna.Vector2 Window = new(1400, 900);
        public static readonly Xna.Vector2 Middle = new Xna.Vector2(Constants.Window.X / 2, Constants.Window.Y / 2);
    }
}
