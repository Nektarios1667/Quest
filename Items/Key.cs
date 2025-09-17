namespace Quest.Items;
public class Key : Item
{
    public Key(int amount) : base(amount)
    {
        MaxAmount = 1;
        Name = "Key";
        Description = "A simple door key.";
    }
}
