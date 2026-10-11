namespace Quest.Items;

public class Lantern : LightItem
{
    public Lantern(int amount, string? name = null) : base(ItemTypes.Lantern, amount, name)
    {
        LightStrength = 5;
    }
}
