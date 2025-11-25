namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public readonly static Point ChestSize = new(6, 3);
    public ILootGenerator LootGenerator { get; private set; }
    public Inventory? Inventory { get; private set; } = null;
    public bool Generated { get; private set; } = false;
    public int Seed { get; private set; } = Random.Shared.Next();
    public Chest(Point location, ILootGenerator lootGenerator, string levelPath) : base(location, TileTypes.Chest)
    {
        LootGenerator = lootGenerator;
        StateManager.SaveChestGenerator(this, levelPath);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        TryGenerateLoot();
        player.OpenContainer(this);
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGenerator = lootGenerator;
        Inventory = lootGenerator.Generate(6, 3, Seed);
        Generated = true;
    }
    public void TryGenerateLoot()
    {
        if (Generated) return;
        Inventory = LootGenerator.Generate(6, 3, Seed);
        Generated = true;
    }
    public void SetEmpty()
    {
        Generated = true;
        Inventory = new(6, 3);
    }
    public void SetSeed(int seed) => Seed = seed;
}
