using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles
{
    public class StoneWall : Tile
    {
        public StoneWall(Xna.Point location) : base(location)
        {
            IsWalkable = false;
        }
    }
}
