using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.World;
public enum WeatherTypeID
{
    None,
    Rain,
    Ocean,
    Snow,
    Sandstorm,
    Volcano,
}
public readonly record struct WeatherType
{
    public readonly WeatherTypeID Type;
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
    public WeatherType(WeatherTypeID type, string ambientSoundName, string[] soundEffectNames, float ambientVolumeMult, float sfxVolumeMult, float ambientPitch, float sfxPitch, Color weatherColor, Color tileColor, float sfxChance, float sfxThreshold)
    {
        Type = type;
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
    public static readonly WeatherType None = new(WeatherTypeID.None, "", [], 0, 0, 0, 0, Color.Transparent, Color.Transparent, 0, 1);
    public static readonly WeatherType Rain = new(WeatherTypeID.Rain, "Rain", [..Enumerable.Range(1, 6).Select(i => $"Thunder{i}")], 0.5f, 0.75f, 1.0f, 1.0f, Color.MediumBlue, Color.MediumBlue, 0.1f, 0.2f);
    public static readonly WeatherType Ocean = new(WeatherTypeID.Rain, "Rain", [..Enumerable.Range(1, 6).Select(i => $"Thunder{i}")], 0.5f, 0.75f, 1.0f, 1.0f, Color.MediumBlue, Color.MediumBlue, 0.2f, 0.15f);
    public static readonly WeatherType Snow = new(WeatherTypeID.Snow, "Snow", [], 0.4f, 1.0f, 1.0f, 1.0f, new(200, 200, 216), new(200, 200, 216), 0.1f, 0.2f);
    public static readonly WeatherType Sandstorm = new(WeatherTypeID.Sandstorm, "Sandstorm", [], 0.3f, 1.0f, 1.0f, 1.0f, Color.OrangeRed, Color.OrangeRed, 0.1f, 0.2f);
    public static readonly WeatherType Volcano = new(WeatherTypeID.Volcano, "VolcanoAmbience", [.. Enumerable.Range(1, 6).Select(i => $"Rumble{i}")], 1.25f, 1.0f, 0.8f, 1.0f, new(107, 75, 52), new(107, 75, 52), 0.2f, 0.2f);
    public static readonly Dictionary<BiomeType, WeatherType> Types = new()
    {
        { BiomeType.Indoors, None },
        { BiomeType.Temperate, Rain },
        { BiomeType.Ocean, Ocean },
        { BiomeType.Snowy, Snow },
        { BiomeType.Desert, Sandstorm },
        { BiomeType.Volcanic, Volcano },
    };
}
