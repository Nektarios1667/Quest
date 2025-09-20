namespace Quest.Utilities;

public static class PointExtensions
{
    public static float Distance(this Point point, Point other)
    {
        return PointTools.Distance(point, other);
    }
    public static float DistanceSquared(this Point point, Point other)
    {
        return PointTools.DistanceSquared(point, other);
    }
}
public static class PointTools
{
    public static float DistanceSquared(Point a, Point b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
    public static float Distance(Point a, Point b)
    {
        return (float)Math.Sqrt(DistanceSquared(a, b));
    }
}
