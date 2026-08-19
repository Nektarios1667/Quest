using Quest.Interaction;

namespace Quest.Tiles;

public class Furnace : Tile, IContainer
{
    public const float SmeltingTime = 4; // Seconds
    public Interaction.Container Container { get; private set; }
    public Furnace(Point location, string levelName) : base(location, TileTypeID.Furnace)
    {
        Container = new([null, null, null]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.FurnaceUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.FurnaceUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
}
