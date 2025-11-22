namespace Quest.Items;
public enum ItemType : byte
{
    ActivePalantir,
    SteelSword,
    DeltaCoin,
    DiamondSword,
    GammaCoin,
    InactivePalantir,
    GoldKey,
    PhiCoin,
    Pickaxe,
    WoodKey,
    IronKey,
    DiamondKey,
    EmeraldKey,
    RubyKey,
    MagicKey,
    Apple,
    Bread,
    Skull,
    Cherries,
    Cheese,
    Chicken,
    Potato,
    Orange,
    Lantern,
    WoodPlanks,
    Stone,
    GlassBottle,
    BottledWater,
    BottledCloud,
    BottledStorm,
    // ITEMS
}

public class Item
{
    public string Name { get; protected set; } = "NUL_ITEM";
    public string Description { get; protected set; } = "NUL_DESCR";
    public byte Amount { get; set; }
    public byte MaxAmount { get; protected set; }
    public TextureID Texture { get; protected set; }
    public int UID { get; protected set; }
    public Item(int amount)
    {
        Amount = (byte)amount;
        MaxAmount = Constants.MaxStack;
        Name = GetType().Name;
        Texture = (TextureID)Enum.Parse(typeof(TextureID), GetType().Name);
        UID = UIDManager.NewUID("Items");
    }
    public virtual void PrimaryUse(PlayerManager player) { }
    public virtual void SecondaryUse(PlayerManager player) { }

    public static Item ItemFromName(string name, int amount)
    {
        string fullTypeName = $"Quest.Items.{name}";
        var type = Type.GetType(fullTypeName);

        if (type == null || !typeof(Item).IsAssignableFrom(type))
        {
            Logger.Error($"Invalid ItemFromName name '{name}'");
            return new Item(0);
        }

        var created = (Item?)Activator.CreateInstance(type, amount);
        return created ?? throw new InvalidOperationException($"Failed to create item '{name}'");
    }
    public static Item ItemFromItemType(ItemType itemType, int amount)
    {
        return ItemFromName(itemType.ToString(), amount);
    }
    public Item ShallowCopy()
    {
        return (Item)MemberwiseClone();
    }
}
