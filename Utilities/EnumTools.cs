using SharpDX.MediaFoundation.DirectX;

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
    public static N ConvertEnum<T, N>(T value) where N : Enum where T : Enum
    {
        return (N)Enum.Parse(typeof(N), value.ToString());
    }
}
