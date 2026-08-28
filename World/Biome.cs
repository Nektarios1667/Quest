namespace Quest.World;

// Limited to 256 biome types b/c of save files
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
