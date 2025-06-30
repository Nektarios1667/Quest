namespace Quest.Items
{
    public class GammaCoin : Item
    {
        public GammaCoin(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A diamond coin.";
        }
    }
}