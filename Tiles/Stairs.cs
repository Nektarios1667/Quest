namespace Quest.Tiles;

public class Stairs : Tile
{
    public LevelPath DestLevel { get; set; }
    public ByteCoord Dest { get; set; }
    public Stairs(Point location, LevelPath destLevel, Point destPosition) : base(location, TileTypeID.Stairs)
    {
        Dest = new(destPosition);
        DestLevel = destLevel;
    }
    public override void OnPlayerEnter(GameManager game, PlayerManager player)
    {
        if (DestLevel.IsNull()) return;

        TimerManager.SetTimer("ScreenFadeOut", 1.5f, null);
        TimerManager.SetTimer("StairsTeleport", 1.5f, () => Teleport(game));
    }
    private void Teleport(GameManager game)
    {
        // Load another level
        bool read = game.LevelManager.ReadLevel(game, DestLevel.ToString(), reload: false);
        bool loaded = game.LevelManager.LoadLevel(game, DestLevel.ToString());
        if (!read || !loaded)
        {
            Logger.Error($"Failed to teleport to level '{DestLevel.LevelName}'");
            return;
        }

        CameraManager.CameraDest = (Dest * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        CameraManager.Camera = CameraManager.CameraDest;
        CameraManager.Update(0f); // Force update to avoid visual glitches

        TimerManager.SetTimer("ScreenFadeIn", 1.5f, null);

        Logger.System($"Teleporting to level '{DestLevel.LevelName}' @ {Dest}");
    }
}
