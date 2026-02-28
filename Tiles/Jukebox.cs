using MonoGUI;
namespace Quest.Tiles;

public class Jukebox : Tile
{
    public static GUI MusicSelectGUI = null!;
    public bool IsPlaying { get; set; } = false;
    public Jukebox(Point location) : base(location, TileTypeID.Jukebox) { }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        StateManager.OverlayState = OverlayState.Jukebox;
    }
    public override void Draw(GameManager gameManager)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Rectangle source = GetAnimationSource(TextureID.Jukebox, gameManager.GameTime, 0.5f, row: SoundtrackManager.Playing == null ? 0 : 1);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
}
