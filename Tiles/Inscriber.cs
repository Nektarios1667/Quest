using Quest.Interaction;
using Quest.World;

namespace Quest.Tiles;

public class Inscriber : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public Inscriber(Point location, LevelPath level) : base(location, TileTypeID.Inscriber)
    {
        Container = new([null]);
        SaveManager.SaveContainer(this, level);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.InscriberUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.InscriberUI);
        gameManager.StateManager.OverlayState = OverlayState.Interface;
    }
}
