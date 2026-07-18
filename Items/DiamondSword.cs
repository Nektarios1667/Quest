namespace Quest.Items;

public class DiamondSword : MeleeWeapon
{
    public DiamondSword(byte amount, string? customName = null) : base(ItemTypes.DiamondSword, amount, 1.2f, 60, 40, customName)
    {
    }
}
