using System.IO;

namespace Quest.Tiles;

public class Door : Tile, IHasState
{
    public ItemRef? Key { get; set; }
    public bool ConsumeKey { get; set; }
    public bool IsOpened { get; private set; }
    public override bool IsTransparent => Type.IsTransparent || IsOpened;
    public override bool IsWalkable => Type.IsWalkable || IsOpened;
    public override float Weight => IsOpened ? 1 : float.MaxValue;
    public Door(Point location, ItemRef? key = null, bool consumeKey = true) : base(location, TileTypeID.Door)
    {
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = CameraManager.TileToScreen(Location);
        Rectangle source = new(IsWalkable ? 16 : 0, 0, 16, 16);
        DrawTexture(gameManager.Batch, TextureID.Door, dest, source: source, scale: Constants.TileSizeScale);
    }
    public override void OnPlayerCollide(GameManager gameManager,PlayerManager player)
    {
        if (Key == null || player.Inventory.Count(Key.Type) >= Key.Amount)
        {
            if (Key != null && ConsumeKey)
            {
                gameManager.OverlayManager.Notification($"-{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Red, 3);
                player.Inventory.Consume(Key, ignoreCheck: true);
            }
            else if (Key != null)
                gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)}", Color.Gray, 2);

            SoundManager.PlaySoundInstance("DoorUnlock");
            Open(gameManager);
        }
        else
        {
            string timerName = $"DoorLocked_{X + Y * Constants.MapSize.X}";
            if (TimerManager.IsCompleteOrMissing(timerName))
            {
                // Notif
                gameManager.OverlayManager.Notification($"{Key.Amount} {StringTools.FillCamelSpaces(Key.Name)} needed to unlock", Color.Red, 5);
                // Sfx
                SoundManager.PlaySoundInstance("DoorLocked");

                TimerManager.SetTimer(timerName, 5, null);
            }
        }
    }
    public void Open(GameManager game)
    {
        IsOpened = true;
        LightingManager.SetLightGridBlocking(Location.ToPoint(), false);
        SaveManager.SaveStateTile(this, game.LevelManager.Level.LevelName);
    }
    public void WriteState(BinaryWriter writer, GameManager gameManager)
    {
        // If a door is even written to the save, then it means it's open
    }
    public void ReadState(BinaryReader reader, GameManager gameManager)
    {
        // If a door is even written to the save, then it means it's open
        Open(gameManager);
    }
}
