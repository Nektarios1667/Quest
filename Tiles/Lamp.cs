
namespace Quest.Tiles;

public class Lamp : Tile, IDynamicTile
{
    public byte LightRadius { get; set; }
    public Lamp(Point location, byte lightRadius = 10) : base(location, TileTypeID.Lamp)
    {
        LightRadius = lightRadius;
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);

        Color tintColor = Color.Lerp(Color.Transparent, Color.Yellow, Math.Clamp(LightRadius / 15f, 0.3f, 0.75f));
        gameManager.Batch.FillRectangle(new(CameraManager.TileToScreen(Location).ToVector2(), Constants.TileSize), tintColor);

        LightingManager.SetLight($"LampTile_{X}_{Y}", Location, LightRadius, singleFrame: true);
    }
}
