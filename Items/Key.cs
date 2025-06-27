namespace Quest.Items;
public class Key : Item
{
    public Key(PlayerManager playerManager, int amount) : base(playerManager, amount)
    {
        MaxAmount = 1;
        Name = "Key";
        Description = "A simple door key.";
    }
}
