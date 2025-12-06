namespace Quest.Items;
public class DiamondSword : MeleeWeapon
{
    public DiamondSword(int amount) : base(ItemTypes.DiamondSword, amount, .8f, 60, 40)
    {
    }
    public override void PrimaryUse(PlayerManager player)
    {
        base.PrimaryUse(player);
        SoundManager.PlaySound("Swoosh", pitch: RandomManager.RandomFloat() / 2 - .25f);
    }
}
