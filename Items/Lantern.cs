namespace Quest.Items
{
    public class Lantern : Light
    {
        public Lantern(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A burning lantern used for light.";
            LightStrength = 300;
            LightColor = Color.Orange * .1f;
        }
    }
}