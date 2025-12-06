namespace Quest.Tiles;

public class Stairs : Tile
{
    public string DestLevel { get; set; }
    public byte DestX { get; set; }
    public byte DestY { get; set; }
    public Stairs(Point location, string destLevel, Point destPosition) : base(location, TileTypeID.Stairs)
    {
        DestX = (byte)destPosition.X;
        DestY = (byte)destPosition.Y;
        DestLevel = destLevel.Replace('\\', '/'); // For consistency
    }
    public override void OnPlayerEnter(GameManager game, PlayerManager player)
    {
        // Load another level
        bool read = game.LevelManager.ReadLevel(game.UIManager, DestLevel, reload: false);
        bool loaded = game.LevelManager.LoadLevel(game, DestLevel);
        if (!read || !loaded)
        {
            Logger.Error($"Failed to teleport to level '{DestLevel}'");
            return;
        }

        CameraManager.CameraDest = (new Point(DestX, DestY) * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(0f); // Force update to avoid visual glitches

        Logger.System($"Teleporting to level '{DestLevel}' @ {DestX}, {DestY}");
    }
}
