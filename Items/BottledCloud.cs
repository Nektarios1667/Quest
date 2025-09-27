namespace Quest.Items;
public class BottledCloud : Item
{
    public BottledCloud(int amount) : base(amount)
    {
        MaxAmount = 3;
        Description = "A cloud somehow trapped in a glass bottle...";
    }
}
