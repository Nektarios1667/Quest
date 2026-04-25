using Migs.MPath.Core.Data;

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
    public static Point Scaled(this Point point, float scaleX, float scaleY)
    {
        return new((int)(point.X * scaleX), (int)(point.Y * scaleY));

    }
    public static string CoordString(this Point point)
    {
        return $"{point.X},{point.Y}";
    }
    public static Point Negative(this Point point)
    {
        return point * PointTools.Negative;
    }
    public static ByteCoord ToByteCoord(this Point point)
    {
        return new((byte)Math.Clamp(point.X, 0, 255), (byte)Math.Clamp(point.Y, 0, 255));
    }
}
public static class CoordinateExtensions
{
    public static Point ToPoint(this Coordinate coord) => new(coord.X, coord.Y);
}
public static class SizeExtensions
{
    public static Vector2 ToVector2(this SizeF size) => new(size.Width, size.Height);
    public static Point ToPoint(this Size size) => new(size.Width, size.Height);
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

