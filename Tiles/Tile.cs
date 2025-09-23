using System.Linq;

namespace Quest.Tiles;


public enum TileType
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
    // TILES
}
public class Tile
{
    protected static readonly Dictionary<TileType, TextureID> TileToTexture = Enum.GetValues<TileType>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureID>(tileType.ToString(), out var texture) ? texture : TextureID.Null
    );

    // Debug
    public int TileID { get; } = UIDManager.NewUID("Tiles");
    public bool Marked { get; set; }
    // Auto generated - no setter
    public Point Location { get; }
    public TextureID Texture { get; }
    // Properties - protected setter
    public bool IsWalkable { get; protected set; }
    public bool IsWall { get; protected set; }
    public TileType Type { get; protected set; }
    // Private 
    protected Color lightCache { get; set; }
    public Tile(Point location)
    {
        Location = location;
        IsWalkable = true;
        IsWall = false;
        Type = (TileType)Enum.Parse(typeof(TileType), GetType().Name);
        Texture = TileToTexture[Type];
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
