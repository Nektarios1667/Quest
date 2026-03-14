using Quest.Interaction;

namespace Quest.Tiles;

public class Inscriber : Tile
{
    public Interaction.Container Container { get; private set; }
    public Inscriber(Point location) : base(location, TileTypeID.Inscriber)
    {
        Container = new([null]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.InscriberUI.BindContainer(Container);
        player.OpenInterface(UserInterface.InscriberUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
