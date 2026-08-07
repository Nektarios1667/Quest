namespace Quest.Items;
public class NinjaStar : RangedWeapon
{
    public NinjaStar(byte amount, string? customName = null) : base(ItemTypes.NinjaStar, amount, 0.7f, 5f, 20, TextureID.Arrow, customName) // TOOD FIX Ninja star projectile texture
    {
        Ammo = new ItemRef(ItemTypes.NinjaStar, 1);
    }
}
