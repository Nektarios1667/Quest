namespace Quest.Items
{
    public class PhiCoin : Item
    {
        public PhiCoin(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A gold coin.";
        }
    }
}