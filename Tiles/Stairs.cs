using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles
{
    public class Stairs : Tile
    {
        public Stairs(Xna.Point location) : base(location)
        {
            IsWalkable = true;
        }
        public override void OnPlayerEnter(GameHandler game)
        {
            // TODO Load another level
        }
    }
}
