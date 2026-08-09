namespace Quest.Items;

public enum ItemTypeID : byte
{
    ActiveOrb,
    IronSword,
    DeltaCoin,
    DiamondSword,
    GammaCoin,
    InactiveOrb,
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
    Arrow,
    HealthPotion,
    SpeedPotion,
    SlownessPotion,
    RegenerationPotion,
    PoisonPotion,
    StrengthPotion,
    WeaknessPotion,
    ProtectionPotion,
    VulnerabilityPotion,
    DeleriumPotion,
    LifestealPotion,
    IronSpear,
    DiamondSpear,
    IronAxe,
    DiamondAxe,
    NinjaStar,
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
    // Premade lists
    public static readonly ItemTypeID[] FuelTypes = [ItemTypeID.Coal];
    public static readonly ItemTypeID[] FurnaceableTypes = [ItemTypeID.RawCopper, ItemTypeID.RawGold, ItemTypeID.RawIron];
    public static readonly ItemTypeID[] StoveableTypes = [ItemTypeID.RawBeef, ItemTypeID.RawFish];
    // ItemTypes
    public static readonly ItemType ActiveOrb = new(ItemTypeID.ActiveOrb, "Magical orb.", 1);
    public static readonly ItemType IronSword = new(ItemTypeID.IronSword, "Sturdy steel sword.", 1);
    public static readonly ItemType DeltaCoin = new(ItemTypeID.DeltaCoin, "Gold coin.");
    public static readonly ItemType DiamondSword = new(ItemTypeID.DiamondSword, "Razor sharp sword made with pure diamonds.", 1);
    public static readonly ItemType GammaCoin = new(ItemTypeID.GammaCoin, "Diamond coin.");
    public static readonly ItemType InactiveOrb = new(ItemTypeID.InactiveOrb, "Magical orb.", 1);
    public static readonly ItemType GoldKey = new(ItemTypeID.GoldKey, "Fancy golden key.", 1);
    public static readonly ItemType PhiCoin = new(ItemTypeID.PhiCoin, "Bronze coin.");
    public static readonly ItemType WoodKey = new(ItemTypeID.WoodKey, "Simple wooden key.", 1);
    public static readonly ItemType IronKey = new(ItemTypeID.IronKey, "Simple iron key.", 1);
    public static readonly ItemType DiamondKey = new(ItemTypeID.DiamondKey, "Fancy diamond key.", 1);
    public static readonly ItemType EmeraldKey = new(ItemTypeID.EmeraldKey, "Fancy emerald key.", 1);
    public static readonly ItemType RubyKey = new(ItemTypeID.RubyKey, "Fancy ruby key.", 1);
    public static readonly ItemType MagicKey = new(ItemTypeID.MagicKey, "Magical key.", 1);
    public static readonly ItemType Apple = new(ItemTypeID.Apple, "Nutritious red apple.");
    public static readonly ItemType Bread = new(ItemTypeID.Bread, "Freshly baked loaf of bread.");
    public static readonly ItemType Skull = new(ItemTypeID.Skull, "Why are you holding this?");
    public static readonly ItemType Cherries = new(ItemTypeID.Cherries, "Juicy red cherries.");
    public static readonly ItemType Cheese = new(ItemTypeID.Cheese, "Wedge of Swiss cheese.");
    public static readonly ItemType Chicken = new(ItemTypeID.Chicken, "Chicken meat.");
    public static readonly ItemType Potato = new(ItemTypeID.Potato, "Earthy potato.");
    public static readonly ItemType Orange = new(ItemTypeID.Orange, "Fresh juicy orange.");
    public static readonly ItemType Lantern = new(ItemTypeID.Lantern, "Burning lantern used for light.", 1, isLight: true);
    public static readonly ItemType WoodPlanks = new(ItemTypeID.WoodPlanks, "Sturdy wooden boards cut from trees.");
    public static readonly ItemType Rock = new(ItemTypeID.Rock, "Hard rock taken from the ground.");
    public static readonly ItemType GlassBottle = new(ItemTypeID.GlassBottle, "Empty bottle made of glass.", 3);
    public static readonly ItemType BottledWater = new(ItemTypeID.BottledWater, "Glass bottle of potable water.", 3);
    public static readonly ItemType BottledCloud = new(ItemTypeID.BottledCloud, "Cloud somehow trapped in a glass bottle...", 3);
    public static readonly ItemType BottledStorm = new(ItemTypeID.BottledStorm, "Storm somehow trapped in a glass bottle...", 3);
    public static readonly ItemType Disc = new(ItemTypeID.Disc, "Music disc.");
    public static readonly ItemType Cloth = new(ItemTypeID.Cloth, "Piece of fabric.");
    public static readonly ItemType Coal = new(ItemTypeID.Coal, "Hard lump of coal.");
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
    public static readonly ItemType HeartRune = new(ItemTypeID.HeartRune, "Mysterious rune in the shape of a heart.", 1);
    public static readonly ItemType LightningStaff = new(ItemTypeID.LightningStaff, "Magical staff infused with lightning.", 1);
    public static readonly ItemType TimeStaff = new(ItemTypeID.TimeStaff, "Magical staff able to control time.", 1);
    public static readonly ItemType Scroll = new(ItemTypeID.Scroll, "Antique scroll with writings on it.", 1);
    public static readonly ItemType Carrot = new(ItemTypeID.Carrot, "Hearty carrot from the ground.");
    public static readonly ItemType RawFish = new(ItemTypeID.RawFish, "Uncooked fish from the sea.");
    public static readonly ItemType CookedFish = new(ItemTypeID.CookedFish, "Cooked fish from the sea.");
    public static readonly ItemType RawBeef = new(ItemTypeID.RawBeef, "Uncooked cow meat.");
    public static readonly ItemType CookedBeef = new(ItemTypeID.CookedBeef, "Cooked cow meat.");
    public static readonly ItemType Crossbow = new(ItemTypeID.Crossbow, "Wooden crossbow capable of shooting arrows.", 1);
    public static readonly ItemType HealthPotion = new(ItemTypeID.HealthPotion, "Drink used to instantly heal.", 1);
    public static readonly ItemType Arrow = new(ItemTypeID.Arrow, "Used with a bow to shoot enemies.");
    public static readonly ItemType SpeedPotion = new(ItemTypeID.SpeedPotion, "Special brew that temporarily boosts walk speed.", 1);
    public static readonly ItemType SlownessPotion = new(ItemTypeID.SlownessPotion, "Special brew that temporarily slows walk speed.", 1);
    public static readonly ItemType RegenerationPotion = new(ItemTypeID.RegenerationPotion, "Special brew that gives a heal over time effect.", 1);
    public static readonly ItemType PoisonPotion = new(ItemTypeID.PoisonPotion, "Special brew that deals damage over time.", 1);
    public static readonly ItemType StrengthPotion = new(ItemTypeID.StrengthPotion, "Special brew that temporarily boosts player damage.", 1);
    public static readonly ItemType WeaknessPotion = new(ItemTypeID.WeaknessPotion, "Special brew that temporarily decreases player damage.", 1);
    public static readonly ItemType ProtectionPotion = new(ItemTypeID.ProtectionPotion, "Special brew that temporarily decreases damage taken by the player.", 1);
    public static readonly ItemType VulnerabilityPotion = new(ItemTypeID.VulnerabilityPotion, "Special brew that temporarily increases damage taken by the player.", 1);
    public static readonly ItemType DeleriumPotion = new(ItemTypeID.DeleriumPotion, "Special brew that temporarily causes delerious vision.", 1);
    public static readonly ItemType LifestealPotion = new(ItemTypeID.LifestealPotion, "Special brew that temporarily causes the player to steal health from enemies.", 1);
    public static readonly ItemType IronSpear = new(ItemTypeID.IronSpear, "Long ranged weapon made of iron and wood.", 1);
    public static readonly ItemType DiamondSpear = new(ItemTypeID.DiamondSpear, "Long ranged weapon made of diamond and wood.", 1);
    public static readonly ItemType IronAxe = new(ItemTypeID.IronAxe, "Iron weapon with short range but high damage.", 1);
    public static readonly ItemType DiamondAxe = new(ItemTypeID.DiamondAxe, "Diamond weapon with short range but high damage.", 1);
    public static readonly ItemType NinjaStar = new(ItemTypeID.NinjaStar, "Sharpened blades that can be thrown.");
    // ITEMS REGISTER
    public static readonly ItemType[] All = [
    ActiveOrb,
    IronSword,
    DeltaCoin,
    DiamondSword,
    GammaCoin,
    InactiveOrb,
    GoldKey,
    PhiCoin,
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
    Arrow,
    HealthPotion,
    SpeedPotion,
    SlownessPotion,
    RegenerationPotion,
    PoisonPotion,
    StrengthPotion,
    WeaknessPotion,
    ProtectionPotion,
    VulnerabilityPotion,
    DeleriumPotion,
    LifestealPotion,
    IronSpear,
    DiamondSpear,
    IronAxe,
    DiamondAxe,
    NinjaStar,
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
    public virtual bool PrimaryUse(GameManager gameManager, PlayerManager player) => false;
    public virtual bool SecondaryUse(GameManager gameManager, PlayerManager player) => false;
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
            ItemTypeID.IronSword => new IronSword(amount, customName),
            ItemTypeID.DiamondSword => new DiamondSword(amount, customName),
            ItemTypeID.Crossbow => new Crossbow(amount, customName),
            ItemTypeID.HealthPotion => new HealthPotion(amount, customName),
            ItemTypeID.DeleriumPotion => new DeleriumPotion(amount, customName),
            ItemTypeID.LifestealPotion => new LifestealPotion(amount, customName),
            ItemTypeID.PoisonPotion => new PoisonPotion(amount, customName),
            ItemTypeID.ProtectionPotion => new ProtectionPotion(amount, customName),
            ItemTypeID.RegenerationPotion => new RegenerationPotion(amount, customName),
            ItemTypeID.SlownessPotion => new SlownessPotion(amount, customName),
            ItemTypeID.SpeedPotion => new SpeedPotion(amount, customName),
            ItemTypeID.StrengthPotion => new StrengthPotion(amount, customName),
            ItemTypeID.VulnerabilityPotion => new VulnerabilityPotion(amount, customName),
            ItemTypeID.WeaknessPotion => new WeaknessPotion(amount, customName),
            ItemTypeID.DiamondAxe => new DiamondAxe(amount, customName),
            ItemTypeID.DiamondSpear => new DiamondSpear(amount, customName),
            ItemTypeID.IronAxe => new IronAxe(amount, customName),
            ItemTypeID.IronSpear => new IronSpear(amount, customName),
            ItemTypeID.NinjaStar => new NinjaStar(amount, customName),
            // ITEMFROMID
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
