namespace Quest.Items;
public class DiamondKey : Item
{
    public DiamondKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A fancy diamond key.";
    }
}
