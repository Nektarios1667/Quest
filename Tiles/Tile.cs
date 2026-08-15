namespace Quest.Tiles;


public enum TileTypeIDOld : byte
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
    GrayTiles,
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
    Crafter,
    ConcreteFlooring,
    StoneWindow,
    ConcreteWindow,
    IronWindow,
    SandstoneWindow,
    WoodWindow,
    IceWall,
    MudWall,
    SnowWall,
    MudWindow,
    SnowWindow,
    IceWindow,
    DryWall,
    DryWallWindow,
    Gravel,
    WetSand,
    RedSand,
    WetRedSand,
    VolcanicSand,
    WetVolcanicSand,
    WhiteTiles,
    Farmland,
    PressurePlate,
    TimedPressurePlate,
    // TILES ID
}
public enum TileTypeID : byte
{
    Darkness,
    Sky,
    Grass,
    Dirt,
    Sand,
    RedSand,
    WetSand,
    WetRedSand,
    VolcanicSand,
    WetVolcanicSand,
    Gravel,
    Farmland,
    Path,
    Stone,
    StoneWall,
    Stairs,
    Sandstone,
    SandstoneWall,
    SandstoneWindow,
    StoneWindow,
    ConcreteWall,
    ConcreteFlooring,
    ConcreteWindow,
    WoodWall,
    WoodFlooring,
    WoodWindow,
    DryWall,
    DryWallWindow,
    MudWall,
    MudWindow,
    Snow,
    SnowyGrass,
    SnowWall,
    SnowWindow,
    Ice,
    IceWall,
    IceWindow,
    IronWall,
    IronWindow,
    Flooring,
    GrayTiles,
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
    WhiteTiles,
    Water,
    Lava,
    Door,
    Chest,
    Crate,
    DisplayCase,
    Lamp,
    Jukebox,
    DiscWriter,
    Inscriber,
    Stove,
    Furnace,
    Crafter,
    PressurePlate,
    TimedPressurePlate,
    // TILES ID
}
public class TileType
{
    public TileTypeID ID { get; }
    public TextureID Texture { get; }
    public bool IsWalkable { get; }
    public bool IsWall { get; }
    public bool IsTransparent { get; }
    public float Weight { get; }
    public TileType(TileTypeID id, TextureID texture, bool isWalkable, bool isWall, bool isTransparent = false, float weight = 1f)
    {
        ID = id;
        Texture = texture;
        IsWalkable = isWalkable;
        IsWall = isWall;
        Weight = weight;
        IsTransparent = isTransparent;
    }
}

public static class TileTypes
{
    // Must be in same order as TileTypeID enum
    public static readonly TileType[] All = [
        new(TileTypeID.Darkness, TextureID.Darkness, false, false),
        new(TileTypeID.Sky, TextureID.Sky, false, false, weight: 0),
        new(TileTypeID.Grass, TextureID.Grass, true, false),
        new(TileTypeID.Dirt, TextureID.Dirt, true, false, weight: 1.1f),
        new(TileTypeID.Sand, TextureID.Sand, true, false, weight: 1.5f),
        new(TileTypeID.RedSand, TextureID.RedSand, true, false),
        new(TileTypeID.WetSand, TextureID.WetSand, true, false),
        new(TileTypeID.WetRedSand, TextureID.WetRedSand, true, false),
        new(TileTypeID.VolcanicSand, TextureID.VolcanicSand, true, false),
        new(TileTypeID.WetVolcanicSand, TextureID.WetVolcanicSand, true, false),
        new(TileTypeID.Gravel, TextureID.Gravel, true, false),
        new(TileTypeID.Farmland, TextureID.Farmland, true, false),
        new(TileTypeID.Path, TextureID.Path, true, false, weight: 0.5f),
        new(TileTypeID.Stone, TextureID.Stone, true, false, weight: 0.9f),
        new(TileTypeID.StoneWall, TextureID.StoneWall, false, true, weight: 7),
        new(TileTypeID.Stairs, TextureID.Stairs, true, false, weight: float.MaxValue),
        new(TileTypeID.Sandstone, TextureID.Sandstone, true, false),
        new(TileTypeID.SandstoneWall, TextureID.SandstoneWall, false, true, weight: 5),
        new(TileTypeID.SandstoneWindow, TextureID.SandstoneWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.StoneWindow, TextureID.StoneWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.ConcreteWall, TextureID.ConcreteWall, false, true, weight: 6.5f),
        new(TileTypeID.ConcreteFlooring, TextureID.ConcreteFlooring, true, false),
        new(TileTypeID.ConcreteWindow, TextureID.ConcreteWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.WoodWall, TextureID.WoodWall, false, true, weight: 6),
        new(TileTypeID.WoodFlooring, TextureID.WoodFlooring, true, false, weight: 0.75f),
        new(TileTypeID.WoodWindow, TextureID.WoodWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.DryWall, TextureID.DryWall, false, true, weight: 8),
        new(TileTypeID.DryWallWindow, TextureID.DryWallWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.MudWall, TextureID.MudWall, false, true, weight: 8),
        new(TileTypeID.MudWindow, TextureID.MudWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.Snow, TextureID.Snow, true, false, weight: 1.5f),
        new(TileTypeID.SnowyGrass, TextureID.SnowyGrass, true, false),
        new(TileTypeID.SnowWall, TextureID.SnowWall, false, true, weight: 7),
        new(TileTypeID.SnowWindow, TextureID.SnowWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.Ice, TextureID.Ice, true, false, weight: 3),
        new(TileTypeID.IceWall, TextureID.IceWall, false, true, weight: 5),
        new(TileTypeID.IceWindow, TextureID.IceWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.IronWall, TextureID.IronWall, false, true, weight: 7),
        new(TileTypeID.IronWindow, TextureID.IronWindow, false, true, isTransparent: true, weight: 4),
        new(TileTypeID.Flooring, TextureID.Flooring, true, false, weight: 0.75f),
        new(TileTypeID.GrayTiles, TextureID.GrayTiles, true, false, weight: 0.75f),
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
        new(TileTypeID.WhiteTiles, TextureID.WhiteTiles, true, false),
        new(TileTypeID.Water, TextureID.Water, false, false),
        new(TileTypeID.Lava, TextureID.Lava, false, false),
        new(TileTypeID.Door, TextureID.Door, false, true),
        new(TileTypeID.Chest, TextureID.Chest, false, false),
        new(TileTypeID.Crate, TextureID.Crate, false, false, weight: 0),
        new(TileTypeID.DisplayCase, TextureID.DisplayCase, false, false, weight: 0),
        new(TileTypeID.Lamp, TextureID.Lamp, true, false, weight: 1.5f),
        new(TileTypeID.Jukebox, TextureID.Jukebox, false, false, weight: 0),
        new(TileTypeID.DiscWriter, TextureID.DiscWriter, false, false, weight: 0),
        new(TileTypeID.Inscriber, TextureID.Inscriber, false, false, weight: 0),
        new(TileTypeID.Stove, TextureID.Stove, false, false, weight: 0),
        new(TileTypeID.Furnace, TextureID.Furnace, false, false, weight: 0),
        new(TileTypeID.Crafter, TextureID.Crafter, false, false, weight: 0),
        new(TileTypeID.PressurePlate, TextureID.PressurePlate, true, false),
        new(TileTypeID.TimedPressurePlate, TextureID.TimedPressurePlate, true, false),
        // TILES REGISTER
    ];
}

public class Tile
{
    // Properties
    public TileTypeID TypeID { get; }
    public ByteCoord Location { get; }
    // Computed properties
    public byte X => Location.X;
    public byte Y => Location.Y;
    public bool IsWall => Type.IsWall;
    public virtual bool IsWalkable => Type.IsWalkable; // Door changes this depending on if its open/closed
    public virtual bool IsTransparent => Type.IsTransparent; // Door changes this depending on if its open/closed
    public virtual float Weight => Type.Weight;
    public ushort UID => (ushort)(X + Y * Constants.MapSize.X);
    public TileType Type => TileTypes.All[(byte)TypeID];

    public Tile(Point location, TileTypeID type)
    {
        Location = new(location);
        TypeID = type;
    }
    public Tile(ByteCoord location, TileTypeID type)
    {
        Location = location;
        TypeID = type;
    }
    public virtual void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = CameraManager.TileToScreen(Location);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: Constants.TileSizeScale);

        // Connected textures debug
        if (!DebugManager.TileConnectionsDebug) return;
        int mask = gameManager.LevelManager.TileConnectionsMask(this);
        gameManager.Batch.DrawPoint(dest.ToVector2() + new Vector2(0, Constants.TileSize.Y / 2), (mask & 1) == 0 ? Color.Red : Color.Green, size: 5);     // Left
        gameManager.Batch.DrawPoint(dest.ToVector2() + new Vector2(Constants.TileSize.X, Constants.TileSize.Y / 2), (mask & 4) == 0 ? Color.Red : Color.Green, size: 5); // Right
        gameManager.Batch.DrawPoint(dest.ToVector2() + new Vector2(Constants.TileSize.X / 2, 0), (mask & 8) == 0 ? Color.Red : Color.Green, size: 5); // Up
        gameManager.Batch.DrawPoint(dest.ToVector2() + new Vector2(Constants.TileSize.X / 2, Constants.TileSize.Y), (mask & 2) == 0 ? Color.Red : Color.Green, size: 5); // Down
    }

    public virtual void OnPlayerEnter(GameManager gameManager,PlayerManager player) { }
    public virtual void OnPlayerCollide(GameManager gameManager,PlayerManager player) { }
    public static Tile TileFromId(TileTypeID type, Point location, string levelName)
    {
        // Create a tile from an id
        return type switch
        {
            TileTypeID.Water => new Water(location),
            TileTypeID.Lava => new Lava(location),
            TileTypeID.Stairs => new Stairs(location, LevelPath.Null, location),
            TileTypeID.Door => new Door(location, null),
            TileTypeID.Chest => new Chest(location, LootPreset.EmptyPreset, levelName),
            TileTypeID.Lamp => new Lamp(location),
            TileTypeID.Jukebox => new Jukebox(location, levelName),
            TileTypeID.DiscWriter => new DiscWriter(location, levelName),
            TileTypeID.Inscriber => new Inscriber(location, levelName),
            TileTypeID.Furnace => new Furnace(location, levelName),
            TileTypeID.Stove => new Stove(location, levelName),
            TileTypeID.DisplayCase => new DisplayCase(location, levelName),
            TileTypeID.Crate => new Crate(location, levelName),
            TileTypeID.Crafter => new Crafter(location, levelName),
            TileTypeID.PressurePlate => new PressurePlate(location, levelName, TileEffect.None, ByteCoord.Zero, LevelPath.Null),
            TileTypeID.TimedPressurePlate => new TimedPressurePlate(location, levelName, TileEffect.None, ByteCoord.Zero, LevelPath.Null, 10f),
            // TILEFROMID
            _ => new(location, type)
        };
    }
}
