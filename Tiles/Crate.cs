
using Quest.Interaction;

namespace Quest.Tiles;

public class Crate : Tile, IContainer
{
    public readonly static Point Size = new(4, 2);
    public Container Container { get; set; }
    public Crate(Point location, string levelName) : base(location, TileTypeID.Crate)
    {
        Container = new(new Item?[Size.X * Size.Y]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager gameManager,PlayerManager player)
    {
        UserInterface.CrateUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.CrateUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
}
