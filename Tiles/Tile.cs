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
    Jukebox,
    DiscWriter,
    Inscriber,
    Stove,
    Furnace,
    DisplayCase,
    Crate,
    // TILES ID
}

public class TileType
{
    public TileTypeID ID { get; }
    public TextureID Texture { get; }
    public bool IsWalkable { get; }
    public bool IsWall { get; }
    public float Weight { get; }
    public TileType(TileTypeID id, TextureID texture, bool isWalkable, bool isWall, float weight = 1f)
    {
        ID = id;
        Texture = texture;
        IsWalkable = isWalkable;
        IsWall = isWall;
        Weight = weight;
    }
}

public static class TileTypes
{
    // Must be in same order as TileTypeID enum
    public static readonly TileType[] All =
    [
        new(TileTypeID.Sky, TextureID.Sky, false, false),
        new(TileTypeID.Grass, TextureID.Grass, true, false),
        new(TileTypeID.Water, TextureID.Water, false, false),
        new(TileTypeID.StoneWall, TextureID.StoneWall, false, true),
        new(TileTypeID.Stairs, TextureID.Stairs, true, false, weight: float.MaxValue),
        new(TileTypeID.Flooring, TextureID.Flooring, true, false, weight : 0.75f),
        new(TileTypeID.Sand, TextureID.Sand, true, false, weight : 1.5f),
        new(TileTypeID.Dirt, TextureID.Dirt, true, false, weight : 1.1f),
        new(TileTypeID.Darkness, TextureID.Darkness, false, false),
        new(TileTypeID.Door, TextureID.Door, false, true),
        new(TileTypeID.WoodFlooring, TextureID.WoodFlooring, true, false, weight:0.75f),
        new(TileTypeID.Stone, TextureID.Stone, true, false, weight : 0.9f),
        new(TileTypeID.Chest, TextureID.Chest, false, false),
        new(TileTypeID.ConcreteWall, TextureID.ConcreteWall, false, true),
        new(TileTypeID.WoodWall, TextureID.WoodWall, false, true),
        new(TileTypeID.Path, TextureID.Path, true, false, weight:0.5f),
        new(TileTypeID.Lava, TextureID.Lava, false, false),
        new(TileTypeID.StoneTiles, TextureID.StoneTiles, true, false, weight: 0.75f),
        new(TileTypeID.RedTiles, TextureID.RedTiles, true, false, weight: 0.75f),
        new(TileTypeID.OrangeTiles, TextureID.OrangeTiles, true, false, weight: 0.75f),
        new(TileTypeID.YellowTiles, TextureID.YellowTiles, true, false, weight: 0.75f),
        new(TileTypeID.LimeTiles, TextureID.LimeTiles, true, false, weight: 0.75f),
        new(TileTypeID.GreenTiles, TextureID.GreenTiles, true, false, weight: 0.75f),
        new(TileTypeID.CyanTiles, TextureID.CyanTiles, true, false, weight: 0.75f),
        new(TileTypeID.BlueTiles, TextureID.BlueTiles, true, false, weight: 0.75f),
        new(TileTypeID.PurpleTiles, TextureID.PurpleTiles, true, false, weight: 0.75f),
        new(TileTypeID.PinkTiles, TextureID.PinkTiles, true, false, weight: 0.75f),
        new(TileTypeID.BlackTiles, TextureID.BlackTiles, true, false, weight: 0.75f),
        new(TileTypeID.BrownTiles, TextureID.BrownTiles, true, false, weight: 0.75f),
        new(TileTypeID.IronWall, TextureID.IronWall, false, true),
        new(TileTypeID.Snow, TextureID.Snow, true, false, weight:1.5f),
        new(TileTypeID.Ice, TextureID.Ice, true, false, weight:3),
        new(TileTypeID.SnowyGrass, TextureID.SnowyGrass, true, false),
        new(TileTypeID.Lamp, TextureID.Lamp, true, false, weight:1.5f),
        new(TileTypeID.Sandstone, TextureID.Sandstone, true, false),
        new(TileTypeID.SandstoneWall, TextureID.SandstoneWall, false, true),
        new(TileTypeID.Jukebox, TextureID.Jukebox, false, false),
        new(TileTypeID.DiscWriter, TextureID.DiscWriter, false, false),
        new(TileTypeID.Inscriber, TextureID.Inscriber, false, false),
        new(TileTypeID.Stove, TextureID.Stove, false, false),
        new(TileTypeID.Furnace, TextureID.Furnace, false, false),
        new(TileTypeID.DisplayCase, TextureID.DisplayCase, false, false),
        new(TileTypeID.Crate, TextureID.Crate, false, false),
        // TILES REGISTER
    ];
}

public class Tile
{
    // Properties
    public byte TypeID { get; }
    public ByteCoord Location { get; }
    // Computed properties
    public byte X => Location.X;
    public byte Y => Location.Y;
    public virtual bool IsWalkable => Type.IsWalkable;
    public bool IsWall => Type.IsWall;
    public virtual float Weight => Type.Weight;
    public ushort TileID => (ushort)(X + Y * Constants.MapSize.X);
    public TileType Type => TileTypes.All[TypeID];

    public Tile(Point location, TileTypeID type)
    {
        Location = new(location);
        TypeID = (byte)type;
    }
    public Tile(ByteCoord location, TileTypeID type)
    {
        Location = location;
        TypeID = (byte)type;
    }
    public virtual void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale);
    }

    public virtual void OnPlayerEnter(GameManager game, PlayerManager player) { }
    public virtual void OnPlayerCollide(GameManager game, PlayerManager player) { }
    public static Tile TileFromId(TileTypeID type, Point location, string levelName)
    {
        // Create a tile from an id
        return type switch
        {
            TileTypeID.Water => new Water(location),
            TileTypeID.Lava => new Lava(location),
            TileTypeID.Stairs => new Stairs(location, "", Constants.MiddleCoord),
            TileTypeID.Door => new Door(location, null),
            TileTypeID.Chest => new Chest(location, LootPreset.EmptyPreset, "_"),
            TileTypeID.Lamp => new Lamp(location),
            TileTypeID.Jukebox => new Jukebox(location, levelName),
            TileTypeID.DiscWriter => new DiscWriter(location, levelName),
            TileTypeID.Inscriber => new Inscriber(location, levelName),
            TileTypeID.Furnace => new Furnace(location, levelName),
            TileTypeID.Stove => new Stove(location, levelName),
            TileTypeID.DisplayCase => new DisplayCase(location, levelName),
            TileTypeID.Crate => new Crate(location, levelName),
            _ => new(location, type)
        };
    }
}
