using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles
{
    public class Stairs : Tile
    {
        public string DestLevel { get; set; }
        public Xna.Point DestPosition { get; set; }
        public Stairs(Xna.Point location, string destLevel, Xna.Point destPosition) : base(location)
        {
            IsWalkable = true;
            DestLevel = destLevel;
            DestPosition = destPosition;
        }
        public override void OnPlayerEnter(GameHandler game)
        {
            // Load another level
            Console.WriteLine($"[System] Teleporting to level '{DestLevel}' @ {DestPosition.X}, {DestPosition.Y}");
            game.LoadLevel(DestLevel);
            game.Camera = DestPosition.ToVector2() * Constants.TileSize;
        }
    }
}
