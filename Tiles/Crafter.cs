using Quest.Interaction;
using Quest.World;

namespace Quest.Tiles;

public class Crafter : Tile, IContainer
{
    public readonly static Point IngredientsSize = new(4, 1);
    public Container Container { get; private set; } = null!;

    public Crafter(Point location, LevelPath level) : base(location, TileTypeID.Crafter)
    {
        // w x h + 1 slots
        Container = new(new Item?[IngredientsSize.X * IngredientsSize.Y + 1]);
        SaveManager.SaveContainer(this, level);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.CrafterUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.CrafterUI);
        gameManager.StateManager.OverlayState = OverlayState.Interface;
    }
}
