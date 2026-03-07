namespace Quest.Items;

public class Lantern : Light
{
    public Lantern(int amount, string? name = null) : base(ItemTypes.Lantern, amount, name)
    {
        LightStrength = 5;
    }
}
