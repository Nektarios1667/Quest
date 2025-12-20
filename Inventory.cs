namespace Quest;


public interface IContainer
{
    Inventory? Inventory { get; }
}

public class Inventory
{
    // Events
    public event Action<int>? EquippedSlotChanged;
    public event Action<Item>? ItemDropped;
    // Properties
    public Item?[,] Items { get; }
    public bool Opened { get; set; } = false;
    private int _equippedSlot;
    public int EquippedSlot
    {
        get => _equippedSlot;
        set { _equippedSlot = value; EquippedSlotChanged?.Invoke(value); }
    }
    public Item? Equipped => Items.Length > 0 ? Items[EquippedSlot, 0] : null;
    private int HoverSlot;
    public byte Width { get; }
    public byte Height { get; }
    public readonly bool IsPlayer;
    private readonly Rectangle[,] slotHitboxes;
    // Consts/statics
    private static readonly Point itemOffset = new(14, 14);
    private const float itemScale = 3;
    private static readonly Point slotSize = TextureManager.Metadata[TextureID.Slot].Size;
    // Helpers
    private readonly Point itemStart;
    public Inventory(int width, int height, Item?[,]? items = null, bool isPlayer = false)
    {
        IsPlayer = isPlayer;
        Items = items ?? new Item?[width, height];
        slotHitboxes = new Rectangle[width, height];
        Width = (byte)width;
        Height = (byte)height;
        HoverSlot = 0;
        itemStart = new(Constants.Middle.X - (slotSize.X * Width / 2), Constants.NativeResolution.Y - (slotSize.Y + 8) - (isPlayer ? 4 : (slotSize.Y + 8) * 4 + 50));

        // Precalculate hitboxes
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                slotHitboxes[x, y] = new(new(itemStart.X + (slotSize.X + 4) * x, itemStart.Y - (slotSize.Y + 8) * y - (y != 0 && isPlayer ? 15 : 0)), slotSize);
    }
    public void Update(GameManager gameManager, PlayerManager playerManager)
    {
        if (Width < 1 || Height < 1) return; // No slots to update

        // Scroll slot
        if (InputManager.ScrollWheelChange > 0)
            EquippedSlot = (ushort)((EquippedSlot + 1) % Width);
        if (InputManager.ScrollWheelChange < 0)
            EquippedSlot -= 1;
        if (EquippedSlot < 0) EquippedSlot += Width;

        // Handle slot interactions
        SlotInteractions(gameManager, playerManager);
    }
    public void Draw(GameManager gameManager, PlayerManager playerManager)
    {
        if (!Opened && !IsPlayer) return; // Don't draw if not opened

        if (Width < 1 || Height < 1) return; // No slots to draw

        // Draw
        for (byte x = 0; x < Width; x++)
        {
            for (byte y = 0; y < (Opened ? Height : 1); y++) // If not opened but player owned draw hotbar
            {
                // Item
                Item? item = Items[x, y];

                // Draw inventory slots
                Vector2 itemDest = new(itemStart.X + (slotSize.X + 4) * x, itemStart.Y - (slotSize.Y + 8) * y - (y != 0 && IsPlayer ? 15 : 0));
                DrawTexture(gameManager.Batch, TextureID.Slot, itemDest.ToPoint(), color: SlotColor(playerManager, x, y));

                // Draw inventory items
                if (item == null) continue;
                DrawTexture(gameManager.Batch, item.Texture, itemDest.ToPoint() + itemOffset, scale: itemScale);

                // Amount text
                if (item.Amount <= 1) continue; // Don't draw amount text for single items
                Vector2 textDest = itemDest + slotSize.ToVector2() - new Vector2(PixelOperator.MeasureString($"{item.Amount}").X + 6, 36);
                gameManager.Batch.DrawString(PixelOperatorBold, $"{item.Amount}", textDest, Color.White);
            }
        }

        // Item label
        Point hoverCoord = Expand(HoverSlot);
        if (Opened && hoverCoord.X != -1 && Items[hoverCoord.X, hoverCoord.Y] is Item hovered)
        {
            string display = StringTools.FillCamelSpaces(hovered.Name);
            Point textSize = PixelOperator.MeasureString(display).ToPoint();
            Vector2 labelPos = new(itemStart.X + (slotSize.X - textSize.X) / 2 + (slotSize.X + 4) * hoverCoord.X - 4, slotHitboxes[hoverCoord.X, hoverCoord.Y].Y - textSize.Y / 2);
            FillRectangle(gameManager.Batch, labelPos.ToPoint() + new Point(4, -8), new Point(textSize.X + 4, 30), Color.Black * 0.7f);
            gameManager.Batch.DrawRectangle(labelPos + new Vector2(2, -10), new Vector2(textSize.X + 8, 34), Color.Blue * 0.7f, 2);
            gameManager.Batch.DrawString(PixelOperator, display, labelPos + new Vector2(8, -8), Color.White);
        }
    }
    public void SlotInteractions(GameManager gameManager, PlayerManager playerManager)
    {
        // Get
        int mouseSlot = Flatten(GetMouseSlot());

        // Hover slot
        HoverSlot = mouseSlot;

        // Swap items
        if (Opened && InputManager.LMouseReleased)
        {
            if (mouseSlot >= 0 && (mouseSlot != playerManager.SelectedSlot || this != playerManager.SelectedInventory))
            {
                // Merge items
                Item? mouseItem = GetItem(mouseSlot);
                Item? selectedItem = playerManager.SelectedInventory!.GetItem(playerManager.SelectedSlot);
                if (SameItem(mouseItem, selectedItem))
                {
                    playerManager.SelectedInventory!.MergeItems(playerManager, selectedItem, mouseItem);
                    SoundManager.PlaySound("Trinkets");
                }
                else if (selectedItem != null && playerManager.SelectedInventory != null)
                {
                    // Swap items
                    Swap(playerManager.SelectedInventory, playerManager.SelectedSlot, this, mouseSlot);
                    SoundManager.PlaySound("Trinkets");
                    playerManager.SelectedSlot = mouseSlot;
                    playerManager.SelectedInventory = this;
                }
            }
        }

        // Spread items
        if (Opened && InputManager.RMouseReleased)
        {
            if (mouseSlot >= 0 && (mouseSlot != playerManager.SelectedSlot || this != playerManager.SelectedInventory))
            {
                // Merge items
                Item? mouseItem = GetItem(mouseSlot);
                Item? selectedItem = playerManager.SelectedInventory!.GetItem(playerManager.SelectedSlot);
                if (selectedItem != null)
                {
                    byte move = (byte)Math.Ceiling(selectedItem!.Amount / 2f);
                    if (SameItem(mouseItem, selectedItem))
                    {
                        move = (byte)Math.Min(move, mouseItem!.MaxAmount - mouseItem.Amount);
                        mouseItem!.Amount += move;
                        selectedItem.Amount -= move;
                        SoundManager.PlaySound("Trinkets");
                    }
                    else if (mouseItem == null)
                    {
                        SetSlot(mouseSlot, Item.ItemFromName(selectedItem.Name, move));
                        selectedItem.Amount -= move;
                        SoundManager.PlaySound("Trinkets");
                    }
                    if (selectedItem.Amount < 1)
                        playerManager.SelectedInventory!.SetSlot(playerManager.SelectedSlot, null); // Remove empty item from selected slot
                }
            }
        }

        // Select slot
        if (Opened && (InputManager.LMouseClicked || InputManager.RMouseClicked) && mouseSlot >= 0)
        {
            playerManager.SelectedSlot = mouseSlot;
            playerManager.SelectedInventory = this;
        }

        // Drop items
        if (Opened && HoverSlot >= 0 && IsPlayer && InputManager.KeyDown(Keys.D))
        {
            Item? item = GetItem(HoverSlot);
            if (item != null)
            {
                gameManager.LevelManager.DropLoot(gameManager, new Loot(item.Name, item.Amount, CameraManager.PlayerFoot + Constants.MageDrawShift, gameManager.GameTime));
                item.Dispose();
                ItemDropped?.Invoke(item);
            }
            SetSlot(HoverSlot, null);
            SoundManager.PlaySoundInstance("Trinkets");
        }
    }
    // Slot interactions
    private Color SlotColor(PlayerManager playerManager, byte x, byte y)
    {
        int slot = Flatten(x, y);
        if (slot == EquippedSlot && IsPlayer) return Constants.CottonCandy; // Equipped
        if (Opened && slot == playerManager.SelectedSlot && this == playerManager.SelectedInventory) return Constants.FocusBlue; // Selected
        if (Opened && slot == HoverSlot) return Color.LightGray; // Hovered
        return Color.White; // Default
    }
    public void MergeItems(PlayerManager playerManager, Item? from, Item? dest)
    {
        if (from != null && dest != null)
        {
            byte moved = (byte)Math.Min(from.Amount, dest.MaxAmount - dest.Amount);
            dest.Amount += moved;
            from.Amount -= moved;
            if (from.Amount < 1 && playerManager.SelectedInventory == this)
                playerManager.SelectedInventory!.SetSlot(playerManager.SelectedSlot, null);
        }
    }
    // AddItem
    public (bool success, Item leftover) AddItem(Item item)
    {
        for (byte y = 0; y < Height; y++)
        {
            for (byte x = 0; x < Width; x++)
            {
                Item? current = Items[x, y];
                if (current == null)
                {
                    SetSlot(Flatten(x, y), item);
                    return (true, item);
                }
                if (SameItem(current, item))
                {
                    byte moved = (byte)Math.Min(item.Amount, current.MaxAmount - current.Amount);
                    current!.Amount += moved; // Add to existing item
                    item.Amount -= moved; // Reduce amount of new item
                    if (item.Amount < 1) return (true, item); // If item is fully added exit
                }
            }
        }
        // Not enough space
        return (false, item);
    }
    // SameItem
    public static bool SameItem(Item? item1, Item? item2)
    {
        if (item1 == null || item2 == null) return false;
        return item1.Name == item2.Name && item1.Description == item2.Description;
    }
    // Contains
    public bool Contains(string item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Item
                Item? currentItem = Items[x, y];
                if (currentItem == null) continue; // Skip empty slots
                if (currentItem.Name == item) return true;
            }
        }
        return false;
    }
    // GetItem
    public Item? GetItem(Point pos)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height) return null;
        return Items[pos.X, pos.Y];
    }
    public Item? GetItem(int x, int y) { return GetItem(new Point(x, y)); }
    public Item? GetItem(int idx)
    {
        if (idx < 0 || idx >= Width * Height) return null;
        Point pos = Expand(idx);
        return GetItem(pos.X, pos.Y);
    }
    // ItemIndex
    public Point ItemLocation(Item item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Item
                Item? currentItem = Items[x, y];
                if (currentItem == null) continue; // Skip empty slots
                if (SameItem(currentItem, item)) return new(x, y);
            }
        }
        throw new Exception($"Item '{item.Name}' not found in inventory.");
    }
    // Replace
    public void ReplaceItem(Item item, Item repl)
    {
        Point loc = ItemLocation(item);
        Items[loc.X, loc.Y] = repl;
    }
    public void ReplaceItem(int slot, Item repl)
    {
        Point pos = Expand(slot);
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height) return; // Out of bounds
        Items[pos.X, pos.Y] = repl;
    }
    public void ReplaceItem(Point pos, Item repl)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height) return; // Out of bounds
        Items[pos.X, pos.Y] = repl;
    }
    // Swap
    public static void Swap(Inventory inv1, int slot1, Inventory inv2, int slot2)
    {
        Point pos1 = inv1.Expand(slot1);
        Point pos2 = inv2.Expand(slot2);
        (inv2.Items[pos2.X, pos2.Y], inv1.Items[pos1.X, pos1.Y]) = (inv1.Items[pos1.X, pos1.Y], inv2.Items[pos2.X, pos2.Y]);
    }
    public void Swap(Point slot1, Point slot2)
    {
        (Items[slot2.X, slot2.Y], Items[slot1.X, slot1.Y]) = (Items[slot1.X, slot1.Y], Items[slot2.X, slot2.Y]);
    }
    // SetSlot
    public void SetSlot(Point pos, Item? item)
    {
        if (pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height) Items[pos.X, pos.Y] = item;
    }
    public void SetSlot(int x, int y, Item? item)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height) Items[x, y] = item;
    }
    public void SetSlot(int idx, Item? item)
    {
        if (idx >= 0 && idx < Width * Height)
        {
            Point pos = Expand(idx);
            SetSlot(pos.X, pos.Y, item);
        }
    }
    // Consume
    public void Consume(int idx)
    {
        Point expanded = Expand(idx);
        if (expanded.X < 0 || expanded.X >= Width || expanded.Y < 0 || expanded.Y >= Height) return; // Out of bounds
        Items[expanded.X, expanded.Y] = null; // Remove item from inventory
    }
    public void Consume(Point slot)
    {
        if (slot.X < 0 || slot.X >= Width || slot.Y < 0 || slot.Y >= Height) return; // Out of bounds
        Items[slot.X, slot.Y] = null; // Remove item from inventory
    }
    public void Consume(string item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Item
                Item? currentItem = Items[x, y];
                if (currentItem == null) continue; // Skip empty slots
                if (currentItem.Name == item) Items[x, y] = null;
            }
        }
    }
    // MouseSlot
    public Point GetMouseSlot()
    {
        for (byte x = 0; x < Width; x++)
            for (byte y = 0; y < Height; y++)
                if (slotHitboxes[x, y].Contains(InputManager.MousePosition))
                    return new(x, y);

        return new(-1, -1);
    }
    // Utilites
    public string GetItemsString()
    {
        string result = "";
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Item? item = Items[x, y];
                result += $"{(item == null ? "NUL" : item.Name)};";
            }
            result += "/";
        }
        return result;
    }
    public string GetItemsAmountString()
    {
        string result = "";
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Item? item = Items[x, y];
                result += $"{(item == null ? "0" : item.Amount)};";
            }
            result += "/";
        }
        return result;
    }
    // Flatten
    public int Flatten(int x, int y) { return y * Width + x; }
    public int Flatten(Point pos) { return Flatten(pos.X, pos.Y); }
    // Expand
    public Point Expand(int slot)
    {
        int columns = Items.GetLength(0);
        return new Point(slot % columns, slot / columns);
    }
    // IsFull
    public bool IsFull()
    {
        for (int x = 0; x < Items.GetLength(0); x++)
        {
            for (int y = 0; y < Items.GetLength(1); y++)
            {
                if (Items[x, y] == null) return false;
            }
        }
        return true;
    }
    // LootPreset
    public LootPreset GetLootPreset(string name)
    {
        return new(Items, name);
    }
}
