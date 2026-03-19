using Quest.Interaction;
using System.ComponentModel;

namespace Quest.Tiles;

public class DiscWriter : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public DiscWriter(Point location, string levelName) : base(location, TileTypeID.DiscWriter)
    {
        Container = new([null]);
        StateManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.DiscWriterUI.BindContainer(Container);
        player.OpenInterface(UserInterface.DiscWriterUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
