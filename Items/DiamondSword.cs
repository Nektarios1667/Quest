namespace Quest.Items;
public class DiamondSword : MeleeWeapon
{
    public DiamondSword(int amount): base(amount, .8f, 60, 40)
    {
        Description = "A razor sharp sword made with pure diamonds.";
        MaxAmount = 1;
    }
    public override void PrimaryUse(PlayerManager player)
    {
        base.PrimaryUse(player);
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
