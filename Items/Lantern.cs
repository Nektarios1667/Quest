namespace Quest.Items
{
    public class Lantern : Light
    {
        public Lantern(int amount) : base(amount)
        {
            Description = "A burning lantern used for light.";
            LightStrength = 300;
            LightColor = new(40, 40, 0, 255);
        }
    }
}