using Quest.Interaction;

namespace Quest.Tiles;

public class DiscWriter : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public DiscWriter(Point location, string levelName) : base(location, TileTypeID.DiscWriter)
    {
        Container = new([null]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        UserInterface.DiscWriterUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.DiscWriterUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
}
