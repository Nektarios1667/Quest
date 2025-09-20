namespace Quest.Items;
public class GoldKey : Item
{
    public GoldKey(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A fancy golden key.";
    }
}
