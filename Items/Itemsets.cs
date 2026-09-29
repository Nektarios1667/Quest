using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Items;

public enum ItemsetTypes
{
    Tool,
    Weapon,
    Collectable,
    Sustenance,
    Material,
}
public static class Itemsets
{
    public static readonly ItemTypeID[] Tools = [
        ItemTypeID.Lantern,
        ItemTypeID.Clock,
        ItemTypeID.Compass,
        ItemTypeID.Map,
    ];
    public static readonly ItemTypeID[] Weapons = [
        ItemTypeID.IronSword,
        ItemTypeID.DiamondSword,
        ItemTypeID.IronSpear,
        ItemTypeID.DiamondSpear,
        ItemTypeID.IronAxe,
        ItemTypeID.DiamondAxe,
        ItemTypeID.Bow,
        ItemTypeID.Arrow,
        ItemTypeID.Crossbow,
        ItemTypeID.Slingshot,
        ItemTypeID.NinjaStar,
        ItemTypeID.FireStaff,
        ItemTypeID.LightningStaff,
    ];
    public static readonly ItemTypeID[] Collectables = [
        ItemTypeID.ActiveOrb,
        ItemTypeID.InactiveOrb,
        ItemTypeID.PhiCoin,
        ItemTypeID.DeltaCoin,
        ItemTypeID.GammaCoin,
        ItemTypeID.WoodKey,
        ItemTypeID.IronKey,
        ItemTypeID.GoldKey,
        ItemTypeID.DiamondKey,
        ItemTypeID.EmeraldKey,
        ItemTypeID.RubyKey,
        ItemTypeID.MagicKey,
        ItemTypeID.CopperMedal,
        ItemTypeID.IronMedal,
        ItemTypeID.GoldMedal,
        ItemTypeID.DiamondMedal,
        ItemTypeID.EmeraldMedal,
        ItemTypeID.RubyMedal,
        ItemTypeID.HeartRune,
        ItemTypeID.Scroll,
        ItemTypeID.Disc,
        ItemTypeID.Skull,
    ];
    public static readonly ItemTypeID[] Sustenance = [
        ItemTypeID.Apple,
        ItemTypeID.Cherries,
        ItemTypeID.Orange,
        ItemTypeID.Carrot,
        ItemTypeID.Potato,
        ItemTypeID.Bread,
        ItemTypeID.Cheese,
        ItemTypeID.Chicken,
        ItemTypeID.RawFish,
        ItemTypeID.CookedFish,
        ItemTypeID.RawBeef,
        ItemTypeID.CookedBeef,
        ItemTypeID.BottledWater,
        ItemTypeID.HealthPotion,
        ItemTypeID.SpeedPotion,
        ItemTypeID.SlownessPotion,
        ItemTypeID.RegenerationPotion,
        ItemTypeID.PoisonPotion,
        ItemTypeID.StrengthPotion,
        ItemTypeID.WeaknessPotion,
        ItemTypeID.ProtectionPotion,
        ItemTypeID.VulnerabilityPotion,
        ItemTypeID.DeleriumPotion,
        ItemTypeID.LifestealPotion,
    ];
    public static readonly ItemTypeID[] Materials = [
        ItemTypeID.WoodPlanks,
        ItemTypeID.Rock,
        ItemTypeID.GlassBottle,
        ItemTypeID.Cloth,
        ItemTypeID.Coal,
        ItemTypeID.RawIron,
        ItemTypeID.Iron,
        ItemTypeID.RawCopper,
        ItemTypeID.Copper,
        ItemTypeID.RawGold,
        ItemTypeID.Gold,
        ItemTypeID.Ink,
        ItemTypeID.Diamond,
        ItemTypeID.Emerald,
        ItemTypeID.Ruby,
        ItemTypeID.BottledCloud,
        ItemTypeID.BottledStorm,
    ];
    public static readonly Dictionary<ItemsetTypes, ItemTypeID[]> TypeToArray = new()
    {
        { ItemsetTypes.Tool, Tools },
        { ItemsetTypes.Weapon, Weapons },
        { ItemsetTypes.Collectable, Collectables },
        { ItemsetTypes.Sustenance, Sustenance },
        { ItemsetTypes.Material, Materials },
    };
}
