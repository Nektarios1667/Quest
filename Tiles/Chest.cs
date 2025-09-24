namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public ILootGenerator LootGenerator { get; private set; }
    public Inventory Inventory { get; private set; }
    public bool Generated { get; private set; } = false;
    private int seed { get; } = Random.Shared.Next();
    public Chest(Point location, ILootGenerator lootGenerator) : base(location)
    {
        IsWalkable = false;
        LootGenerator = lootGenerator;
        Inventory = new(6, 3);
        StateManager.SaveChestGenerator(this);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        TryGenerateLoot();
        player.OpenContainer(this);
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGenerator = lootGenerator;
        Inventory = lootGenerator.Generate(6, 3, seed);
        Generated = true;
    }
    public void TryGenerateLoot()
    {
        if (Generated) return;
        Inventory = LootGenerator.Generate(6, 3, seed);
        Generated = true;
    }
}
