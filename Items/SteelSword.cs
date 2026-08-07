namespace Quest.Items;

public class IronSword : MeleeWeapon
{
    public IronSword(byte amount, string? customName = null) : base(ItemTypes.IronSword, amount, 1.2f, 1f, 40, customName)
    {
    }
}
