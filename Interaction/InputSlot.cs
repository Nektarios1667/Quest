using Quest.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;
public enum FilterType
{
    Whitelist,
    Blacklist,
}
public struct ItemFilter
{
    public ItemTypeID[] Items { get; set; }
    public FilterType Type { get; set; }
    public ItemFilter(ItemTypeID[] items, FilterType type)
    {
        Items = items;
        Type = type;
    }
    public readonly bool Passes(ItemTypeID item)
    {
        if (Type == FilterType.Whitelist)
            return Items.Contains(item);
        else
            return !Items.Contains(item);
    }
}
public class InputSlot : Slot
{
    // Preset whitelists
    // public static readonly ItemType[] CoinTypes = [ItemTypes.DeltaCoin, ItemTypes.GammaCoin, ItemTypes.PhiCoin];
    //
    public ItemFilter Filter { get; private set; }
    public InputSlot(Point location, ItemFilter filter) : base(location)
    {
        Color = Color.LightGreen;
        Filter = filter;
    }
    //public override bool SetItem(Item? item)
    //{
    //    if (CanAccept(item))
    //        return base.SetItem(item);
    //    return false;
    //}
    public override bool CanAccept(Item? item) => item == null || Filter.Passes(item.Type.TypeID);
}
