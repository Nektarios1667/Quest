using Quest.Gui;
using System.Linq;

namespace Quest.Entities;

public class ShopOption
{
    public Item Item;
    public Item? Cost;
    public int Stock;

    public ShopOption(Item item, Item? cost, int stock)
    {
        Item = item;
        Cost = cost;
        Stock = stock;
    }

    public bool Buy(Inventory inventory)
    {
        if (Cost == null || inventory.Consume(Cost))
            inventory.AddItem(Item);
        else
            return false;
        return true;
    }
}

public class NPC
{
    public static readonly NPC Null = new(null!, TextureID.Null, Point.Zero, "NUL_NAME", "NUL_DIALOG");
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
        AddShopOption(new(ItemTypes.SteelSword, 1), new(ItemTypes.DeltaCoin, 3), 1);
        AddShopOption(new(ItemTypes.SteelSword, 1), new(ItemTypes.PhiCoin, 7), 1);
        AddShopOption(new(ItemTypes.Lantern, 1), new(ItemTypes.PhiCoin, 4), 2);
        AddShopOption(new(ItemTypes.Lantern, 1), null, 4);
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
        if (ShopOptions.Count >= 5)
        {
            Logger.Warning($"NPC '{Name}' reached max shop options of 5");
            return;
        }
        ShopOptions.Add(option);
    }
    public void AddShopOption(Item bought, Item? cost, int stock)
    {
        AddShopOption(new(bought, cost, stock));
    }
    public string GetFullDialog()
    {
        // Name and dialog
        string dialog = $"[{Name}] {Dialog}-------------------------------------------------------";
        
        // Shop
        int o = 1;
        foreach (var option in ShopOptions)
        {
            dialog += $"\n{o}] {option.Item.Name} ({option.Item.Amount}) : ";
            if (option.Cost == null)
                dialog += $"FREE | Stock: {option.Stock}";
            else
                dialog += $"{option.Cost.Name} ({option.Cost.Amount}) | Stock: {option.Stock}";
            o++;
        }

        return dialog;
    }
    public bool Buy(ShopOption option, Inventory inv, GameManager gameManager)
    {
        // Check
        if (!ShopOptions.Contains(option)) return false;

        // Buy
        if ((option.Cost == null || inv.Consume(option.Cost)) && option.Stock > 0)
        {
            (bool success, Item leftover) = inv.AddItem(option.Item);
            if (!success)
                gameManager.LevelManager.Level.Loot.Add(new(leftover.Name, leftover.Amount, Location, gameManager.GameTime));
            if (leftover.Amount < option.Item.Amount)
                SoundManager.PlaySound("Trinkets");
            option.Stock -= 1;
        }

        // Quickly rewrite dialog
        DialogBox!.SetText(GetFullDialog(), respeak: DialogRespeak.Instant);

        return true;
    }
    public bool Buy(int option, Inventory inv, GameManager gameManager)
    {
        if (option >= ShopOptions.Count) return false;
        return Buy(ShopOptions[option], inv, gameManager);
    }
}
