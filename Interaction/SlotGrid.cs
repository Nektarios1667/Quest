using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;

public enum SlotType
{
    Normal,
    Input,
    Output,
}

public class SlotGrid : UIElement
{
    public byte Width { get; private set; }
    public byte Height { get; private set; }
    public Slot[,] Slots { get; private set; }
    public SlotGrid(Point location, byte width, byte height) : base(location)
    {
        Width = width;
        Height = height;

        Slots = new Slot[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Slots[x, y] = new Slot(new(Location.X + (Slot.SlotSize.X + 4) * x, Location.Y + (Slot.SlotSize.Y + 4) * y));
    }
    public override void Update(UserInterface ui)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Slots[x, y].Update(ui);
            }
        }
    }
    public override void Draw(UserInterface ui)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Slots[x, y].Draw(ui);
            }
        }
    }
}
