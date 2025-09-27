namespace Quest.Items;
public class WoodPlanks : Item
{
    public WoodPlanks(int amount) : base(amount)
    {
        MaxAmount = 10;
        Description = "Sturdy wooden boards cut from trees.";
    }
}
