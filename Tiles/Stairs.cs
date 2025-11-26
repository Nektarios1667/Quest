using System.IO;

namespace Quest.Tiles;

public class Stairs : Tile
{
    public string DestLevel { get; set; }
    public Point DestPosition { get; set; }
    public Stairs(Point location, string destLevel, Point destPosition) : base(location, TileTypes.Stairs)
    {
        DestLevel = destLevel.Replace('\\', '/'); // For consistency
        DestPosition = destPosition;
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

        CameraManager.CameraDest = (DestPosition * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(0f); // Force update to avoid visual glitches

        Logger.System($"Teleporting to level '{DestLevel}' @ {DestPosition.X}, {DestPosition.Y}");
    }
}
