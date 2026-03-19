using Quest.Interaction;

namespace Quest.Tiles;

public class Inscriber : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public Inscriber(Point location, string levelName) : base(location, TileTypeID.Inscriber)
    {
        Container = new([null]);
        StateManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.InscriberUI.BindContainer(Container);
        player.OpenInterface(UserInterface.InscriberUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
