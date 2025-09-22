namespace Quest.Items;

public class Lantern : Light
{
    public Lantern(int amount) : base(amount)
    {
        MaxAmount = 1;
        Description = "A burning lantern used for light.";
        LightStrength = 400;
        LightColor = new(10, 10, 0, 255);
    }
}