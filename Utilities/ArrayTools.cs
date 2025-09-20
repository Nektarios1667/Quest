namespace Quest.Utilities;
public static class ArrayTools
{
    public static T[,] Resize2DArray<T>(T[,] array, int width, int height)
    {
        int oldWidth = array.GetLength(0);
        int oldHeight = array.GetLength(1);

        // New array
        T[,] resized = new T[width, height];

        // Copy only the overlapping region
        int copyHeight = Math.Min(oldHeight, height);
        int copyWidth = Math.Min(oldWidth, width);

        for (int y = 0; y < copyHeight; y++)
        {
            for (int x = 0; x < copyWidth; x++)
            {
                resized[x, y] = array[x, y];
            }
        }

        return resized;
    }
    public static bool InBounds<T>(T[,] array, int row, int col)
    {
        return row >= 0 && row < array.GetLength(0) &&
               col >= 0 && col < array.GetLength(1);
    }
    public static void Print2DArray<T>(T[,] array)
    {
        int rows = array.GetLength(0); // y
        int cols = array.GetLength(1); // x

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                T value = array[y, x];
                string s = value == null ? " . " : " X ";
                Console.Write(s);
            }
            Console.WriteLine();
        }
    }
}
