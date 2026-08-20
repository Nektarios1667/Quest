namespace Quest.Items;

public class Consumable : Item
{
    public int HungerRestored { get; }
    public (StatusEffect Effect, float Chance, float Duration)? StatusGiven { get; } // Chance is 0-1, Duration is in seconds
    public ItemRef? Leftover { get; }
    public Consumable(ItemType itemType, int amount, int hungerRestored, ItemRef? leftover = null, (StatusEffect Effect, float Chance, float Duration)? statusGiven = null, string? customName = null) : base(itemType, amount, customName)
    {
        HungerRestored = hungerRestored;
        StatusGiven = statusGiven;
        Leftover = leftover;
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        // Timer
        if (!TimerManager.IsCompleteOrMissing("PlayerConsume")) return false;
        TimerManager.SetTimer("PlayerConsume", 1f, null);

        // Check hunger
        if (player.Hunger >= player.MaxHunger && HungerRestored > 0)
        {
            gameManager.OverlayManager.Notification("You are not hungry.", Color.Yellow, 2f);
            return false;
        }

        // Status
        if (StatusGiven != null && RandomManager.RandomFloat() < StatusGiven.Value.Chance)
            StatusManager.AddStatusEffect(player, StatusGiven.Value.Effect, StatusGiven.Value.Duration);

        // Consume
        player.Inventory.Consume(Take(1)!.GetItemRef());

        // Leftover
        if (Leftover != null)
            player.Inventory.AddItem(new(Leftover, CustomName));

        // Hunger system or direct healing
        player.Eat(gameManager, HungerRestored);

        // Sound
        SoundManager.PlaySound("Gulp", pitchVariation: 0.25f);

        return true;
    }
}
