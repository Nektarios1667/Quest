using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;
public static class EnumTools
{
    public static bool IsBetween<T>(T value, T lower, T upper) where T : Enum
    {
        int intValue = Convert.ToInt32(value);
        int intLower = Convert.ToInt32(lower);
        int intUpper = Convert.ToInt32(upper);
        return intValue >= intLower && intValue <= intUpper;
    }
}
