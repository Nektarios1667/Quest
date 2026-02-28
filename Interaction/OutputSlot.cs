using Quest.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public class OutputSlot : Slot
{
    public OutputSlot(Point location) : base(location)
    {
        Bounds = new Rectangle(Location, new Point(32, 32));
        Color = Color.LightBlue;
    }
    public override Item AddItem(Item item) => item; // Can not add items to output slot; SetItem is used instead
}
