
namespace Quest.Tiles;

public class Lamp : Tile
{
    private Color _lightColor { get; set; }
    public Color LightColor
    {
        get => _lightColor;
        set { _lightColor = value; RecalculateTint(); }
    }
    public int LightRadius { get; set; }
    private Color tintColor;
    public Lamp(Point location, Color? lightColor = null, int lightRadius = 10) : base(location)
    {
        IsWalkable = true;
        LightColor = lightColor ?? new(10, 10, 0, 255);
        LightRadius = lightRadius;

        RecalculateTint();
    }
    private void RecalculateTint()
    {
        float scale = 256.0f / ColorTools.GetMaxComponent(LightColor);
        tintColor.A = (byte)(LightColor.A / 2f);
        tintColor = LightColor * scale * (tintColor.A / 255f);
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);

        gameManager.Batch.FillRectangle(new((Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle).ToVector2(), Constants.TileSize), tintColor);

        LightingManager.SetLight($"LampTile_{Location.X}_{Location.Y}", Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, LightRadius, LightColor);
    }
}
