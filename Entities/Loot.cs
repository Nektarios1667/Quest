using Quest.Managers;

namespace Quest.Entities;
public struct Loot : IEntity
{
    public ushort UID { get; } = UIDManager.Get(UIDCategory.Loot);
    public ItemRef Item { get; }
    public Point Position { get; set; }
    public float Birth { get; private set; }
    public readonly RectangleF Bounds => new(Position.ToVector2(), Size);
    // Generated
    private static readonly Point Size = new(32);
    public readonly TextureID Texture => Item.Texture;
    public readonly string DisplayName => $"{Item.Amount} {StringTools.FillCamelSpaces(Item.Name)}";
    // Helpers
    public static readonly Point lootStackOffset = new(4, 4);
    public Loot(ItemRef item, Point location, float time)
    {
        Item = item;
        Position = location;
        Birth = time;
    }
    public readonly void Dispose()
    {
        UIDManager.Release(UIDCategory.Loot, UID);
    }
    public readonly void Draw(GameManager gameManager)
    {
        Point pos = Position - CameraManager.Camera.ToPoint() + Constants.Middle;
        pos.Y += (int)(Math.Sin((GameManager.GameTime - Birth) * 2 % (Math.PI * 2)) * 6); // Bob up and down
        DrawTexture(gameManager.Batch, Texture, pos, scale: 2);
        // Draw stacks if multiple
        if (Item.Amount > 1)
            DrawTexture(gameManager.Batch, Texture, pos + lootStackOffset, scale: 2);
        if (Item.Amount > 2)
            DrawTexture(gameManager.Batch, Texture, pos + lootStackOffset.Scaled(2), scale: 2);
        // Draw hitbox
        DebugManager.DrawHitbox(gameManager.Batch, this);
    }
}