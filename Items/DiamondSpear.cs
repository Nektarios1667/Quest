namespace Quest.Items;

public class DiamondSpear : MeleeWeapon
{
    public DiamondSpear(byte amount, string? customName = null) : base(ItemTypes.DiamondSpear, amount, 1.3f, 2f, 50, customName)
    { }
}
