namespace Quest.Items;
public class MeleeWeapon : Item
{
    public float FireRate { get; } // Attacks per second
    public float Range { get; } // Range in tiles
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
        if (TimerManager.TryTimeLeft($"MeleeSwing_{UID}") <= 0)
        {
            TimerManager.SetTimer($"MeleeSwing_{UID}", 1f / FireRate, null);
            RectangleF hitbox = new(CameraManager.Camera.X, CameraManager.Camera.Y, Range, Constants.MageSize.Y);
            PlayerManager.AddAttack(new(Damage, hitbox));
        }
    }
}

