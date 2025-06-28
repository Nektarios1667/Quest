namespace Quest.Items;
public class DiamondSword : MeleeWeapon
{
    public DiamondSword(PlayerManager playerManager, int amount): base(playerManager, amount, .8f, 60, 40)
    {
        Description = "A razor sharp sword made with pure diamonds.";
        MaxAmount = 1;
    }
    public override void PrimaryUse()
    {
        base.PrimaryUse();
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
