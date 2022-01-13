using System;
using System.Linq;
using Dinkur.Pages;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur
{
    public sealed partial class MainPage : Page
    {
        private readonly (string tag, Type page)[] pages = {
            ("tasks", typeof(TasksPage)),
            ("settings", typeof(SettingsPage)),
        };

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

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.IsSettingsSelected)
            {
                Navigate("settings", e.RecommendedNavigationTransitionInfo);
            } else if (e.SelectedItemContainer != null)
            {
                var tag = e.SelectedItemContainer.Tag?.ToString();
                Navigate(tag, e.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            NavigateBack();
        }

        private void ContentFrame_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException("Failed to load page " + e.SourcePageType.FullName);
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = pages.FirstOrDefault(p => p.page == e.SourcePageType);
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.tag));
            }
        }

        private bool Navigate(string tag, NavigationTransitionInfo transitionInfo)
        {
            var page = pages.FirstOrDefault(p => p.tag == tag).page;
            if (page == null || ContentFrame.CurrentSourcePageType == page)
            {
                return false;
            }

            ContentFrame.Navigate(page, null, transitionInfo);
            return true;
        }

        private bool NavigateBack()
        {
            if (!ContentFrame.CanGoBack)
            {
                return false;
            }

            ContentFrame.GoBack();
            return true;
        }
    }
}
