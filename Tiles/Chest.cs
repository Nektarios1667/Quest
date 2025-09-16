using Quest.Managers;

namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public Inventory Inventory { get; set; }
    public Chest(Point location, Inventory? inventory = null) : base(location)
    {
        IsWalkable = false;
        Inventory = new(6, 3);
    }
    public override void Draw(GameManager gameManager)
    {
        base.Draw(gameManager);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        player.OpenContainer(this);
    }
}
