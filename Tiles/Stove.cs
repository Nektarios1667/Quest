using Quest.Interaction;
using System.Windows.Forms;

namespace Quest.Tiles;

public class Stove : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public Stove(Point location, string levelName) : base(location, TileTypeID.Stove)
    {
        Container = new([null, null, null]);
        StateManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.StoveUI.BindContainer(Container);
        player.OpenInterface(UserInterface.StoveUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
