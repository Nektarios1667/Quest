using Quest.Interaction;
using System.ComponentModel;

namespace Quest.Tiles;

public class DiscWriter : Tile
{
    public Interaction.Container Container { get; private set; }
    public DiscWriter(Point location) : base(location, TileTypeID.DiscWriter)
    {
        Container = new([null]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.DiscWriterUI.BindContainer(Container);
        player.OpenInterface(UserInterface.DiscWriterUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
