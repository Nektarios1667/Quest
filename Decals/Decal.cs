using System.Linq;

namespace Quest.Decals;
public enum DecalType
{
    Footprint,
    Torch,
    BlueTorch,
    WaterPuddle,
    BloodPuddle,
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
    public int UID { get; } = UIDManager.NewUID("Decals");
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
        Rectangle source = GetAnimationSource(Texture, gameManager.GameTime, duration: .75f);
        DrawTexture(gameManager.Batch, Texture, dest, source: source, scale: 4, color: Tint);
    }
}
