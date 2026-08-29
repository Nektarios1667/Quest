using Quest.Interaction;

namespace Quest.Items;
public class Compass : Item
{
    public Compass(byte amount, string? customName = null) : base(ItemTypes.Compass, amount, customName)
    {}
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        // Toggle open
        if (player.OpenedInterface != null)
            player.CloseInterface();
        else
            player.OpenInterface(gameManager, UserInterface.CompassUI);

        return true;
    }
}
