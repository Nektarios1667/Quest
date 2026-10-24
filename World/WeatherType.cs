using System.Linq;

namespace Quest.World;

public readonly record struct WeatherType
{
    public readonly BiomeType Biome;
    public readonly string AmbientSoundName;
    public readonly string[] SoundEffectNames;
    public readonly float AmbientSoundVolumeMult;
    public readonly float SoundEffectVolumeMult;
    public readonly float AmbientSoundPitch;
    public readonly float SoundEffectPitch;
    public readonly Color WeatherColor;
    public readonly Color BiomeTileColor;
    public readonly float SoundEffectChance; // Chance per second as a fraction
    public readonly float SoundEffectThreshold; // Weather intensity threshold to allow sfx
    public WeatherType(BiomeType biome, string ambientSoundName, string[] soundEffectNames, float ambientVolumeMult, float sfxVolumeMult, float ambientPitch, float sfxPitch, Color weatherColor, Color tileColor, float sfxChance, float sfxThreshold)
    {
        Biome = biome;
        AmbientSoundName = ambientSoundName;
        SoundEffectNames = soundEffectNames;
        AmbientSoundVolumeMult = ambientVolumeMult;
        SoundEffectVolumeMult = sfxVolumeMult;
        AmbientSoundPitch = ambientPitch;
        SoundEffectPitch = sfxPitch;
        WeatherColor = weatherColor;
        BiomeTileColor = tileColor;
        SoundEffectChance = sfxChance;
        SoundEffectThreshold = sfxThreshold;
    }
}

public static class WeatherTypes
{
    public static readonly WeatherType None = new(BiomeType.Indoors, "", [], 0, 0, 0, 0, Color.Transparent, Color.Transparent, 0, 1);
    public static readonly WeatherType Rain = new(BiomeType.Temperate, "Rain", [.. Enumerable.Range(1, 6).Select(i => $"Thunder{i}")], 0.5f, 0.75f, 1.0f, 1.0f, Color.MediumBlue, Color.MediumBlue, 0.1f, 0.2f);
    public static readonly WeatherType Ocean = new(BiomeType.Ocean, "Rain", [.. Enumerable.Range(1, 6).Select(i => $"Thunder{i}")], 0.5f, 0.75f, 1.0f, 1.0f, Color.MediumBlue, Color.MediumBlue, 0.2f, 0.15f);
    public static readonly WeatherType Snow = new(BiomeType.Snowy, "Snow", [], 0.4f, 1.0f, 1.0f, 1.0f, new(200, 200, 216), new(200, 200, 216), 0.1f, 0.2f);
    public static readonly WeatherType Sandstorm = new(BiomeType.Desert, "Sandstorm", [], 0.3f, 1.0f, 1.0f, 1.0f, Color.OrangeRed, Color.OrangeRed, 0.1f, 0.2f);
    public static readonly WeatherType Volcano = new(BiomeType.Volcanic, "VolcanoAmbience", [.. Enumerable.Range(1, 6).Select(i => $"Rumble{i}")], 1.25f, 1.0f, 0.8f, 1.0f, new(107, 75, 52), new(107, 75, 52), 0.2f, 0.2f);
    public static readonly WeatherType Underground = new(BiomeType.Underground, "", [.. Enumerable.Range(1, 6).Select(i => $"Rumble{i}")], 1f, 0.5f, 1f, 1.0f, Color.Transparent, Color.Brown, 0.1f, 0f);
    public static readonly Dictionary<BiomeType, WeatherType> Types = new()
    {
        { BiomeType.Indoors, None },
        { BiomeType.Temperate, Rain },
        { BiomeType.Ocean, Ocean },
        { BiomeType.Snowy, Snow },
        { BiomeType.Desert, Sandstorm },
        { BiomeType.Volcanic, Volcano },
        { BiomeType.Underground, Underground },
    };
}
