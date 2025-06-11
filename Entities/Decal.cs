using System;
using System.Collections.Generic;
using System.Linq;

namespace Quest.Entities;
public enum DecalType
{
    Footprint = 0,
    Torch = 1,
    BlueTorch = 2,
    Sign = 3,
}
public class Decal
{
    protected static readonly Dictionary<DecalType, TextureID> TileToTexture = Enum.GetValues<DecalType>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureID>(tileType.ToString(), out var texture) ? texture : TextureID.Null
    );
    // Auto generated - no setter
    public TextureID Texture { get; }
    public Point Location { get; }
    // Properties - protected setter
    public Color Tint { get; protected set; } = Color.White;
    public DecalType Type { get; protected set; }
    public Decal(Point location)
    {
        // Initialize the tile
        Location = location;
        Type = (DecalType)Enum.Parse(typeof(DecalType), GetType().Name);
        Texture = TileToTexture[Type];
    }
    public virtual void Draw(IGameManager game)
    {
        // Draw
        Point dest = Location * Constants.TileSize - game.Camera.ToPoint() + Constants.Middle;
        Point size = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
        Rectangle source = new((int)((game.Time % .75) * TextureManager.Metadata[Texture].TileMap.X * 4 / 3) * size.X, 0, size.X, size.Y);
        DrawTexture(game.Batch, Texture, dest, source: source, scale: new(4), color: Tint);
    }
}
