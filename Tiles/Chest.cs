using Quest.Interaction;

namespace Quest.Tiles;

public class Chest : Tile, IContainer
{
    public readonly static Point Size = new(6, 3);
    public ILootGenerator LootGenerator { get; private set; }
    public Container Container { get; private set; } = null!;
    public bool Generated { get; private set; } = false;
    public int Seed { get; private set; } = Random.Shared.Next();
    public ItemRef? Key { get; set; }
    public bool ConsumeKey { get; set; }
    public Chest(Point location, ILootGenerator lootGenerator, string levelPath, ItemRef? key = null, bool consumeKey = true) : base(location, TileTypeID.Chest)
    {
        LootGenerator = lootGenerator;
        SaveManager.SaveChestGenerator(this, levelPath);
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void OnPlayerCollide(GameManager gameManager,PlayerManager player)
    {
        // Unlocked
        if (Generated)
        {
            Open(gameManager, player);
            return;
        }

        // Locked
        // Has key
        if (Key == null || player.Inventory.Count(Key.Type) >= Key.Amount)
        {
            if (Key != null && ConsumeKey)
            {
                gameManager.OverlayManager.Notification($"-{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Red, 3);
                player.Inventory.Consume(Key, ignoreCheck: true);
            }
            else if (Key != null)
                gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Gray, 2);

            SoundManager.PlaySoundInstance("ChestUnlock");
            Open(gameManager, player);
        }
        // No key
        else
        {
            // Notif
            gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)} needed to unlock", Color.Red, 5);

            // Sound fx
            string timerName = $"ChestLocked_{X + Y * Constants.MapSize.X}";
            if (TimerManager.IsCompleteOrMissing(timerName))
            {
                SoundManager.PlaySoundInstance("ChestLocked");
                TimerManager.SetTimer(timerName, 5, null);
            }
        }

    }
    private void Open(GameManager gameManager, PlayerManager player)
    {
        TryGenerateLoot();
        UserInterface.ChestUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.ChestUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGenerator = lootGenerator;
        Container = new(lootGenerator.Generate(Size.X * Size.Y, Seed));
        Generated = true;
    }
    public void TryGenerateLoot()
    {
        if (Generated) return;
        Container = new(LootGenerator.Generate(Size.X * Size.Y, Seed));
        Generated = true;
    }
    public void SetEmpty()
    {
        Generated = true;
        Container = new(new Item?[Chest.Size.X * Chest.Size.Y]);
    }
    public void SetSeed(int seed) => Seed = seed;
}
