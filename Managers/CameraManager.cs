namespace Quest.Managers;
public static class CameraManager
{
    public static Vector2 Camera { get; set; } = (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2();
    public static Vector2 CameraDest { get; set; } = Camera;
    public static Vector2 CameraOffset => CameraDest - Camera;
    public static Point PlayerFoot => CameraDest.ToPoint() + new Point(0, Constants.MageHalfSize.Y);
    public static Point TileCoord  => PlayerFoot / Constants.TileSize;
    public static void Update(float deltaTime)
    {
        // Lerp camera
        DebugManager.StartBenchmark("CameraUpdate");
        if (Vector2.DistanceSquared(Camera, CameraDest) < 4 * deltaTime * 60) Camera = CameraDest; // If close enough snap to destination
        if (CameraDest != Camera) // If not, lerp towards destination
            Camera = Vector2.Lerp(Camera, CameraDest, 1f - MathF.Pow(1f - Constants.CameraRigidity, deltaTime * 60f));
        DebugManager.EndBenchmark("CameraUpdate");
    }
}
