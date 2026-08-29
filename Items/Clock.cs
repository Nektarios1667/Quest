using Quest.Interaction;

namespace Quest.Items;
public class Clock : Item
{
    public Clock(byte amount, string? customName = null) : base(ItemTypes.Clock, amount, customName)
    {

    }
    public override bool PrimaryUse(GameManager gameManager, PlayerManager player)
    {
        // Toggle open
        if (player.OpenedInterface != null)
            player.CloseInterface();
        else
            player.OpenInterface(gameManager, UserInterface.ClockUI);

        return true;
    }
}
