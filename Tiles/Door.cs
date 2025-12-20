namespace Quest.Tiles;

public class Door : Tile
{
    public Item? Key { get; set; }
    public bool ConsumeKey { get; set; }
    public bool IsOpened { get; set; }
    public override bool IsWalkable => Type.IsWalkable || IsOpened;
    public Door(Point location, Item? key = null, bool consumeKey = true) : base(location, TileTypeID.Door)
    {
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        Rectangle source = new(IsWalkable ? 16 : 0, 0, 16, 16);
        DrawTexture(gameManager.Batch, TextureID.Door, dest, source: source, scale: Constants.TileSizeScale, color: color);

        // Handling
        Marked = false;
    }
    public override void OnPlayerCollide(GameManager game, PlayerManager player)
    {
        if (Key == null || player.Inventory.Count(Key.Type) >= Key.Amount)
        {
            if (Key != null && ConsumeKey)
            {
                player.Inventory.Consume(Key);
                game.UIManager.Notification($"-1 {StringTools.FillCamelSpaces(Key.Name)}", Color.Red, 3);
                SoundManager.PlaySoundInstance("DoorUnlock");
            }
            IsOpened = true;
            StateManager.SaveDoorOpened(TileID);
        }
        else
        {
            // Notif
            game.UIManager.Notification($"{StringTools.FillCamelSpaces(Key.Name)} needed to unlock.", Color.Red, 5);

            // Sound fx
            string timerName = $"DoorLocked_{X + Y * Constants.MapSize.X}";
            if (TimerManager.IsCompleteOrMissing(timerName))
            {
                SoundManager.PlaySoundInstance("DoorLocked");
                TimerManager.SetTimer(timerName, 5, null);
            }
        }
    }
    public void Open()
    {
        IsOpened = true;
    }
    public void Close()
    {
        IsOpened = false;
    }
}
