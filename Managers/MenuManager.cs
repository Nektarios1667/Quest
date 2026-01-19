using Microsoft.Xna.Framework.Content;
using MonoGUI;
using Quest.Editor;
using System.IO;
using System.Linq;

namespace Quest.Managers;
public class MenuManager
{
    public GUI MainMenu { get; private set; }
    public GUI SettingsMenu { get; private set; }
    public GUI CreditsMenu { get; private set; }
    public GUI LevelSelectMenu { get; private set; }
    public GUI PauseMenu { get; private set; }
    public GUI DebugMenu { get; private set; }
    private readonly GameManager gameManager;
    private readonly PlayerManager playerManager;
    // Widgets
    private readonly ScrollBox worlds;
    private readonly ScrollBox saves;
    private readonly Label saveListLabel;
    public MenuManager(Window window, SpriteBatch batch, ContentManager content, GameManager gameManager, PlayerManager playerManager)
    {
        this.gameManager = gameManager;
        this.playerManager = playerManager;

        // Main Menu
        MainMenu = new(window, batch, PixelOperator);
        MainMenu.LoadContent(content, "Images/Gui");
        Button startButton = new(MainMenu, new(Constants.Middle.X - 150, 220), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, LevelSelect, [], text: "Start", font: PixelOperatorSubtitle, border: 0);
        Button continueButton = new(MainMenu, new(Constants.Middle.X - 150, 310), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, ContinueSave, [], text: "Continue", font: PixelOperatorSubtitle, border: 0);
        Button settingsButton = new(MainMenu, new(Constants.Middle.X - 150, 400), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => StateManager.State = GameState.Settings, [], text: "Settings", font: PixelOperatorSubtitle, border: 0);
        Button creditsButton = new(MainMenu, new(Constants.Middle.X - 150, 490), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => StateManager.State = GameState.Credits, [], text: "Credits", font: PixelOperatorSubtitle, border: 0);
        Button exitButton = new(MainMenu, new(Constants.Middle.X - 150, 580), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => window.Exit(), [], text: "Exit", font: PixelOperatorSubtitle, border: 0);
        MainMenu.Widgets = [startButton, continueButton, settingsButton, creditsButton, exitButton];

        // Settings Menu
        SettingsMenu = new(window, batch, PixelOperator);
        SettingsMenu.LoadContent(content, "Images/Gui");
        Label settingsLabel = new(SettingsMenu, new(Constants.Middle.X - 130, 50), Color.White, "Settings", PixelOperatorTitle);
        Button settingsBackButton = new(SettingsMenu, new(20, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);

        HorizontalSlider musicSlider = new(SettingsMenu, new(200, 300), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        musicSlider.Value = SoundManager.MusicVolume;
        musicSlider.ValueChanged += (value) => SoundManager.MusicVolume = value;
        Label musicLabel = new(SettingsMenu, new(200, 250), Color.White, "Music Volume", PixelOperator);
        Label musicValue = new(SettingsMenu, new(520, 285), Color.White, $"{(int)(musicSlider.Value * 100)}%", PixelOperator);
        musicSlider.ValueChanged += (value) => musicValue.Text = $"{(int)(value * 100)}%";

        HorizontalSlider soundSlider = new(SettingsMenu, new(200, 365), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        soundSlider.Value = SoundManager.SoundVolume;
        soundSlider.ValueChanged += (value) => SoundManager.SoundVolume = value;
        Label soundLabel = new(SettingsMenu, new(200, 315), Color.White, "Sound Volume", PixelOperator);
        Label soundValue = new(SettingsMenu, new(520, 350), Color.White, $"{(int)(soundSlider.Value * 100)}%", PixelOperator);
        soundSlider.ValueChanged += (value) => soundValue.Text = $"{(int)(value * 100)}%";

        HorizontalSlider saturationSlider = new(SettingsMenu, new(200, 430), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        saturationSlider.Value = 0.5f;
        saturationSlider.ValueChanged += (value) => window.Grading.Parameters["Saturation"].SetValue(value * 2);
        Label saturationLabel = new(SettingsMenu, new(200, 380), Color.White, "Saturation", PixelOperator);
        Label saturationValue = new(SettingsMenu, new(520, 415), Color.White, $"{(int)(saturationSlider.Value * 2)}", PixelOperator);
        saturationSlider.ValueChanged += (value) => saturationValue.Text = $"{value * 2:F1}x";

        HorizontalSlider contrastSlider = new(SettingsMenu, new(200, 495), 300, Color.Gray, Color.White, thickness: 5, size: 12);
        contrastSlider.Value = 0.5f;
        contrastSlider.ValueChanged += (value) => window.Grading.Parameters["Contrast"].SetValue(value + 0.5f);
        Label contrastLabel = new(SettingsMenu, new(200, 445), Color.White, "Contrast", PixelOperator);
        Label contrastValue = new(SettingsMenu, new(520, 480), Color.White, $"{(int)(contrastSlider.Value + 0.5f)}", PixelOperator);
        contrastSlider.ValueChanged += (value) => contrastValue.Text = $"{value + 0.5f:F1}x";

        SettingsMenu.Widgets = [settingsLabel, settingsBackButton, musicSlider, musicLabel, musicValue, soundSlider, soundLabel, soundValue, saturationSlider, saturationLabel, saturationValue, contrastSlider, contrastLabel, contrastValue];

        // Credits Menu
        CreditsMenu = new(window, batch, PixelOperator);
        CreditsMenu.LoadContent(content, "Images/Gui");
        Label creditsTitleLabel = new(CreditsMenu, new(Constants.Middle.X - 100, 50), Color.White, "Credits", PixelOperatorTitle);
        Button creditsBackButton = new(CreditsMenu, new(20, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);
        Label creditsLabel = new(CreditsMenu, new(150, 150), Color.White, "- Design and programming by Nektarios\n- Written in C# with MonoGame framework\n- Programming done in Visual Studio\n- Game assets made with Gimp\n- Sounds and music from Pixabay\n- Pixel font from DaFont\n- Main menu artwork generated by ChatGPT", PixelOperatorLarge);
        Label licenseLabel = new(CreditsMenu, new(15, Constants.NativeResolution.Y - 80), Color.White, "Code and assets licensed under Creative Commons Attribution-NonCommercial-ShareAlike (CC BY-NC-SA)\nhttps://creativecommons.org/licenses/by-nc-sa/4.0/", PixelOperator);
        CreditsMenu.Widgets = [creditsTitleLabel, creditsBackButton, creditsLabel, licenseLabel];

        // Level select
        LevelSelectMenu = new(window, batch, PixelOperator);
        LevelSelectMenu.LoadContent(content, "Images/Gui");

        worlds = new(LevelSelectMenu, new(220, 50), new(520, 600), Color.White, Color.Black * .6f, Color.LightBlue * .5f, border: 2, borderColor: Color.Cyan * .2f, troughColor: Color.Black * .6f, seperation: 0);
        saves = new(LevelSelectMenu, new(800, 50), new(300, 600), Color.White, Color.Black * .6f, Color.LightBlue * .5f, border: 2, borderColor: Color.Cyan * .2f, troughColor: Color.Black * .6f, seperation: 0) { Visible = false };

        Label worldListLabel = new(LevelSelectMenu, new(435, 5), Color.White, "Worlds", PixelOperatorLarge);
        saveListLabel = new(LevelSelectMenu, new(900, 5), Color.White, "Saves", PixelOperatorLarge);
        Button levelSelectBackButton = new(LevelSelectMenu, new(20, 20), new(100, 40), Color.White, Color.Black * 0.5f, ColorTools.NearBlack * 0.5f, StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);
        Button openButton = new(LevelSelectMenu, new(200, 660), new(180, 40), Color.White, Color.DarkGreen * 0.6f, Color.Green * 0.6f, OpenSave, [], text: "Open", border: 0);
        Button renameButton = new(LevelSelectMenu, new(400, 660), new(180, 40), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, Rename, [], text: "Rename", border: 0);
        Button refreshButton = new(LevelSelectMenu, new(600, 660), new(180, 40), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => LoadSaves(worlds.Selected), [], text: "Refresh", border: 0);
        Button deleteButton = new(LevelSelectMenu, new(800, 660), new(180, 40), Color.White, Color.DarkRed * 0.6f, Color.Red * 0.6f, DeleteSelectedSave, [], text: "Delete", border: 0);
        worlds.ItemSelected += (item) => { LoadSaves(item); saves.Visible = true; saveListLabel.Visible = true; };

        LevelSelectMenu.Widgets = [levelSelectBackButton, worlds, saves, openButton, deleteButton, renameButton, refreshButton, worldListLabel, saveListLabel];

        // Pause Menu
        PauseMenu = new(window, batch, PixelOperator);
        PauseMenu.LoadContent(content, "Images/Gui");
        Label pauseLabel = new(PauseMenu, new(Constants.Middle.X - 110, 150), Color.White, "PAUSED", PixelOperatorTitle);
        Button resumeButton = new(PauseMenu, new(Constants.Middle.X - 150, 300), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => StateManager.OverlayState = OverlayState.None, [], text: "Resume", font: PixelOperatorSubtitle, border: 0);
        Button quicksaveButton = new(PauseMenu, new(Constants.Middle.X - 150, 380), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => { StateManager.OverlayState = OverlayState.None; StateManager.SaveGameState(gameManager, playerManager); }, [], text: "Quick Save", font: PixelOperatorSubtitle, border: 0);
        Button pauseSettingsButton = new(PauseMenu, new(Constants.Middle.X - 150, 460), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => { StateManager.OverlayState = OverlayState.None; StateManager.State = GameState.Settings; }, [], text: "Settings", font: PixelOperatorSubtitle, border: 0);
        Button mainMenuButton = new(PauseMenu, new(Constants.Middle.X - 150, 540), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, ExitToMainMenu, [], text: "Main Menu", font: PixelOperatorSubtitle, border: 0);
        Button quitButton = new(PauseMenu, new(Constants.Middle.X - 150, 620), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => window.Exit(), [], text: "Quit", font: PixelOperatorSubtitle, border: 0);

        PauseMenu.Widgets = [resumeButton, quicksaveButton, pauseSettingsButton, mainMenuButton, quitButton, pauseLabel];

        // In-game debug
        DebugMenu = new(window, batch, PixelOperator);
        DebugMenu.LoadContent(content, "Images/Gui");
        HorizontalSlider timeSlider = new(DebugMenu, new(Constants.Middle.X, 20), 200, Color.Black, Color.Gray);
        timeSlider.ValueChanged += (value) => gameManager.DayTime = value * 500;
        Label timeLabel = new(DebugMenu, new(Constants.Middle.X - 100, 0), Color.Black, "Daytime");
        HorizontalSlider weatherSlider = new(DebugMenu, new(Constants.Middle.X, 40), 200, Color.Black, Color.Gray);
        //weatherSlider.ValueChanged += (value) => StateManager.currentWeatherNoise = value;
        Label weatherLabel = new(DebugMenu, new(Constants.Middle.X - 100, 20), Color.Black, "Weather");
        DebugMenu.Widgets = [timeSlider, timeLabel, weatherLabel, weatherSlider];
    }
    public void ExitToMainMenu()
    {
        StateManager.OverlayState = OverlayState.None;
        StateManager.State = GameState.MainMenu;
        gameManager.LevelManager.UnloadWorld(gameManager.LevelManager.Level.World);
    }
    public bool ContinueSave()
    {

        if (StateManager.ReadKeyValueFile("continue").TryGetValue("save", out var loadSave))
        {
            StateManager.State = GameState.Game;
            return StateManager.ReadGameState(gameManager, playerManager, loadSave);
        }
        // else
        StateManager.State = GameState.LevelSelect;
        return true;
    }
    public void LevelSelect()
    {
        worlds?.SelectItem("");
        if (saves != null)
            saves.Visible = false;
        saveListLabel.Visible = false;
        StateManager.State = GameState.LevelSelect;
    }
    public void RefreshWorldList()
    {
        worlds.Items.Clear();
        worlds.AddItems([.. Directory.GetDirectories("GameData\\Worlds").Select(d => d.Split('\\')[^1])]);
    }
    public void DeleteSelectedSave()
    {
        if (saves.Selected != null && saves.Selected != "(New Save)")
        {
            if (Constants.DEVMODE)
                File.Delete($"../../../GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv");
            File.Delete($"GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv");

            // Check continue save
            var continueData = StateManager.ReadKeyValueFile("continue");
            if (continueData.TryGetValue("save", out string? value) && value.Replace('\\', '/') == $"{worlds.Selected}/{saves.Selected}")
                continueData.Remove("save");
            StateManager.WriteKeyValueFile("continue", continueData);

            // Refresh
            LoadSaves(worlds.Selected);
        }
    }
    public void LoadSaves(string level)
    {
        saves.Items.Clear();
        string path = $"GameData/Worlds/{level}/saves";
        if (level == "") return;
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        var savesList = Directory.GetFiles(path, "*.qsv").Select(f => System.IO.Path.GetFileNameWithoutExtension(f)).ToArray();
        saves.AddItems(savesList);
        saves.AddItems("(New Save)");
    }
    public void OpenSave()
    {
        StateManager.State = GameState.Loading;
        if (saves.Selected == "(New Save)")
        {
            StateManager.CurrentSave = new($"{worlds.Selected}/{DateTime.Now:Save MM-dd-yy HH-mm-ss}");
            gameManager.LevelManager.ReadWorld(gameManager, worlds.Selected, reload: true);
            if (!gameManager.LevelManager.LoadLevel(gameManager, $"{worlds.Selected}/{worlds.Selected}"))
                gameManager.LevelManager.LoadLevel(gameManager, 0);
        } else 
            StateManager.ReadGameState(gameManager, playerManager, $"{worlds.Selected}/{saves.Selected}");

        StateManager.State = GameState.Game;
    }
    public void Rename()
    {
        // Check
        if (saves.Selected == null || saves.Selected == "(New Save)") return;

        // Rename
        var (success, values) = PopupFactory.ShowInputForm("Rename Save", [new("Name:", PopupFactory.IsAlphaNumeric)]);
        if (success && values.Length > 0 && !string.IsNullOrWhiteSpace(values[0]))
        {
            string oldPath = $"GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv";
            string newPath = $"GameData/Worlds/{worlds.Selected}/saves/{values[0]}.qsv";
            if (!File.Exists(newPath))
            {
                File.Move(oldPath, newPath);
                if (Constants.DEVMODE)
                    File.Move(oldPath.Replace("GameData", "../../../GameData"), newPath.Replace("GameData", "../../../GameData"));
            }
            else
                PopupFactory.ShowMessage("A save with that name already exists.", "Error");
        }
        LoadSaves(worlds.Selected);
    }
    public void Update(GameManager gameManager)
    {
        DebugManager.StartBenchmark("MenuUpdate");
        switch (StateManager.State)
        {
            case GameState.MainMenu:
                MainMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Settings:
                SettingsMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Credits:
                CreditsMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.LevelSelect:
                LevelSelectMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Game:
                //DebugMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Loading:
                DrawLoading();
                break;
        }

        switch (StateManager.OverlayState)
        {
            case OverlayState.Pause:
                PauseMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
        }


        DebugManager.EndBenchmark("MenuUpdate");
    }
    public void Draw()
    {
        DebugManager.StartBenchmark("MenuDraw");
        switch (StateManager.State)
        {
            case GameState.MainMenu:
                DrawMenu();
                break;
            case GameState.Settings:
                DrawSettings();
                break;
            case GameState.Credits:
                DrawCredits();
                break;
            case GameState.LevelSelect:
                DrawLevelSelection();
                break;
            case GameState.Game:
                //DebugMenu.Draw();
                break;
        }

        switch (StateManager.OverlayState)
        {
            case OverlayState.Pause:
                DrawPauseMenu();
                break;
        }
        DebugManager.EndBenchmark("MenuDraw");
    }
    private void DrawMenu()
    {

        Vector2 loc = Vector2.Zero - Vector2.Max(InputManager.MousePosition.ToVector2() / 50f, Vector2.Zero);
        gameManager.Batch.Draw(Textures[TextureID.MenuBackground], loc, null, Color.White, 0f, Vector2.Zero, Constants.NativeResolution.ToVector2() / TextureManager.Metadata[TextureID.MenuBackground].Size.ToVector2() + Vector2.One * 0.3f, SpriteEffects.None, 0.0f);
        Vector2 logoCenter = new(Constants.Middle.X - TextureManager.Metadata[TextureID.QuestTitle].Size.X / 2, 20);
        gameManager.Batch.Draw(Textures[TextureID.QuestTitle], logoCenter, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);

        MainMenu.Draw();
    }
    private void DrawSettings()
    {
        gameManager.Batch.Draw(Textures[TextureID.MenuBackground], new(0, 0), null, Color.White, 0f, Vector2.Zero, Constants.NativeResolution.ToVector2() / TextureManager.Metadata[TextureID.MenuBackground].Size.ToVector2(), SpriteEffects.None, 0.0f);
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        SettingsMenu.Draw();
    }
    private void DrawCredits()
    {
        gameManager.Batch.Draw(Textures[TextureID.MenuBackground], new(0, 0), null, Color.White, 0f, Vector2.Zero, Constants.NativeResolution.ToVector2() / TextureManager.Metadata[TextureID.MenuBackground].Size.ToVector2(), SpriteEffects.None, 0.0f);
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        CreditsMenu.Draw();
    }
    private void DrawLevelSelection()
    {
        gameManager.Batch.Draw(Textures[TextureID.MenuBackground], new(0, 0), null, Color.White, 0f, Vector2.Zero, Constants.NativeResolution.ToVector2() / TextureManager.Metadata[TextureID.MenuBackground].Size.ToVector2(), SpriteEffects.None, 0.0f);
        LevelSelectMenu.Draw();
    }
    private void DrawPauseMenu()
    {
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        PauseMenu.Draw();
    }
    private void DrawLoading()
    {
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black);
        string loadingText = "Loading...";
        Vector2 textSize = PixelOperatorTitle.MeasureString(loadingText);
        Vector2 position = new((Constants.NativeResolution.X - textSize.X) / 2, (Constants.NativeResolution.Y - textSize.Y) / 2);
        gameManager.Batch.DrawString(PixelOperatorTitle, loadingText, position, Color.White);
    }
}
