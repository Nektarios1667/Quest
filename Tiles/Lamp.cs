
namespace Quest.Tiles;

public class Lamp : Tile
{
    public byte LightRadius { get; set; }
    public Lamp(Point location, byte lightRadius = 10) : base(location, TileTypeID.Lamp)
    {
        LightRadius = lightRadius;
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);

        Color tintColor = Color.Lerp(Color.White, Color.Yellow, LightRadius / 20f);
        gameManager.Batch.FillRectangle(new((Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle).ToVector2(), Constants.TileSize), tintColor);

        LightingManager.SetLight($"LampTile_{X}_{Y}", Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, LightRadius, Color.Transparent, singleFrame: true);
    }
}
