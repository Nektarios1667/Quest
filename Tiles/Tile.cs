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
    Sandstone,
    SandstoneWall,
    // TILES ID
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
    public static readonly TileType[] All = new TileType[]
    {
        new(TileTypeID.Sky, TextureID.Sky, false, false),
        new(TileTypeID.Grass, TextureID.Grass, true, false),
        new(TileTypeID.Water, TextureID.Water, false, false),
        new(TileTypeID.StoneWall, TextureID.StoneWall, false, true),
        new(TileTypeID.Stairs, TextureID.Stairs, true, false),
        new(TileTypeID.Flooring, TextureID.Flooring, true, false),
        new(TileTypeID.Sand, TextureID.Sand, true, false),
        new(TileTypeID.Dirt, TextureID.Dirt, true, false),
        new(TileTypeID.Darkness, TextureID.Darkness, false, false),
        new(TileTypeID.Door, TextureID.Door, false, true),
        new(TileTypeID.WoodFlooring, TextureID.WoodFlooring, true, false),
        new(TileTypeID.Stone, TextureID.Stone, true, false),
        new(TileTypeID.Chest, TextureID.Chest, false, false),
        new(TileTypeID.ConcreteWall, TextureID.ConcreteWall, false, true),
        new(TileTypeID.WoodWall, TextureID.WoodWall, false, true),
        new(TileTypeID.Path, TextureID.Path, true, false),
        new(TileTypeID.Lava, TextureID.Lava, false, false),
        new(TileTypeID.StoneTiles, TextureID.StoneTiles, true, false),
        new(TileTypeID.RedTiles, TextureID.RedTiles, true, false),
        new(TileTypeID.OrangeTiles, TextureID.OrangeTiles, true, false),
        new(TileTypeID.YellowTiles, TextureID.YellowTiles, true, false),
        new(TileTypeID.LimeTiles, TextureID.LimeTiles, true, false),
        new(TileTypeID.GreenTiles, TextureID.GreenTiles, true, false),
        new(TileTypeID.CyanTiles, TextureID.CyanTiles, true, false),
        new(TileTypeID.BlueTiles, TextureID.BlueTiles, true, false),
        new(TileTypeID.PurpleTiles, TextureID.PurpleTiles, true, false),
        new(TileTypeID.PinkTiles, TextureID.PinkTiles, true, false),
        new(TileTypeID.BlackTiles, TextureID.BlackTiles, true, false),
        new(TileTypeID.BrownTiles, TextureID.BrownTiles, true, false),
        new(TileTypeID.IronWall, TextureID.IronWall, false, true),
        new(TileTypeID.Snow, TextureID.Snow, true, false),
        new(TileTypeID.Ice, TextureID.Ice, true, false),
        new(TileTypeID.SnowyGrass, TextureID.SnowyGrass, true, false),
        new(TileTypeID.Lamp, TextureID.Lamp, true, false),
        new(TileTypeID.Sandstone, TextureID.Sandstone, true, false),
        new(TileTypeID.SandstoneWall, TextureID.SandstoneWall, false, true),
        // TILES REGISTER
    };
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
    public byte X => Location.X;
    public byte Y => Location.Y;
    public ByteCoord Location { get; }
    public ushort TileID => (ushort)(X + Y * Constants.MapSize.X);
    // Type
    public byte TypeID { get; }
    public TileType Type => TileTypes.All[TypeID];
    public TextureID Texture => Type.Texture;
    public virtual bool IsWalkable => Type.IsWalkable;
    public bool IsWall => Type.IsWall;
    // Private 
    public Tile(Point location, TileTypeID type)
    {
        Location = new(location);
        Marked = false;
        TypeID = (byte)type;
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
