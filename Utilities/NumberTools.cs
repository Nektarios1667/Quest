using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
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
    public static void CycleDown(ref int num, int max)
    {
        num -= 1;
        if (num < 0) num = max - 1;
    }
    public static void CycleUp(ref int num, int max)
    {
        num = (num + 1) % max;
    }
    public static void CycleUp<T>(ref T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        value = values[(Array.IndexOf(values, value) + 1) % values.Length];
    }
    public static void CycleDown<T>(ref T value) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int newIdx = (Array.IndexOf(values, value) - 1);
        if (newIdx < 0) newIdx = values.Length - 1;
        value = values[newIdx];
    }
}
