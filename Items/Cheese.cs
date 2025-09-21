namespace Quest.Items;
public class Cheese : Item
{
    public Cheese(int amount) : base(amount)
    {
        MaxAmount = 5;
        Description = "A wedge of Swiss cheese.";
    }
}
