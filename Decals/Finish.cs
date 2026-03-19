
namespace Quest.Decals;
public class Finish(Point location) : Decal(location)
{
    public override void Draw(GameManager gameManager)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime, duration: .75f);
        DrawTexture(gameManager.Batch, Texture, dest, source: source, scale: Constants.TileSizeScale, color: Color.White * (float)((Math.Cos(GameManager.GameTime * MathHelper.Pi) + 1.01f) / 2f));
    }
    public override void OnPlayerEnter(GameManager gameManager, PlayerManager playerManager)
    {
        StateManager.OverlayState = OverlayState.Finished;
    }
}
