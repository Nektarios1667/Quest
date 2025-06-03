using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace Quest
{
    public class Item
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Amount { get; set; }
        public int Max { get; private set; }
        public Item(string name, string description, int amount = 1, int max = Constants.MaxStack)
        {
            Name = name;
            Description = description;
            Amount = amount;
            Max = max;
        }
    }
    public class Inventory
    {
        public GameHandler Game { get; private set; }
        public Item?[,] Items { get; private set; }
        public bool Opened { get; set; } = false;
        public Dictionary<string, Texture2D> ItemTextures { get; private set; }
        public Texture2D Slot { get; private set; }
        public Texture2D GuiBackground { get; private set; }
        private SpriteFont PressStart { get; set; }
        public int EquippedSlot { get; set; }
        public int SelectedSlot { get; set; }
        public int HoverSlot { get; set; }
        public int Width { get; }
        public int Height { get; }
        private bool _loaded { get; set; } = false;
        private readonly Xna.Point itemOffset = new(8, 8);
        private readonly Vector2 itemScale = new(3, 3);
        public Inventory(GameHandler game, int width, int height)
        {
            Game = game;
            Items = new Item?[width, height];
            Width = width;
            Height = height;
            ItemTextures = [];
            SelectedSlot = width * height - 1;
            HoverSlot = 0;
        }
        public void Update(MouseState previousMouseState, MouseState mouseState)
        {
            // Scroll slot
            if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
            {
                EquippedSlot = (EquippedSlot + 1) % Width;
            }
            if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
            {
                EquippedSlot -= 1;
                if (EquippedSlot < 0) EquippedSlot += Width;
            }

            // Handle slot interactions
            SlotInteractions(previousMouseState, mouseState);
        }
        public void Draw()
        {
            // Check loaded
            if (!_loaded) return;

            // Draw
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < (Opened ? Height : 1); y++) {
                    // Item
                    Item? item = Items[x, y];

                    // Draw inventory slots
                    Vector2 itemDest = new(Constants.Middle.X - (Constants.SlotTextureSize.X * Width / 2) + (Constants.SlotTextureSize.X + 4) * x, Constants.Window.Y - (Slot.Height + 8) * (y + 1) - (y != 0 ? 20 : 0));
                    Game.Batch.Draw(Slot, itemDest, SlotColor(x, y));

                    // Draw inventory items
                    if (item == null) continue;
                    Texture2D? itemTexture = ItemTextures.TryGetValue(item.Name, out var tex) ? tex : null;
                    Game.TryDraw(itemTexture, new(itemDest.ToPoint() + itemOffset, Constants.ItemTextureSize), scale:itemScale);

                    // Text
                    Vector2 textDest = itemDest + Constants.SlotTextureSize.ToVector2() - new Vector2(PressStart.MeasureString($"{item.Amount}").X + 8, 30);
                    Game.Batch.DrawString(PressStart, $"{item.Amount}", textDest, Color.Black);
                }
            }
        }
        public void LoadContent(ContentManager content)
        {
            // Dynamically load item images
            foreach (string filename in Constants.ItemNames)
            {
                Texture2D texture = content.Load<Texture2D>($"Images/Items/{filename}");
                ItemTextures[filename] = texture;
            }

            // Load gui textures
            Slot = content.Load<Texture2D>("Images/Gui/Slot");
            PressStart = content.Load<SpriteFont>("Fonts/PressStart");
            GuiBackground = content.Load<Texture2D>("Images/Gui/GuiBackground");

            _loaded = true;
        }
        public void SlotInteractions(MouseState previousMouseState, MouseState mouseState)
        {
            // Get
            int mouseSlot = Flatten(GetMouseSlot(mouseState));

            // Hover slot
            HoverSlot = mouseSlot;

            // Swap items
            if (Opened && Game.Window.LMouseRelease)
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

            // Select slot
            if (Opened && Game.Window.LMouseClick && mouseSlot >= 0)
                SelectedSlot = mouseSlot;
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
                int moved = Math.Min(Math.Abs(from.Amount), dest.Max - dest.Amount);
                dest.Amount += moved;
                from.Amount -= moved;
                if (from.Amount < 1)
                    SetSlot(SelectedSlot, null);
            }
        }
        // SameItem
        public static bool SameItem(Item? item1, Item? item2)
        {
            if (item1 == null || item2 == null) return false;
            return item1.Name == item2.Name && item1.Description == item2.Description;
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
        public Xna.Point GetMouseSlot(MouseState mouseState)
        {
            // x coord
            int left = (int)Constants.Middle.X - (Constants.SlotTextureSize.X * Width / 2);
            int x = (mouseState.Position.X - left) / (Constants.SlotTextureSize.X + 4);
            if (x < 0 || x >= Width) return Constants.NegOne; // Out of bounds

            // y coord
            int top = (int)Constants.Window.Y - (Slot.Height + 8) * (Height + 1) - (Height != 0 ? 20 : 0);
            int y = (mouseState.Position.Y - top) / (Constants.SlotTextureSize.Y + 8);
            y = Height - y; // Flip y axis
            if (y < 0 || y >= Height) return Constants.NegOne; // Out of bounds

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
            for (int y = 0; y < Items.GetLength(0); y++) {
                for (int x = 0; x < Items.GetLength(1); x++) {
                    if (Items[y, x] == null) return false;
                }
            }
            return true;
        }
    }
}
