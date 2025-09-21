namespace Quest.Items;
public class Chicken : Item
{
    public Chicken(int amount) : base(amount)
    {
        MaxAmount = 5;
        Description = "Chicken meat.";
    }
}
