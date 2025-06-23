using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
namespace Quest.Managers;
public static class SoundManager
{
    private static readonly Dictionary<string, SoundEffect> soundEffects = [];
    private static readonly Dictionary<string, Song> songs = [];

    public static float SoundVolume { get; set; } = 1f;
    public static float MusicVolume
    {
        get => MediaPlayer.Volume;
        set => MediaPlayer.Volume = MathHelper.Clamp(value, 0f, 1f);
    }
    public static bool IsMusicPlaying => MediaPlayer.State == MediaState.Playing;

    public static void LoadSound(ContentManager content, string key, string path)
    {
        if (!soundEffects.ContainsKey(key))
            soundEffects[key] = content.Load<SoundEffect>(path);
    }
    public static void LoadSong(ContentManager content, string key, string path)
    {
        if (!songs.ContainsKey(key))
            songs[key] = content.Load<Song>(path);
    }
    public static void PlaySound(string key, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        if (soundEffects.TryGetValue(key, out var sfx))
            sfx.Play(MathHelper.Clamp(volume * SoundVolume, 0f, 1f), pitch, pan);
    }
    public static void PlayMusic(string key, bool loop = true)
    {
        if (songs.TryGetValue(key, out var song))
        {
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(song);
        }
    }

    public static void StopMusic()
    {
        MediaPlayer.Stop();
    }

    public static void PauseMusic()
    {
        if (IsMusicPlaying)
            MediaPlayer.Pause();
    }

    public static void ResumeMusic()
    {
        if (MediaPlayer.State == MediaState.Paused)
            MediaPlayer.Resume();
    }
}
