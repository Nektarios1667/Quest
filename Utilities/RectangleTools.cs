namespace Quest.Utilities;
public static class RectangleTools
{
    public static Rectangle Inflated(this Rectangle rect, int amountX, int amountY)
    {
        rect.Inflate(amountX, amountY);
        return rect;
    }
}
