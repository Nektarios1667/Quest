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
    protected static readonly Dictionary<DecalType, TextureManager.TextureID> TileToTexture = Enum.GetValues<DecalType>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureManager.TextureID>(tileType.ToString(), out var texture) ? texture : TextureManager.TextureID.Null
    );
    // Auto generated - no setter
    public TextureManager.TextureID Texture { get; }
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
    public virtual void Draw(GameManager gameManager)
    {
        // Draw
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Point size = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;
        Rectangle source = TextureManager.GetAnimationSource(Texture, gameManager.TotalTime, duration: .75f);
        TextureManager.DrawTexture(gameManager.Batch, Texture, dest, source: source, scale: 4, color: Tint);
    }
}
