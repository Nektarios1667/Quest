namespace Quest.Items;
public class GlassBottle : Item
{
    public GlassBottle(int amount) : base(amount)
    {
        MaxAmount = 3;
        Description = "An empty bottle made of glass.";
    }
}
