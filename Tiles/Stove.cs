using Quest.Interaction;

namespace Quest.Tiles;

public class Stove : Tile
{
    public Interaction.Container Container { get; private set; }
    public Stove(Point location) : base(location, TileTypeID.Stove)
    {
        Container = new([null, null, null]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.StoveUI.BindContainer(Container);
        player.OpenInterface(UserInterface.StoveUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
