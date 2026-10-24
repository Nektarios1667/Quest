namespace Quest.World;

public enum BiomeType : byte
{
    Temperate,
    Indoors,
    Snowy,
    Desert,
    Ocean,
    Volcanic,
    Underground,
}

public static class Biome
{
    public static readonly Dictionary<BiomeType, Color> BiomeTileColors = new()
    {
        { BiomeType.Temperate, Color.Lime },
        { BiomeType.Indoors, Color.Gray },
        { BiomeType.Snowy, Color.White },
        { BiomeType.Desert, Color.Yellow },
        { BiomeType.Ocean, Color.DarkBlue },
        { BiomeType.Volcanic, new(107, 75, 52) },
        { BiomeType.Underground, Color.Brown },
    };
}
