namespace Quest.Utilities;

public enum AlphaBlend
{
    Add,
    Average,
    Min,
    Max,
    Keep,
}
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
    public static Color Add(Color color, Color add, AlphaBlend alpha = AlphaBlend.Average)
    {
        Color newColor = new(
            (byte)Math.Min(color.R + add.R, 255),
            (byte)Math.Min(color.G + add.G, 255),
            (byte)Math.Min(color.B + add.B, 255)
        );

        // Alpha blend
        if (alpha == AlphaBlend.Add)
            newColor.A = (byte)Math.Min(color.A + add.A, 255);
        else if (alpha == AlphaBlend.Average)
            newColor.A = (byte)Math.Clamp((color.A + add.A) / 2f, 0, 255);
        else if (alpha == AlphaBlend.Min)
            newColor.A = Math.Min(color.A, add.A);
        else if (alpha == AlphaBlend.Max)
            newColor.A = Math.Max(color.A, add.A);
        else if (alpha == AlphaBlend.Keep)
            newColor.A = color.A;

        return newColor;
    }
    public static Color Blend(Color color1, Color color2, float blend, AlphaBlend alpha = AlphaBlend.Average)
    {
        blend = Math.Clamp(blend, 0, 1);
        Color newColor = new(
            (byte)Math.Clamp((color1.R * (1 - blend) + color2.R * blend) / 2, 0, 255),
            (byte)Math.Clamp((color1.G * (1 - blend) + color2.G * blend) / 2, 0, 255),
            (byte)Math.Clamp((color1.B * (1 - blend) + color2.B * blend) / 2, 0, 255)
        );

        // Alpha blend
        if (alpha == AlphaBlend.Add)
            newColor.A = (byte)Math.Min(color1.A + color2.A, 255);
        else if (alpha == AlphaBlend.Average)
            newColor.A = (byte)Math.Clamp((color1.A + color2.A) / 2f, 0, 255);
        else if (alpha == AlphaBlend.Min)
            newColor.A = Math.Min(color1.A, color2.A);
        else if (alpha == AlphaBlend.Max)
            newColor.A = Math.Max(color1.A, color2.A);
        else if (alpha == AlphaBlend.Keep)
            newColor.A = color1.A;

        return newColor;
    }
}