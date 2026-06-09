using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ToolData;

public static class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern uint GetPrivateProfileString(
        string lpAppName, string lpKeyName, string lpDefault,
        StringBuilder lpReturnedString, uint nSize, string lpFileName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern bool WritePrivateProfileString(
        string lpAppName, string lpKeyName, string lpString, string lpFileName);

    public static string ReadValue(string iniPath, string section, string key, string defaultValue = "")
    {
        var buffer = new StringBuilder(32767);
        GetPrivateProfileString(section, key, defaultValue, buffer, (uint)buffer.Capacity, iniPath);
        return buffer.ToString();
    }

    // 写入值
    public static void WriteValue(string iniPath, string section, string key, string value)
    {
        WritePrivateProfileString(section, key, value, iniPath);
    }
}