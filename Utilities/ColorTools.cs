using System.Linq;

namespace Quest.Utilities;
public static class ColorTools
{
    public static readonly Color NearBlack = new(35, 35, 35);
    public static readonly Color GrayBlack = new(90, 90, 90);
    public static readonly Color NearWhite = new(225, 225, 225);
    public static readonly List<(float pos, Color color)> stops = [
        (0, Color.Transparent),
        (0.2f, Color.Transparent),
        (0.3f, Color.Black),
        (0.5f, Color.Black),
        (0.7f, Color.Black),
        (0.8f, Color.Transparent),
        (1, Color.Transparent),
    ];
    public static Color GetSkyColor(float time)
    {
        float cycle = time / Constants.DayLength;
        // Find stops
        var start = stops.LastOrDefault(s => s.pos <= cycle, stops[^1]);
        var end = stops.FirstOrDefault(s => s.pos >= cycle, stops[^1]);

        Color color = Color.Lerp(start.color, end.color, (cycle - start.pos) / (end.pos - start.pos));
        return color;
    }
    public static float GetDaylightPercent(float time)
    {
        float distDay = (GetSkyColor(time).ToVector4() - stops[0].color.ToVector4()).LengthSquared();
        float distNight = (stops[stops.Count / 2].color.ToVector4() - GetSkyColor(time).ToVector4()).LengthSquared();
        float percent = distDay / (distDay + distNight) * 100;
        return 100 - percent;
    }

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
}
