using Quest.Gui;

namespace Quest.Entities;

public struct ShopOption(Item item, Item? cost)
{
    public Item Item = item;
    public Item? Cost = cost;
}

public class NPC
{
    public static Dialog? DialogBox { get; set; }
    public static List<(NPC npc, float dist)> NPCsNearby { get; set; } = [];
    public List<ShopOption> ShopOptions { get; private set; } = [];
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
        Texture = texture;

        // Private
        tilemap = TextureManager.Metadata[Texture].TileMap;
        tilesize = TextureManager.Metadata[Texture].Size / tilemap;

        Location = location;
        Name = name;
        Dialog = dialog;
        TextureColor = textureColor == default ? Color.White : textureColor;
        Scale = scale;
        AddShopOption(new(ItemTypes.SteelSword, 1), new(ItemTypes.DeltaCoin, 3));
        AddShopOption(new(ItemTypes.SteelSword, 1), new(ItemTypes.PhiCoin, 7));
        AddShopOption(new(ItemTypes.Lantern, 1), new(ItemTypes.PhiCoin, 4));
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
    public void AddShopOption(ShopOption option)
    {
        ShopOptions.Add(option);
    }
    public void AddShopOption(Item bought, Item? cost)
    {
        ShopOptions.Add(new(bought, cost));
    }
    public string GetFullDialog()
    {
        // Name and dialog
        string dialog = $"[{Name}] {Dialog}";
        
        // Shop
        if (ShopOptions.Count > 0)
            dialog += "\nSHOP:";
        int o = 1;
        foreach (var option in ShopOptions)
        {
            dialog += $"\n{o}] {option.Item.Name} ({option.Item.Amount}) : ";
            if (option.Cost == null)
                dialog += "FREE";
            else
                dialog += $"{option.Cost.Name} ({option.Cost.Amount})";
            o++;
        }

        return dialog;
    }
}
