using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ControlBeeWPF.Utils;

public class WindowUtils
{
    private const int SC_CLOSE = 0xF060;
    private const int MF_BYCOMMAND = 0x00000000;
    private const int MF_GRAYED = 0x00000001;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern int EnableMenuItem(IntPtr hMenu, int uIDEnableItem, int uEnable);

    public static void DisableCloseButton(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var hMenu = GetSystemMenu(hwnd, false);
        if (hMenu != IntPtr.Zero)
            // SC_CLOSE = 0xF060, MF_BYCOMMAND = 0x00000000, MF_GRAYED = 0x00000001
            EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
    }
}