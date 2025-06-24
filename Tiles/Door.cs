namespace Quest.Tiles;

public class Door : Tile
{
    public Item Key { get; set; }
    public bool ConsumeKey { get; set; }
    public Door(Point location, Item key, bool consumeKey = true) : base(location)
    {
        IsWalkable = false;
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void Draw(GameManager game)
    {
        // Draw
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        Rectangle source = new(IsWalkable ? 16 : 0, 0, 16, 16);
        TextureManager.DrawTexture(game.Batch, TextureManager.TextureID.Door, dest, source: source, scale: 4, color: color);

        // Handling
        Marked = false;
    }
    public override void OnPlayerCollide(GameManager game)
    {
        if (game.Inventory.Contains(Key))
        {
            if (ConsumeKey)
            {
                game.Inventory.Consume(Key);
                game.UIManager.Notification($"-1 {StringTools.FillCamelSpaces(Key.Name)}", Color.Red, 3);
            }
            IsWalkable = true;
        }
        else
            game.UIManager.Notification($"{StringTools.FillCamelSpaces(Key.Name)} needed to unlock.", Color.Red, 5);
    }
}
