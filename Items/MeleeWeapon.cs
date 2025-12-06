namespace Quest.Items;
public class MeleeWeapon : Item
{
    public float FireRate { get; } // Attacks per second
    public float Range { get; } // Pixels
    public int Damage { get; } // Damage dealt per hit
    public MeleeWeapon(ItemType itemType, int amount, float firerate, float range, int damage) : base(itemType, amount)
    {
        Amount = 1;
        FireRate = firerate;
        Range = range;
        Damage = damage;
    }
    public override void PrimaryUse(PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"MeleeSwing_{UID}"))
        {
            TimerManager.SetTimer($"MeleeSwing_{UID}", 1f / FireRate, null);
            RectangleF hitbox = new(CameraManager.Camera.X, CameraManager.Camera.Y, Range, Constants.MageHalfSize.Y);
            player.AddAttack(new(Damage, hitbox));
        }
    }
}

