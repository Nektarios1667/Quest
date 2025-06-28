namespace Quest.Items;
public class SteelSword : MeleeWeapon
{
    public SteelSword(PlayerManager playerManager, int amount): base(playerManager, amount, 1f, .6f, 20)
    {
        Description = "A sturdy bronze sword.";
        MaxAmount = 1;
    }
    public override void PrimaryUse()
    {
        base.PrimaryUse();
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
