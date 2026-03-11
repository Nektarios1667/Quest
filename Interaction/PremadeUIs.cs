using MonoGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Interaction;
public partial class UserInterface
{
    public static UserInterface ChestUI { get; private set; } = null!;
    public static UserInterface InventoryUI { get; private set; } = null!;
    public static UserInterface JukeboxUI { get; private set; } = null!;
    public static UserInterface DiscWriterUI { get; private set; } = null!;
    public static void Init(SpriteBatch batch)
    {
        // ----- Chest UI -----
        ChestUI = new(batch);

        // Create slots
        Point itemStart = new(Constants.Middle.X - Slot.SlotSize.X * Chest.Size.X / 2, Constants.NativeResolution.Y - (Slot.SlotSize.Y + 4) * 8);
        for (int y = 0; y < Chest.Size.Y; y++)
        {
            for (int x = 0; x < Chest.Size.X; x++)
            {
                Slot slot = new(new(itemStart.X + (Slot.SlotSize.X + 4) * x, itemStart.Y + (Slot.SlotSize.Y + 4) * y));
                ChestUI.AddElement($"slot_{x + y * Chest.Size.X}", slot);
            }
        }

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("CHEST").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "CHEST", PixelOperatorLarge, Color.White);
        ChestUI.AddElement("title", title);

        // ----- Player Inventory UI -----
        InventoryUI = new(batch);

        // Create slots
        itemStart = new(Constants.Middle.X - Slot.SlotSize.X * Chest.Size.X / 2, Constants.NativeResolution.Y - Slot.SlotSize.Y - 4);
        for (int y = 0; y < Chest.Size.Y + 1; y++)
        {
            for (int x = 0; x < Chest.Size.X; x++)
            {
                Slot slot = new(new(itemStart.X + (Slot.SlotSize.X + 4) * x, itemStart.Y - (Slot.SlotSize.Y + 4) * y));
                if (y == 0) slot.Tag("hotbar");
                InventoryUI.AddElement($"slot_{x + y * Chest.Size.X}", slot);
            }
        }

        // ----- Jukebox -----
        JukeboxUI = new(batch);

        // Title
        titleSize = PixelOperatorLarge.MeasureString("JUKEBOX").ToPoint();
        title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "JUKEBOX", PixelOperatorLarge, Color.White);
        JukeboxUI.AddElement("title", title);

        // Track
        InputSlot input = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75), [ItemTypes.Disc]);
        JukeboxUI.AddElement("disc", input);

        // Play button in the middle of the track slots - green text
        Button play = new(new(Constants.Middle.X - 50, 160), new(100, 35), "Play", PixelOperatorLarge, Color.White, Color.Green * 0.6f, Color.Green * 0.4f);
        play.Clicked += () =>
        {
            // Get the disc in the first slot
            InputSlot slot = (InputSlot)JukeboxUI.GetElements()["disc"]!;
            if (slot.Item != null)
                SoundtrackManager.PlaySoundtrack(slot.Item.Name.Replace(" ", ""));
        };
        JukeboxUI.AddElement("play_button", play);

        // ----- Disc Writer -----
        DiscWriterUI = new(batch);

        // Title
        titleSize = PixelOperatorLarge.MeasureString("DISC WRITER").ToPoint();
        title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "DISC WRITER", PixelOperatorLarge, Color.White);
        DiscWriterUI.AddElement("title", title);

        // Input disc
        input = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75), [ItemTypes.Disc]);
        DiscWriterUI.AddElement("disc", input);

        // Rename input
        TextInput discName = new(new(Constants.Middle.X - 100, 160), new(200, 35), PixelOperator, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness:0);
        DiscWriterUI.AddElement("disc_name", discName);
        input.OnItemChange += () =>
        {
            if (input.Item != null)
                discName.SetText(StringTools.FillCamelSpaces(input.Item.Name));
            else
                discName.SetText("");
        };
        // Rename button
        Button rename = new(new(Constants.Middle.X - 50, 215), new(100, 35), "Rename", PixelOperator, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        rename.Clicked += () =>
        {
            if (input.Item != null && discName.Text != "")
                input.Item.CustomName = discName.Text;
        };
        DiscWriterUI.AddElement("rename", rename);
    }
}
