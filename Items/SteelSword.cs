namespace Quest.Items;

public class SteelSword : MeleeWeapon
{
    public SteelSword(byte amount, string? customName = null) : base(ItemTypes.SteelSword, amount, 1.2f, 1f, 40, customName)
    {
    }
}
