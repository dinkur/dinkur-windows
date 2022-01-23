using System;
using Dinkur.Services;
using Dinkur.Types;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditEntryPage : Page
    {
        private readonly DinkurService dinkurService = App.DinkurService;

        private ImmutableEntry? entry;
        public string? EntryName { get; set; }

        private DateTimeOffset? entryStart;
        private DateTimeOffset? entryEnd;

        public EditEntryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            entry = (ImmutableEntry)e.Parameter;
            EntryName = entry.Name;
            entryStart = entry.Start;
            entryEnd = entry.End;

            EntryEndTimeInfoBar.Visibility = entry.End.HasValue ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (entry == null)
            {
                throw new InvalidOperationException("The entry edit page has not been properly initialized with an entry.");
            }

            var newName = entry.Name != EntryName ? EntryName : null;
            await dinkurService.UpdateEntry(entry.Id, newName, entryStart ?? DateTimeOffset.Now, entryEnd);
            MainPage.Current.NavigateBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.NavigateBack();
        }
    }
}
