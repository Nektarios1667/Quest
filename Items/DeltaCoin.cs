namespace Quest.Items
{
    public class DeltaCoin : Item
    {
        public DeltaCoin(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A copper coin.";
        }
    }
}