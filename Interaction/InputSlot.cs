using Quest.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public class InputSlot : Slot
{
    // Preset whitelists
    // public static readonly ItemType[] CoinTypes = [ItemTypes.DeltaCoin, ItemTypes.GammaCoin, ItemTypes.PhiCoin];
    //
    public ItemType[] Allowed { get; }
    public InputSlot(Point location, ItemType[]? allowed = null) : base(location)
    {
        Bounds = new Rectangle(Location, new Point(32, 32));
        Color = Color.LightGreen;
        Allowed = allowed ?? [];
    }
    public override Item AddItem(Item item)
    {
        if (Allowed.Contains(item.Type))
            return base.AddItem(item);
        return item;
    }
}
