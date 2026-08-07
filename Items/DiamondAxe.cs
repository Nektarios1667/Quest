namespace Quest.Items;
public class DiamondAxe : MeleeWeapon
{
    public DiamondAxe(byte amount, string? customName = null) : base(ItemTypes.DiamondAxe, amount, 1.4f, 1f, 60, customName)
    {}
}
