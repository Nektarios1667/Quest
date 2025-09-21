namespace Quest.Items;
public class Cherries : Item
{
    public Cherries(int amount) : base(amount)
    {
        MaxAmount = 5;
        Description = "Juicy red cherries.";
    }
}
