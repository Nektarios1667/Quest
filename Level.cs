using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xna = Microsoft.Xna.Framework;
using Quest.Tiles;
using System.Drawing.Text;

// TODO
// 3. Stairs
// 2. Signs
// 3. NPCs w/ dialog

namespace Quest {

    public class Level
    {
        public NPC[] NPCs { get; private set; }
        public string Name { get; private set; }
        public Tile[] Tiles { get; private set; }
        public Xna.Point Spawn { get; private set; }
        public Level(string name, Tile[] tiles, Xna.Point spawn, NPC[] npcs)
        {
            // Initialize the level
            Name = name;
            Tiles = tiles;
            Spawn = spawn;
            NPCs = npcs;
        }
    }
}
