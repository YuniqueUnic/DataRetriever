using System;
using System.Runtime.InteropServices;


namespace VSTSDataProvider.ConsoleRelated;

public struct COORD
{
    public ushort X;
    public ushort Y;
};

public struct CONSOLE_FONT
{
    public uint index;
    public COORD dim;
};

public static class ConsoleEx
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32" , CharSet = CharSet.Auto)]
    internal static extern bool AllocConsole( );

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32" , CharSet = CharSet.Auto)]
    internal static extern bool SetConsoleFont(IntPtr consoleFont , uint index);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32" , CharSet = CharSet.Auto)]
    internal static extern bool GetConsoleFontInfo(IntPtr hOutput , byte bMaximize , uint count , [In, Out] CONSOLE_FONT[] consoleFont);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32" , CharSet = CharSet.Auto)]
    internal static extern uint GetNumberOfConsoleFonts( );

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32" , CharSet = CharSet.Auto)]
    internal static extern COORD GetConsoleFontSize(IntPtr HANDLE , uint DWORD);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll ")]
    internal static extern IntPtr GetStdHandle(int nStdHandle);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll" , CharSet = CharSet.Auto , SetLastError = true)]
    internal static extern int GetConsoleTitle(String sb , int capacity);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("user32.dll" , EntryPoint = "UpdateWindow")]
    internal static extern int UpdateWindow(IntPtr hwnd);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("user32.dll")]
    internal static extern IntPtr FindWindow(String sClassName , String sAppName);

    public static void OpenConsole( )
    {
        var consoleTitle = "> Debug Console";
        AllocConsole();


        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WindowWidth = 80;
        Console.CursorVisible = false;
        Console.Title = consoleTitle;
        Console.WriteLine("DEBUG CONSOLE WAIT OUTPUTING...{0} \n" , DateTime.Now.ToLongTimeString());
    }

    public static void Log(String format , params object[] args)
    {
        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] " + format , args);
    }
    public static void Log(Object arg)
    {
        Console.WriteLine(arg);
    }
}
