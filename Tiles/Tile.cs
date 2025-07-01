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
    WoodPlanks,
    Stone,
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
    public virtual void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        DrawTexture(gameManager.Batch, Texture, dest, source: gameManager.LevelManager.TileTextureSource(this), scale: 4, color: color);

        // Lighting
        float distSq = Vector2.DistanceSquared(dest.ToVector2() + Constants.HalfVec, Constants.Middle.ToVector2() + CameraManager.CameraOffset - Constants.MageHalfSize.ToVector2()) / (Constants.TileSize.X * Constants.TileSize.Y);
        Color lighting = Color.Lerp(Color.Transparent, gameManager.LevelManager.SkyLight, Math.Clamp(distSq / 25, 0, 1));
        gameManager.Batch.FillRectangle(new(dest.ToVector2(), Constants.TileSize), lighting);

        Marked = false;
    }
    public virtual void OnPlayerEnter(GameManager game) { }
    public virtual void OnPlayerExit(GameManager game) { }
    public virtual void OnPlayerInteract(GameManager game) { }
    public virtual void OnPlayerCollide(GameManager game) { }
}
