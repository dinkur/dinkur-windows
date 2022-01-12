using System;
using Dinkur.Api;
using Grpc.Core;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using AsyncTask = System.Threading.Tasks.Task;

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

            _ = ReloadTasks();
        }

        private async AsyncTask ReloadTasks()
        {
            try
            {
                TaskProgressBar.Visibility = Visibility.Visible;
                TaskErrorResults.Visibility = Visibility.Collapsed;
                TaskNoResults.Visibility = Visibility.Collapsed;
                TaskListView.Visibility = Visibility.Collapsed;

                var response = await App.Tasker.GetTaskListAsync(new GetTaskListRequest {
                    Shorthand = GetTaskListRequest.Types.Shorthand.ThisDay
                }, new CallOptions());
                if (response?.Tasks == null || response.Tasks.Count == 0)
                {
                    TaskNoResults.Visibility = Visibility.Visible;
                    return;
                }

                foreach (var task in response.Tasks)
                {
                    TaskListView.Items.Add(task);
                }
                TaskListView.Visibility = Visibility.Visible;
            }
            catch (Exception e)
            {
                TaskErrorResults.Message = e.Message;
                TaskErrorResults.Visibility = Visibility.Visible;
            }
            finally
            {
                TaskProgressBar.Visibility = Visibility.Collapsed;
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

        private void ReloadTasksButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ReloadTasks();
        }
    }
}
