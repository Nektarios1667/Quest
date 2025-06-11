using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Xna = Microsoft.Xna.Framework;
namespace Quest;

public class Item
{
    public string DisplayText => $"{Amount} {StringTools.FillCamelSpaces(Amount != 1 ? StringTools.Pluralize(Name) : Name)}";
    public string Name { get; private set; }
    public string Description { get; private set; }
    public byte Amount { get; set; }
    public byte Max { get; private set; }
    public Item(string name, string description, byte amount = 1, byte max = Constants.MaxStack)
    {
        Name = name;
        Description = description;
        Amount = amount;
        Max = max;
    }
    public bool IsSameItemType(Item? other)
    {
        if (other == null) return false;
        return Name == other.Name && Description == other.Description;
    }
}
public class Inventory
{
    // Input generators
    public bool LMouseClick => MouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;
    public bool LMouseDown => MouseState.LeftButton == ButtonState.Pressed;
    public bool LMouseRelease => MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;
    public bool RMouseClick => MouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released;
    public bool RMouseDown => MouseState.RightButton == ButtonState.Pressed;
    public bool RMouseRelease => MouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;
    private MouseState MouseState { get; set; }
    private MouseState PreviousMouseState { get; set; }

    //
    public IGameManager Game { get; private set; }
    public Item?[,] Items { get; private set; }
    public bool Opened { get; set; } = false;
    private SpriteFont PixelOperator { get; set; }
    private SpriteFont PixelOperatorBold { get; set; }
    public int EquippedSlot { get; set; }
    public Item? Equipped => Items[EquippedSlot, 0];
    public int SelectedSlot { get; set; }
    public int HoverSlot { get; set; }
    public int Width { get; }
    public int Height { get; }
    private readonly Xna.Point itemOffset = new(14, 14);
    private readonly Vector2 itemScale = new(3, 3);
    private readonly Point slotSize = TextureManager.Metadata[TextureID.Slot].Size;
    private readonly Point itemStart;
    public Inventory(IGameManager game, int width, int height)
    {
        Game = game;
        Items = new Item?[width, height];
        Width = width;
        Height = height;
        SelectedSlot = width * height - 1;
        HoverSlot = 0;
        PixelOperator = Game.Content.Load<SpriteFont>("Fonts/PixelOperator");
        PixelOperatorBold = Game.Content.Load<SpriteFont>("Fonts/PixelOperatorBold");

        itemStart = new(Constants.Middle.X - (slotSize.X * Width / 2), Constants.Window.Y - slotSize.Y - 8);
    }
    public void Update(MouseState previousMouseState, MouseState mouseState)
    {
        // Inputs
        PreviousMouseState = previousMouseState;
        MouseState = mouseState;

        // Scroll slot
        if (MouseState.ScrollWheelValue > PreviousMouseState.ScrollWheelValue)
        {
            EquippedSlot = (EquippedSlot + 1) % Width;
        }
        if (MouseState.ScrollWheelValue < PreviousMouseState.ScrollWheelValue)
        {
            EquippedSlot -= 1;
            if (EquippedSlot < 0) EquippedSlot += Width;
        }

        // Handle slot interactions
        SlotInteractions(PreviousMouseState, MouseState);
    }
    public void Draw()
    {
        // Draw
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < (Opened ? Height : 1); y++)
            {
                // Item
                Item? item = Items[x, y];

                // Draw inventory slots
                Vector2 itemDest = new(itemStart.X + (slotSize.X + 4) * x, itemStart.Y - (slotSize.Y + 8) * y - (y != 0 ? 20 : 0));
                DrawTexture(Game.Batch, TextureID.Slot, new(itemDest.ToPoint(), slotSize), color: SlotColor(x, y));

                // Draw inventory items
                if (item == null) continue;
                TextureID textureId = (TextureID)(Enum.TryParse(typeof(TextureID), item.Name, out var tex) ? tex : TextureID.Null);
                DrawTexture(Game.Batch, textureId, new(itemDest.ToPoint() + itemOffset, slotSize - new Point(16)), scale: itemScale);

                // Amount text
                if (item.Amount <= 1) continue; // Don't draw amount text for single items
                Vector2 textDest = itemDest + slotSize.ToVector2() - new Vector2(PixelOperator.MeasureString($"{item.Amount}").X + 6, 36);
                Game.Batch.DrawString(PixelOperatorBold, $"{item.Amount}", textDest, Color.White);
            }
        }

        // Item label
        Point hoverCoord = Expand(HoverSlot);
        if (Opened && hoverCoord.X != -1 && Items[hoverCoord.X, hoverCoord.Y] is Item hovered)
        {
            string display = StringTools.FillCamelSpaces(hovered.Name);
            Point textSize = PixelOperator.MeasureString(display).ToPoint();
            Vector2 labelPos = new(itemStart.X + (slotSize.X - textSize.X) / 2 + (slotSize.X + 4) * hoverCoord.X - 4, itemStart.Y - (slotSize.Y + 8) * hoverCoord.Y - textSize.Y / 2 - 10 - (hoverCoord.Y != 0 ? 20 : 0));
            Game.Batch.FillRectangle(labelPos + new Vector2(4, -8), new Vector2(textSize.X + 4, 30), Color.Black * 0.7f);
            Game.Batch.DrawRectangle(labelPos + new Vector2(2, -10), new Vector2(textSize.X + 8, 34), Color.Blue * 0.7f, 2);
            Game.Batch.DrawString(PixelOperator, display, labelPos + new Vector2(8, -8), Color.White);
        }
    }
    public void SlotInteractions(MouseState previousMouseState, MouseState mouseState)
    {
        // Get
        int mouseSlot = Flatten(GetMouseSlot(mouseState));

        // Hover slot
        HoverSlot = mouseSlot;

        // Swap items
        if (Opened && LMouseRelease)
        {
            if (mouseSlot >= 0 && mouseSlot != SelectedSlot)
            {
                // Merge items
                Item? mouseItem = GetItem(mouseSlot);
                Item? selectedItem = GetItem(SelectedSlot);
                if (SameItem(mouseItem, selectedItem))
                    MergeItems(selectedItem, mouseItem);
                else if (selectedItem != null)
                {
                    // Swap items
                    Swap(SelectedSlot, mouseSlot);
                    SelectedSlot = mouseSlot;
                }
            }
        }

        // Spread items
        if (Opened && RMouseRelease)
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
                        move = (byte)Math.Min(move, mouseItem!.Max - mouseItem.Amount);
                        mouseItem!.Amount += move;
                        selectedItem.Amount -= move;
                    }
                    else if (mouseItem == null)
                    {
                        SetSlot(mouseSlot, new Item(selectedItem.Name, selectedItem.Description, move, selectedItem.Max));
                        selectedItem.Amount -= move;
                    }
                    if (selectedItem.Amount < 1)
                        SetSlot(SelectedSlot, null); // Remove empty item from selected slot
                }
            }
        }

        // Select slot
        if (Opened && (LMouseClick || RMouseClick) && mouseSlot >= 0)
            SelectedSlot = mouseSlot;

        // Deselect
        if (LMouseDown && (mouseSlot < 0 || mouseSlot > Items.Length)) SelectedSlot = -1;

        // Drop items
        if (Opened && HoverSlot >= 0 && Game.IsKeyDown(Keys.D))
        {
            Item? item = GetItem(HoverSlot);
            if (item != null) Game.DropLoot(new Loot(item, Game.PlayerFoot + Constants.MageHalfSize, Game.Time));
            SetSlot(HoverSlot, null);
        }
    }
    // Slot interactions
    public void DrawSlot(int slot)
    {
        Item? item = GetItem(slot);
        if (item != null)
        {
            Game.AddLoot(new Loot(item, Game.PlayerFoot + Constants.MageHalfSize, Game.Time));
            SetSlot(HoverSlot, null);
        }
    }
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
            byte moved = (byte)Math.Min(from.Amount, dest.Max - dest.Amount);
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
                    byte moved = (byte)Math.Min(item.Amount, current.Max - current.Amount);
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
    public bool Contains(Item item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Item
                Item? currentItem = Items[x, y];
                if (currentItem == null) continue; // Skip empty slots
                if (currentItem.Name == item.Name && currentItem.Description == item.Description) return true;
            }
        }
        return false;
    }
    // GetItem
    public Item? GetItem(Xna.Point pos)
    {
        if (pos.X < 0 || pos.X >= Width || pos.Y < 0 || pos.Y >= Height) return null;
        return Items[pos.X, pos.Y];
    }
    public Item? GetItem(int x, int y) { return GetItem(new Xna.Point(x, y)); }
    public Item? GetItem(int idx)
    {
        if (idx < 0 || idx >= Width * Height) return null;
        Xna.Point pos = Expand(idx);
        return GetItem(pos.X, pos.Y);
    }
    // Swap
    public void Swap(int slot1, int slot2)
    {
        Xna.Point pos1 = Expand(slot1);
        Xna.Point pos2 = Expand(slot2);
        (Items[pos2.X, pos2.Y], Items[pos1.X, pos1.Y]) = (Items[pos1.X, pos1.Y], Items[pos2.X, pos2.Y]);
    }
    public void Swap(Xna.Point slot1, Xna.Point slot2)
    {
        (Items[slot2.X, slot2.Y], Items[slot1.X, slot1.Y]) = (Items[slot1.X, slot1.Y], Items[slot2.X, slot2.Y]);
    }
    // SetSlot
    public void SetSlot(Xna.Point pos, Item? item)
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
    public void Consume(int idx) {
        Point expanded = Expand(idx);
        if (expanded.X < 0 || expanded.X >= Width || expanded.Y < 0 || expanded.Y >= Height) return; // Out of bounds
        Items[expanded.X, expanded.Y] = null; // Remove item from inventory
    }
    public void Consume(Point slot)
    {
        if (slot.X < 0 || slot.X >= Width || slot.Y < 0 || slot.Y >= Height) return; // Out of bounds
        Items[slot.X, slot.Y] = null; // Remove item from inventory
    }
    public void Consume(Item item)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Item
                Item? currentItem = Items[x, y];
                if (currentItem == null) continue; // Skip empty slots
                if (currentItem.Name == item.Name && currentItem.Description == item.Description) Items[x, y] = null;
            }
        }
    }
    // MouseSlot
    public Xna.Point GetMouseSlot(MouseState MouseState)
    {
        // x coord
        int left = Constants.Middle.X - (slotSize.X * Width / 2);
        int x = (MouseState.Position.X - left) / (slotSize.X + 4);
        if (x < 0 || x >= Width) return Constants.NegativePoint; // Out of bounds

        // y coord
        int top = Constants.Window.Y - (GetTexture(TextureID.Slot).Texture.Height + 8) * (Height + 1) - (Height != 0 ? 20 : 0);
        int y = (MouseState.Position.Y - top) / (slotSize.Y + 8);
        y = Height - y; // Flip y axis
        if (y < 0 || y >= Height) return Constants.NegativePoint; // Out of bounds

        return new(x, y);
    }
    // Utilites
    // Flatten
    public int Flatten(int x, int y) { return y * Width + x; }
    public int Flatten(Xna.Vector2 pos) { return Flatten((int)pos.X, (int)pos.Y); }
    public int Flatten(Xna.Point pos) { return Flatten(pos.X, pos.Y); }
    // Expand
    public Xna.Point Expand(int slot)
    {
        int columns = Items.GetLength(0);
        return new Xna.Point(slot % columns, slot / columns);
    }
    // Booleans
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
