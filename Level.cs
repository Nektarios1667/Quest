using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xna = Microsoft.Xna.Framework;

// TODO
// 1. Player model w/ directions
// 2. Collision
// 3. Stairs
// 2. GUI system (MonoGUI or custom)
// 2. Signs
// 3. NPCs w/ dialog

namespace Quest {
    public struct Tile
    {
        public enum TileType
        {
            Water = 0,
            Grass = 1,
            Wall = 2,
            Door = 3,
        }
        public static readonly TileType[] Walkable = [TileType.Grass];

        public Xna.Vector2 Location { get; private set; }
        public readonly bool IsWalkable => Walkable.Contains(Type);
        public TileType Type { get; private set; }
        public Tile(Xna.Vector2 location, TileType type)
        {
            // Initialize the tile
            Location = location;
            Type = type;
        }

    }

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
