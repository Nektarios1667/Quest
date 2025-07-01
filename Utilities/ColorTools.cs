using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public static class ColorTools
{
    public static Color GetSkyColor(float time)
    {
        float cycle = (time % Constants.DayLength) / Constants.DayLength;
        List<(float pos, Color color)> stops = [
            (0, Color.Transparent),
            (0.2f, Color.Transparent),
            (0.3f, Color.Black),
            (0.5f, Color.Black),
            (0.7f, Color.Black),
            (0.8f, Color.Transparent),
            (1, Color.Transparent),
        ];
        // Find stops
        var start = stops.LastOrDefault(s => s.pos <= cycle, stops[^1]);
        var end = stops.FirstOrDefault(s => s.pos >= cycle, stops[^1]);
        // Interpolate between the two colors
        Color color = Color.Lerp(start.color, end.color, (cycle - start.pos) / (end.pos - start.pos));
        return color;
    }
}
