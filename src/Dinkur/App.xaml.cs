﻿using System;
using Dinkur.Api;
using Grpc.Net.Client;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static Window? _window;
        private static Entries.EntriesClient? _entries;
        private static Alerter.AlerterClient? _alerter;

        internal static Window Window => _window ?? throw new InvalidOperationException("Window has not yet been initialized.");
        internal static Entries.EntriesClient Entries => _entries ?? throw new InvalidOperationException("Dinkur Entries service has not yet been initialized.");
        internal static Alerter.AlerterClient Alerter => _alerter ?? throw new InvalidOperationException("Dinkur Alerter service has not yet been initialized.");

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            var channel = GrpcChannel.ForAddress("http://localhost:59122");
            _entries = new Entries.EntriesClient(channel);
            _alerter = new Alerter.AlerterClient(channel);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            SetWindowSize(hwnd, 640, 480);

            _window.Activate();
        }

        private static void SetWindowSize(IntPtr hwnd, int width, int height)
        {
            // Win32 uses pixels and WinUI 3 uses effective pixels, so you should apply the DPI scale factor
            var dpi = PInvoke.User32.GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            PInvoke.User32.SetWindowPos(hwnd, PInvoke.User32.SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE);
        }
    }
}
