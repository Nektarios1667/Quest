using Quest.Interaction;

namespace Quest.Tiles;

public class Crafter : Tile, IContainer
{
    public readonly static Point IngredientsSize = new(4, 1);
    public Container Container { get; private set; } = null!;

    public Crafter(Point location, string levelName) : base(location, TileTypeID.Crafter)
    {
        // w x h + 1 slots
        Container = new(new Item?[IngredientsSize.X * IngredientsSize.Y + 1]);
        StateManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        UserInterface.CrafterUI.BindContainer(Container);
        player.OpenInterface(UserInterface.CrafterUI);
        StateManager.OverlayState = OverlayState.Container;
    }
}
