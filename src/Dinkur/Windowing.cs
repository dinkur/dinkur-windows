using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Dinkur
{
    internal static class Windowing
    {
        public static AppWindow GetAppWindow(this Window window)
        {
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            return GetAppWindowFromHandle(windowHandle);
        }

        public static AppWindow GetAppWindowFromHandle(IntPtr windowHandle)
        {
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            return AppWindow.GetFromWindowId(windowId);
        }
    }
}
