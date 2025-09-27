namespace Quest.Items;
public class BottledWater : Item
{
    public BottledWater(int amount) : base(amount)
    {
        MaxAmount = 3;
        Description = "A glass bottle of potable water.";
    }
}
