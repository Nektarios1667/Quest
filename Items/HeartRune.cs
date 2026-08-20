namespace Quest.Items;

public class HeartRune : Item
{
    public HeartRune(int amount, string? customName) : base(ItemTypes.HeartRune, amount, customName)
    { }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing("HeartRuneCooldown"))
        {
            StatusManager.AddStatusEffect(player, StatusEffect.Regeneration, 6);
            SoundManager.PlaySoundInstance("Spook");
            TimerManager.SetTimer("HeartRuneCooldown", 60, null);
            return true;
        }
        return false;
    }
}
