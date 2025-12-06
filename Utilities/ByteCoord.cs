using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public struct ByteCoord
{
    public byte X;
    public byte Y;
    public ByteCoord(Point pos)
    {
        X = (byte)pos.X;
        Y = (byte)pos.Y;
    }
    public ByteCoord(byte x, byte y)
    {
        X = x;
        Y = y;
    }
    public static ByteCoord operator +(ByteCoord a, ByteCoord b)
    {
        return new ByteCoord((byte)(a.X + b.X), (byte)(a.Y + b.Y));
    }
    public static ByteCoord operator +(ByteCoord a, int b)
    {
        return new ByteCoord((byte)(a.X + b), (byte)(a.Y + b));
    }
    public static ByteCoord operator -(ByteCoord a, ByteCoord b)
    {
        return new ByteCoord((byte)(a.X - b.X), (byte)(a.Y - b.Y));
    }
    public static ByteCoord operator -(ByteCoord a, int b)
    {
        return new ByteCoord((byte)(a.X - b), (byte)(a.Y - b));
    }
    public static ByteCoord operator *(ByteCoord a, int b)
    {
        return new ByteCoord((byte)(a.X * b), (byte)(a.Y * b));
    }
    public static ByteCoord operator *(ByteCoord a, float b)
    {
        return new ByteCoord((byte)(a.X * b), (byte)(a.Y * b));
    }
    public static ByteCoord operator /(ByteCoord a, int b)
    {
        return new ByteCoord((byte)(a.X / b), (byte)(a.Y / b));
    }
    public static ByteCoord operator /(ByteCoord a, float b)
    {
        return new ByteCoord((byte)(a.X / b), (byte)(a.Y / b));
    }
    public static Point operator +(ByteCoord a, Point b)
    {
        return new Point(a.X + b.X, a.Y + b.Y);
    }
    public static Point operator -(ByteCoord a, Point b)
    {
        return new Point(a.X - b.X, a.Y - b.Y);
    }
    public static Point operator *(ByteCoord a, Point b)
    {
        return new Point(a.X * b.X, a.Y * b.Y);
    }
    public static Point operator /(ByteCoord a, Point b)
    {
        return new Point(a.X / b.X, a.Y / b.Y);
    }
    public static bool operator ==(ByteCoord a, Point b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(ByteCoord a, Point b)
    {
        return !(a == b);
    }
    public static bool operator ==(ByteCoord a, ByteCoord b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(ByteCoord a, ByteCoord b)
    {
        return !(a == b);
    }
    public override bool Equals(object? obj)
    {
        if (obj is ByteCoord other)
        {
            return this == other;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public Point ToPoint()
    {
        return new Point(X, Y);
    }
    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
    public override string ToString()
    {
        return $"{X},{Y}";
    }
}
