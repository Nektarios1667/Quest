namespace Quest.Items;
public class Crossbow : RangedWeapon
{
    public Crossbow(byte amount, string? customName = null) : base(ItemTypes.Crossbow, amount, 1.8f, 800, 25, TextureID.Arrow, customName)
    {}
}
