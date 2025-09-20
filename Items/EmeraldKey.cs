namespace Quest.Items;
public class EmeraldKey : Item
{
    public EmeraldKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A fancy emerald key.";
    }
}
