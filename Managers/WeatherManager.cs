using Quest.World;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Managers;

public class WeatherManager
{
    private enum WeatherSound
    {
        None,
        Rain,
        Snow,
        Sandstorm,
    }
    private readonly Dictionary<WeatherSound, float> WeatherSoundMults = new()
    {
        { WeatherSound.None, 0 },
        { WeatherSound.Rain, 0.5f },
        { WeatherSound.Snow, 0.5f },
        { WeatherSound.Sandstorm, 0.3f },
    };
    // Weather
    private readonly Dictionary<BiomeType, WeatherSound> WeatherSounds = new()
    {
        { BiomeType.Temperate, WeatherSound.Rain },
        { BiomeType.Ocean, WeatherSound.Rain },
        { BiomeType.Indoors, WeatherSound.None },
        { BiomeType.Snowy, WeatherSound.Snow },
        { BiomeType.Desert, WeatherSound.Sandstorm }
    };
    private const float WeatherFadeOut = 3;
    private const float WeatherFadeIn = 2;
    private Dictionary<WeatherSound, float> FadeStartVolume = new();
    private Dictionary<WeatherSound, float> TimeSinceSound = new()
    {
        { WeatherSound.None, float.MaxValue },
        { WeatherSound.Rain, float.MaxValue },
        { WeatherSound.Snow, float.MaxValue },
        { WeatherSound.Sandstorm, float.MaxValue },
    };
    public readonly FastNoiseLite WeatherNoise = new((int)(DateTime.Now.Ticks ^ (DateTime.Now.Ticks >> 32)));
    public int WeatherSeed { get => _weatherSeed; set { _weatherSeed = value; WeatherNoise.SetSeed(value); } }
    public float WeatherIntensity { get; private set; }
    public float WeatherValue { get; private set; }
    public float LastWeather { get; private set; } = 0f;
    // Private
    private int _weatherSeed = Environment.TickCount;
    public const float weatherThreshold = 0.65f;
    private float lastTime = -1f;
    public WeatherManager()
    {
        WeatherNoise.SetSeed(WeatherSeed);
        WeatherNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        WeatherNoise.SetFrequency(0.005f);
        WeatherNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        WeatherNoise.SetFractalOctaves(3);
    }
    // Values
    public void SetWeatherPersistent(int seed = -1, float lastWeatherTime = 0f, float lastTimeValue = -1f)
    {
        if (seed != -1)
            WeatherNoise.SetSeed(seed);
        LastWeather = lastWeatherTime;
        lastTime = lastTimeValue;
    }
    public static float NoiseToIntensity(float noise) => Math.Min((float)Math.Sqrt(Math.Max(noise - weatherThreshold, 0) / (1 - weatherThreshold)), 0.8f);
    public float GetWeatherBoost(float time) => time - LastWeather > 600 ? Math.Min((time - LastWeather - 600) / 1800f, 0.1f) : 0;
    public float GetWeatherIntensity(float time) => NoiseToIntensity(GetWeatherValue(time));
    public float GetWeatherValue(float time)
    {
        float val = WeatherNoise.GetNoise(time, 0) * 0.5f + 0.5f;
        val = 1f / (1 + (float)Math.Pow(MathHelper.E, -8 * (val - 0.5f)));
        float delta = lastTime == -1 ? 0 : (time - lastTime);

        // Weather buildup
        val += GetWeatherBoost(time);
        if (val >= weatherThreshold)
        {
            LastWeather += Math.Min(12 * delta * (val - weatherThreshold) / (1 - weatherThreshold), time - LastWeather);
        }

        lastTime = time;

        return val;
    }

    // Updates
    public void Update(GameManager gameManager)
    {
        if (StateManager.State == GameState.Game)

        WeatherIntensity = GetWeatherIntensity(GameManager.GameTime);
        WeatherValue = GetWeatherValue(GameManager.GameTime);

        UpdateWeatherSounds(gameManager);
    }
    // Sounds
    private void UpdateWeatherSounds(GameManager gameManager)
    {
        // Weather sounds
        DebugManager.StartBenchmark("WeatherSounds");
        if (!Constants.EDITOR && WeatherIntensity > 0)
        {
            // Biome weather ambience
            BiomeType currentBiome = gameManager.LevelManager.GetBiome(CameraManager.TileCoord)!.Value;
            UpdateAmbientWeatherSound(currentBiome);
            // Biome weather sounds
            UpdateWeatherSfx(currentBiome);
        }
        DebugManager.EndBenchmark("WeatherSounds");
    }
    private void UpdateAmbientWeatherSound(BiomeType biome)
    {
        WeatherSounds.TryGetValue(biome, out WeatherSound sound);

        // Play sounds
        if (sound != WeatherSound.None)
        {
            float volume = sound switch
            {
                WeatherSound.Rain => WeatherIntensity * WeatherSoundMults[WeatherSound.Rain],
                WeatherSound.Snow => WeatherIntensity * WeatherSoundMults[WeatherSound.Snow],
                WeatherSound.Sandstorm => WeatherIntensity * WeatherSoundMults[WeatherSound.Sandstorm],
                _ => 0f
            };

            // Either reset time or count how long in biome
            if (TimeSinceSound[sound] > 0)
            {
                FadeStartVolume.Remove(sound);
                TimeSinceSound[sound] = 0;
            }
            else
                TimeSinceSound[sound] -= GameManager.DeltaTime;

            SoundManager.PlaySoundInstance(sound.ToString(), volume * Math.Clamp(-TimeSinceSound[sound] / WeatherFadeIn, 0, 1), loop: true);
        }

        // End others sounds
        foreach (var weatherSound in WeatherSounds.Values)
        {
            if (weatherSound != sound && weatherSound != WeatherSound.None)
            {
                // Either reset time or count how long since in biome
                if (TimeSinceSound[weatherSound] < 0)
                    TimeSinceSound[weatherSound] = 0;
                else
                    TimeSinceSound[weatherSound] += GameManager.DeltaTime;

                // Fade out or clear
                var instance = SoundManager.GetInstance(weatherSound.ToString());
                if (TimeSinceSound[weatherSound] >= WeatherFadeOut)
                {
                    // Clear
                    SoundManager.EndInstance(weatherSound.ToString()); 
                    FadeStartVolume.Remove(weatherSound);
                }
                else if (instance != null)
                {
                    if (!FadeStartVolume.ContainsKey(weatherSound)) FadeStartVolume[weatherSound] = instance.Volume; // When first starting to fade record what the original volume was
                    instance.Volume = (WeatherFadeOut - TimeSinceSound[weatherSound]) / WeatherFadeOut * FadeStartVolume[weatherSound]; // Fade away logic
                }
            }
        }
    }
    private void UpdateWeatherSfx(BiomeType currentBiome)
    {
        if (WeatherIntensity > .2f && RandomManager.ChancePerSecond(0.1f))
        {
            switch (currentBiome)
            {
                case BiomeType.Temperate: SoundManager.PlaySoundInstance($"Thunder{RandomManager.RandomIntRange(1, 6)}", volume: WeatherIntensity * 0.75f); break;
                case BiomeType.Ocean: SoundManager.PlaySoundInstance($"Thunder{RandomManager.RandomIntRange(1, 6)}", volume: WeatherIntensity * 0.75f); break;
                case BiomeType.Indoors: break;
                case BiomeType.Snowy: break;
                case BiomeType.Desert: break;
            }
        }
    }
    // Sky
    public static readonly List<(float pos, Color color)> darkGradient = [
        (0, Color.Transparent),
        (0.2f, Color.Transparent),
        (0.3f, Color.Black),
        (0.5f, Color.Black),
        (0.7f, Color.Black),
        (0.8f, Color.Transparent),
        (1, Color.Transparent),
    ];
    public static Color GetSkyColor(float time)
    {
        float cycle = time / Constants.DayLength;
        // Find stops
        var start = darkGradient.LastOrDefault(s => s.pos <= cycle, darkGradient[^1]);
        var end = darkGradient.FirstOrDefault(s => s.pos >= cycle, darkGradient[^1]);

        Color color = Color.Lerp(start.color, end.color, (cycle - start.pos) / (end.pos - start.pos));
        return color;
    }
    public static float GetDaylightPercent(float time)
    {
        float distDay = (GetSkyColor(time).ToVector4() - darkGradient[0].color.ToVector4()).LengthSquared();
        float distNight = (darkGradient[darkGradient.Count / 2].color.ToVector4() - GetSkyColor(time).ToVector4()).LengthSquared();
        float percent = distDay / (distDay + distNight) * 100;
        return 100 - percent;
    }
    public Color GetWeatherColor(GameManager gameManager, Point loc, float? blend = null)
    {
        // Calculate sky colors from weather, biome, and time
        BiomeType? currentBiome = gameManager.LevelManager.GetBiome(loc);
        blend ??= WeatherIntensity;

        Color weatherColor = default;
        if (currentBiome == null || currentBiome == BiomeType.Indoors || blend == 0) weatherColor = Color.Transparent;
        else
        {
            switch (currentBiome)
            {
                case BiomeType.Temperate: weatherColor = Color.MediumBlue; break;
                case BiomeType.Snowy: weatherColor = Color.White; break;
                case BiomeType.Desert: weatherColor = Color.OrangeRed; break;
                case BiomeType.Ocean: weatherColor = Color.SlateGray; break;
            }
        }
        weatherColor *= blend.Value;
        return weatherColor;
    }
}
