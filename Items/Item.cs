namespace Quest.Items;
public enum ItemTypeID : byte
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
    Rock,
    GlassBottle,
    BottledWater,
    BottledCloud,
    BottledStorm,
    // ITEMS
}

public class ItemType
{
    public ItemTypeID TypeID { get; protected set; }
    public string Name { get; protected set; } = "NUL_NAME";
    public string Description { get; protected set; } = "NUL_DESCR";
    public TextureID Texture { get; protected set; }
    public byte MaxAmount { get; protected set; }
    public ItemType(ItemTypeID typeID, string descr, int maxAmount = Constants.MaxStack, string? name = null)
    {
        TypeID = typeID;
        Name = name ?? typeID.ToString();
        Texture = (TextureID)Enum.Parse(typeof(TextureID), Name);
        MaxAmount = (byte)maxAmount;
    }
}

public class ItemTypes
{
    public static readonly ItemType ActivePalantir = new(ItemTypeID.ActivePalantir, "A seeing stone used to communicate with sauron.", 1);
    public static readonly ItemType Apple = new(ItemTypeID.Apple, "A nutritious red apple.");
    public static readonly ItemType BottledCloud = new(ItemTypeID.BottledCloud, "A cloud somehow trapped in a glass bottle...", 3);
    public static readonly ItemType BottledStorm = new(ItemTypeID.BottledStorm, "A storm somehow trapped in a glass bottle...", 3);
    public static readonly ItemType BottledWater = new(ItemTypeID.BottledWater, "A glass bottle of potable water.", 3);
    public static readonly ItemType Bread = new(ItemTypeID.Bread, "A freshly baked loaf of bread.");
    public static readonly ItemType Cheese = new(ItemTypeID.Cheese, "A wedge of Swiss cheese.");
    public static readonly ItemType Cherries = new(ItemTypeID.Cherries, "Juicy red cherries.");
    public static readonly ItemType Chicken = new(ItemTypeID.Chicken, "Chicken meat.");
    public static readonly ItemType DeltaCoin = new(ItemTypeID.DeltaCoin, "A gold coin.");
    public static readonly ItemType DiamondKey = new(ItemTypeID.DiamondKey, "A fancy diamond key.", 1);
    public static readonly ItemType DiamondSword = new(ItemTypeID.DiamondSword, "A razor sharp sword made with pure diamonds.", 1);
    public static readonly ItemType EmeraldKey = new(ItemTypeID.EmeraldKey, "A fancy emerald key.", 1);
    public static readonly ItemType GammaCoin = new(ItemTypeID.GammaCoin, "A diamond coin.");
    public static readonly ItemType GlassBottle = new(ItemTypeID.GlassBottle, "An empty bottle made of glass.", 3);
    public static readonly ItemType GoldKey = new(ItemTypeID.GoldKey, "A fancy golden key.", 1);
    public static readonly ItemType InactivePalantir = new(ItemTypeID.InactivePalantir, "A seeing stone used to communicate with sauron.", 1);
    public static readonly ItemType IronKey = new(ItemTypeID.IronKey, "A simple iron key.", 1);
    public static readonly ItemType Lantern = new(ItemTypeID.Lantern, "A burning lantern used for light.", 1);
    public static readonly ItemType MagicKey = new(ItemTypeID.MagicKey, "A magical key.", 1);
    public static readonly ItemType Orange = new(ItemTypeID.Orange, "A fresh juicy orange.");
    public static readonly ItemType PhiCoin = new(ItemTypeID.PhiCoin, "A bronze coin.");
    public static readonly ItemType Pickaxe = new(ItemTypeID.Pickaxe, "A sturdy metal pickaxe used for mining.", 1);
    public static readonly ItemType Potato = new(ItemTypeID.Potato, "An earthy potato.");
    public static readonly ItemType Rock = new(ItemTypeID.Rock, "Hard rock mined from the ground.");
    public static readonly ItemType RubyKey = new(ItemTypeID.RubyKey, "A fancy ruby key.", 1);
    public static readonly ItemType Skull = new(ItemTypeID.Skull, "Why are you holding this?");
    public static readonly ItemType SteelSword = new(ItemTypeID.SteelSword, "A sturdy steel sword.", 1);
    public static readonly ItemType WoodKey = new(ItemTypeID.WoodKey, "A simple wooden key.", 1);
    public static readonly ItemType WoodPlanks = new(ItemTypeID.WoodPlanks, "Sturdy wooden boards cut from trees.");
    // ITEMS REGISTER
}

public class Item
{
    public string Name => Type.Name;
    public string Description => Type.Description;
    public byte Amount { get; set; }
    public byte MaxAmount => Type.MaxAmount;
    public TextureID Texture => Type.Texture;
    public ItemType Type { get; protected set; }
    public ushort UID { get; protected set; }
    public Item(ItemType itemType, int amount)
    {
        Type = itemType;
        Amount = (byte)amount;
        UID = UIDManager.Get(UIDCategory.Items);
    }
    public virtual void PrimaryUse(PlayerManager player) { }
    public virtual void SecondaryUse(PlayerManager player) { }

    public static Item ItemFromName(string name, int amount)
    {
        string fullTypeName = $"Quest.Items.{name}";
        var type = System.Type.GetType(fullTypeName);

        if (type == null || !typeof(Item).IsAssignableFrom(type))
        {
            Logger.Error($"Invalid ItemFromName name '{name}'");
            return new Item(ItemTypes.Skull, 0);
        }

        var created = (Item?)Activator.CreateInstance(type, amount);
        return created ?? throw new InvalidOperationException($"Failed to create item '{name}'");
    }
    public static Item ItemFromItemType(ItemTypeID itemType, int amount)
    {
        return ItemFromName(itemType.ToString(), amount);
    }
    public Item ShallowCopy()
    {
        return (Item)MemberwiseClone();
    }
    public void Dispose()
    {
        UIDManager.Release(UIDCategory.Items, UID);
    }
}
