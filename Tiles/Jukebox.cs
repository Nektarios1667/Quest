using Quest.Interaction;
using Quest.World;

namespace Quest.Tiles;

public class Jukebox : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public bool IsPlaying { get; set; } = false;
    public Jukebox(Point location, LevelPath level) : base(location, TileTypeID.Jukebox)
    {
        Container = new([null]);
        SaveManager.SaveContainer(this, level);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.JukeboxUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.JukeboxUI);
        gameManager.StateManager.OverlayState = OverlayState.Interface;
    }
    public override void Draw(GameManager gameManager)
    {
        Point dest = CameraManager.TileToScreen(Location);
        Rectangle source = GetAnimationSource(TextureID.Jukebox, GameManager.GameTime, 0.5f, row: SoundtrackManager.Playing == null ? 0 : 1);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
}
