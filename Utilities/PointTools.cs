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
    public static Point Scaled(this Point point, float scale)
    {
        return new((int)(point.X * scale), (int)(point.Y * scale));
    }
    public static string CoordString(this Point point)
    {
        return $"{point.X},{point.Y}";
    }
    public static Point Negative(this Point point)
    {
        return point * PointTools.Negative;
    }
}
public static class PointTools
{
    public static readonly Point Negative = new(-1, -1);
    public static readonly Point Up = new(0, -1);
    public static readonly Point Down = new(0, 1);
    public static readonly Point Left = new(-1, 0);
    public static readonly Point Right = new(1, 0);
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

