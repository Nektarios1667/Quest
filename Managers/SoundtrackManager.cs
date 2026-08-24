using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

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
    Bells,
    Euphoria,
    Everlost,
    Exotic,
    Mourning,
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
    private static (Point source, int radius, string level)? musicSource = null;
    public static void SetSource((Point source, int radius, string level)? source) => musicSource = source;
    private static Dictionary<Mood, Soundtracks[]> Tracks { get; set; } = [];
    private static readonly Timer PlayNextSong = TimerManager.SetTimer("PlayNextSong", RandomManager.RandomIntRange(30, 60), EndSong, repetitions: int.MaxValue);
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
                Soundtracks.Beauty,
                Soundtracks.Bells,
                Soundtracks.Euphoria,
                Soundtracks.Everlost,
                Soundtracks.Exotic,
                Soundtracks.Mourning,
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
    public static void Update(GameManager gameManager)
    {
        DebugManager.StartBenchmark("SoundtrackManagerUpdate");

        if (!SoundManager.IsMusicPlaying && QueueNextSong && gameManager.StateManager.IsPlayingState)
        {
            Soundtracks? soundtrack = GetRandomSoundtrack(gameManager.StateManager.Mood);
            if (soundtrack != null)
                PlaySoundtrack(soundtrack.Value);
        }
        else if (musicSource.HasValue)
        {
            // Pause on level change
            if (gameManager.LevelManager.Level.LevelName != musicSource.Value.level)
            {
                SoundManager.PauseMusic();
                return;
            }

            // Resume and update
            SoundManager.ResumeMusic();
            if (TimerManager.IsCompleteOrMissing("LocationalMusicPathfind"))
            {
                // --- A* Sound Pathfinding ---
                // Something is making it choose an inefficient path. Moving one tile over makes it go through two walls for a cost of 15,
                // instead of just going around the one wall for a cost of like 11.
                // Update pathfinding grid
                //PathfindingManager.SetGrid(gameManager.LevelManager.Level,
                //    CameraManager.TopLeftTileCoord - Constants.TileDrawPadding,
                //    Constants.NativeResolutionTiles + Constants.TileDrawPadding.Scaled(2)
                //);

                //// Pathfind to the music source
                //Point source = CameraManager.TileToRelativeTile(CameraManager.TileCoord, true);
                //Point dest = CameraManager.TileToRelativeTile(musicSource.Value.source / Constants.TileSize, true);
                //var (path, cost) = PathfindingManager.GetPath(source, dest, false) ?? (new Coordinate[0], float.MaxValue);

                //// Adjust
                //Console.WriteLine($"{path.Length}, {cost}");
                //MediaPlayer.Volume = Math.Clamp(1 - cost / musicSource.Value.radius, 0, 1);

                // --- Simple Distance Check ---
                Vector2 vec = CameraManager.WorldToTile(CameraManager.PlayerCenter.ToVector2()) - CameraManager.WorldToTile(musicSource.Value.source.ToVector2());
                float dist = vec.Length();
                float volume = Math.Clamp(1 - NumberTools.Square(dist / musicSource.Value.radius), 0, 1);
                MediaPlayer.Volume = volume;

                TimerManager.SetTimer("LocationalMusicPathfind", 0.2f, null);
            }
        }

        DebugManager.EndBenchmark("SoundtrackManagerUpdate");
    }
    public static Soundtracks? GetRandomSoundtrack(Mood mood)
    {
        if (Tracks.TryGetValue(mood, out var soundtracks) && soundtracks.Length > 0)
            return soundtracks[new Random().Next(soundtracks.Length)];

        Logger.Error($"No soundtracks found for mood '{mood}'");
        return null;
    }
    public static void StopSoundtrack()
    {
        SoundManager.StopMusic();
        Playing = null;
        PlayNextSong.Left = RandomManager.RandomIntRange(180, 240);
    }
    public static bool PlaySoundtrack(Soundtracks soundtrack, (Point source, int radius, string level)? locationalSource = null)
    {
        musicSource = locationalSource;
        if (SoundManager.TryPlayMusic(soundtrack.ToString()))
        {
            PlayNextSong.Left = (int)MediaPlayer.Queue.ActiveSong.Duration.TotalSeconds + RandomManager.RandomIntRange(180, 240);
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
