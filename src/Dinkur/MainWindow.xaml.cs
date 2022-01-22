using System;
using System.Threading;
using Dinkur.Api;
using Dinkur.Services;
using Grpc.Net.Client;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static AppWindow AppWindow => _appWindow ?? throw new InvalidOperationException("App window has not yet been initialized.");
        private static AppWindow? _appWindow;

        public Entries.EntriesClient Entries { get; }
        public Alerter.AlerterClient Alerter { get; }
        public DinkurService DinkurService { get; }

        private readonly CancellationTokenSource windowCancellationSource = new();

        public MainWindow()
        {
            _appWindow = this.GetAppWindow();

            var channel = GrpcChannel.ForAddress("http://localhost:59122");
            Entries = new Entries.EntriesClient(channel);
            Alerter = new Alerter.AlerterClient(channel);
            DinkurService = new DinkurService(Entries, Alerter);

            InitializeComponent();

            Closed += MainWindow_Closed;
            _ = DinkurService.StreamEntriesAsEvents(windowCancellationSource.Token);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            windowCancellationSource.Cancel();
        }
    }
}
