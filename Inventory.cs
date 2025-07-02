namespace Quest;
public class Inventory
{
    public Item?[,] Items { get; private set; }
    public bool Opened { get; set; } = false;
    public int EquippedSlot { get; set; }
    public Item? Equipped => Items[EquippedSlot, 0];
    public int SelectedSlot { get; set; }
    public int HoverSlot { get; set; }
    public int Width { get; }
    public int Height { get; }
    private readonly Point itemOffset = new(14, 14);
    private readonly float itemScale = 3;
    private readonly Point slotSize = TextureManager.Metadata[TextureID.Slot].Size;
    private readonly Point itemStart;
    public Inventory(int width, int height)
    {
        Items = new Item?[width, height];
        Width = width;
        Height = height;
        SelectedSlot = width * height - 1;
        HoverSlot = 0;
        itemStart = new(Constants.Middle.X - (slotSize.X * Width / 2), Constants.Window.Y - slotSize.Y - 8);
    }
    public void Update(GameManager gameManager, PlayerManager playerManager)
    {
        if (Width < 1 || Height < 1) return; // No slots to update

        // Scroll slot
        if (InputManager.ScrollWheelChange > 0)
            EquippedSlot = (EquippedSlot + 1) % Width;
        if (InputManager.ScrollWheelChange < 0)
            EquippedSlot -= 1;
        if (EquippedSlot < 0) EquippedSlot += Width;

        // Handle slot interactions
        SlotInteractions(gameManager, playerManager);
    }
    public void Draw(GameManager gameManager)
    {
        if (Width < 1 || Height < 1) return; // No slots to draw

        // Draw
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < (Opened ? Height : 1); y++)
            {
                // Item
                Item? item = Items[x, y];

                // Draw inventory slots
                Vector2 itemDest = new(itemStart.X + (slotSize.X + 4) * x, itemStart.Y - (slotSize.Y + 8) * y - (y != 0 ? 20 : 0));
                DrawTexture(gameManager.Batch, TextureID.Slot, itemDest.ToPoint(), color: SlotColor(x, y));

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
            Vector2 labelPos = new(itemStart.X + (slotSize.X - textSize.X) / 2 + (slotSize.X + 4) * hoverCoord.X - 4, itemStart.Y - (slotSize.Y + 8) * hoverCoord.Y - textSize.Y / 2 - 10 - (hoverCoord.Y != 0 ? 20 : 0));
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
            if (mouseSlot >= 0 && mouseSlot != SelectedSlot)
            {
                // Merge items
                Item? mouseItem = GetItem(mouseSlot);
                Item? selectedItem = GetItem(SelectedSlot);
                if (SameItem(mouseItem, selectedItem))
                {
                    MergeItems(selectedItem, mouseItem);
                    SoundManager.PlaySound("Trinkets");
                }
                else if (selectedItem != null)
                {
                    // Swap items
                    Swap(SelectedSlot, mouseSlot);
                    SoundManager.PlaySound("Trinkets");
                    SelectedSlot = mouseSlot;
                }
            }
        }

        // Spread items
        if (Opened && InputManager.RMouseReleased)
        {
            if (mouseSlot >= 0 && mouseSlot != SelectedSlot)
            {
                // Merge items
                Item? mouseItem = GetItem(mouseSlot);
                Item? selectedItem = GetItem(SelectedSlot);
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
                        SetSlot(mouseSlot, Item.ItemFromName(playerManager, selectedItem.Name, move));
                        selectedItem.Amount -= move;
                        SoundManager.PlaySound("Trinkets");
                    }
                    if (selectedItem.Amount < 1)
                        SetSlot(SelectedSlot, null); // Remove empty item from selected slot
                }
            }
        }

        // Select slot
        if (Opened && (InputManager.LMouseClicked || InputManager.RMouseClicked) && mouseSlot >= 0)
            SelectedSlot = mouseSlot;

        // Deselect
        if (InputManager.LMouseClicked && (mouseSlot < 0 || mouseSlot > Items.Length)) SelectedSlot = -1;

        // Drop items
        if (Opened && HoverSlot >= 0 && InputManager.KeyDown(Keys.D))
        {
            Item? item = GetItem(HoverSlot);
            if (item != null) gameManager.LevelManager.DropLoot(gameManager, new Loot(item.Name, item.Amount, CameraManager.PlayerFoot + Constants.MageDrawShift, gameManager.GameTime));
            SetSlot(HoverSlot, null);
            SoundManager.PlaySoundInstance("Trinkets");
        }
    }
    // Slot interactions
    private Color SlotColor(int x, int y)
    {
        int slot = Flatten(x, y);
        if (slot == EquippedSlot) return Constants.CottonCandy; // Equipped
        if (Opened && slot == SelectedSlot) return Constants.FocusBlue; // Selected
        if (Opened && slot == HoverSlot) return Color.LightGray; // Hovered
        return Color.White; // Default
    }
    public void MergeItems(Item? from, Item? dest)
    {
        if (from != null && dest != null)
        {
            byte moved = (byte)Math.Min(from.Amount, dest.MaxAmount - dest.Amount);
            dest.Amount += moved;
            from.Amount -= moved;
            if (from.Amount < 1)
                SetSlot(SelectedSlot, null);
        }
    }
    // AddItem
    public void AddItem(Item item)
    {
        if (item == null || IsFull()) return;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Item? current = Items[x, y];
                if (current == null)
                {
                    SetSlot(Flatten(x, y), item);
                    return;
                }
                if (SameItem(current, item))
                {
                    byte moved = (byte)Math.Min(item.Amount, current.MaxAmount - current.Amount);
                    current!.Amount += moved; // Add to existing item
                    item.Amount -= moved; // Reduce amount of new item
                    if (item.Amount < 1) return; // If item is fully added exit
                }
            }
        }
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
    public void Swap(int slot1, int slot2)
    {
        Point pos1 = Expand(slot1);
        Point pos2 = Expand(slot2);
        (Items[pos2.X, pos2.Y], Items[pos1.X, pos1.Y]) = (Items[pos1.X, pos1.Y], Items[pos2.X, pos2.Y]);
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
        // x coord
        int left = Constants.Middle.X - (slotSize.X * Width / 2);
        int x = (InputManager.MousePosition.X - left) / (slotSize.X + 4);
        if (x < 0 || x >= Width) return Constants.NegativePoint; // Out of bounds

        // y coord
        int top = Constants.Window.Y - (GetTexture(TextureID.Slot).Height + 8) * (Height + 1) - (Height != 0 ? 20 : 0);
        int y = (InputManager.MousePosition.Y - top) / (slotSize.Y + 8);
        y = Height - y; // Flip y axis
        if (y < 0 || y >= Height) return Constants.NegativePoint; // Out of bounds

        return new(x, y);
    }
    // Utilites
    // Flatten
    public int Flatten(int x, int y) { return y * Width + x; }
    public int Flatten(Vector2 pos) { return Flatten((int)pos.X, (int)pos.Y); }
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
}
