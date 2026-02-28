using Quest.Interaction;
using System.Windows.Forms;

namespace Quest.Tiles;

public class Chest : Tile
{
    public readonly static Point Size = new(6, 3);
    public ILootGenerator LootGenerator { get; private set; }
    public Item?[]? Items { get; private set; }
    public bool Generated { get; private set; } = false;
    public int Seed { get; private set; } = Random.Shared.Next();
    public Chest(Point location, ILootGenerator lootGenerator, string levelPath) : base(location, TileTypeID.Chest)
    {
        LootGenerator = lootGenerator;
        StateManager.SaveChestGenerator(this, levelPath);
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        TryGenerateLoot();
        player.OpenInterface(UserInterface.ChestUI);
        UserInterface.ChestUI.BindSlots(Items!);
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGenerator = lootGenerator;
        Items = lootGenerator.Generate(Size.X * Size.Y, Seed);
        Generated = true;
    }
    public void TryGenerateLoot()
    {
        if (Generated) return;
        Items = LootGenerator.Generate(Size.X * Size.Y, Seed);
        Generated = true;
    }
    public void SetEmpty()
    {
        Generated = true;
        Items = new Item?[Size.X * Size.Y];
    }
    public void SetSeed(int seed) => Seed = seed;
}
