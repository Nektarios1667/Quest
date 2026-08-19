using Quest.Interaction;

namespace Quest.Tiles;

public class Stove : Tile, IContainer
{
    public const float CookingTime = 2f; // Seconds
    public Interaction.Container Container { get; private set; }
    public Stove(Point location, string levelName) : base(location, TileTypeID.Stove)
    {
        Container = new([null, null, null]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.StoveUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.StoveUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
}
