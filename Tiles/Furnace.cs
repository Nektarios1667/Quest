using Quest.Interaction;

namespace Quest.Tiles;

public class Furnace : Tile
{
    public Interaction.Container Container { get; private set; }
    public Furnace(Point location) : base(location, TileTypeID.Furnace)
    {
        Container = new([null, null, null]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.FurnaceUI.BindContainer(Container);
        player.OpenInterface(UserInterface.FurnaceUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
