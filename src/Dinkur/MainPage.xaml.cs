using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dinkur.Pages;
using Dinkur.Services;
using Dinkur.Types;
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
        public static MainPage Current => _current ?? throw new InvalidOperationException("Main page has not been initialized yet.");
        private static MainPage? _current;

        private readonly DinkurService _dinkurService = App.DinkurService;

        private readonly (string tag, Type page)[] _pages = {
            ("entries", typeof(EntriesPage)),
            ("settings", typeof(SettingsPage)),
        };

        private ImmutableEntry? _activeEntry;

        public MainPage()
        {
            _current = this;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var appWindow = MainWindow.AppWindow;

            _ = UpdateCurrentTask();

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
                var item = _pages.FirstOrDefault(p => p.page == e.SourcePageType);
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag.Equals(item.tag));
            }
        }

        public bool Navigate(Type pageType)
        {
            if (ContentFrame.CurrentSourcePageType == pageType)
            {
                return false;
            }
            ContentFrame.Navigate(pageType);
            return true;
        }

        public bool Navigate(Type pageType, object parameter)
        {
            if (ContentFrame.CurrentSourcePageType == pageType)
            {
                return false;
            }
            ContentFrame.Navigate(pageType, parameter);
            return true;
        }

        public bool Navigate(Type pageType, NavigationTransitionInfo transitionInfo)
        {
            if (ContentFrame.CurrentSourcePageType == pageType)
            {
                return false;
            }
            ContentFrame.Navigate(pageType, null, transitionInfo);
            return true;
        }

        public bool Navigate(string? tag, NavigationTransitionInfo transitionInfo)
        {
            var pageType = _pages.FirstOrDefault(p => p.tag == tag).page;
            if (pageType == null)
            {
                return false;
            }
            return Navigate(pageType, transitionInfo);
        }

        public bool NavigateBack()
        {
            if (!ContentFrame.CanGoBack)
            {
                return false;
            }

            ContentFrame.GoBack();
            return true;
        }

        private void EntryQuickChangeBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
        }

        private void EntryQuickChangeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ResetQuickChangeBoxToCurrentTask();
        }

        private void EntryQuickChangeBox_ProcessKeyboardAccelerators(UIElement sender, Microsoft.UI.Xaml.Input.ProcessKeyboardAcceleratorEventArgs args)
        {
            if (args.Key == Windows.System.VirtualKey.Escape)
            {
                // Reset field
                ResetQuickChangeBoxToCurrentTask();
            }
        }

        private void ResetQuickChangeBoxToCurrentTask()
        {
            EntryQuickChangeBox.Text = _activeEntry?.Name ?? string.Empty;
        }

        private async Task UpdateCurrentTask()
        {
            EntryQuickChangeBox.PlaceholderText = "Loading…";
            try
            {
                _activeEntry = await _dinkurService.GetActiveEntry(App.Window.CloseCancellationToken);
            }
            catch
            {
                // Oh no! Swallow the error
                // TODO: Show error somehow
                _activeEntry = null;
            }
            finally
            {
                EntryQuickChangeBox.PlaceholderText = "New…";
            }
            ResetQuickChangeBoxToCurrentTask();
        }
    }
}
