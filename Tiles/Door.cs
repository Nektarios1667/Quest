﻿using Quest.Managers;

namespace Quest.Tiles;

public class Door : Tile
{
    public string Key { get; set; }
    public bool ConsumeKey { get; set; }
    public Door(Point location, string key, bool consumeKey = true) : base(location)
    {
        IsWalkable = false;
        Key = key;
        ConsumeKey = consumeKey;
    }
    public override void Draw(GameManager gameManager)
    {
        // Draw tile
        Point dest = Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
        Color color = Marked ? Color.Red : Color.White;
        Rectangle source = new(IsWalkable ? 16 : 0, 0, 16, 16);
        DrawTexture(gameManager.Batch, TextureID.Door, dest, source: source, scale: 4, color: color);

        // Handling
        Marked = false;
    }
    public override void OnPlayerCollide(GameManager game)
    {
        if (Key == "" || game.Inventory.Contains(Key))
        {
            if (Key != "" && ConsumeKey)
            {
                game.Inventory.Consume(Key);
                game.UIManager.Notification($"-1 {StringTools.FillCamelSpaces(Key)}", Color.Red, 3);
                SoundManager.PlaySoundInstance("DoorUnlock");
            }
            IsWalkable = true;
        }
        else
        {
            // Notif
            game.UIManager.Notification($"{StringTools.FillCamelSpaces(Key)} needed to unlock.", Color.Red, 5);

            // Sound fx
            string timerName = $"DoorLocked_{Location.X + Location.Y * Constants.MapSize.X}";
            if (TimerManager.IsCompleteOrMissing(timerName))
            {
                SoundManager.PlaySoundInstance("DoorLocked");
                TimerManager.SetTimer(timerName, 5, null);
            }
        }
    }
}
