using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AppWindow appWindow = MainWindow.AppWindow;

            // AppWindow.TitleBar is not yet supported on Windows 10 and is therefore always null
            // https://github.com/microsoft/WindowsAppSDK-Samples/issues/116
            // https://github.com/microsoft/microsoft-ui-xaml/issues/6308
            if (appWindow.TitleBar != null)
            {
                AppTitleBar.Visibility = Visibility.Visible;
                appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                appWindow.Changed += AppWindow_Changed;
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidSizeChange && sender.TitleBar != null && sender.TitleBar.ExtendsContentIntoTitleBar)
            {
                SetDragRegionForCustomTitleBar(sender);
            }
        }

        private void SetDragRegionForCustomTitleBar(AppWindow sender)
        {
            int titleBarHeight = sender.TitleBar.Height;
            AppTitleBar.Height = titleBarHeight;

            int rightInset = sender.TitleBar.RightInset;
            int windowWidth = sender.Size.Width;

            RectInt32[] dragRects = new[]{
                new RectInt32
                {
                    X = 0,
                    Y = 0,
                    Height = titleBarHeight,
                    Width = windowWidth - rightInset,
                },
            };

            sender.TitleBar.SetDragRectangles(dragRects);
        }

    }
}
