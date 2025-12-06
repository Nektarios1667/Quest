namespace Quest.Items;
public class SteelSword : MeleeWeapon
{
    public SteelSword(int amount) : base(ItemTypes.SteelSword, amount, 1f, 45, 20)
    {
    }
    public override void PrimaryUse(PlayerManager player)
    {
        base.PrimaryUse(player);
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
