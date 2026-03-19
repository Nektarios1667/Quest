using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public static class RectangleTools
{
    public static Rectangle Inflated(this Rectangle rect, int amountX, int amountY)
    {
        rect.Inflate(amountX, amountY);
        return rect;
    }
}
