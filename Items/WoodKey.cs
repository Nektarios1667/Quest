namespace Quest.Items;
public class WoodKey : Item
{
    public WoodKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A simple wooden key.";
    }
}
