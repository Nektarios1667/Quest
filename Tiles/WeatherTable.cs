using Quest.Interaction;

namespace Quest.Tiles;

public class WeatherTable : Tile
{
    public WeatherTable(Point location) : base(location, TileTypeID.WeatherTable) { }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        player.OpenInterface(gameManager, UserInterface.WeatherTableUI);
    }
}
