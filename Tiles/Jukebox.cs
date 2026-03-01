using MonoGUI;
using Quest.Interaction;
using System.ComponentModel;
namespace Quest.Tiles;

public class Jukebox : Tile
{
    public Interaction.Container Container { get; private set; }
    public bool IsPlaying { get; set; } = false;
    public Jukebox(Point location) : base(location, TileTypeID.Jukebox)
    {
        Container = new(new Item[5]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.JukeboxUI.BindContainer(Container);
        player.OpenInterface(UserInterface.JukeboxUI);
        StateManager.OverlayState = OverlayState.Container;
    }
    public override void Draw(GameManager gameManager)
    {
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Rectangle source = GetAnimationSource(TextureID.Jukebox, gameManager.GameTime, 0.5f, row: SoundtrackManager.Playing == null ? 0 : 1);
        DrawTexture(gameManager.Batch, Type.Texture, dest, source: source, scale: Constants.TileSizeScale);
    }
}
