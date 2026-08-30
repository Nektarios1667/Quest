using Quest.Interaction;
using Quest.World;

namespace Quest.Tiles;

public class Stove : Tile, IContainer
{
    public const float CookingTime = 2f; // Seconds
    public Interaction.Container Container { get; private set; }
    public Stove(Point location, LevelPath level) : base(location, TileTypeID.Stove)
    {
        Container = new([null, null, null]);
        SaveManager.SaveContainer(this, level);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.StoveUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.StoveUI);
        gameManager.StateManager.OverlayState = OverlayState.Interface;
    }
}
