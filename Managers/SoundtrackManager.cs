using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Linq;

namespace Quest.Managers;
public enum Soundtracks
{
    CavesOfDawn,
    Clouds,
    SacredGarden,
    NightmareAlley,
    TerrorHeights,
    Pulse,
    Beauty,
    WanderingWind,
    Mystical,
    DuskToDawn,
    Maps,
    OldDevil,
}
public class Soundtrack(Soundtracks track, Mood mood)
{
    public Soundtracks Track { get; } = track;
    public Mood Mood { get; } = mood;
}
public static class SoundtrackManager
{
    // Events
    public static event Action<Soundtracks?>? SoundtrackChanged;
    //
    public static Soundtracks? Playing { get; private set; }
    private static Dictionary<Mood, Soundtracks[]> Tracks { get; set; } = [];
    private static readonly Timer PlayNextSong = TimerManager.SetTimer("PlayNextSong", RandomManager.RandomIntRange(30, 60), EndSong, repetitions:int.MaxValue);
    private static bool QueueNextSong = false;
    public static void LoadSoundtracks(ContentManager content)
    {

        // Soundtracks and their categories
        Tracks = new()
        {
            { Mood.Dark, [
                Soundtracks.CavesOfDawn,
                Soundtracks.NightmareAlley,
                Soundtracks.TerrorHeights,
                Soundtracks.Pulse,
                Soundtracks.WanderingWind,
            ]},
            { Mood.Calm, [
                Soundtracks.Clouds,
                Soundtracks.SacredGarden,
                Soundtracks.Mystical,
            ]},
            { Mood.Epic, [
                Soundtracks.DuskToDawn,
                Soundtracks.Maps,
                Soundtracks.OldDevil,
            ]},
        };

        // Load sound files
        foreach (string soundtrack in Enum.GetNames(typeof(Soundtracks)))
        {
            // Load the soundtrack file
            string path = $"Sounds/Music/{soundtrack}";
            try
            {
                SoundManager.LoadSong(content, soundtrack, path);
                Logger.System($"Loaded soundtrack '{soundtrack}'");
            }
            catch
            {
                Logger.Error($"Failed to load soundtrack '{soundtrack}'");
            }
        }
    }
    public static void Update()
    {
        if (!SoundManager.IsMusicPlaying && QueueNextSong)
        {
            Soundtracks? soundtrack = GetRandomSoundtrack(StateManager.Mood);
            if (soundtrack != null)
                PlaySoundtrack(soundtrack.Value);
        }
    }
    public static Soundtracks? GetRandomSoundtrack(Mood mood)
    {
        if (Tracks.TryGetValue(mood, out var soundtracks) && soundtracks.Length > 0)
            return soundtracks[new Random().Next(soundtracks.Length)];

        Logger.Error($"No soundtracks found for mood '{mood}'");
        return null;
    }
    public static bool PlaySoundtrack(string soundtrack)
    {
        if (Enum.TryParse<Soundtracks>(soundtrack, out var st))
            return PlaySoundtrack(st);
        return false;
    }
    public static bool PlaySoundtrack(Soundtracks soundtrack)
    {
        if (SoundManager.TryPlayMusic(soundtrack.ToString()!))
        {
            PlayNextSong.Left = (int)MediaPlayer.Queue.ActiveSong.Duration.TotalSeconds + RandomManager.RandomIntRange(60, 120);
            Playing = soundtrack;
            SoundtrackChanged?.Invoke(soundtrack);
            QueueNextSong = false;
            return true;
        }
        return false;
    }
    private static void EndSong()
    {
        QueueNextSong = true;
        Playing = null;
    }
}
