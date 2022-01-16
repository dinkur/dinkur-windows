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
    internal sealed partial class EditTaskPage : Page
    {
        private readonly DinkurService dinkurService = new(App.Tasker, App.Alerter);

        private ImmutableTask task;
        public string TaskName { get; set; }

        public DateTimeOffset? taskStart;
        public DateTimeOffset? taskEnd;

        public EditTaskPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            task = (ImmutableTask)e.Parameter;
            TaskName = task.Name;
            taskStart = task.Start;
            taskEnd = task.End;

            TaskEndTimeInfoBar.Visibility = task.End.HasValue ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var newName = task.Name != TaskName ? TaskName : null;
            await dinkurService.UpdateTask(task.Id, newName, taskStart ?? DateTimeOffset.Now, taskEnd);
            MainPage.Current.NavigateBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.NavigateBack();
        }
    }
}
