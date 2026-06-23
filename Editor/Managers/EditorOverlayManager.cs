using Quest.World;
using System.Text;

namespace Quest.Editor.Managers;

public class EditorOverlayManager
{
    private static readonly Color[] FrameTimeColors = {
        Color.Purple, new(255, 128, 128), new(128, 255, 128), new(255, 255, 180), new(128, 255, 255),
        Color.Brown, Color.Gray, new(192, 128, 64), new(64, 128, 192), new(192, 192, 64),
        new(64, 192, 128), new(192, 64, 128), new(160, 80, 0), new(80, 160, 0), new(0, 160, 80),
        new(160, 0, 80), new(96, 96, 192), new(192, 96, 96), new(96, 192, 96), new(192, 192, 96)
    };
    // Managers and Devices
    public GameManager GameManager { get; private set; }
    public SpriteBatch Batch { get; private set; }
    public GraphicsDevice Graphics { get; private set; }
    private LevelManager LevelManager => GameManager.LevelManager;
    // 
    private readonly StringBuilder DebugSb;
    private readonly StringBuilder FrameTimeSb;
    private float CacheDelta;
    private Dictionary<string, double> FrameTimes = [];
    private RenderTarget2D Minimap = null!;
    private bool RebuildMiniMapFlag = true;
    public EditorOverlayManager(GameManager gameManager, SpriteBatch batch, GraphicsDevice graphics)
    {
        GameManager = gameManager;
        Batch = batch;
        Graphics = graphics;
        DebugSb = new();
        FrameTimeSb = new();
    }
    public void Update()
    {
        if (RebuildMiniMapFlag)
            RebuildMiniMap();
    }
    public void DrawTileOverlay(SpriteBatch spriteBatch, TileTypeID selection, Tile? mouseTile)
    {
        if (mouseTile is Stairs stair)
            DrawBottomInfo(spriteBatch, selection, $"[Stairs] dest: '{stair.DestLevel}' @ {stair.Dest}");
        else if (mouseTile is Door door)
            DrawBottomInfo(spriteBatch, selection, $"[Door] key: {(door.Key == null ? "NUL" : $"'{door.Key.Name}' x{door.Key.Amount}")} consume: {door.ConsumeKey}");
        else if (mouseTile is Chest chest)
            DrawBottomInfo(spriteBatch, selection, $"[Chest] gen: '{chest.LootGenerator.FileName}' key: {(chest.Key == null ? "NUL" : $"'{chest.Key.Name}' x{chest.Key.Amount}")} consume: {chest.ConsumeKey}");
        else if (mouseTile is Lamp lamp)
            DrawBottomInfo(spriteBatch, selection, $"[Lamp] radius: {lamp.LightRadius}");
        else if (mouseTile is DisplayCase displayCase)
            DrawBottomInfo(spriteBatch, selection, $"[Display Case] item: {displayCase.Container.Items[0]?.Name} x{displayCase.Container.Items[0]?.Amount}");
        else
            DrawBottomInfo(spriteBatch, selection, $"[{mouseTile?.Type.Texture}]");
    }
    public void DrawBottomInfo(SpriteBatch spriteBatch, TileTypeID tileSelection, string text)
    {
        text = $"{tileSelection} | {text}";
        Vector2 textSize = Arial.MeasureString(text);
        Vector2 pos = new(Constants.Middle.X - textSize.X / 2, Constants.NativeResolution.Y - textSize.Y - 3);
        spriteBatch.FillRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Gray * 0.5f);
        spriteBatch.DrawRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Black * 0.5f);
        spriteBatch.DrawString(Arial, text, pos, Color.Black);
    }
    public void DrawBiomes()
    {
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                Point loc = new(x, y);
                Point dest = loc * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
                BiomeType? biome = LevelManager.GetBiome(loc);
                Color color = biome == null ? Color.Magenta : Biome.Colors[(int)biome];
                GameManager.Batch.Draw(Textures[TextureID.TileOutline], dest.ToVector2(), LevelManager.BiomeTextureSource(loc), color, 0, Vector2.Zero, Constants.TileSizeScale, SpriteEffects.None, 1.0f);
            }
        }
    }
    public void DrawMiniMap()
    {
        DebugManager.StartBenchmark("DrawMinimap");

        // Frame
        GameManager.Batch.DrawRectangle(new(7, Constants.NativeResolution.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Draw minimap texture
        if (Minimap != null)
            Batch.Draw(Minimap, new Rectangle(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10);
        Batch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }
    public void RebuildMiniMap()
    {
        Minimap = new RenderTarget2D(Graphics, Constants.MapSize.X, Constants.MapSize.Y);
        Graphics.SetRenderTarget(Minimap);
        Graphics.Clear(Color.Transparent);
        Batch.Begin();

        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                Tile tile = GameManager.LevelManager.GetTile(new Point(x, y))!;
                Batch.DrawPoint(new(x, y), Constants.MiniMapColors[(byte)tile.Type.ID]);
            }
        }

        Batch.End();
        Graphics.SetRenderTarget(null);
        RebuildMiniMapFlag = false;
    }
    public void FlagRebuildMinimap() { RebuildMiniMapFlag = true; }
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        FrameTimeSb.Clear();
        foreach (var kv in FrameTimes)
        {
            FrameTimeSb.Append(kv.Key);
            FrameTimeSb.Append(": ");
            FrameTimeSb.AppendFormat("{0:0.0}ms", kv.Value);
            FrameTimeSb.Append('\n');
        }

        if (!DebugManager.FrameInfo) return;

        FillRectangle(Batch, new(Constants.NativeResolution.X - 190, 0, 190, (int)boxHeight), Color.Black * 0.8f);
        Batch.DrawString(Arial, FrameTimeSb.ToString(), new Vector2(Constants.NativeResolution.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        DebugSb.Clear();
        DebugSb.Append("FPS: ");
        DebugSb.AppendFormat("{0:0.0}", CacheDelta != 0 ? 1f / CacheDelta : 0);
        DebugSb.Append("\nTotalTime: ");
        DebugSb.AppendFormat("{0:0.00}", GameManager.TotalTime);
        DebugSb.Append("\nCamera: ");
        DebugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        DebugSb.Append("\nCoord: ");
        DebugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        if (EditorManager.MouseTile != null)
        {
            DebugSb.Append("\nMouse Tile: ");
            DebugSb.AppendFormat("{0:0},{1:0}", EditorManager.MouseTile.X, EditorManager.MouseTile.Y);
        }
        DebugSb.Append("\nLevel: ");
        DebugSb.Append(LevelManager.Level.Path);

        if (!DebugManager.TextInfo) return;

        FillRectangle(Batch, new(0, 0, 200, 150), Color.Black * 0.8f);
        Batch.DrawString(Arial, DebugSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        // Background
        FillRectangle(Batch, new(Constants.NativeResolution.X - 320, Constants.NativeResolution.Y - FrameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        FillRectangle(Batch, new(Constants.NativeResolution.X - 310, Constants.NativeResolution.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in FrameTimes)
        {
            Batch.DrawString(Arial, process.Key, new Vector2(Constants.NativeResolution.X - Arial.MeasureString(process.Key).X - 5, Constants.NativeResolution.Y - 20 * c - 60), FrameTimeColors[c]);
            FillRectangle(Batch, new Rectangle(Constants.NativeResolution.X - 310 + start, Constants.NativeResolution.Y - 40, (int)(process.Value / (CacheDelta * 1000) * 300), 25), FrameTimeColors[c]);
            start += (int)(process.Value / (CacheDelta * 1000)) * 300;
            c++;
        }
    }
    public void UpdateFrameTimes()
    {
        FrameTimes.Clear();
        FrameTimes = new(DebugManager.FrameTimes);
        CacheDelta = GameManager.DeltaTime;
    }
    public string GetDebugString() => DebugSb.ToString();
}
