namespace Quest.Items;
public class Potato : Item
{
    public Potato(int amount) : base(amount)
    {
        MaxAmount = 5;
        Description = "An earthy potato.";
    }
}
