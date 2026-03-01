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
        Color = Color.LightGreen;
        Allowed = allowed ?? [];
    }
    public override bool SetItem(Item? item)
    {
        if (CanAccept(item))
            return base.SetItem(item);
        return false;
    }
    public override bool CanAccept(Item? item) => item == null || Allowed.Contains(item.Type);
}
