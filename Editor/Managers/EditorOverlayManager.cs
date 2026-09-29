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
    private Dictionary<string, float> FrameTimes = [];
    public RenderTarget2D? Minimap { get; set; } = null;
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

    }
    public void InvalidateMinimap() => Minimap = null;
    public static void DrawTileOverlay(SpriteBatch spriteBatch, Tile? previewTile, Tile? mouseTile)
    {
        // Selected tile
        if (previewTile != null)
        {
            string text = $"{previewTile.TypeID} @ {previewTile.Location}";
            if (previewTile is IHasLevelData dataTile) text += $" | {dataTile.GetDataString()}";
            DrawCornerInfo(spriteBatch, text, 0);
        }

        // Hovered tile
        if (mouseTile != null)
        {
            string text = $"{mouseTile.TypeID}";
            if (mouseTile is IHasLevelData dataTile) text += $" | {dataTile.GetDataString()}";
            DrawCornerInfo(spriteBatch, text, 24);
        }
    }
    public static void DrawDecalOverlay(SpriteBatch spriteBatch, DecalType selection, DecalType? mouseDecal)
    {
        DrawCornerInfo(spriteBatch, $"{selection}", 0);
        DrawCornerInfo(spriteBatch, $"{mouseDecal}", 24);
    }
    public static void DrawBiomeOverlay(SpriteBatch spriteBatch, BiomeType selection, BiomeType? mouseBiome) {
        DrawCornerInfo(spriteBatch, $"{selection}", 0);
        DrawCornerInfo(spriteBatch, $"{mouseBiome}", 24);
    }
    public static void DrawLootOverlay(SpriteBatch spriteBatch, ItemTypeID selection, int selectionAmount) {
        DrawCornerInfo(spriteBatch, $"{selection} x {selectionAmount}", 0);
    }
    public static void DrawCornerInfo(SpriteBatch spriteBatch, string text, int offsetY = 0)
    {
        Vector2 textSize = PixelOperatorVerySmall.MeasureString(text);
        Vector2 pos = new(MathF.Round(Constants.NativeResolution.X - textSize.X - 4), 4 + offsetY);
        spriteBatch.FillRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Gray * 0.6f);
        spriteBatch.DrawRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Black * 0.6f, thickness: 2);
        spriteBatch.DrawString(PixelOperatorVerySmall, text, pos, Color.White);
    }
    public void DrawBiomes()
    {
        Point start = CameraManager.TopLeftTileCoord;
        Point end = CameraManager.BottomRightTileCoord;
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                Point loc = new(x, y);
                Point dest = CameraManager.TileToScreen(loc);
                BiomeType? biome = LevelManager.GetBiome(loc);
                Color color = biome == null ? Color.Magenta : Biome.BiomeTileColors[(int)biome];
                GameManager.Batch.Draw(Textures[TextureID.TileOutline], dest.ToVector2(), LevelManager.BiomeTextureSource(loc), color, 0, Vector2.Zero, Constants.TileSizeScale, SpriteEffects.None, 1.0f);
            }
        }
    }
    public void DrawTransitions()
    {
        foreach (LevelTransition transition in LevelManager.Level.Transitions)
        {
            Point dest = CameraManager.TileToScreen(transition.Area.Location);
            Rectangle rect = new(dest, transition.Area.Size * Constants.TileSize);

            // Outline
            GameManager.Batch.DrawRectangle(rect, color: Color.Orange, thickness: 4);
            // Info
            GameManager.Batch.DrawString(PixelOperator, transition.ToString(), dest.ToVector2() + new Vector2(6, 6), Color.Green);
        }
    }
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
        DebugSb.Append("\nReal Time: ");
        DebugSb.AppendFormat("{0:0.00}", GameManager.RealTime);
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
        foreach (var process in FrameTimes)
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
        CacheDelta = GameManager.DeltaRealTime;
    }
    public string GetDebugString() => DebugSb.ToString();
}
