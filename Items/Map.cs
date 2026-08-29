using Quest.Interaction;

namespace Quest.Items;
public class Map : Item
{
    public Map(byte amount, string? customName = null) : base(ItemTypes.Map, amount, customName)
    {}
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        player.OpenInventory(gameManager);
        return true;
    }
}
