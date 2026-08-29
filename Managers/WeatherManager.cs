using Quest.World;
using System.Linq;

namespace Quest.Managers;

public class WeatherManager
{
    // Weather
    private const float WeatherFadeOut = 3;
    private const float WeatherFadeIn = 2;
    private readonly Dictionary<WeatherTypeID, float> FadeStartVolume = new();
    private readonly Dictionary<WeatherTypeID, float> TimeSinceSound = new()
    {
        { WeatherTypeID.None, float.MaxValue },
        { WeatherTypeID.Rain, float.MaxValue },
        { WeatherTypeID.Ocean, float.MaxValue },
        { WeatherTypeID.Snow, float.MaxValue },
        { WeatherTypeID.Sandstorm, float.MaxValue },
        { WeatherTypeID.Volcano, float.MaxValue },
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
        if (gameManager.StateManager.State == GameState.Game)

            WeatherIntensity = GetWeatherIntensity(GameManager.GameTime);
        WeatherValue = GetWeatherValue(GameManager.GameTime);

        UpdateWeatherSounds(gameManager);
    }
    // Sounds
    private void UpdateWeatherSounds(GameManager gameManager)
    {
        // Weather sounds
        DebugManager.StartBenchmark("WeatherSounds");
        if (gameManager.StateManager.State == GameState.Game && WeatherIntensity > 0)
        {
            // Biome weather ambience
            BiomeType currentBiome = gameManager.LevelManager.GetBiome(CameraManager.TileCoord)!.Value;
            UpdateAmbientWeatherSound(currentBiome);
            // Biome weather sounds
            UpdateWeatherSfx(currentBiome);
        }
        // End sounds
        else if (WeatherIntensity <= 0)
        {
            foreach (var weatherSound in WeatherTypes.Types.Values)
                SoundManager.GetInstance(weatherSound.AmbientSoundName)?.Stop();
        }
        DebugManager.EndBenchmark("WeatherSounds");
    }
    private void UpdateAmbientWeatherSound(BiomeType biome)
    {
        WeatherTypes.Types.TryGetValue(biome, out var weatherType);

        // -TimeSinceSound = how long the player has been in that biome
        // TimeSinceSound = how long the player has been out of that biome

        // Play sounds
        if (weatherType.Type != WeatherTypeID.None)
        {
            float volume = WeatherIntensity * weatherType.AmbientSoundVolumeMult;
            // Reset time and count how long in biome
            float time = TimeSinceSound[weatherType.Type];
            if (time > 0)
            {
                FadeStartVolume.Remove(weatherType.Type);

                // If its in the process of fading out, swap it to fade in at same volume
                if (time < WeatherFadeOut)
                    // Since fade out and fade in might not be the same length, do the equivalent amount complete
                    TimeSinceSound[weatherType.Type] = -(time / WeatherFadeOut) * WeatherFadeIn;
                else
                    TimeSinceSound[weatherType.Type] = 0;
            }
            TimeSinceSound[weatherType.Type] -= GameManager.DeltaTime;

            SoundManager.PlaySoundInstance(weatherType.AmbientSoundName, volume * Math.Clamp(-TimeSinceSound[weatherType.Type] / WeatherFadeIn, 0, 1), loop: true);
        }

        // End others sounds
        foreach (var weathers in WeatherTypes.Types.Values)
        {
            if (weathers.Type != weatherType.Type && weathers.Type != WeatherTypeID.None)
            {
                // Reset time and count how long since in biome
                if (TimeSinceSound[weathers.Type] < 0)
                    TimeSinceSound[weathers.Type] = 0;
                TimeSinceSound[weathers.Type] += GameManager.DeltaTime;

                // Fade out or clear
                var instance = SoundManager.GetInstance(weathers.AmbientSoundName);
                if (TimeSinceSound[weathers.Type] >= WeatherFadeOut)
                {
                    // Clear
                    SoundManager.EndInstance(weathers.AmbientSoundName);
                    FadeStartVolume.Remove(weathers.Type);
                }
                else if (instance != null)
                {
                    if (!FadeStartVolume.ContainsKey(weathers.Type)) FadeStartVolume[weathers.Type] = instance.Volume; // When first starting to fade record what the original volume was
                    instance.Volume = (WeatherFadeOut - TimeSinceSound[weathers.Type]) / WeatherFadeOut * FadeStartVolume[weathers.Type]; // Fade away logic
                }
            }
        }
    }
    private void UpdateWeatherSfx(BiomeType currentBiome)
    {
        WeatherType type = WeatherTypes.Types[currentBiome];

        if (WeatherIntensity > type.SoundEffectThreshold && RandomManager.ChancePerSecond(type.SoundEffectChance))
        {
            var sound = ArrayTools.Random(type.SoundEffectNames);
            if (sound != null)
                SoundManager.PlaySoundInstance(sound, volume: WeatherIntensity * type.SoundEffectVolumeMult, pitch: type.SoundEffectPitch);
        }
    }
    // Sky
    public static readonly List<(float time, Color color)> darkGradient = [
        // Day
        (0.00f, new(100, 149, 237, 0)),         // Transparent blue
        (0.20f, new(100, 149, 237, 0)),         // Transparent blue

        // Sunset
        (0.23f, new Color(255, 200, 80, 170)),   // Golden
        (0.25f, new Color(255, 120, 50, 170)),  // Orange
        (0.27f, new Color(180, 50, 80, 220)),   // Red/pink
        (0.30f, new Color(40, 20, 60, 240)),    // Purple twilight

        // Night 
        (0.35f, Color.Black),
        (0.65f, Color.Black),

        // Sunrise
        (0.70f, new Color(40, 20, 60, 240)),    // Purple twilight
        (0.73f, new Color(180, 50, 80, 220)),   // Pink/redr
        (0.75f, new Color(255, 120, 50, 170)),  // Orange
        (0.77f, new Color(255, 200, 80, 170)),   // Golden

        // Day
        (0.80f, new(100, 149, 237, 0)),         // Transparent blue
        (1.00f, new(100, 149, 237, 0)),         // Transparent blue
    ];
    public static Color GetSkyColor(float time)
    {
        float cycle = time / Constants.DayLength;
        // Find stops
        var start = darkGradient.LastOrDefault(s => s.time <= cycle, darkGradient[^1]);
        var end = darkGradient.FirstOrDefault(s => s.time >= cycle, darkGradient[^1]);

        float lerp = start.time == end.time ? 1f : (cycle - start.time) / (end.time - start.time);
        Color color = Color.Lerp(start.color, end.color, lerp);
        if (color == Color.Transparent)
        {
            Console.WriteLine("uh oh ");
        }
        return color;
    }
    public static float GetDaylightPercent(float time)
    {
        float distDay = (GetSkyColor(time).ToVector4() - darkGradient[0].color.ToVector4()).LengthSquared();
        float distNight = (darkGradient[darkGradient.Count / 2].color.ToVector4() - GetSkyColor(time).ToVector4()).LengthSquared();
        float percent = distDay / (distDay + distNight) * 100;
        return 100 - percent;
    }
    public Color GetWeatherColor(GameManager gameManager, Point loc, float blend)
    {
        // Calculate sky colors from weather, biome, and time
        BiomeType? currentBiome = gameManager.LevelManager.GetBiome(loc);
        if (currentBiome == null) return Color.Transparent;

        Color weatherColor = WeatherTypes.Types.GetValueOrDefault(currentBiome.Value).WeatherColor;
        weatherColor *= blend + 1 - (weatherColor.A / 255f); // Use alpha channel to adjust transparancy per biome - lower alpha = more opaque
        return weatherColor;
    }
}
