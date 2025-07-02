namespace Quest.Items;
public class MeleeWeapon : Item
{
    public float FireRate { get; } // Attacks per second
    public float Range { get; } // Pixels
    public int Damage { get; } // Damage dealt per hit
    public MeleeWeapon(PlayerManager playerManager, int amount, float firerate, float range, int damage) : base(playerManager, amount)
    {
        Amount = 1;
        MaxAmount = 1;
        FireRate = firerate;
        Range = range;
        Damage = damage;
    }
    public override void PrimaryUse()
    {
        if (TimerManager.IsCompleteOrMissing($"MeleeSwing_{UID}"))
        {
            TimerManager.SetTimer($"MeleeSwing_{UID}", 1f / FireRate, null);
            RectangleF hitbox = new(CameraManager.Camera.X, CameraManager.Camera.Y, Range, Constants.MageHalfSize.Y);
            PlayerManager.AddAttack(new(Damage, hitbox));
        }
    }
}

