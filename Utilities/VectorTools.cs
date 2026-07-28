namespace Quest.Utilities;

public static class VectorExtensions
{
    public static float MinComp(this Vector2 vec) => Math.Min(vec.X, vec.Y);
    public static float MaxComp(this Vector2 vec) => Math.Max(vec.X, vec.Y);
}
