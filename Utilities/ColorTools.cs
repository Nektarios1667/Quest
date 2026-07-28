using System.Linq;

namespace Quest.Utilities;

public static class ColorTools
{
    public static readonly Color NearBlack = new(35, 35, 35);
    public static readonly Color GrayBlack = new(90, 90, 90);
    public static readonly Color NearWhite = new(225, 225, 225);
    public static byte GetMaxComponent(Color color)
    {
        return Math.Max(color.R, Math.Max(color.G, color.B));
    }
    public static float Luminance(Color color)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        return 0.2126f * r + 0.7152f * g + 0.0722f * b;
    }
    public static Color Add(Color color, Color add)
    {
        return new Color(
            (byte)Math.Min(color.R + add.R, 255),
            (byte)Math.Min(color.G + add.G, 255),
            (byte)Math.Min(color.B + add.B, 255),
            (byte)Math.Min(color.A + add.A, 255)
        );
    }
}