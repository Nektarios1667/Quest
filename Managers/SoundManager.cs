using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Quest.Managers;

public static class SoundManager
{
    public static readonly float? RandomPitch = null;
    private static readonly Dictionary<string, SoundEffect> soundEffects = [];
    private static readonly Dictionary<string, SoundEffectInstance> soundInstances = [];
    private static readonly Dictionary<string, Song> songs = [];
    public static void Init(ContentManager content)
    {
        // Load sounds
        LoadSound(content, "Footstep", "Sounds/Effects/Footstep");
        LoadSound(content, "Fire", "Sounds/Effects/Fire");
        LoadSound(content, "Fire2", "Sounds/Effects/Fire2");
        LoadSound(content, "Rain", "Sounds/Effects/Rain");
        LoadSound(content, "Sandstorm", "Sounds/Effects/Sandstorm");
        LoadSound(content, "Trinkets", "Sounds/Effects/Trinkets");
        LoadSound(content, "Click", "Sounds/Effects/Click");
        LoadSound(content, "DoorLocked", "Sounds/Effects/DoorLocked");
        LoadSound(content, "DoorUnlock", "Sounds/Effects/DoorUnlock");
        LoadSound(content, "Spook", "Sounds/Effects/Spook");
        LoadSound(content, "Typing", "Sounds/Effects/Typing");
        LoadSound(content, "Whoosh", "Sounds/Effects/Whoosh");
        LoadSound(content, "Pickup", "Sounds/Effects/Pickup");
        LoadSound(content, "Swoosh", "Sounds/Effects/Swoosh");
        LoadSound(content, "MetalScrape", "Sounds/Effects/MetalScrape");
        LoadSound(content, "Scribble", "Sounds/Effects/Scribble");
        LoadSound(content, "Bow", "Sounds/Effects/Bow");
        LoadSound(content, "Gulp", "Sounds/Effects/Gulp");
    }
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

    public static void PlaySound(string key, float volume = 1f, float pitch = 0f, float pitchVariation = 0f, float pan = 0f)
    {
        if (soundEffects.TryGetValue(key, out var sfx))
            sfx.Play(MathHelper.Clamp(volume * SoundVolume, 0f, 1f), pitch + RandomManager.RandomFloatRange(-pitchVariation, pitchVariation), pan);
    }

    public static void PlaySoundInstance(string key, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        var instance = GetOrCreateInstance(key);
        if (instance != null)
        {
            instance.Volume = MathHelper.Clamp(volume * SoundVolume, 0f, 1f);
            instance.Pitch = pitch;
            instance.Pan = pan;
            if (instance.State != SoundState.Playing)
                instance.Play();
        }
    }
    public static SoundEffectInstance? GetInstance(string key)
    {
        if (soundInstances.TryGetValue(key, out var instance))
            return instance;
        Logger.Error($"No sound instance '{key}'");
        return null;
    }
    public static void SetInstanceVolume(string key, float volume)
    {
        var inst = GetInstance(key);
        if (inst != null)
            inst.Volume = volume;
    }
    private static SoundEffectInstance? GetOrCreateInstance(string key)
    {
        if (!soundEffects.TryGetValue(key, out var sfx))
            return null;

        if (!soundInstances.TryGetValue(key, out var instance) || instance.State == SoundState.Stopped)
        {
            instance = sfx.CreateInstance();
            instance.Volume = SoundVolume;
            soundInstances[key] = instance;
        }

        return instance;
    }

    public static void StopSoundInstance(string key)
    {
        if (soundInstances.TryGetValue(key, out var instance))
            instance.Stop();
    }

    public static bool TryPlayMusic(string key, bool loop = false)
    {
        if (songs.TryGetValue(key, out var song))
        {
            StopMusic();
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(song);
            return true;
        }
        return false;
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
