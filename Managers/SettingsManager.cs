using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using MonoGUI;

namespace Quest.Managers;

public static class SettingsManager
{
    // Settings
    public static Point ScreenResolution { get; private set; } = new(1280, 720); // Actual screen resolution
    public static Vector2 ScreenScale => ScreenResolution.ToVector2() / Constants.NativeResolution.ToVector2(); // Scale factor from native resolution to actual screen resolution
    public static bool Fullscreen { get; private set; } = false;
    public static int FPS { get; private set; } = 241; // >240 means unlimited
    public static bool VSYNC { get; private set; } = false;
    public static float MusicVolume
    {
        get => MediaPlayer.Volume;
        set => MediaPlayer.Volume = MathHelper.Clamp(value, 0f, 1f);
    }
    public static float SoundVolume { get; set; } = 1f;
    // GUI / Widgets
    private static GUI settingsMenu = null!;
    private static HorizontalSlider musicSlider = null!;
    private static HorizontalSlider soundSlider = null!;
    private static HorizontalSlider fpsSlider = null!;
    private static Checkbox vsyncCheckbox = null!;
    private static Checkbox fullscreenCheckbox = null!;
    private static Dropdown resolutionDropdown = null!;
    public static GUI CreateSettingsMenu(Window window, SpriteBatch batch, ContentManager content)
    {
        settingsMenu = new(window, batch, PixelOperator);
        settingsMenu.LoadContent(content, "Images/Gui");
        Label settingsLabel = new(settingsMenu, new(Constants.Middle.X - 130, 50), Color.White, "Settings", PixelOperatorTitle);
        Button settingsBackButton = new(settingsMenu, new(20, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);

        // Save button in top rigjht
        Button saveButton = new(settingsMenu, new(Constants.NativeResolution.X - 120, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, () =>
        {
            WriteSettings();
            StateManager.RevertGameState();
        }, [], text: "Save", font: PixelOperator, border: 0);
        Button revertButton = new(settingsMenu, new(Constants.NativeResolution.X - 240, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, () => LoadSettings(window), [], text: "Revert", font: PixelOperator, border: 0);

        // Sound
        // Music
        musicSlider = new(settingsMenu, new(100, 200), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        musicSlider.SetValue(SettingsManager.MusicVolume);
        musicSlider.ValueChanged += (value) => SettingsManager.MusicVolume = value;
        Label musicLabel = new(settingsMenu, new(100, 150), Color.White, "Music Volume", PixelOperator);
        Label musicValue = new(settingsMenu, new(420, 185), Color.White, $"{(int)(musicSlider.Value * 100)}%", PixelOperator);
        musicSlider.ValueChanged += (value) => musicValue.Text = $"{(int)(value * 100)}%";

        // Sound
        soundSlider = new(settingsMenu, new(100, 265), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        soundSlider.SetValue(SettingsManager.SoundVolume);
        soundSlider.ValueChanged += (value) => SettingsManager.SoundVolume = value;
        Label soundLabel = new(settingsMenu, new(100, 215), Color.White, "Sound Volume", PixelOperator);
        Label soundValue = new(settingsMenu, new(420, 250), Color.White, $"{(int)(soundSlider.Value * 100)}%", PixelOperator);
        soundSlider.ValueChanged += (value) => soundValue.Text = $"{(int)(value * 100)}%";

        // Screen
        // Fps limit
        UpdateFPSLimit(window);
        fpsSlider = new(settingsMenu, new(100, 330), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        fpsSlider.SetValue(SettingsManager.FPS > 240 ? 1 : (SettingsManager.FPS - 30) / 211);
        fpsSlider.ValueChanged += (value) =>
        {
            SettingsManager.FPS = (int)(value * 211 + 30);
            UpdateFPSLimit(window);
        };
        Label fpsLabel = new(settingsMenu, new(100, 280), Color.White, "FPS Limit", PixelOperator);
        Label fpsValue = new(settingsMenu, new(420, 315), Color.White, $"{(SettingsManager.FPS > 240 ? "Unlimited" : SettingsManager.FPS)}", PixelOperator);
        fpsSlider.ValueChanged += (value) => fpsValue.Text = ((int)(value * 211 + 30) == 241) ? "Unlimited" : $"{(int)(value * 211 + 30)}";

        // Vsync
        vsyncCheckbox = new(settingsMenu, new(100, 355), 40, Color.Black, Color.Gray, Color.DarkGray);
        Label vsyncLabel = new(settingsMenu, new(150, 355), Color.White, "V-Sync", PixelOperator);
        vsyncCheckbox.SetValue(SettingsManager.VSYNC);
        vsyncCheckbox.ValueChanged += (isChecked) =>
        {
            SettingsManager.VSYNC = isChecked;
            window.SetVsync(SettingsManager.VSYNC);
        };

        // Resolution
        resolutionDropdown = new(settingsMenu, new(100, 420), new(200, 30), Color.Black, Color.Gray, Color.DarkGray, font: PixelOperator);
        resolutionDropdown.AddItems(["1280x720", "1366x768", "1600x900", "1920x1080", "2560x1440", "3840x2160"]);
        resolutionDropdown.SelectItem($"{ScreenResolution.X}x{ScreenResolution.Y}");
        resolutionDropdown.ItemSelected += (item) =>
        {
            var parts = item.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                ScreenResolution = new Point(width, height);
                window.SetResolution(ScreenResolution.X, ScreenResolution.Y);
            }
        };

        // Fullscreen checkbox
        fullscreenCheckbox = new(settingsMenu, new(100, 485), 40, Color.Black, Color.Gray, Color.DarkGray);
        Label fullscreenLabel = new(settingsMenu, new(150, 485), Color.White, "Fullscreen", PixelOperator);
        fullscreenCheckbox.SetValue(SettingsManager.Fullscreen);
        fullscreenCheckbox.ValueChanged += (isChecked) =>
        {
            SettingsManager.Fullscreen = isChecked;
            window.SetFullscreen(SettingsManager.Fullscreen);
        };

        settingsMenu.Widgets = [saveButton, revertButton, settingsLabel, settingsBackButton, musicSlider, musicLabel, musicValue, soundSlider, soundLabel, soundValue, fpsSlider, fpsLabel, fpsValue, vsyncCheckbox, vsyncLabel, resolutionDropdown, fullscreenCheckbox, fullscreenLabel];
        LoadSettings(window);
        return settingsMenu;
    }
    private static void UpdateFPSLimit(Window window)
    {
        window.IsFixedTimeStep = SettingsManager.FPS <= 240;
        if (window.IsFixedTimeStep)
            window.TargetElapsedTime = TimeSpan.FromSeconds(1d / SettingsManager.FPS);
    }
    public static void WriteSettings()
    {
        // Write to settings.qkv in GameData/Persistent
        StateManager.WriteKeyValueFile("settings", new Dictionary<string, string>
        {
            { "ScreenResolution", $"{ScreenResolution.X}x{ScreenResolution.Y}" },
            { "FullScreen", Fullscreen.ToString() },
            { "FPS", FPS.ToString() },
            { "VSYNC", VSYNC.ToString() },
            { "MusicVolume", MusicVolume.ToString() },
            { "SoundVolume", SoundVolume.ToString() }
        });
    }
    public static void LoadSettings(Window window)
    {
        // Read from settings.qkv in GameData/Persistent
        var settings = StateManager.ReadKeyValueFile("settings");
        if (settings.TryGetValue("ScreenResolution", out string? res))
        {
            var parts = res.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                ScreenResolution = new Point(width, height);
                resolutionDropdown.SelectItem($"{ScreenResolution.X}x{ScreenResolution.Y}");
                window.SetResolution(ScreenResolution.X, ScreenResolution.Y);
            }
        }
        if (settings.TryGetValue("FullScreen", out string? fullscreenStr) && bool.TryParse(fullscreenStr, out bool fullscreen))
        {
            Fullscreen = fullscreen;
            fullscreenCheckbox.SetValue(Fullscreen);
            window.SetFullscreen(Fullscreen);
        }
        if (settings.TryGetValue("FPS", out string? fpsStr) && int.TryParse(fpsStr, out int fps))
        {
            FPS = fps;
            fpsSlider.SetValue(FPS > 240 ? 1 : (FPS - 30) / 211f);
            UpdateFPSLimit(window);
        }
        if (settings.TryGetValue("VSYNC", out string? vsyncStr) && bool.TryParse(vsyncStr, out bool vsync))
        {
            VSYNC = vsync;
            vsyncCheckbox.SetValue(VSYNC);
            window.SetVsync(vsync);
        }
        if (settings.TryGetValue("MusicVolume", out string? musicVolStr) && float.TryParse(musicVolStr, out float musicVol))
        {
            MusicVolume = musicVol;
            musicSlider.SetValue(musicVol);
        }
        if (settings.TryGetValue("SoundVolume", out string? soundVolStr) && float.TryParse(soundVolStr, out float soundVol))
        {
            SoundVolume = soundVol;
            soundSlider.SetValue(soundVol);
        }
    }
}
