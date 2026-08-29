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
        LoadSound(content, "Snow", "Sounds/Effects/Sandstorm");
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
        LoadSound(content, "Hammer", "Sounds/Effects/Hammer");
        LoadSound(content, "Thunder1", "Sounds/Effects/Thunder1");
        LoadSound(content, "Thunder2", "Sounds/Effects/Thunder2");
        LoadSound(content, "Thunder3", "Sounds/Effects/Thunder3");
        LoadSound(content, "Thunder4", "Sounds/Effects/Thunder4");
        LoadSound(content, "Thunder5", "Sounds/Effects/Thunder5");
        LoadSound(content, "Rumble1", "Sounds/Effects/Rumble1");
        LoadSound(content, "Rumble2", "Sounds/Effects/Rumble2");
        LoadSound(content, "Rumble3", "Sounds/Effects/Rumble3");
        LoadSound(content, "Rumble4", "Sounds/Effects/Rumble4");
        LoadSound(content, "Rumble5", "Sounds/Effects/Rumble5");
        LoadSound(content, "Rumble6", "Sounds/Effects/Rumble6");
        LoadSound(content, "VolcanoAmbience", "Sounds/Effects/VolcanoAmbience");
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
            sfx.Play(MathHelper.Clamp(volume * SettingsManager.SoundVolume, 0f, 1f), pitch + RandomManager.RandomFloatRange(-pitchVariation, pitchVariation), pan);
    }
    // Only one instance of a sound can be played at a time, but its properties can be changed while it is playing
    public static void PlaySoundInstance(string key, float volume = 1f, float pitch = 0f, float pan = 0f, bool loop = false)
    {
        var instance = GetOrCreateInstance(key);
        if (instance == null) return;

        instance.Volume = MathHelper.Clamp(volume * SettingsManager.SoundVolume, 0f, 1f);
        instance.Pitch = pitch;
        instance.Pan = pan;
        instance.IsLooped = loop;
        if (instance.State != SoundState.Playing)
            instance.Play();
    }
    public static SoundEffectInstance? GetInstance(string key)
    {
        if (soundInstances.TryGetValue(key, out var instance))
            return instance;
        return null;
    }
    public static void EndInstance(string key)
    {
        soundInstances.GetValueOrDefault(key)?.Stop();
        soundInstances.Remove(key);
    }
    private static SoundEffectInstance? GetOrCreateInstance(string key)
    {
        if (!soundEffects.TryGetValue(key, out var sfx))
            return null;

        if (!soundInstances.TryGetValue(key, out var instance) || instance.State == SoundState.Stopped)
        {
            instance = sfx.CreateInstance();
            instance.Volume = SettingsManager.SoundVolume;
            soundInstances[key] = instance;
        }

        return instance;
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

    public static void StopMusic() => MediaPlayer.Stop();

    public static void PauseMusic() => MediaPlayer.Pause();

    public static void ResumeMusic() => MediaPlayer.Resume();
}
