namespace Quest.Items;
public class IronAxe : MeleeWeapon
{
    public IronAxe(byte amount, string? customName = null) : base(ItemTypes.IronAxe, amount, 1.4f, 1f, 45, customName)
    {}
}
