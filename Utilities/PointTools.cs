using Xna = Microsoft.Xna.Framework;

namespace Quest.Tools;

public static class PointTools
{
    public static float DistanceSquared(Xna.Point a, Xna.Point b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
    public static float Distance(Xna.Point a, Xna.Point b)
    {
        return (float)Math.Sqrt(DistanceSquared(a, b));
    }
}
