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
    Disc,
    Cloth,
    Coal,
    RawIron,
    Iron,
    Ink,
    RawCopper,
    Copper,
    RawGold,
    Gold,
    Diamond,
    Emerald,
    Ruby,
    CopperMedal,
    IronMedal,
    GoldMedal,
    DiamondMedal,
    EmeraldMedal,
    RubyMedal,
    HeartRune,
    LightningStaff,
    TimeStaff,
    Scroll,
    Carrot,
    RawFish,
    CookedFish,
    RawBeef,
    CookedBeef,
    Crossbow,
    HealthPotion,
    // ITEMS ENUM
}

public class ItemType
{
    public ItemTypeID TypeID { get; protected set; }
    public string Name { get; protected set; } = "NUL_NAME";
    public string Description { get; protected set; } = "NUL_DESCR";
    public TextureID Texture { get; protected set; }
    public byte MaxAmount { get; protected set; }
    public bool IsLight { get; protected set; }
    public ItemType(ItemTypeID typeID, string descr, int maxAmount = Constants.MaxStack, bool isLight = false, string? name = null)
    {
        TypeID = typeID;
        Name = name ?? typeID.ToString();
        Texture = (TextureID)Enum.Parse(typeof(TextureID), Name);
        Description = descr;
        MaxAmount = (byte)maxAmount;
        IsLight = isLight;
    }
}

public class ItemTypes
{
    public static ItemType Get(ItemTypeID typeID) => All[(byte)typeID];
    public static ItemType Get(string typeID)
    {
        if (!Enum.TryParse<ItemTypeID>(typeID, out var parsedTypeID))
            throw new ArgumentException($"ItemType {typeID} does not exist");
        return All[(byte)Enum.Parse(typeof(ItemTypeID), typeID)];
    }
    public static readonly ItemType ActivePalantir = new(ItemTypeID.ActivePalantir, "A seeing stone used to communicate with sauron.", 1);
    public static readonly ItemType SteelSword = new(ItemTypeID.SteelSword, "A sturdy steel sword.", 1);
    public static readonly ItemType DeltaCoin = new(ItemTypeID.DeltaCoin, "A gold coin.");
    public static readonly ItemType DiamondSword = new(ItemTypeID.DiamondSword, "A razor sharp sword made with pure diamonds.", 1);
    public static readonly ItemType GammaCoin = new(ItemTypeID.GammaCoin, "A diamond coin.");
    public static readonly ItemType InactivePalantir = new(ItemTypeID.InactivePalantir, "A seeing stone used to communicate with sauron.", 1);
    public static readonly ItemType GoldKey = new(ItemTypeID.GoldKey, "A fancy golden key.", 1);
    public static readonly ItemType PhiCoin = new(ItemTypeID.PhiCoin, "A bronze coin.");
    public static readonly ItemType Pickaxe = new(ItemTypeID.Pickaxe, "A sturdy metal pickaxe used for mining.", 1);
    public static readonly ItemType WoodKey = new(ItemTypeID.WoodKey, "A simple wooden key.", 1);
    public static readonly ItemType IronKey = new(ItemTypeID.IronKey, "A simple iron key.", 1);
    public static readonly ItemType DiamondKey = new(ItemTypeID.DiamondKey, "A fancy diamond key.", 1);
    public static readonly ItemType EmeraldKey = new(ItemTypeID.EmeraldKey, "A fancy emerald key.", 1);
    public static readonly ItemType RubyKey = new(ItemTypeID.RubyKey, "A fancy ruby key.", 1);
    public static readonly ItemType MagicKey = new(ItemTypeID.MagicKey, "A magical key.", 1);
    public static readonly ItemType Apple = new(ItemTypeID.Apple, "A nutritious red apple.");
    public static readonly ItemType Bread = new(ItemTypeID.Bread, "A freshly baked loaf of bread.");
    public static readonly ItemType Skull = new(ItemTypeID.Skull, "Why are you holding this?");
    public static readonly ItemType Cherries = new(ItemTypeID.Cherries, "Juicy red cherries.");
    public static readonly ItemType Cheese = new(ItemTypeID.Cheese, "A wedge of Swiss cheese.");
    public static readonly ItemType Chicken = new(ItemTypeID.Chicken, "Chicken meat.");
    public static readonly ItemType Potato = new(ItemTypeID.Potato, "An earthy potato.");
    public static readonly ItemType Orange = new(ItemTypeID.Orange, "A fresh juicy orange.");
    public static readonly ItemType Lantern = new(ItemTypeID.Lantern, "A burning lantern used for light.", 1, isLight: true);
    public static readonly ItemType WoodPlanks = new(ItemTypeID.WoodPlanks, "Sturdy wooden boards cut from trees.");
    public static readonly ItemType Rock = new(ItemTypeID.Rock, "Hard rock mined from the ground.");
    public static readonly ItemType GlassBottle = new(ItemTypeID.GlassBottle, "An empty bottle made of glass.", 3);
    public static readonly ItemType BottledWater = new(ItemTypeID.BottledWater, "A glass bottle of potable water.", 3);
    public static readonly ItemType BottledCloud = new(ItemTypeID.BottledCloud, "A cloud somehow trapped in a glass bottle...", 3);
    public static readonly ItemType BottledStorm = new(ItemTypeID.BottledStorm, "A storm somehow trapped in a glass bottle...", 3);
    public static readonly ItemType Disc = new(ItemTypeID.Disc, "A music disc.");
    public static readonly ItemType Cloth = new(ItemTypeID.Cloth, "A piece of fabric.");
    public static readonly ItemType Coal = new(ItemTypeID.Coal, "A hard lump of coal.");
    public static readonly ItemType RawIron = new(ItemTypeID.RawIron, "Unprocessed iron ore.");
    public static readonly ItemType Iron = new(ItemTypeID.Iron, "Processed iron ore.");
    public static readonly ItemType Ink = new(ItemTypeID.Ink, "Ink used for dyes and writing.");
    public static readonly ItemType RawCopper = new(ItemTypeID.RawCopper, "Unprocessed copper ore.");
    public static readonly ItemType Copper = new(ItemTypeID.Copper, "Processed copper ore.");
    public static readonly ItemType RawGold = new(ItemTypeID.RawGold, "Unprocessed gold ore.");
    public static readonly ItemType Gold = new(ItemTypeID.Gold, "Processed gold ore.");
    public static readonly ItemType Diamond = new(ItemTypeID.Diamond, "Pure shiny diamond.");
    public static readonly ItemType Emerald = new(ItemTypeID.Emerald, "Pure shiny emerald.");
    public static readonly ItemType Ruby = new(ItemTypeID.Ruby, "Pure shiny ruby.");
    public static readonly ItemType CopperMedal = new(ItemTypeID.CopperMedal, "Award medal made of copper.");
    public static readonly ItemType IronMedal = new(ItemTypeID.IronMedal, "Award medal made of iron.");
    public static readonly ItemType GoldMedal = new(ItemTypeID.GoldMedal, "Award medal made of gold.");
    public static readonly ItemType DiamondMedal = new(ItemTypeID.DiamondMedal, "Award medal made of diamond.");
    public static readonly ItemType EmeraldMedal = new(ItemTypeID.EmeraldMedal, "Award medal made of emerald.");
    public static readonly ItemType RubyMedal = new(ItemTypeID.RubyMedal, "Award medal made of ruby.");
    public static readonly ItemType HeartRune = new(ItemTypeID.HeartRune, "A mysterious rune in the shape of a heart.", 1);
    public static readonly ItemType LightningStaff = new(ItemTypeID.LightningStaff, "A magical staff infused with lightning.", 1);
    public static readonly ItemType TimeStaff = new(ItemTypeID.TimeStaff, "A magical staff able to control time.", 1);
    public static readonly ItemType Scroll = new(ItemTypeID.Scroll, "An antique scroll with writings on it.", 1);
    public static readonly ItemType Carrot = new(ItemTypeID.Carrot, "A hearty carrot from the ground.");
    public static readonly ItemType RawFish = new(ItemTypeID.RawFish, "Uncooked fish from the sea.");
    public static readonly ItemType CookedFish = new(ItemTypeID.CookedFish, "Cooked fish from the sea.");
    public static readonly ItemType RawBeef = new(ItemTypeID.RawBeef, "Uncooked cow meat.");
    public static readonly ItemType CookedBeef = new(ItemTypeID.CookedBeef, "Cooked cow meat.");
    public static readonly ItemType Crossbow = new(ItemTypeID.Crossbow, "A wooden crossbow capable of shooting arrows.", 1);
    public static readonly ItemType HealthPotion = new(ItemTypeID.HealthPotion, "A drink used to instantly heal.", 1);
    // ITEMS REGISTER
    public static readonly ItemType[] All = [
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
    Disc,
    Cloth,
    Coal,
    RawIron,
    Iron,
    Ink,
    RawCopper,
    Copper,
    RawGold,
    Gold,
    Diamond,
    Emerald,
    Ruby,
    CopperMedal,
    IronMedal,
    GoldMedal,
    DiamondMedal,
    EmeraldMedal,
    RubyMedal,
    HeartRune,
    LightningStaff,
    TimeStaff,
    Scroll,
    Carrot,
    RawFish,
    CookedFish,
    RawBeef,
    CookedBeef,
    Crossbow,
    HealthPotion,
    // ITEMS ENUM
    ];
}
public class ItemRef
{
    public byte Amount { get; set; }
    public ItemType Type { get; }
    public string? CustomName { get; set; }
    public string Name => CustomName ?? Type.Name;
    public string Description => Type.Description;
    public byte MaxAmount => Type.MaxAmount;
    public TextureID Texture => Type.Texture;
    public bool IsLight => Type.IsLight;
    public ItemRef(ItemType type, byte amount, string? name = null)
    {
        Type = type;
        Amount = amount;
        CustomName = name;
    }
    public ItemRef Copy() => new(Type, Amount, CustomName);
}
public class Item
{
    public byte Amount { get; set; }
    public ItemType Type { get; protected set; }
    public ushort UID { get; protected set; }
    public string? CustomName { get; set; }
    public string Name => CustomName ?? Type.Name;
    public string Description => Type.Description;
    public byte MaxAmount => Type.MaxAmount;
    public TextureID Texture => Type.Texture;
    public bool IsLight => Type.IsLight;
    public Item(ItemType itemType, int amount, string? name = null)
    {
        Type = itemType;
        Amount = (byte)amount;
        UID = UIDManager.Get(UIDCategory.Items);
        CustomName = name;
    }
    public Item(ItemTypeID itemTypeID, int amount, string? name = null)
    {
        Type = ItemTypes.All[(int)itemTypeID];
        Amount = (byte)amount;
        UID = UIDManager.Get(UIDCategory.Items);
        CustomName = name;
    }
    public Item(ItemRef itemRef, string? name = null)
    {
        Type = itemRef.Type;
        Amount = itemRef.Amount;
        UID = UIDManager.Get(UIDCategory.Items);
        CustomName = name;
    }
    public Item(Item item)
    {
        Type = item.Type;
        Amount = item.Amount;
        UID = UIDManager.Get(UIDCategory.Items);
        CustomName = item.CustomName;
    }
    public virtual void PrimaryUse(GameManager gameManager, PlayerManager player) { }
    public virtual void SecondaryUse(GameManager gameManager, PlayerManager player) { }
    public static Item Create(ItemType type, byte amount, string? customName = null)
    {
        return Create(type.TypeID, amount, customName);
    }

    public static Item Create(ItemTypeID itemType, byte amount, string? customName = null)
    {
        // Error check
        if ((byte)itemType >= ItemTypes.All.Length)
            Logger.Error($"Item Create failed - ItemTypeID {(byte)itemType} does not exist", exit: true);

        // Create
        ItemType type = ItemTypes.All[(byte)itemType];
        return itemType switch
        {
            ItemTypeID.Lantern => new Lantern(amount, customName),
            ItemTypeID.SteelSword => new SteelSword(amount, customName),
            ItemTypeID.DiamondSword => new DiamondSword(amount, customName),
            ItemTypeID.Crossbow => new Crossbow(amount, customName),
            ItemTypeID.HealthPotion => new HealthPotion(amount, customName),
            _ => new Item(type, amount, customName),
        };
    }
    public Item ShallowCopy()
    {
        return (Item)MemberwiseClone();
    }
    public void Dispose()
    {
        UIDManager.Release(UIDCategory.Items, UID);
    }
    public Item? Take(byte amount)
    {
        // Failed
        if (amount > Amount)
            return null;
        else if (amount <= 0)
            return null;

        // Split
        Amount -= amount;
        return new Item(Type, amount, CustomName);
    }
    public bool Consume(byte amount) => Take(amount) != null;
    private string Tags()
    {
        string tags = "";
        if (this is Light) tags += "L";
        return tags;
    }
    public ItemRef GetItemRef() => new(Type, Amount, CustomName);
    public override string ToString() => $"{Name}{(CustomName != null ? $" [{CustomName}]" : "")} x{Amount} {Tags()}";
}
