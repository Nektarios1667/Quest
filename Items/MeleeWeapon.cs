namespace Quest.Items;
public class MeleeWeapon : Item
{
    public float FireRate { get; } // Seconds between swings
    public float Range { get; } // Pixels
    public int Damage { get; } // Damage dealt per hit
    public MeleeWeapon(ItemType itemType, byte amount, float firerate, float range, int damage, string? customName = null) : base(itemType, amount, customName)
    {
        Amount = amount;
        FireRate = firerate;
        Range = range;
        Damage = damage;
    }
    public override void PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        if (TimerManager.IsCompleteOrMissing($"MeleeSwing_{UID}"))
        {
            TimerManager.SetTimer($"MeleeSwing_{UID}", FireRate, null);
            RectangleF hitbox = new(CameraManager.Camera.X, CameraManager.Camera.Y, Range, Constants.MageHalfSize.Y);
            player.AddAttack(new(Damage, hitbox));
        }
    }
}

