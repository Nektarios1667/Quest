namespace Quest.Items;
public class SteelSword : MeleeWeapon
{
    public SteelSword(int amount): base(amount, 1f, 45, 20)
    {
        Description = "A sturdy bronze sword.";
        MaxAmount = 1;
    }
    public override void PrimaryUse(PlayerManager player)
    {
        base.PrimaryUse(player);
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
