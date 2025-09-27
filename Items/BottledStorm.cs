namespace Quest.Items;
public class BottledStorm : Item
{
    public BottledStorm(int amount) : base(amount)
    {
        MaxAmount = 3;
        Description = "A storm somehow trapped in a glass bottle...";
    }
}
