using Quest.Items;
using System.Linq;

namespace Quest.Interaction;

public class Container
{
    public Item?[] Items { get; private set; }
    public Container(Item?[]? items = null)
    {
        Items = items ?? [];
    }
    // Item management
    public static bool MoveItemUI(UserInterface from, int fromIdx, UserInterface to, int toIdx, bool split = false)
    {
        // Check nulls
        if (from.BoundContainer == null || to.BoundContainer == null) return false;

        // Check acceptance for both slots
        if (!from.GetSlot(fromIdx).CanAccept(to.BoundContainer.Items[toIdx])) return false;
        if (!to.GetSlot(toIdx).CanAccept(from.BoundContainer.Items[fromIdx])) return false;
        MoveItem(from.BoundContainer, fromIdx, to.BoundContainer, toIdx, split);

        // Cleanup
        from.BoundContainer.RemoveEmptyItems();
        to.BoundContainer.RemoveEmptyItems();
        return true;
    }
    public static void MoveItem(Container from, int fromIdx, Container to, int toIdx, bool split = false)
    {
        // Get and check
        Item? fromItem = from.Items[fromIdx];
        Item? toItem = to.Items[toIdx];
        if (fromItem == null) return; // Can't move empty slot

        // Move
        if (toItem == null)
        {
            if (split)
            {
                Split(from, fromIdx, to, toIdx);
            }
            else
            {
                to.SetSlot(toIdx, fromItem);
                from.SetSlot(fromIdx, null);
            }
        }
        // Merge
        else if (SameItem(fromItem, toItem))
        {
            MergeItems(from, fromIdx, to, toIdx);
        }
        // Swap
        else
        {
            Swap(from, fromIdx, to, toIdx);
        }

        // Cleanup
        from.RemoveEmptyItems();
        to.RemoveEmptyItems();
    }
    public static void MergeItems(Container from, int fromIdx, Container to, int toIdx)
    {
        // Get and check
        Item? fromItem = from.Items[fromIdx];
        Item? destItem = to.Items[toIdx];
        if (fromItem == null || destItem == null) return; // Can't merge empty slots

        // Merge
        byte moved = (byte)Math.Min(fromItem.Amount, destItem.MaxAmount - destItem.Amount);
        destItem.Amount += moved;
        fromItem.Amount -= moved;

        // Cleanup
        from.RemoveEmptyItems();
        to.RemoveEmptyItems();
    }
    public static void Split(Container from, int fromIdx, Container to, int toIdx)
    {
        // Get and check
        Item? fromItem = from.Items[fromIdx];
        Item? toItem = to.Items[toIdx];
        if (fromItem == null) return; // Can't split empty slot
        if (toItem != null) return; // Can't split into non-empty slot
        // Split
        byte half = (byte)Math.Ceiling(fromItem.Amount / 2f);
        to.SetSlot(toIdx, fromItem.Take(half));

        // Cleanup
        from.RemoveEmptyItems();
        to.RemoveEmptyItems();
    }
    public static void Swap(Container from, int fromIdx, Container to, int toIdx)
    {
        (from.Items[fromIdx], to.Items[toIdx]) = (to.Items[toIdx], from.Items[fromIdx]);

        // Cleanup
        from.RemoveEmptyItems();
        to.RemoveEmptyItems();
    }
    public Item AddItem(Item adding)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Item? item = Items[i];
            if (item == null)
            {
                byte moved = Math.Min(adding.Amount, adding.MaxAmount);
                SetSlot(i, Item.Create(adding.Type, moved, adding.CustomName));
                adding.Amount -= moved; // Reduce amount of new item
            }
            if (SameItem(item, adding))
            {
                byte moved = (byte)Math.Min(adding.Amount, item!.MaxAmount - item.Amount);
                item.Amount += moved; // Add to existing item
                adding.Amount -= moved; // Reduce amount of new item
            }

            // Check if done
            if (adding.Amount <= 0) break;
        }

        // Cleanup
        RemoveEmptyItems();

        return adding;
    }
    public int Count(ItemType itemType)
    {
        int count = 0;
        foreach (Item? item in Items)
            if (item != null && item.Type == itemType) count += item.Amount;

        return count;
    }
    public bool Has(ItemRef target)
    {
        int count = 0;
        foreach (Item? item in Items)
        {
            if (SameItem(target, item)) count += item?.Amount ?? 0;
            if (count >= target.Amount) return true;
        }

        return false;
    }
    public bool Consume(ItemRef consume, bool ignoreCheck = false)
    {
        // Not enough
        if (!ignoreCheck && Count(consume.Type) < consume.Amount) return false;
        consume = consume.Copy(); // Create a copy to modify

        // Consume
        for (int i = 0; i < Items.Length; i++)
        {
            Item? item = Items[i];
            if (item == null || item.Type != consume.Type || item.Amount <= 0) continue; // Skip empty slots or different items

            // Consume in this slot
            byte toConsume = Math.Min(item.Amount, consume.Amount);
            item.Amount -= toConsume;
            consume.Amount -= toConsume;
            // Check if enough
            if (item.Amount <= 0) Items[i] = null;
            if (consume.Amount <= 0) return true;
        }

        Logger.Error($"Item consume failed despite previous checks.", exit: true);
        return false;
    }
    public bool IsFull() => !Items.Any(item => item == null);
    public void SetSlot(int idx, Item? item)
    {
        if (idx >= 0 && idx < Items.Length)
            Items[idx] = item;

        if (item?.Amount <= 0)
            Items[idx] = null;
    }
    public int? Locate(ItemType item)
    {
        for (int i = 0; i < Items.Length; i++)
            if (Items[i] != null && Items[i]!.Type == item) return i;
        return null;
    }
    public int? LocateByUID(int id)
    {
        for (int i = 0; i < Items.Length; i++)
            if (Items[i] != null && Items[i]!.UID == id) return i;
        return null;
    }
    public void SetItems(Item?[] items)
    {
        Items = items;
        // Cleanup
        RemoveEmptyItems();
    }
    public void Clear() => Items = new Item?[Items.Length];
    public void RemoveEmptyItems()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Item? item = Items[i];
            if (item != null && item.Amount <= 0)
            {
                Items[i] = null;
                item.Dispose();
            }
        }
    }
    public static bool SameItem(Item? item1, Item? item2) => item1 != null && item2 != null && item1.Type == item2.Type && item1.CustomName == item2.CustomName;
    public static bool SameItem(ItemRef? item1, Item? item2) => item1 != null && item2 != null && item1.Type == item2.Type && item1.CustomName == item2.CustomName;
    public static bool SameItem(Item? item1, ItemRef? item2) => item1 != null && item2 != null && item1.Type == item2.Type && item1.CustomName == item2.CustomName;
    public static bool SameItem(ItemRef? item1, ItemRef? item2) => item1 != null && item2 != null && item1.Type == item2.Type && item1.CustomName == item2.CustomName;
}
