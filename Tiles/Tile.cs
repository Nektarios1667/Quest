using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Quest.TextureManager;

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
        public Tile(Point location)
        {
            // Initialize the tile
            Location = location;
            IsWalkable = true;
            Type = (TileType)Enum.Parse(typeof(TileType), GetType().Name);
            Marked = false;
        }
        public virtual void Draw(IGameManager game)
        {
            // Draw
            Vector2 dest = Location.ToVector2() * Constants.TileSize - game.Camera + Constants.Middle;
            TextureID texture = (TextureID)(Enum.TryParse(typeof(TextureID), Type.ToString(), out var tex) ? tex : TextureID.Null);
            Color color = Marked ? Color.Red : Color.White;
            Rectangle rect = new((int)dest.X, (int)dest.Y, (int)Constants.TileSize.X, (int)Constants.TileSize.Y);
            DrawTexture(game.Batch, texture, rect, source:game.TileTextureSource(this), scale: new(4), color:color);
            Marked = false;
        }
        public virtual void OnPlayerEnter(IGameManager game) { }
        public virtual void OnPlayerExit(IGameManager game) { }
        public virtual void OnPlayerInteract(IGameManager game) { }

    }
}
