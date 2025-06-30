namespace Quest.Items
{
    public class Pickaxe : Item
    {
        public Pickaxe(PlayerManager playerManager, int amount) : base(playerManager, amount)
        {
            Description = "A sturdy metal pickaxe used for mining.";
        }
    }
}