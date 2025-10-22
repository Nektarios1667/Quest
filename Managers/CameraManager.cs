namespace Quest.Managers;
public static class CameraManager
{
    public static Vector2 Camera { get; set; } = (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2();
    private static Vector2 _cameraDest { get; set; } = Camera;
    public static Vector2 CameraDest
    {
        get => _cameraDest;
        set
        {
            if (value == _cameraDest) return;
            CameraDestMove?.Invoke(_cameraDest, value);
            _cameraDest = value;
        }
    }
    public static Vector2 CameraOffset => CameraDest - Camera;
    public static Point PlayerFoot => CameraDest.ToPoint() + new Point(0, Constants.MageHalfSize.Y);
    public static Point TileCoord => PlayerFoot / Constants.TileSize;
    // Events
    public static event Action<Vector2, Vector2>? CameraMove;
    public static event Action<Vector2, Vector2>? CameraDestMove;
    public static event Action<Point, Point>? TileChange;
    public static void Update(float deltaTime)
    {
        if (StateManager.OverlayState == OverlayState.Pause) return;

        DebugManager.StartBenchmark("CameraUpdate");

        // Clamp
        CameraDest = Vector2.Clamp(CameraDest, Vector2.Zero, (Constants.MapSize * Constants.TileSize).ToVector2());

        // Lerp camera
        if (Vector2.DistanceSquared(Camera, CameraDest) < 4 * deltaTime * 60) Camera = CameraDest; // If close enough snap to destination
        if (CameraDest != Camera)
        {
            Vector2 beforeCamera = Camera;
            Camera = Vector2.Lerp(Camera, CameraDest, 1f - MathF.Pow(1f - Constants.CameraRigidity, deltaTime * 60f));

            // Events
            CameraMove?.Invoke(beforeCamera, Camera);
            Point beforeTile = (beforeCamera.ToPoint() + new Point(0, Constants.MageHalfSize.Y)) / Constants.TileSize;
            Point afterTile = (Camera.ToPoint() + new Point(0, Constants.MageHalfSize.Y)) / Constants.TileSize;
            if (beforeTile != afterTile) TileChange?.Invoke(beforeTile, afterTile);
        }
        // Clamp
        Camera = Vector2.Clamp(Camera, Constants.Middle.ToVector2(), (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2());

        DebugManager.EndBenchmark("CameraUpdate");
    }
}
