using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Items;

public class Consumable : Item
{
    public int HungerRestored { get; }
    public (StatusEffect Effect, float Chance, float Duration)? StatusGiven { get; } // Chance is 0-1, Duration is in seconds
    public ItemRef? Leftover { get; }
    public Consumable(ItemType itemType, byte amount, int hungerRestored, ItemRef? leftover = null, (StatusEffect Effect, float Chance, float Duration)? statusGiven = null, string? customName = null) : base(itemType, amount, customName)
    {
        HungerRestored = hungerRestored;
        StatusGiven = statusGiven;
        Leftover = leftover;
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        // Status
        if (StatusGiven != null && RandomManager.RandomFloat() < StatusGiven.Value.Chance)
            player.StatusManager.AddStatusEffect(player, StatusGiven.Value.Effect, StatusGiven.Value.Duration);

        // Consume
        player.Inventory.Consume(GetItemRef());

        // Leftover
        if (Leftover != null)
            player.Inventory.AddItem(new(Leftover, CustomName));

        // Sound
        SoundManager.PlaySound("Gulp", pitchVariation: 0.25f);

        return true;
    }
}
