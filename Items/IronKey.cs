namespace Quest.Items;
public class IronKey : Item
{
    public IronKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A simple iron key.";
    }
}
