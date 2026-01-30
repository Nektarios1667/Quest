namespace Quest;

// Limited to 256 biome types b/c of save files
public enum BiomeType : byte
{
    Temperate,
    Indoors,
    Snowy,
    Desert,
    Ocean,
}

public static class Biome
{
    public static readonly Color[] Colors =
    [
        Color.Lime,
        Color.Gray,
        Color.White,
        Color.Yellow,
        Color.DarkBlue,
    ];
}
