namespace Quest.Items;

public class NinjaStar : RangedWeapon
{
    public NinjaStar(byte amount, string? customName = null) : base(ItemTypes.NinjaStar, amount, 0.7f, 5f, 20, TextureID.NinjaStarProjectile, customName) // TOOD FIX Ninja star projectile texture
    {
        Ammo = new ItemRef(ItemTypes.NinjaStar, 1);
    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        bool success = base.PrimaryUse(gameManager, player);
        if (success)
            SoundManager.PlaySound("Swoosh", pitch: 1.1f, pitchVariation: 0.1f);
        return success;
    }
}
