namespace Quest.Items;

public class Lantern : Light
{
    public Lantern(int amount) : base(ItemTypes.Lantern, amount)
    {
        LightStrength = 5;
        LightColor = new(10, 10, 0, 255);
    }
}
