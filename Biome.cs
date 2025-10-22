namespace Quest;

// Limited to 16 biome types b/c of save files
public enum BiomeType
{
    Temperate,
    Indoors,
    Snowy,
    Desert,
    Ocean,
}

public static class Biome
{
    public static Color[] Colors =
    {
        Color.Lime,
        Color.Gray,
        Color.White,
        Color.Yellow,
        Color.DarkBlue,
    };
}
