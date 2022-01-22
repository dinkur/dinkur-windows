using System;
using System.Linq;
using System.Threading.Tasks;
using Dinkur.Api;
using Dinkur.Services;
using Dinkur.Types;
using Grpc.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntriesPage : Page
    {
        private readonly SortedEntryList entries = new();
        private readonly DinkurService dinkurService = App.DinkurService;
        private ImmutableEntry? entriesCommandTarget;

        public EntriesPage()
        {
            InitializeComponent();

            _ = ReloadEntries();
            _ = StreamEntryUpdatesLoop();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Bug", "S2190:Recursion should not be infinite", Justification = "There's no recursion here.")]
        private async Task StreamEntryUpdatesLoop()
        {
            while (true)
            {
                try
                {
                    await foreach (var update in dinkurService.GetEntryStream())
                    {
                        switch (update.EventType)
                        {
                            case EventType.Created:
                            case EventType.Updated:
                                entries.AddOrUpdateById(update.Entry);
                                ShowHideEntryListBasedOnCount();
                                break;
                            case EventType.Deleted:
                                entries.RemoveById(update.Entry.Id);
                                ShowHideEntryListBasedOnCount();
                                break;
                        }
                    }
                }
                catch
                {
                    // Silently swallow exception
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async Task ReloadEntries()
        {
            try
            {
                EntryProgressBar.Visibility = Visibility.Visible;
                EntryErrorResults.Visibility = Visibility.Collapsed;
                EntryNoResults.Visibility = Visibility.Collapsed;
                EntryListView.Visibility = Visibility.Collapsed;

                entries.Clear();
                foreach (var entry in await dinkurService.GetEntryListToday(App.Window.CloseCancellationToken))
                {
                    entries.AddOrUpdateById(entry);
                }
                ShowHideEntryListBasedOnCount();
            }
            catch (Exception e)
            {
                EntryErrorResults.Message = e.ToString();
                EntryErrorResults.Visibility = Visibility.Visible;
            }
            finally
            {
                EntryProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowHideEntryListBasedOnCount()
        {
            var hasEntries = entries.Count > 0;
            var visibleWhenEntries = hasEntries ? Visibility.Visible : Visibility.Collapsed;
            var visibleWhenNoEntries = hasEntries ? Visibility.Collapsed : Visibility.Visible;
            EntryNoResults.Visibility = visibleWhenNoEntries;
            EntryListView.Visibility = visibleWhenEntries;
        }

        private void ReloadEntriesButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ReloadEntries();
        }

        private void EntryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var entry = (ImmutableEntry)e.ClickedItem;
            entriesCommandTarget = entry;
            EntryCommandStop.IsEnabled = entry.End == null;
            var container = (FrameworkElement)EntryListView.ContainerFromItem(entry);
            EntryCommandBarFlyout.ShowAt(container);
        }

        private async void EntryCommandStop_Click(object sender, RoutedEventArgs e)
        {
            if (entriesCommandTarget == null)
            {
                return;
            }
            EntryCommandStop.IsEnabled = false;
            EntryCommandBarFlyout.Hide();
            await dinkurService.StopEntry(entriesCommandTarget.Id);
        }

        private void EntryCommandEdit_Click(object sender, RoutedEventArgs e)
        {
            if (entriesCommandTarget == null)
            {
                return;
            }
            EntryCommandBarFlyout.Hide();
            MainPage.Current.Navigate(typeof(EditEntryPage), entriesCommandTarget);
        }

        private async void EntryCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            if (entriesCommandTarget == null)
            {
                return;
            }
            EntryCommandBarFlyout.Hide();
            var entry = entriesCommandTarget;
            var dialog = new ContentDialog
            {
                Title = "Are you sure?",
                PrimaryButtonText = "Delete entry",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                Content = new DeleteEntryDialog(entry),
                XamlRoot = Content.XamlRoot, // https://github.com/microsoft/microsoft-ui-xaml/issues/2504#issuecomment-632352279
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await dinkurService.DeleteEntry(entry.Id);
                ShowHideEntryListBasedOnCount();
            }
        }
    }
}
