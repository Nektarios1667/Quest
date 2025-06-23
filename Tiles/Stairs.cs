namespace Quest.Tiles;

public class Stairs : Tile
{
    public string DestLevel { get; set; }
    public Point DestPosition { get; set; }
    public Stairs(Point location, string destLevel, Point destPosition) : base(location)
    {
        IsWalkable = true;
        DestLevel = destLevel;
        DestPosition = destPosition;
    }
    public override void OnPlayerEnter(GameManager game)
    {
        // Load another level
        Console.WriteLine($"[System] Teleporting to level '{DestLevel}' @ {DestPosition.X}, {DestPosition.Y}");
        game.LevelManager.LoadLevel(game, DestLevel);
        CameraManager.CameraDest = (DestPosition * Constants.TileSize).ToVector2() + new Vector2(Constants.TileSize.X / 2, 0);
        CameraManager.Camera = CameraManager.CameraDest;
    }
}
