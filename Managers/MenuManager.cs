using Microsoft.Xna.Framework.Content;
using MonoGUI;
using Quest.Editor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.Managers;

public class MenuManager
{
    public static Vector2 MenuBackgroundScale => Constants.NativeResolution.ToVector2() / TextureManager.Metadata[TextureID.MenuBackground].Size.ToVector2();
    public GUI MainMenu { get; private set; }
    public GUI SettingsMenu { get; private set; }
    public GUI CreditsMenu { get; private set; }
    public GUI LevelSelectMenu { get; private set; }
    public GUI LoadingMenu { get; private set; }
    public GUI PauseMenu { get; private set; }
    public GUI DebugMenu { get; private set; }

    private readonly GameManager gameManager;
    private readonly PlayerManager playerManager;
    // Widgets
    private readonly ScrollBox worlds;
    private readonly ScrollBox saves;
    private readonly Label saveListLabel;
    private static Label currentlyLoadingLabel = null!;
    public static void SetCurrentlyLoading(string loading)
    {
        currentlyLoadingLabel.Text = loading;
        currentlyLoadingLabel.Location = new((int)(Constants.Middle.X - currentlyLoadingLabel.Font!.MeasureString(loading).X / 2), currentlyLoadingLabel.Location.Y);
    }
    public MenuManager(Window window, SpriteBatch batch, ContentManager content, Game game, GameManager gameManager, PlayerManager playerManager)
    {
        this.gameManager = gameManager;
        this.playerManager = playerManager;

        // Main Menu
        MainMenu = new(window, batch, PixelOperator);
        MainMenu.LoadContent();
        Button startButton = new(MainMenu, new(Constants.Middle.X - 150, 220), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, LevelSelect, [], text: "Start", font: PixelOperatorSubtitle, border: 0);
        Button continueButton = new(MainMenu, new(Constants.Middle.X - 150, 310), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, ContinueSaveButton, [], text: "Continue", font: PixelOperatorSubtitle, border: 0);
        Button settingsButton = new(MainMenu, new(Constants.Middle.X - 150, 400), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => gameManager.StateManager.State = GameState.Settings, [], text: "Settings", font: PixelOperatorSubtitle, border: 0);
        Button creditsButton = new(MainMenu, new(Constants.Middle.X - 150, 490), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => gameManager.StateManager.State = GameState.Credits, [], text: "Credits", font: PixelOperatorSubtitle, border: 0);
        Button exitButton = new(MainMenu, new(Constants.Middle.X - 150, 580), new(300, 70), Color.White, Color.Black * 0.6f, ColorTools.GrayBlack * 0.6f, () => window.Exit(), [], text: "Exit", font: PixelOperatorSubtitle, border: 0);
        MainMenu.AddWidgets(startButton, continueButton, settingsButton, creditsButton, exitButton);

        // Settings Menu
        SettingsMenu = SettingsManager.CreateSettingsMenu(window, game, gameManager, batch, content);

        // Credits Menu
        CreditsMenu = new(window, batch, PixelOperator);
        CreditsMenu.LoadContent();
        Label creditsTitleLabel = new(CreditsMenu, new(Constants.Middle.X - 100, 50), Color.White, "Credits", PixelOperatorTitle);
        Button creditsBackButton = new(CreditsMenu, new(20, 20), new(100, 40), Color.White, Color.Gray * 0.5f, Color.DarkGray * 0.5f, gameManager.StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);
        Label creditsLabel = new(CreditsMenu, new(150, 150), Color.White, "- Design and programming by Nektarios\n- Written in C# with MonoGame framework\n- Programming done in Visual Studio\n- Game assets made with Gimp\n- Sounds and music from Pixabay\n- Pixel font from DaFont", PixelOperatorLarge);
        Label licenseLabel = new(CreditsMenu, new(15, Constants.NativeResolution.Y - 80), Color.White, "Code and assets licensed under Creative Commons Attribution-NonCommercial-ShareAlike (CC BY-NC-SA)\nhttps://creativecommons.org/licenses/by-nc-sa/4.0/", PixelOperator);
        CreditsMenu.AddWidgets(creditsTitleLabel, creditsBackButton, creditsLabel, licenseLabel);

        // Level select
        LevelSelectMenu = new(window, batch, PixelOperator);
        LevelSelectMenu.LoadContent();

        worlds = new(LevelSelectMenu, new(220, 50), new(520, 600), Color.White, Color.Black * .6f, Color.LightBlue * .5f, border: 2, borderColor: Color.Cyan * .2f, troughColor: Color.Black * .6f, seperation: 0);
        saves = new(LevelSelectMenu, new(800, 50), new(300, 600), Color.White, Color.Black * .6f, Color.LightBlue * .5f, border: 2, borderColor: Color.Cyan * .2f, troughColor: Color.Black * .6f, seperation: 0) { Visible = false };

        Label worldListLabel = new(LevelSelectMenu, new(435, 5), Color.White, "Worlds", PixelOperatorLarge);
        saveListLabel = new(LevelSelectMenu, new(900, 5), Color.White, "Saves", PixelOperatorLarge);
        Button levelSelectBackButton = new(LevelSelectMenu, new(20, 20), new(100, 40), Color.White, Color.Black * 0.5f, ColorTools.NearBlack * 0.5f, gameManager.StateManager.RevertGameState, [], text: "Back", font: PixelOperator, border: 0);
        Button openButton = new(LevelSelectMenu, new(200, 660), new(180, 40), Color.White, Color.DarkGreen * 0.6f, Color.Green * 0.6f, OpenSave, [], text: "Open", border: 0);
        Button renameButton = new(LevelSelectMenu, new(400, 660), new(180, 40), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, Rename, [], text: "Rename", border: 0);
        Button refreshButton = new(LevelSelectMenu, new(600, 660), new(180, 40), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => LoadSaves(worlds.Selected), [], text: "Refresh", border: 0);
        Button deleteButton = new(LevelSelectMenu, new(800, 660), new(180, 40), Color.White, Color.DarkRed * 0.6f, Color.Red * 0.6f, DeleteSelectedSave, [], text: "Delete", border: 0);
        worlds.ItemSelected += (item) => { LoadSaves(item); saves.Visible = true; saveListLabel.Visible = true; };

        LevelSelectMenu.AddWidgets(levelSelectBackButton, worlds, saves, openButton, deleteButton, renameButton, refreshButton, worldListLabel, saveListLabel);

        // Loading menu
        LoadingMenu = new(window, batch, PixelOperator);
        LoadingMenu.LoadContent();
        Label loadingLabel = new(LoadingMenu, new(Constants.Middle.X - 130, 50), Color.White, "Loading", PixelOperatorTitle);
        ProgressBar progressBar = new(LoadingMenu, new(Constants.Middle.X - 250, 150), new(500, 40), ColorTools.NearBlack * 0.6f, Color.White * 0.6f, border: 0, textColor: Color.White, showPercentage: true);
        currentlyLoadingLabel = new(LoadingMenu, new(Constants.Middle.X - 150, 200), Color.White * 0.6f, "", PixelOperator);
        gameManager.LevelManager.LoadingProgressed += (prog) => progressBar.SetValue(prog);

        LoadingMenu.AddWidgets(loadingLabel, progressBar, currentlyLoadingLabel);

        // Pause Menu
        PauseMenu = new(window, batch, PixelOperator);
        PauseMenu.LoadContent();
        Label pauseLabel = new(PauseMenu, new(Constants.Middle.X - 120, 50), Color.White, "PAUSED", PixelOperatorTitle);
        Button resumeButton = new(PauseMenu, new(Constants.Middle.X - 150, 200), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => gameManager.StateManager.OverlayState = OverlayState.None, [], text: "Resume", font: PixelOperatorSubtitle, border: 0);
        ProgressBar savingProgressBar = new(PauseMenu, new(Constants.Middle.X - 150, 600), new(300, 40), ColorTools.NearBlack * 0.6f, Color.White * 0.6f, border: 0, textColor: Color.White, showPercentage: true);
        Button quicksaveButton = new(PauseMenu, new(Constants.Middle.X - 150, 280), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => { SaveManager.SaveGameStateAsync(gameManager, playerManager); savingProgressBar.Show(); }, [], text: "Quick Save", font: PixelOperatorSubtitle, border: 0);
        Button pauseSettingsButton = new(PauseMenu, new(Constants.Middle.X - 150, 360), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => { gameManager.StateManager.OverlayState = OverlayState.None; gameManager.StateManager.State = GameState.Settings; }, [], text: "Settings", font: PixelOperatorSubtitle, border: 0);
        Button mainMenuButton = new(PauseMenu, new(Constants.Middle.X - 150, 440), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, ExitToMainMenu, [], text: "Main Menu", font: PixelOperatorSubtitle, border: 0);
        Button quitButton = new(PauseMenu, new(Constants.Middle.X - 150, 520), new(300, 75), Color.White, Color.Transparent, ColorTools.GrayBlack * 0.5f, () => window.Exit(), [], text: "Quit", font: PixelOperatorSubtitle, border: 0);
        savingProgressBar.Hide();
        SaveManager.LoadingProgressed += (prog) =>
        {
            savingProgressBar.SetValue(prog);
            if (prog >= 1)
            {
                savingProgressBar.Hide();
                gameManager.StateManager.OverlayState = OverlayState.None;
            }
        };

        PauseMenu.AddWidgets(resumeButton, quicksaveButton, pauseSettingsButton, mainMenuButton, quitButton, pauseLabel, savingProgressBar);

        // In-game debug
        DebugMenu = new(window, batch, PixelOperator);
        DebugMenu.LoadContent();
        HorizontalSlider timeSlider = new(DebugMenu, new(Constants.Middle.X, 20), 200, Color.Black, Color.Gray);
        timeSlider.ValueChanged += (value) => GameManager.DayTime = value * 500;
        Label timeLabel = new(DebugMenu, new(Constants.Middle.X - 100, 0), Color.Black, "Daytime");
        DebugMenu.AddWidgets(timeSlider, timeLabel);
    }
    public void ExitToMainMenu()
    {
        StatusManager.ClearAllStatusEffects(gameManager, playerManager);

        gameManager.StateManager.OverlayState = OverlayState.None;
        gameManager.StateManager.State = GameState.MainMenu;

        SoundtrackManager.StopSoundtrack();

        gameManager.LevelManager.UnloadWorld(gameManager.LevelManager.Level.WorldName);
    }
    public async void ContinueSaveButton()
    {
        try
        {
            await ContinueSave();
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString(), true);
        }
    }
    public async Task<bool> ContinueSave()
    {

        if (SaveManager.ReadKeyValueFile("Persistent/continue").TryGetValue("save", out var loadSave))
        {
            gameManager.StateManager.State = GameState.Loading;
            bool success = await SaveManager.ReadGameState(gameManager, playerManager, new(loadSave));
            gameManager.StateManager.State = GameState.Game;
            return success;
        }
        // else
        gameManager.StateManager.State = GameState.LevelSelect;
        return true;
    }
    public void LevelSelect()
    {
        worlds?.SelectItem("");
        if (saves != null)
            saves.Visible = false;
        saveListLabel.Visible = false;
        gameManager.StateManager.State = GameState.LevelSelect;
    }
    public void RefreshWorldList()
    {
        worlds.Items.Clear();
        if (Directory.Exists("GameData/Worlds"))
            worlds.AddItems([.. Directory.GetDirectories("GameData/Worlds").Select(d => d.Split('\\')[^1])]);
        else
            Logger.Error("Worlds directory not found. Please ensure that the 'GameData/Worlds' directory exists.");
    }
    public void DeleteSelectedSave()
    {
        if (saves.Selected != null && saves.Selected != "(New Save)")
        {
            if (Constants.DEVMODE && File.Exists($"../../../GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv"))
                File.Delete($"../../../GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv");
            if (File.Exists($"GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv"))
                File.Delete($"GameData/Worlds/{worlds.Selected}/saves/{saves.Selected}.qsv");

            // Check continue save
            var continueData = SaveManager.ReadKeyValueFile("Persistent/continue");
            if (continueData.TryGetValue("save", out string? value) && value.Replace('\\', '/') == $"{worlds.Selected}/{saves.Selected}")
                continueData.Remove("save");
            SaveManager.WriteKeyValueFile("Persistent/continue", continueData);

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
    public async void OpenSave()
    {
        gameManager.StateManager.State = GameState.Loading;
        if (saves.Selected == "(New Save)")
        {
            SaveManager.CurrentSave = new($"{worlds.Selected}/{DateTime.Now:Save MM-dd-yy HH-mm-ss}");

            await LevelFileManager.ReadWorldAsync(gameManager, worlds.Selected, reload: true);

            if (!gameManager.LevelManager.LoadLevel(gameManager, $"{worlds.Selected}/{worlds.Selected}"))
                gameManager.LevelManager.LoadLevel(gameManager, 0);
        }
        else
            await SaveManager.ReadGameState(gameManager, playerManager, new($"{worlds.Selected}/{saves.Selected}"));

        gameManager.StateManager.State = GameState.Game;
    }
    public void Rename()
    {
        // Check
        if (saves.Selected == null || saves.Selected == "(New Save)") return;

        // Rename
        var (success, values) = PopupFactory.ShowInputForm("Rename Save", [new("Name:", PopupFactory.IsAlphaNum)]);
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

        switch (gameManager.StateManager.State)
        {
            case GameState.MainMenu:
                MainMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Settings:
                SettingsMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Credits:
                CreditsMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.LevelSelect:
                LevelSelectMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Loading:
                LoadingMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Game:
                //DebugMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
        }

        switch (gameManager.StateManager.OverlayState)
        {
            case OverlayState.Pause:
                PauseMenu.Update(GameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
        }


        DebugManager.EndBenchmark("MenuUpdate");
    }
    public void Draw(SpriteBatch batch)
    {
        DebugManager.StartBenchmark("MenuDraw");

        switch (gameManager.StateManager.State)
        {
            case GameState.MainMenu:
                DrawMenu(batch);
                break;
            case GameState.Settings:
                DrawSettings(batch);
                break;
            case GameState.Credits:
                DrawCredits(batch);
                break;
            case GameState.LevelSelect:
                DrawLevelSelection(batch);
                break;
            case GameState.Loading:
                DrawLoading(batch);
                break;
            case GameState.Game:
                //DebugMenu.Draw(batch);
                break;
        }

        switch (gameManager.StateManager.OverlayState)
        {
            case OverlayState.Pause:
                DrawPauseMenu(batch);
                break;
        }
        DebugManager.EndBenchmark("MenuDraw");
    }
    private void DrawMenu(SpriteBatch batch)
    {

        TextureManager.DrawTexture(batch, TextureID.MenuBackground, Point.Zero, scale: MenuBackgroundScale);
        Vector2 logoCenter = new(Constants.Middle.X - TextureManager.Metadata[TextureID.QuestTitle].Size.X / 2, 20);
        gameManager.Batch.Draw(Textures[TextureID.QuestTitle], logoCenter, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);

        MainMenu.Draw();
    }
    private void DrawSettings(SpriteBatch batch)
    {
        TextureManager.DrawTexture(batch, TextureID.MenuBackground, Point.Zero, scale: MenuBackgroundScale);
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        SettingsMenu.Draw();
    }
    private void DrawCredits(SpriteBatch batch)
    {
        TextureManager.DrawTexture(batch, TextureID.MenuBackground, Point.Zero, scale: MenuBackgroundScale);
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        CreditsMenu.Draw();
    }
    private void DrawLevelSelection(SpriteBatch batch)
    {
        TextureManager.DrawTexture(batch, TextureID.MenuBackground, Point.Zero, scale: MenuBackgroundScale);
        LevelSelectMenu.Draw();
    }
    private void DrawLoading(SpriteBatch batch)
    {
        TextureManager.DrawTexture(batch, TextureID.MenuBackground, Point.Zero, scale: MenuBackgroundScale);
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        LoadingMenu.Draw();
    }
    private void DrawPauseMenu(SpriteBatch batch)
    {
        gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.NativeResolution), Color.Black * 0.6f);
        PauseMenu.Draw();
    }
}
