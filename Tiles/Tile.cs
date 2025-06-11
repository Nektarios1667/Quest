using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Tiles;


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
    Darkness = 8,
    Door = 9,
}
public class Tile
{
    protected static readonly Dictionary<TileType, TextureID> TileToTexture = Enum.GetValues<TileType>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureID>(tileType.ToString(), out var texture) ? texture : TextureID.Null
    );

    // Debug
    public bool Marked { get; set; }
    // Auto generated - no setter
    public Point Location { get; }
    public TextureID Texture { get; }
    // Properties - protected setter
    public bool IsWalkable { get; protected set; }
    public TileType Type { get; protected set; }
    public Tile(Point location)
    {
        Location = location;
        IsWalkable = true;
        Type = (TileType)Enum.Parse(typeof(TileType), GetType().Name);
        Texture = TileToTexture[Type];
        Marked = false;
    }
    public virtual void Draw(IGameManager game)
    {
        // Draw
        Point dest = Location * Constants.TileSize - game.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        DrawTexture(game.Batch, Texture, dest, source: game.TileTextureSource(this), scale: new(4), color: color);
        Marked = false;
    }
    public virtual void OnPlayerEnter(IGameManager game) { }
    public virtual void OnPlayerExit(IGameManager game) { }
    public virtual void OnPlayerInteract(IGameManager game) { }
    public virtual void OnPlayerCollide(IGameManager game) { }
}
