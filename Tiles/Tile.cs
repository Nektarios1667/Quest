using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Tiles
{
    public enum TileType
    {
        Sky = 0,
        Grass = 1,
        Water = 2,
        StoneWall = 3,
        Stairs = 4,
        Flooring = 5,
        Sand = 6,
        Dirt = 7,
    }
    public class Tile
    {
        // Debug
        public bool Marked { get; set; }
        // Auto generated - no setter
        public Xna.Point Location { get;}
        // Properties - protected setter
        public bool IsWalkable { get; protected set; }
        public TileType Type { get; protected set; }
        public Tile(Xna.Point location)
        {
            // Initialize the tile
            Location = location;
            IsWalkable = true;
            Type = (TileType)Enum.Parse(typeof(TileType), GetType().Name);
            Marked = false;
        }
        public virtual void OnPlayerEnter(GameHandler game) { }
        public virtual void OnPlayerExit(GameHandler game) { }
        public virtual void OnPlayerInteract(GameHandler game) { }
    }
}
