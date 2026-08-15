namespace Quest.Tiles;

public interface IDynamicTile { }
public enum TilesetTypes
{
    Flooring,
    Walls,
    Windows,
    Interactables,
    Natural,
}
public static class Tilesets
{
    public static readonly TileTypeID[] Flooring = [
        TileTypeID.BlackTiles,
        TileTypeID.RedTiles,
        TileTypeID.OrangeTiles,
        TileTypeID.YellowTiles,
        TileTypeID.LimeTiles,
        TileTypeID.GreenTiles,
        TileTypeID.CyanTiles,
        TileTypeID.BlueTiles,
        TileTypeID.PurpleTiles,
        TileTypeID.PinkTiles,
        TileTypeID.BrownTiles,
        TileTypeID.GrayTiles,
        TileTypeID.WhiteTiles,
        TileTypeID.Sandstone,
        TileTypeID.ConcreteFlooring,
        TileTypeID.Flooring,
        TileTypeID.WoodFlooring,
    ];
    public static readonly TileTypeID[] Walls = [
        TileTypeID.ConcreteWall,
        TileTypeID.IronWall,
        TileTypeID.SandstoneWall,
        TileTypeID.StoneWall,
        TileTypeID.WoodWall,
        TileTypeID.MudWall,
        TileTypeID.SnowWall,
        TileTypeID.IceWall,
        TileTypeID.DryWall,
        TileTypeID.Door,
    ];
    public static readonly TileTypeID[] Windows = [
        TileTypeID.ConcreteWindow,
        TileTypeID.IronWindow,
        TileTypeID.SandstoneWindow,
        TileTypeID.StoneWindow,
        TileTypeID.WoodWindow,
        TileTypeID.MudWindow,
        TileTypeID.SnowWindow,
        TileTypeID.IceWindow,
        TileTypeID.DryWallWindow,
    ];
    public static readonly TileTypeID[] Interactables = [
        TileTypeID.Chest,
        TileTypeID.Crafter,
        TileTypeID.Crate,
        TileTypeID.DiscWriter,
        TileTypeID.DisplayCase,
        TileTypeID.Door,
        TileTypeID.Furnace,
        TileTypeID.Inscriber,
        TileTypeID.Jukebox,
        TileTypeID.Lamp,
        TileTypeID.Stairs,
        TileTypeID.Stove,
        TileTypeID.PressurePlate,
        TileTypeID.TimedPressurePlate,
    ];
    public static readonly TileTypeID[] Natural = [
        TileTypeID.Grass,
        TileTypeID.Dirt,
        TileTypeID.Farmland,
        TileTypeID.Path,
        TileTypeID.Sand,
        TileTypeID.WetSand,
        TileTypeID.Sandstone,
        TileTypeID.RedSand,
        TileTypeID.WetRedSand,
        TileTypeID.VolcanicSand,
        TileTypeID.WetVolcanicSand,
        TileTypeID.Gravel,
        TileTypeID.SnowyGrass,
        TileTypeID.Snow,
        TileTypeID.Ice,
        TileTypeID.Stone,
        TileTypeID.Water,
        TileTypeID.Lava,
        TileTypeID.Sky,
        TileTypeID.Darkness,
    ];
    public static readonly Dictionary<TilesetTypes, TileTypeID[]> TypeToArray = new()
    {
        { TilesetTypes.Flooring, Flooring },
        { TilesetTypes.Walls, Walls },
        { TilesetTypes.Windows, Windows },
        { TilesetTypes.Interactables, Interactables },
        { TilesetTypes.Natural, Natural },
    };
}