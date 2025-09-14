using Quest.Managers;

namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public Inventory Inventory { get; set; }
    public string ContainerName { get; }
    public Chest(Point location, Inventory? inventory = null, string name = "Chest") : base(location)
    {
        IsWalkable = false;
        ContainerName = name;
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
