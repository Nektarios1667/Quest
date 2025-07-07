using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public static class ColorTools
{
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
}
