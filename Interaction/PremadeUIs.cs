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
    public static UserInterface CrateUI { get; private set; } = null!;
    public static UserInterface DiscWriterUI { get; private set; } = null!;
    public static UserInterface DisplayCaseUI { get; private set; } = null!;
    public static UserInterface FurnaceUI { get; private set; } = null!;
    public static UserInterface InscriberUI { get; private set; } = null!;
    public static UserInterface InventoryUI { get; private set; } = null!;
    public static UserInterface JukeboxUI { get; private set; } = null!;
    public static UserInterface StoveUI { get; private set; } = null!;
    public static void Init(SpriteBatch batch, LevelManager levelManager)
    {
        CreateChestUI(batch);
        CreateCrateUI(batch);
        CreateDiscWriterUI(batch);
        CreateDisplayCaseUI(batch);
        CreateFurnaceUI(batch, levelManager);
        CreateInscriberUI(batch);
        CreateInventoryUI(batch);
        CreateJukeboxUI(batch);
        CreateStoveUI(batch, levelManager);
    }

    private static void CreateChestUI(SpriteBatch batch)
    {
        // ----- Chest UI -----
        ChestUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("CHEST").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "CHEST", PixelOperatorLarge, Color.White);
        ChestUI.AddElement("title", title);

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
    }
    private static void CreateCrateUI(SpriteBatch batch)
    {
        // ----- Crate UI -----
        CrateUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("CRATE").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "CRATE", PixelOperatorLarge, Color.White);
        CrateUI.AddElement("title", title);

        // Create slots
        Point itemStart = new(Constants.Middle.X - Slot.SlotSize.X * Crate.Size.X / 2, Constants.NativeResolution.Y - (Slot.SlotSize.Y + 4) * 8);
        for (int y = 0; y < Crate.Size.Y; y++)
        {
            for (int x = 0; x < Crate.Size.X; x++)
            {
                Slot slot = new(new(itemStart.X + (Slot.SlotSize.X + 4) * x, itemStart.Y + (Slot.SlotSize.Y + 4) * y));
                CrateUI.AddElement($"slot_{x + y * Crate.Size.X}", slot);
            }
        }
    }
    private static void CreateDiscWriterUI(SpriteBatch batch)
    {
        // ----- Disc Writer -----
        DiscWriterUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("DISC WRITER").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "DISC WRITER", PixelOperatorLarge, Color.White);
        DiscWriterUI.AddElement("title", title);

        // Input disc
        InputSlot input = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75), new([ItemTypeID.Disc], FilterType.Whitelist));
        DiscWriterUI.AddElement("disc", input);

        // Rename input
        TextInput discName = new(new(Constants.Middle.X - 100, 160), new(200, 35), PixelOperator, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
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
                input.Item.CustomName = discName.Text.Replace(" ", "");
        };
        DiscWriterUI.AddElement("rename", rename);
    }
    private static void CreateDisplayCaseUI(SpriteBatch batch)
    {
        // ----- Display Case -----
        DisplayCaseUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("DISPLAY CASE").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "DISPLAY CASE", PixelOperatorLarge, Color.White);
        DisplayCaseUI.AddElement("title", title);

        // Display item
        Slot display = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75));
        DisplayCaseUI.AddElement("display", display);
    }
    private static void CreateFurnaceUI(SpriteBatch batch, LevelManager levelManager)
    {
        // ----- Jukebox -----
        FurnaceUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("FURNACE").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "FURNACE", PixelOperatorLarge, Color.White);
        FurnaceUI.AddElement("title", title);

        // Fuel input
        InputSlot fuel = new(new(Constants.Middle.X - 168, 75), new([ItemTypeID.Coal], FilterType.Whitelist));
        FurnaceUI.AddElement("fuel", fuel);

        // Unsmelted item input
        InputSlot input = new(new(Constants.Middle.X - 84, 75), new([ItemTypeID.RawCopper, ItemTypeID.RawGold, ItemTypeID.RawIron], FilterType.Whitelist));
        FurnaceUI.AddElement("input", input);

        // Smelted item output
        OutputSlot output = new(new(Constants.Middle.X + 20, 75));
        FurnaceUI.AddElement("output", output);

        // Cook button
        Button smelt = new(new(Constants.Middle.X - 75, 160), new(150, 35), "Smelt", PixelOperatorLarge, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        smelt.Clicked += () =>
        {
            if (input.Item == null || fuel.Item == null) return;

            // Convert to cooked
            Item? outputItem = RecipeRegistry.UseRecipe(input.Item, RecipeType.Furnace, fuel.Item);
            if (FurnaceUI.BoundContainer != null && outputItem != null)
            {
                Item? leftover = FurnaceUI.AddItem(outputItem);
                if (leftover != null && leftover.Amount > 0)
                    levelManager.Level.Loot.Add(new(leftover.GetItemRef(), CameraManager.PlayerFoot, GameManager.GameTime));
            }
        };
        FurnaceUI.AddElement("smelt", smelt);
    }
    private static void CreateInscriberUI(SpriteBatch batch)
    {
        // ----- Inscriber -----
        InscriberUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("INSCRIBER").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "INSCRIBER", PixelOperatorLarge, Color.White);
        InscriberUI.AddElement("title", title);

        // Input item
        InputSlot itemInput = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75), new([ItemTypeID.Disc], FilterType.Blacklist));
        InscriberUI.AddElement("item", itemInput);

        // Rename input
        TextInput itemName = new(new(Constants.Middle.X - 100, 160), new(200, 35), PixelOperator, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        InscriberUI.AddElement("itemName", itemName);
        itemInput.OnItemChange += () =>
        {
            if (itemInput.Item != null)
                itemName.SetText(StringTools.FillCamelSpaces(itemInput.Item.Name));
            else
                itemName.SetText("");
        };

        // Rename button
        Button itemRename = new(new(Constants.Middle.X - 50, 215), new(100, 35), "Rename", PixelOperator, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        itemRename.Clicked += () =>
        {
            if (itemInput.Item != null && itemName.Text != "")
                itemInput.Item.CustomName = itemName.Text.Replace(" ", "");
        };
        InscriberUI.AddElement("rename", itemRename);

        // Clear name button
        Button clear = new(new(Constants.Middle.X - 50, 270), new(100, 35), "Clear", PixelOperator, Color.Red, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        clear.Clicked += () =>
        {
            if (itemInput.Item != null)
                itemInput.Item.CustomName = null;
            itemName.SetText("");
        };
        InscriberUI.AddElement("clear", clear);
    }
    private static void CreateInventoryUI(SpriteBatch batch)
    {
        // ----- Player Inventory UI -----
        InventoryUI = new(batch);

        // Create slots
        Point itemStart = new(Constants.Middle.X - Slot.SlotSize.X * Chest.Size.X / 2, Constants.NativeResolution.Y - Slot.SlotSize.Y - 4);
        for (int y = 0; y < Chest.Size.Y + 1; y++)
        {
            for (int x = 0; x < Chest.Size.X; x++)
            {
                Slot slot = new(new(itemStart.X + (Slot.SlotSize.X + 4) * x, itemStart.Y - (Slot.SlotSize.Y + 4) * y));
                if (y == 0) slot.Tag("hotbar");
                InventoryUI.AddElement($"slot_{x + y * Chest.Size.X}", slot);
            }
        }
    }
    private static void CreateJukeboxUI(SpriteBatch batch)
    {
        // ----- Jukebox -----
        JukeboxUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("JUKEBOX").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "JUKEBOX", PixelOperatorLarge, Color.White);
        JukeboxUI.AddElement("title", title);

        // Track
        InputSlot input = new(new(Constants.Middle.X - Slot.SlotSize.X / 2, 75), new([ItemTypeID.Disc], FilterType.Whitelist));
        JukeboxUI.AddElement("disc", input);

        // Play button in the middle of the track slots - green text
        Button play = new(new(Constants.Middle.X - 50, 160), new(100, 35), "Play", PixelOperatorLarge, Color.White, Color.Green * 0.6f, Color.Green * 0.4f);
        play.Clicked += () =>
        {
            // Get the disc in the first slot
            if (input.Item != null)
                SoundtrackManager.PlaySoundtrack(input.Item.Name.Replace(" ", ""));
        };
        JukeboxUI.AddElement("play_button", play);
    }
    private static void CreateStoveUI(SpriteBatch batch, LevelManager levelManager)
    {
        // ----- Jukebox -----
        StoveUI = new(batch);

        // Title
        Point titleSize = PixelOperatorLarge.MeasureString("STOVE").ToPoint();
        Label title = new(new(Constants.Middle.X - titleSize.X / 2, 20), "STOVE", PixelOperatorLarge, Color.White);
        StoveUI.AddElement("title", title);

        // Fuel input
        InputSlot fuel = new(new(Constants.Middle.X - 168, 75), new([ItemTypeID.Coal], FilterType.Whitelist));
        StoveUI.AddElement("fuel", fuel);


        // Uncooked food input
        InputSlot input = new(new(Constants.Middle.X - 84, 75), new([ItemTypeID.RawBeef, ItemTypeID.RawFish], FilterType.Whitelist));
        StoveUI.AddElement("input", input);

        // Cooked food output
        OutputSlot output = new(new(Constants.Middle.X + 20, 75));
        StoveUI.AddElement("output", output);

        // Cook button
        Button cook = new(new(Constants.Middle.X - 75, 160), new(150, 35), "Cook", PixelOperatorLarge, Color.White, Color.Black * 0.6f, Color.Black * 0.4f, borderThickness: 0);
        cook.Clicked += () =>
        {
            if (input.Item == null || fuel.Item == null) return;

            // Convert to cooked
            Item? outputItem = RecipeRegistry.UseRecipe(input.Item, RecipeType.Stove, fuel.Item);
            if (StoveUI.BoundContainer != null && outputItem != null)
            {
                Item? leftover = StoveUI.AddItem(outputItem);
                if (leftover != null && leftover.Amount > 0)
                    levelManager.Level.Loot.Add(new(leftover.GetItemRef(), CameraManager.PlayerFoot, GameManager.GameTime));
            }
        };
        StoveUI.AddElement("cook", cook);
    }
}
