using System.Linq;

namespace Quest.Decals;
public enum DecalType : byte
{
    Footprint,
    Torch,
    BlueTorch,
    WaterPuddle,
    BloodPuddle,
    Pebbles,
    Bush1,
    Bush2,
    Bush3,
    SnowyBush1,
    SnowyBush2,
    SnowyBush3,
    Cracks1,
    Cracks2,
    Cracks3,
    Mushrooms1,
    Mushrooms2,
    Splat1,
    Splotch1,
    Splotch2,
    Splotch3,
    Splotch4,
    LightTint,
    MediumTint,
    DarkTint,
    // DECALS
}
public class Decal
{
    protected static readonly Dictionary<DecalType, TextureID> TileToTexture = Enum.GetValues<DecalType>()
    .ToDictionary(
        tileType => tileType,
        tileType => Enum.TryParse<TextureID>(tileType.ToString(), out var texture) ? texture : TextureID.Null
    );
    // Auto generated - no setter
    public ByteCoord Location { get; }
    public TextureID Texture { get; }
    public DecalType Type { get; protected set; }
    // Generated properties
    public byte X => Location.X;
    public byte Y => Location.Y;
    public ushort UID => (ushort)(Y * Constants.MapSize.X + X);
    public Decal(Point location, DecalType? type = null)
    {
        // Initialize the tile
        Location = new(location);
        Type = type ?? (DecalType)Enum.Parse(typeof(DecalType), GetType().Name);
        Texture = TileToTexture.TryGetValue(Type, out var tex) ? tex : TextureID.Null;
    }
    public virtual void Draw(GameManager gameManager)
    {
        // Draw
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Rectangle source = GetAnimationSource(Texture, gameManager.GameTime, duration: .75f);
        DrawTexture(gameManager.Batch, Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
}
