namespace Quest.Managers;
public static class RandomManager
{
    const string lower = "abcdefghijklmnopqrstuvwxyz";
    const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string symbols = "!@#$%^&*()-_=+[]{}|;:'\",.<>?/\\`~";
    const string numbers = "1234567890";

    private static readonly Random random = new();
    public static int RandomIntRange(int min, int max)
    {
        return random.Next(min, max);
    }
    public static float RandomFloatRange(float min, float max)
    {
        return random.NextSingle() * (max - min) + min;
    }
    public static double RandomDoubleRange(double min, double max)
    {
        return random.NextDouble() * (max - min) + min;
    }
    public static float RandomFloat()
    {
        return random.NextSingle();
    }
    public static double RandomDouble()
    {
        return random.NextDouble();
    }
    public static bool OneOutOF(int n)
    {
        return random.Next(n) == 0;
    }
    public static bool Chance(float probability)
    {
        return random.NextDouble() < probability;
    }
    public static bool RandomBool()
    {
        return random.Next(2) == 0;
    }
    public static int RandomSign()
    {
        return random.Next(2) == 0 ? -1 : 1;
    }
    public static Vector2 RandomUnitVec2()
    {
        double angle = random.NextDouble() * Math.PI * 2;
        return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
    }
    public static bool ChancePerSecond(float chancePerSecond, float deltaTime)
    {
        if (chancePerSecond <= 0f || deltaTime <= 0f)
            return false;

        float chanceThisFrame = chancePerSecond * deltaTime;
        return random.NextDouble() < chanceThisFrame;
    }
    public static Color RandomColor()
    {
        return new Color(
            (byte)random.Next(256),
            (byte)random.Next(256),
            (byte)random.Next(256),
            (byte)255);
    }
    public static char RandomChar(bool includeCapitals = true, bool includeNumbers = true, bool includeSymbols = true)
    {
        string chars = lower;
        if (includeCapitals) chars += upper;
        if (includeSymbols) chars += symbols;
        if (includeNumbers) chars += numbers;

        return chars[random.Next(chars.Length)];
    }
    public static string RandomString(int length, bool includeCapitals = true, bool includeNumbers = true, bool includeSymbols = true)
    {
        string chars = lower;
        if (includeCapitals) chars += upper;
        if (includeSymbols) chars += symbols;
        if (includeNumbers) chars += numbers;

        string result = "";
        for (int i = 0; i < length; i++)
            result += chars[random.Next(chars.Length)];
        return result;
    }
}
