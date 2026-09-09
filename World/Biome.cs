namespace Quest.World;

public enum BiomeType : byte
{
    Temperate,
    Indoors,
    Snowy,
    Desert,
    Ocean,
    Volcanic,
}

public static class Biome
{
    public static readonly Color[] BiomeTileColors =
    [
        Color.Lime,
        Color.Gray,
        Color.White,
        Color.Yellow,
        Color.DarkBlue,
        new(107, 75, 52),
    ];
}
