namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public string LootGeneratorFileName { get; private set; }
    public Inventory Inventory { get; private set; }
    public Chest(Point location, ILootGenerator lootGenerator) : base(location)
    {
        IsWalkable = false;
        Inventory = lootGenerator.Generate(6, 3);
        LootGeneratorFileName = lootGenerator.FileName;
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        player.OpenContainer(this);
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGeneratorFileName = lootGenerator.FileName;
        Inventory = lootGenerator.Generate(6, 3);
    }
}
