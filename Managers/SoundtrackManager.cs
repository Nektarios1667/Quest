using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;
public enum SoundtrackMoods
{
    Calm,
    Dark,
    Epic,
}
public class Soundtrack(string file, SoundtrackMoods mood)
{
    public string File { get; } = file;
    public SoundtrackMoods Mood { get; } = mood;
}
public static class SoundtrackManager
{
    public static float MusicVolume { get; set; } = 1f;
    public static Soundtrack? Playing { get; private set; }
    private static Soundtrack[] Soundtracks { get; set; } = [];
    private static Dictionary<SoundtrackMoods, Soundtrack[]> SoundtrackCategories { get; set; } = [];
    public static void LoadSoundtracks(ContentManager content)
    {
        // Setup music
        SoundManager.MusicVolume = .6f;

        // Load soundtrack objects
        Soundtracks = [
            new("CavesOfDawn", SoundtrackMoods.Dark),
            new("Clouds", SoundtrackMoods.Calm),
            new("SacredGarden", SoundtrackMoods.Calm),
            new("NightmareAlley", SoundtrackMoods.Dark),
            new("TerrorHeights", SoundtrackMoods.Dark),
        ];

        // Load sound files
        foreach (Soundtrack soundtrack in Soundtracks)
        {
            // Load the soundtrack file
            string path = $"Sounds/Music/{soundtrack.File}";
            SoundManager.LoadSong(content, soundtrack.File, path);
        }

        // Categorize
        foreach (SoundtrackMoods mood in Enum.GetValues<SoundtrackMoods>())
            SoundtrackCategories[mood] = Soundtracks.Where(s => s.Mood == mood).ToArray();
    }
    public static void Update()
    {
        if (!SoundManager.IsMusicPlaying)
        {
            Soundtrack soundtrack = GetRandomSoundtrack(SoundtrackMoods.Calm);
            SoundManager.PlayMusic(soundtrack.File);
            Playing = soundtrack;
        }
    }
    public static Soundtrack GetRandomSoundtrack(SoundtrackMoods mood)
    {
        if (SoundtrackCategories.TryGetValue(mood, out var soundtracks) && soundtracks.Length > 0)
        {
            return soundtracks[new Random().Next(soundtracks.Length)];
        }
        Logger.Error($"No soundtracks found for mood: {mood}");
        return Soundtracks[0];
    }
}
