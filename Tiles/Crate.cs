
using Quest.Interaction;
using SharpDX.XInput;

namespace Quest.Tiles;

public class Crate : Tile
{
    public readonly static Point Size = new(4, 2);
    public Interaction.Container Container { get; set; }
    public Crate(Point location) : base(location, TileTypeID.Crate)
    {
        Container = new(new Item?[Size.X * Size.Y]);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.CrateUI.BindContainer(Container);
        player.OpenInterface(UserInterface.CrateUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
