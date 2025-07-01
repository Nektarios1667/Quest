using Quest.Gui;

namespace Quest.Entities;

public class NPC
{
    public bool HasSpoken { get; set; }
    public bool IsTalking => DialogBox.IsVisible && DialogBox.IsSpeaking;
    public Dialog DialogBox { get; private set; }
    public Point Location { get; set; }
    public string Name { get; set; }
    public string Dialog { get; set; }
    public TextureID Texture { get; set; }
    public Color TextureColor { get; set; }
    public float Scale { get; set; }
    public bool Important { get; set; }
    // Private
    private Point tilemap { get; set; }
    private Point tilesize { get; set; }

    public NPC(UIManager uiManager, TextureID texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1, bool important = true)
    {
        HasSpoken = false;
        Important = important;
        Texture = texture;

        // Private
        tilemap = TextureManager.Metadata[Texture].TileMap;
        tilesize = TextureManager.Metadata[Texture].Size / tilemap;

        Location = location;
        Name = name;
        Dialog = dialog;
        TextureColor = textureColor == default ? Color.White : textureColor;
        Scale = scale;
        DialogBox = new Dialog(uiManager.Gui, new(Constants.Middle.X - 600, Constants.Window.Y - 190), new(1200, 100), new(100, 100, 100), Color.Black, $"[{name}] {dialog}", PixelOperator, borderColor: new(40, 40, 40)) { IsVisible = false };
        uiManager.Gui.Widgets.Add(DialogBox);
    }
    public void Draw(GameManager gameManager)
    {
        // Npc
        Vector2 origin = new(tilesize.X / 2, tilesize.Y);
        Point pos = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle + tilesize / Constants.TwoPoint;
        Rectangle source = GetAnimationSource(Texture, gameManager.TotalTime);
        DrawTexture(gameManager.Batch, Texture, pos, color: TextureColor, scale: Scale, source: source, origin: origin);
        // Debug
        if (DebugManager.DrawHitboxes)
            gameManager.Batch.FillRectangle(new((pos - tilesize).ToVector2(), source.Size.ToVector2() * Scale), Constants.DebugPinkTint);
    }
    public void Update(GameManager gameManager)
    {
        if (DialogBox.HasSpoken) HasSpoken = true;
        // Speaking
        if (Vector2.DistanceSquared(CameraManager.PlayerFoot.ToVector2() / Constants.TileSize.ToVector2(), Location.ToVector2() + Constants.HalfVec) <= 4)
        {
            DialogBox.IsVisible = true;
        }
        // Hiding if away
        else
        {
            DialogBox.IsVisible = false;
            DialogBox.Displayed = "";
        }
    }
}
