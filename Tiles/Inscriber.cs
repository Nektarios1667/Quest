using Quest.Interaction;

namespace Quest.Tiles;

public class Inscriber : Tile, IContainer
{
    public Interaction.Container Container { get; private set; }
    public Inscriber(Point location, string levelName) : base(location, TileTypeID.Inscriber)
    {
        Container = new([null]);
        SaveManager.SaveContainer(this, levelName);
    }
    public override void OnPlayerCollide(GameManager gameManager,PlayerManager player)
    {
        UserInterface.InscriberUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.InscriberUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
}
