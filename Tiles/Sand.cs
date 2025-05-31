using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles
{
    public class Sand : Tile
    {
        public Sand(Xna.Point location) : base(location)
        {
            IsWalkable = true;
        }
    }
}
