using Quest.Gui;

namespace Quest.Entities;

public struct ShopOption
{
    public Item Item;
    public Item Cost;
}
public class NPC
{
    public static Dialog? DialogBox { get; set; }
    public static List<(NPC npc, float dist)> NPCsNearby { get; set; } = [];
    public bool HasSpoken { get; set; }
    public bool IsTalking => DialogBox != null && DialogBox.IsVisible && DialogBox.IsSpeaking;
    public ShopOption[] ShopOptions { get; private set; }
    public Point Location { get; set; }
    public string Name { get; set; }
    public string Dialog { get; set; }
    public TextureID Texture { get; set; }
    public Color TextureColor { get; set; }
    public float Scale { get; set; }
    // Private
    private Point tilemap;
    private Point tilesize;

    public NPC(OverlayManager uiManager, TextureID texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1)
    {
        HasSpoken = false;
        Texture = texture;

        // Private
        tilemap = TextureManager.Metadata[Texture].TileMap;
        tilesize = TextureManager.Metadata[Texture].Size / tilemap;

        Location = location;
        Name = name;
        Dialog = dialog;
        TextureColor = textureColor == default ? Color.White : textureColor;
        Scale = scale;
    }
    public void Draw(GameManager gameManager)
    {
        // Npc
        Vector2 origin = new(tilesize.X / 2, tilesize.Y);
        Point pos = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle + tilesize / Constants.TwoPoint;
        Rectangle source = GetAnimationSource(Texture, gameManager.GameTime);
        DrawTexture(gameManager.Batch, Texture, pos, color: TextureColor, scale: Scale * Constants.NPCScale, source: source, origin: origin);
        // Debug
        if (DebugManager.DrawHitboxes)
            FillRectangle(gameManager.Batch, new(pos - tilesize, (source.Size.ToVector2() * Scale).ToPoint()), Constants.DebugPinkTint);
    }
    public void Update(GameManager gameManager)
    {
        // Mark as dialogue possibility
        float dist = Vector2.DistanceSquared(CameraManager.PlayerFoot.ToVector2() / Constants.TileSize.ToVector2(), Location.ToVector2() + Constants.HalfVec);
        if (dist <= 4)
            NPCsNearby.Add((this, dist));
    }
}
