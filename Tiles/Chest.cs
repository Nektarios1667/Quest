using Quest.Editor;
using Quest.Editor.Managers;
using Quest.Interaction;
using System.IO;

namespace Quest.Tiles;

public class Chest : Tile, IContainer, IEditableTile, IHasLevelData
{
    public readonly static Point Size = new(6, 3);
    public ILootGenerator LootGenerator { get; private set; }
    public Container Container { get; private set; } = null!;
    public bool Generated { get; private set; } = false;
    public int Seed { get; private set; } = Random.Shared.Next();
    public ItemRef? Key { get; set; }
    public bool ConsumeKey { get; set; }
    public Chest(Point location, ILootGenerator lootGenerator, string levelPath, ItemRef? key = null, bool consumeKey = true) : base(location, TileTypeID.Chest)
    {
        LootGenerator = lootGenerator;
        SaveManager.SaveChestGenerator(this, levelPath);
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void OnPlayerCollide(GameManager gameManager, PlayerManager player)
    {
        // Unlocked
        if (Generated)
        {
            Open(gameManager, player);
            return;
        }

        // Locked
        // Has key
        if (Key == null || player.Inventory.Count(Key.Type) >= Key.Amount)
        {
            if (Key != null && ConsumeKey)
            {
                gameManager.OverlayManager.Notification($"-{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Red, 3);
                player.Inventory.Consume(Key, ignoreCheck: true);
            }
            else if (Key != null)
                gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Gray, 2);

            SoundManager.PlaySoundInstance("ChestUnlock");
            Open(gameManager, player);
        }
        // No key
        else
        {
            // Notif
            gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)} needed to unlock", Color.Red, 5);

            // Sound fx
            string timerName = $"ChestLocked_{X + Y * Constants.MapSize.X}";
            if (TimerManager.IsCompleteOrMissing(timerName))
            {
                SoundManager.PlaySoundInstance("ChestLocked");
                TimerManager.SetTimer(timerName, 5, null);
            }
        }

    }
    private void Open(GameManager gameManager, PlayerManager player)
    {
        TryGenerateLoot();
        UserInterface.ChestUI.BindContainer(Container);
        player.OpenInterface(gameManager, UserInterface.ChestUI);
        gameManager.StateManager.OverlayState = OverlayState.Container;
    }
    public void RegenerateLoot(ILootGenerator lootGenerator)
    {
        LootGenerator = lootGenerator;
        Container = new(lootGenerator.Generate(Size.X * Size.Y, Seed));
        Generated = true;
    }
    public void TryGenerateLoot()
    {
        if (Generated) return;
        Container = new(LootGenerator.Generate(Size.X * Size.Y, Seed));
        Generated = true;
    }
    public void SetEmpty()
    {
        Generated = true;
        Container = new(new Item?[Chest.Size.X * Chest.Size.Y]);
    }
    public void SetSeed(int seed) => Seed = seed;
    public void Edit(EditorManager editorManager)
    {
        string genFile = LootGenerator.FileName;
        genFile = genFile.Contains('.') ? genFile : "NUL.qlt";

        var (success, values) = PopupFactory.ShowInputForm("Chest Editor", [
            new("Loot File Name", null, placeholder: genFile.Split('.')[0]),
                new("Loot Type", null, ["Loot Preset", "Loot Table"], genFile.Split('.')[1] == "qlt" ? "Loot Table" : "Loot Preset"),
                new("Key", null, EditorManager.ItemsOptionsWNone, Key == null ? "NONE" : Key.Name),
                new("Amount", PopupFactory.IsByte, placeholder: Key == null ? "0" : Key.Amount),
                new("Consume Key", null, ["True", "False"], ConsumeKey)]);
        if (!success)
        {
            if (!PopupFactory.PopupOpen) Logger.Error("Chest edit failed.");
            return;
        }
        // Loot
        if (values[1] == "Loot Table")
            RegenerateLoot(LootTable.ReadLootTable(editorManager.CurrentLevel.WorldName, $"{values[0]}.qlt"));
        else if (values[1] == "Loot Preset")
            RegenerateLoot(LootPreset.ReadLootPreset(editorManager.CurrentLevel.WorldName, $"{values[0]}.qlp"));
        else
            Logger.Error("Chest loot edit failed.");
        // Key
        Key = values[2].Equals("none", StringComparison.CurrentCultureIgnoreCase) ? null : new(ItemTypes.All[(byte)Enum.Parse(typeof(ItemTypeID), values[2])], byte.Parse(values[3]));
        if (Key?.Amount <= 0)
            Key = null;
        ConsumeKey = bool.Parse(values[4]);
    }
    public void WriteLevelData(BinaryWriter writer)
    {
        writer.Write(LootGenerator);
        SaveManager.WriteItemData(writer, Key);
        writer.Write(ConsumeKey);
    }
    public void ReadLevelData(BinaryReader reader, LevelPath levelPath)
    {
        // Loot gen
        string lootGenFile = reader.ReadString();
        ILootGenerator lootGen = LootGeneratorHelper.Read(levelPath.WorldName, lootGenFile);
        lootGen = (lootGen.FileName.IsNUL() || lootGen.FileName == "_") ? LootPreset.EmptyPreset : lootGen;
        LootGenerator = lootGen;

        // Key
        Key = SaveManager.ReadItemData(reader)?.GetItemRef();
        ConsumeKey = reader.ReadBoolean();
    }
}
