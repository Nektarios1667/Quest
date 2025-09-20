namespace Quest.Items;
public class Skull : Item
{
    public Skull(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "Why are you holding this?";
    }
}
