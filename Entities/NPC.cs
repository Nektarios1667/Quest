using Quest.Gui;
using Quest.Interaction;

namespace Quest.Entities;

public class ShopOption
{
    public ItemRef Item;
    public ItemRef? Cost;
    public byte Stock;

    public ShopOption(ItemRef item, ItemRef? cost, byte stock)
    {
        Item = item;
        Cost = cost;
        Stock = stock;
    }
    public override string ToString()
    {
        string costStr = Cost == null ? "FREE" : $"{Cost.Name} ({Cost.Amount})";
        return $"{Item.Name} ({Item.Amount}) : {costStr} | {Stock}";
    }
    public static ShopOption ParseText(string text)
    {
        // Format: "ITEM_NAME (AMOUNT) : COST_NAME (COST_AMOUNT) | STOCK"
        string[] parts = text.Split(':');
        if (parts.Length != 2)
            throw new FormatException($"Invalid shop option format: {text}");
        // Item
        string itemPart = parts[0].Trim();
        int itemAmountStart = itemPart.IndexOf('(');
        int itemAmountEnd = itemPart.IndexOf(')');
        if (itemAmountStart == -1 || itemAmountEnd == -1 || itemAmountEnd < itemAmountStart)
            throw new FormatException($"Invalid item format in shop option: {itemPart}");
        string itemName = itemPart[..itemAmountStart].Trim();
        int itemAmount = int.Parse(itemPart.Substring(itemAmountStart + 1, itemAmountEnd - itemAmountStart - 1).Trim());

        // Cost and stock
        string costStockPart = parts[1].Trim();
        string[] costStockParts = costStockPart.Split('|');
        if (costStockParts.Length != 2)
            throw new FormatException($"Invalid cost/stock format in shop option: {costStockPart}");
        
        // Cost
        string costPart = costStockParts[0].Trim();
        ItemRef? cost = null;
        if (!costPart.Equals("FREE", StringComparison.OrdinalIgnoreCase))
        {
            int costStart = costPart.IndexOf('(');
            int costEnd = costPart.IndexOf(')');
            if (costStart == -1 || costEnd == -1 || costEnd < costStart)
                throw new FormatException($"Invalid cost format in shop option: {costPart}");
            string costName = costPart[..costStart].Trim();
            byte costAmount = byte.Parse(costPart.Substring(costStart + 1, costEnd - costStart - 1).Trim());
            cost = new ItemRef(ItemTypes.All[(int)Enum.Parse<ItemTypeID>(costName, true)], costAmount);
        }
        // Stock
        byte stock = byte.Parse(costStockParts[1].Trim());
        return new ShopOption(new ItemRef(ItemTypes.All[(int)Enum.Parse<ItemTypeID>(itemName, true)], (byte)itemAmount), cost, stock);
    }
}

public class NPC : IEntity
{
    public static readonly NPC Null = new(TextureID.Null, Point.Zero, "NUL_NAME", "NUL_DIALOG");
    public static Dialog? DialogBox { get; set; }
    public static List<(NPC npc, float dist)> NPCsNearby { get; set; } = [];
    public ushort UID { get; }
    public List<ShopOption> ShopOptions { get; private set; } = [];
    public Point Position { get; set; }
    public string Name { get; set; }
    public string Dialog { get; set; }
    public TextureID Texture { get; set; }
    public Color TextureColor { get; set; }
    public float Scale { get; set; }
    public Point Size => spritesize.Scaled(Scale * Constants.NPCScale);
    public RectangleF Bounds => new((Position * Constants.TileSize + Constants.TileHalfSize - Size.Scaled(0.5f, 1)).ToVector2(), Size);
    // Private
    private Point spritesize;

    public NPC(TextureID texture, Point location, string name, string dialog, Color textureColor = default, float scale = 1, ushort? uid = null)
    {
        Texture = texture;
        UID = uid ?? UIDManager.Get(UIDCategory.NPCs); ;

        // Private
        spritesize = TextureManager.Metadata[Texture].Size / TextureManager.Metadata[Texture].TileMap;

        Position = location;
        Name = name;
        Dialog = dialog;
        TextureColor = textureColor == default ? Color.White : textureColor;
        Scale = scale;
        // DEBUG TODO
        //for (int i = 0; i < 8; i++)
        //    ShopOptions.Add(new(new(ItemTypes.Apple, 1, $"Apple-{i}"), null, i));
    }
    public void Draw(GameManager gameManager)
    {
        // Npc
        Vector2 origin = new(spritesize.X / 2, spritesize.Y);
        Point pos = Position * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle + Constants.TileHalfSize;
        Rectangle source = GetAnimationSource(Texture, GameManager.GameTime);
        DrawTexture(gameManager.Batch, Texture, pos, color: TextureColor, scale: Scale * Constants.NPCScale, source: source, origin: origin);
        // Debug
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
    public void Update(GameManager gameManager)
    {
        // Mark as dialogue possibility
        float dist = Vector2.DistanceSquared(CameraManager.PlayerFoot.ToVector2() / Constants.TileSize.ToVector2(), Position.ToVector2() + Constants.HalfVec);
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
    public void AddShopOption(ItemRef bought, ItemRef? cost, byte stock)
    {
        AddShopOption(new(bought, cost, stock));
    }
    public string GetFullDialog()
    {
        // Name and dialog
        string dialog = $"[{Name}] {Dialog}";

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
    public bool Buy(ShopOption option, Container cont, GameManager gameManager)
    {
        // Check
        if (!ShopOptions.Contains(option)) return false;

        // Buy
        if ((option.Cost == null || cont.Consume(option.Cost)) && option.Stock > 0)
        {
            Item leftover = cont.AddItem(new(option.Item));
            if (leftover.Amount > 0)
                gameManager.LevelManager.Level.Loot.Add(new(new(leftover.Type, leftover.Amount), Position, GameManager.GameTime));
            SoundManager.PlaySound("Trinkets", pitchVariation: 0.25f);
            option.Stock -= 1;
        }

        // Quickly rewrite dialog
        DialogBox!.SetText(GetFullDialog(), respeak: DialogRespeak.Instant);

        return true;
    }
    public bool Buy(int option, Container cont, GameManager gameManager)
    {
        if (option >= ShopOptions.Count) return false;
        return Buy(ShopOptions[option], cont, gameManager);
    }
}
