namespace Quest.Enemies;
public class Thief : Enemy
{
    public Thief(Point position) : base(position)
    {
        // Stats
        Health = 80;
        Damage = 15;
        AttackSpeed = .5f;
        Defense = 10;
        Speed = 100;
        ViewRange = 700;
        AttackRange = 50;
    }
    public override void Attack()
    {
        // TODO
    }
}
