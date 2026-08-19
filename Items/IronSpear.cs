namespace Quest.Items;

public class IronSpear : MeleeWeapon
{
    public IronSpear(byte amount, string? customName = null) : base(ItemTypes.IronSpear, amount, 1.3f, 2f, 35, customName)
    { }
}
