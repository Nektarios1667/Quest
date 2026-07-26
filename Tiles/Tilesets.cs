using System.Reflection.Metadata.Ecma335;

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
        TileTypeID.BlueTiles,
        TileTypeID.BrownTiles,
        TileTypeID.ConcreteFlooring,
        TileTypeID.CyanTiles,
        TileTypeID.Flooring,
        TileTypeID.GreenTiles,
        TileTypeID.LimeTiles,
        TileTypeID.OrangeTiles,
        TileTypeID.PinkTiles,
        TileTypeID.PurpleTiles,
        TileTypeID.RedTiles,
        TileTypeID.Sandstone,
        TileTypeID.StoneTiles,
        TileTypeID.WoodFlooring,
        TileTypeID.YellowTiles,
    ];
    public static readonly TileTypeID[] Walls = [
        TileTypeID.ConcreteWall,
        TileTypeID.Door,
        TileTypeID.IronWall,
        TileTypeID.SandstoneWall,
        TileTypeID.StoneWall,
        TileTypeID.WoodWall,
    ];
    public static readonly TileTypeID[] Windows = [
        TileTypeID.ConcreteWindow,
        TileTypeID.IronWindow,
        TileTypeID.SandstoneWindow,
        TileTypeID.StoneWindow,
        TileTypeID.WoodWindow,
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
    ];
    public static readonly TileTypeID[] Natural = [
        TileTypeID.Darkness,
        TileTypeID.Dirt,
        TileTypeID.Grass,
        TileTypeID.Ice,
        TileTypeID.Lava,
        TileTypeID.Path,
        TileTypeID.Sand,
        TileTypeID.Sandstone,
        TileTypeID.Sky,
        TileTypeID.Snow,
        TileTypeID.SnowyGrass,
        TileTypeID.Stone,
        TileTypeID.Water,
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