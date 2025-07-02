using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace Quest.Managers;
public class Soundtrack(string file, Mood mood)
{
    public string File { get; } = file;
    public Mood Mood { get; } = mood;
}
public static class SoundtrackManager
{
    public static float MusicVolume { get; set; } = 1f;
    public static Soundtrack? Playing { get; private set; }
    private static Soundtrack[] Soundtracks { get; set; } = [];
    private static Dictionary<Mood, Soundtrack[]> SoundtrackCategories { get; set; } = [];
    public static void LoadSoundtracks(ContentManager content)
    {
        // Setup music
        SoundManager.MusicVolume = 0f;

        // Load soundtrack objects
        Soundtracks = [
            new("CavesOfDawn", Mood.Dark),
            new("Clouds", Mood.Calm),
            new("SacredGarden", Mood.Calm),
            new("NightmareAlley", Mood.Dark),
            new("TerrorHeights", Mood.Dark),
            new("Pulse", Mood.Dark),
            new("Beauty", Mood.Calm),
            new("WanderingWind", Mood.Dark),
            new("Mystical", Mood.Calm)
        ];

        // Load sound files
        foreach (Soundtrack soundtrack in Soundtracks)
        {
            // Load the soundtrack file
            string path = $"Sounds/Music/{soundtrack.File}";
            SoundManager.LoadSong(content, soundtrack.File, path);
            Logger.System($"Loaded soundtrack '{soundtrack.File}' with mood '{soundtrack.Mood}'");
        }

        // Categorize
        foreach (Mood mood in Enum.GetValues<Mood>())
            SoundtrackCategories[mood] = Soundtracks.Where(s => s.Mood == mood).ToArray();
    }
    public static void Update()
    {
        if (!SoundManager.IsMusicPlaying)
        {
            Soundtrack soundtrack = GetRandomSoundtrack(StateManager.Mood);
            SoundManager.PlayMusic(soundtrack.File);
            Playing = soundtrack;
        }
    }
    public static Soundtrack GetRandomSoundtrack(Mood mood)
    {
        if (SoundtrackCategories.TryGetValue(mood, out var soundtracks) && soundtracks.Length > 0)
        {
            return soundtracks[new Random().Next(soundtracks.Length)];
        }
        Logger.Error($"No soundtracks found for mood: {mood}");
        return Soundtracks[0];
    }
}
