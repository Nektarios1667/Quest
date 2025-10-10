using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public static class NumberTools
{
    public static int RoundTo<T>(T num, float multiple) where T : INumber<T>
    {
        if (multiple <= 0 || num == T.Zero)
            return 0;
        return (int)(MathF.Round(float.CreateChecked(num) / multiple) * multiple);
    }
}
