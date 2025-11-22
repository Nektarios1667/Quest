using System.Linq;

namespace Quest.Tiles;


public enum TileTypeID : byte
{
    Sky,
    Grass,
    Water,
    StoneWall,
    Stairs,
    Flooring,
    Sand,
    Dirt,
    Darkness,
    Door,
    WoodFlooring,
    Stone,
    Chest,
    ConcreteWall,
    WoodWall,
    Path,
    Lava,
    StoneTiles,
    RedTiles,
    OrangeTiles,
    YellowTiles,
    LimeTiles,
    GreenTiles,
    CyanTiles,
    BlueTiles,
    PurpleTiles,
    PinkTiles,
    BlackTiles,
    BrownTiles,
    IronWall,
    Snow,
    Ice,
    SnowyGrass,
    Lamp,
    // TILES
}

public class TileType
{
    public TileTypeID ID { get; }
    public TextureID Texture { get; }
    public bool IsWalkable { get; }
    public bool IsWall { get; }
    public TileType(TileTypeID id, TextureID texture, bool isWalkable, bool isWall)
    {
        ID = id;
        Texture = texture;
        IsWalkable = isWalkable;
        IsWall = isWall;
    }
}

public static class TileTypes
{
    public static readonly TileType Sky            = new(TileTypeID.Sky, TextureID.Sky, false, false);
    public static readonly TileType Grass          = new(TileTypeID.Grass, TextureID.Grass, true, false);
    public static readonly TileType Water          = new(TileTypeID.Water, TextureID.Water, false, false);
    public static readonly TileType StoneWall      = new(TileTypeID.StoneWall, TextureID.StoneWall,false, true);
    public static readonly TileType Stairs         = new(TileTypeID.Stairs, TextureID.Stairs, true, false);
    public static readonly TileType Flooring       = new(TileTypeID.Flooring, TextureID.Flooring, true,  false);
    public static readonly TileType Sand           = new(TileTypeID.Sand, TextureID.Sand, true, false);
    public static readonly TileType Dirt           = new(TileTypeID.Dirt, TextureID.Dirt, true, false);
    public static readonly TileType Darkness       = new(TileTypeID.Darkness, TextureID.Darkness, false, false);
    public static readonly TileType Door           = new(TileTypeID.Door, TextureID.Door, false, true);   // closed door
    public static readonly TileType WoodFlooring   = new(TileTypeID.WoodFlooring, TextureID.WoodFlooring, true, false);
    public static readonly TileType Stone          = new(TileTypeID.Stone, TextureID.Stone, true, false);
    public static readonly TileType Chest          = new(TileTypeID.Chest, TextureID.Chest, false, false);
    public static readonly TileType ConcreteWall   = new(TileTypeID.ConcreteWall, TextureID.ConcreteWall, false, true);
    public static readonly TileType WoodWall       = new(TileTypeID.WoodWall, TextureID.WoodWall, false, true);
    public static readonly TileType Path           = new(TileTypeID.Path, TextureID.Path, true,  false);
    public static readonly TileType Lava           = new(TileTypeID.Lava, TextureID.Lava, false, false);
    public static readonly TileType StoneTiles     = new(TileTypeID.StoneTiles, TextureID.StoneTiles, true, false);
    public static readonly TileType RedTiles       = new(TileTypeID.RedTiles, TextureID.RedTiles, true, false);
    public static readonly TileType OrangeTiles    = new(TileTypeID.OrangeTiles, TextureID.OrangeTiles, true, false);
    public static readonly TileType YellowTiles    = new(TileTypeID.YellowTiles, TextureID.YellowTiles, true, false);
    public static readonly TileType LimeTiles      = new(TileTypeID.LimeTiles, TextureID.LimeTiles, true,  false);
    public static readonly TileType GreenTiles     = new(TileTypeID.GreenTiles, TextureID.GreenTiles, true, false);
    public static readonly TileType CyanTiles      = new(TileTypeID.CyanTiles, TextureID.CyanTiles, true, false);
    public static readonly TileType BlueTiles      = new(TileTypeID.BlueTiles, TextureID.BlueTiles, true, false);
    public static readonly TileType PurpleTiles    = new(TileTypeID.PurpleTiles, TextureID.PurpleTiles, true, false);
    public static readonly TileType PinkTiles      = new(TileTypeID.PinkTiles, TextureID.PinkTiles, true, false);
    public static readonly TileType BlackTiles     = new(TileTypeID.BlackTiles, TextureID.BlackTiles, true, false);
    public static readonly TileType BrownTiles     = new(TileTypeID.BrownTiles, TextureID.BrownTiles, true, false);
    public static readonly TileType IronWall       = new(TileTypeID.IronWall, TextureID.IronWall, false, true);
    public static readonly TileType Snow           = new(TileTypeID.Snow, TextureID.Snow, true, false);
    public static readonly TileType Ice            = new(TileTypeID.Ice, TextureID.Ice, true, false);
    public static readonly TileType SnowyGrass     = new(TileTypeID.SnowyGrass, TextureID.SnowyGrass, true, false);
    public static readonly TileType Lamp           = new(TileTypeID.Lamp, TextureID.Lamp, true, false);
}


public class Tile
{
    protected static readonly Dictionary<TileTypeID, TextureID> TileToTexture = Enum.GetValues<TileTypeID>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureID>(tileType.ToString(), out var texture) ? texture : TextureID.Null
    );

    // Debug
    public bool Marked { get; set; }
    // Auto generated - no setter
    public byte X { get; }
    public byte Y { get; }
    public Point Location => new(X, Y);
    public ushort TileID => (ushort)(X + Y * Constants.MapSize.X);
    // Type
    public TileType Type { get; }
    public TextureID Texture => Type.Texture;
    public virtual bool IsWalkable => Type.IsWalkable;
    public bool IsWall => Type.IsWall;
    // Private 
    public Tile(Point location, TileType type)
    {
        X = (byte)location.X;
        Y = (byte)location.Y;
        Type = type;
        Marked = false;
    }
    public virtual void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        DrawTexture(gameManager.Batch, Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale, color: color);

        // Final
        Marked = false;
    }

    public virtual void OnPlayerEnter(GameManager game, PlayerManager player) { }
    public virtual void OnPlayerCollide(GameManager game, PlayerManager player) { }
}
