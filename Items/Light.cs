namespace Quest.Items
{
    public class Light : Item
    {
        public int LightStrength { get; protected set; }
        public Color LightColor { get; protected set; } = Color.Transparent;
        public Light(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A light source.";
            LightStrength = 200;
        }
    }
}