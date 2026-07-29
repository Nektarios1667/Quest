#if DEBUG
using System.Runtime.InteropServices;

class ConsoleManager
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    public static void Show()
    {
        AllocConsole();
    }
}
#endif