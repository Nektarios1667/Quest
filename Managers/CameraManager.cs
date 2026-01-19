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
            if (Vector2.DistanceSquared(value, _cameraDest) < 0.001f) return;
            // CameraDestMove
            CameraDestMove?.Invoke(_cameraDest, value);
            // TileChange
            Point beforeTile = TileCoord;
            // Set and clamp
            _cameraDest = Vector2.Clamp(value, Vector2.Zero, (Constants.MapSize * Constants.TileSize).ToVector2());
            // TileChange event
            if (beforeTile != TileCoord) TileChange?.Invoke(beforeTile, TileCoord);
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

        // Lerp camera
        if (Vector2.DistanceSquared(Camera, CameraDest) < 4f) Camera = CameraDest; // If close enough snap to destination
        else if (deltaTime > 0)
        {
            Vector2 beforeCamera = Camera;
            Camera = Vector2.Lerp(Camera, CameraDest, 1f - MathF.Pow(1f - Constants.CameraRigidity, deltaTime * 60f));
            Camera = Vector2.Clamp(Camera, Constants.Middle.ToVector2(), (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2());

            // Events
            if (beforeCamera != Camera)
                CameraMove?.Invoke(beforeCamera, Camera);
        } 
        Camera = Vector2.Clamp(Camera, Constants.Middle.ToVector2(), (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2());

        DebugManager.EndBenchmark("CameraUpdate");
    }
    public static Point PositionToWorldCoord(Vector2 position) => position.ToPoint() / Constants.TileSize;
    public static Point PositionToWorldCoord(Point position) => position / Constants.TileSize;
}
