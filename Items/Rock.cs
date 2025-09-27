namespace Quest.Items;
public class Rock : Item
{
    public Rock(int amount) : base(amount)
    {
        MaxAmount = 10;
        Description = "Hard rock mined from the ground.";
    }
}
