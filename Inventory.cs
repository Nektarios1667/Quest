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
        public int Amount { get; private set; }
        public Item(string name, string description, int amount = 1)
        {
            Name = name;
            Description = description;
            Amount = amount;
        }
    }
    public class Inventory
    {
        public GameHandler Game { get; private set; }
        public Item?[,] Items { get; private set; }
        public bool Visible { get; set; } = false;
        public Dictionary<string, Texture2D> ItemTextures { get; private set; }
        public Texture2D Slot { get; private set; }
        private SpriteFont Arial { get; set; }
        public int EquippedSlot { get; set; } = 0;
        public int SelectedSlot { get; set; } = 0;
        public int Width { get; }
        public int Height { get; }
        private bool _loaded { get; set; } = false;
        public Inventory(GameHandler game, int width, int height)
        {
            Game = game;
            Items = new Item?[width, height];
            Width = width;
            Height = height;
            ItemTextures = [];
            SelectedSlot = width * height - 1;
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

            // Select slot
            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                Point mouseSlot = GetMouseSlot(mouseState);
                if (mouseSlot != Constants.NegOne) SelectedSlot = Flatten(mouseSlot);
            }
            // Swap items
            if (mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                Point mouseSlot = GetMouseSlot(mouseState);
                if (mouseSlot != Constants.NegOne && Flatten(mouseSlot) != SelectedSlot)
                {
                    Swap(SelectedSlot, Flatten(mouseSlot));
                    SelectedSlot = Flatten(mouseSlot);
                }
            }
        }
        public void Draw()
        {
            // Check loaded
            if (!_loaded) return;

            // Draw
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    // Checks
                    Item? item = Items[x, y];
                    if (!Visible && y != 0) continue;

                    // Draw inventory slots
                    Vector2 itemDest = new(Constants.Middle.X - (Constants.SlotTextureSize.X * Width / 2) + (Constants.SlotTextureSize.X + 4) * x, Constants.Window.Y - (Slot.Height + 8) * (y + 1) - (y != 0 ? 20 : 0));
                    Game.Batch.Draw(Slot, itemDest, EquippedSlot == Flatten(x, y) ? Constants.CottonCandy : (SelectedSlot == Flatten(x, y) ? Constants.FocusBlue : Color.White));
                    Game.Batch.DrawString(Arial, $"{Flatten(x, y)}", itemDest + new Vector2(8, 8), Color.Black);

                    // Draw inventory items
                    if (item == null) continue;
                    Texture2D? itemTexture = ItemTextures.TryGetValue(item.Name, out var tex) ? tex : null;
                    Game.TryDraw(itemTexture, new(itemDest.ToPoint() + new Point(8, 8), Constants.ItemTextureSize));
                    Vector2 textDest = itemDest + Constants.SlotTextureSize.ToVector2() - new Vector2(Arial.MeasureString($"{item.Amount}").X + 8, 28);
                    Game.Batch.DrawString(Arial, $"{item.Amount}", textDest, Color.Black);
                }
            }
        }
        public void LoadContent(ContentManager content)
        {
            // Dynamically load item images
            foreach (string filename in Constants.ItemNames)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    Texture2D texture = content.Load<Texture2D>($"Images/Items/{filename}");
                    ItemTextures[filename] = texture;
                }
            }

            // Load gui textures
            Slot = content.Load<Texture2D>("Images/Gui/Slot");
            Arial = content.Load<SpriteFont>("Fonts/Arial");

            _loaded = true;
        }

        // Slot interactions
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
