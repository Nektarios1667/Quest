using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xna = Microsoft.Xna.Framework;
using Quest.Tiles;

// TODO
// 3. Stairs
// 2. Signs
// 3. NPCs w/ dialog

namespace Quest {

    public class Level
    {
        public string Name { get; private set; }
        public Tile[] Tiles { get; private set; }
        public Xna.Vector2 Dimensions { get; private set; }
        public Level(string name, Tile[] tiles, Xna.Vector2 dim)
        {
            // Initialize the level
            Name = name;
            Tiles = tiles;
            Dimensions = dim;
        }
        public void Draw()
        {

        }
    }
}
