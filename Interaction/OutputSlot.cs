namespace Quest.Interaction;

public class OutputSlot : Slot
{
    public OutputSlot(Point location) : base(location)
    {
        Color = Color.LightBlue;
    }
    // Prevents any player from adding items
    public override bool CanAccept(Item? item) => item == null;
}
