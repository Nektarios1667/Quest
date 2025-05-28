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
        Tile = -1,
        Water = 0,
        Grass = 1,
        Wall = 2,
        Stairs = 3,
    }
    public class Tile
    {
        // Auto generated - no setter
        public Xna.Point Location { get;}
        public TileType Type { get; }
        // Properties - protected setter
        public bool IsWalkable { get; protected set; }
        public Tile(Xna.Point location)
        {
            // Initialize the tile
            Location = location;
            Type = (TileType)Enum.Parse(typeof(TileType), GetType().Name);
            IsWalkable = true;
        }
        public virtual void OnPlayerEnter(GameHandler game) { }
        public virtual void OnPlayerExit(GameHandler game) { }
        public virtual void OnPlayerInteract(GameHandler game) { }
    }
}
