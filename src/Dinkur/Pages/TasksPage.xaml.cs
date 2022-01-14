using System;
using System.Linq;
using Dinkur.Api;
using Dinkur.Services;
using Dinkur.Types;
using Grpc.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using AsyncTask = System.Threading.Tasks.Task;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TasksPage : Page
    {
        private readonly SortedTaskList tasks = new();
        private readonly DinkurService dinkurService = new(App.Tasker, App.Alerter);
        private ImmutableTask? taskCommandTarget;

        public TasksPage()
        {
            InitializeComponent();

            _ = ReloadTasks();
            _ = StreamTaskUpdatesLoop();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Bug", "S2190:Recursion should not be infinite", Justification = "There's no recursion here.")]
        private async AsyncTask StreamTaskUpdatesLoop()
        {
            while (true)
            {
                try
                {
                    await foreach (var update in dinkurService.StreamTasks())
                    {
                        switch (update.EventType)
                        {
                            case EventType.Created:
                            case EventType.Updated:
                                tasks.AddOrUpdateById(update.Task);
                                ShowHideTaskListBasedOnCount();
                                break;
                            case EventType.Deleted:
                                tasks.RemoveById(update.Task.Id);
                                ShowHideTaskListBasedOnCount();
                                break;
                        }
                    }
                }
                catch
                {
                    // Silently swallow exception
                }
                await AsyncTask.Delay(TimeSpan.FromSeconds(5));
            }
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
                });

                tasks.Clear();
                foreach (var task in response.Tasks)
                {
                    tasks.AddOrUpdateById(new ImmutableTask(task));
                }
                ShowHideTaskListBasedOnCount();
            }
            catch (Exception e)
            {
                TaskErrorResults.Message = e.ToString();
                TaskErrorResults.Visibility = Visibility.Visible;
            }
            finally
            {
                TaskProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowHideTaskListBasedOnCount()
        {
            var hasTasks = tasks.Count > 0;
            var visibleWhenTasks = hasTasks ? Visibility.Visible : Visibility.Collapsed;
            var visibleWhenNoTasks = hasTasks ? Visibility.Collapsed : Visibility.Visible;
            TaskNoResults.Visibility = visibleWhenNoTasks;
            TaskListView.Visibility = visibleWhenTasks;
        }

        private void ReloadTasksButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ReloadTasks();
        }

        private void TaskListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var task = (ImmutableTask)e.ClickedItem;
            taskCommandTarget = task;
            TaskCommandStop.IsEnabled = task.End == null;
            var container = (FrameworkElement)TaskListView.ContainerFromItem(task);
            TaskCommandBarFlyout.ShowAt(container);
        }

        private async void TaskCommandStop_Click(object sender, RoutedEventArgs e)
        {
            if (taskCommandTarget == null)
            {
                return;
            }
            TaskCommandStop.IsEnabled = false;
            TaskCommandBarFlyout.Hide();
            await dinkurService.StopTask(taskCommandTarget.Id);
        }

        private void TaskCommandEdit_Click(object sender, RoutedEventArgs e)
        {
            if (taskCommandTarget == null)
            {
                return;
            }
            TaskCommandBarFlyout.Hide();
            // TODO: Move to task edit modal or page
        }

        private async void TaskCommandDelete_Click(object sender, RoutedEventArgs e)
        {
            if (taskCommandTarget == null)
            {
                return;
            }
            TaskCommandBarFlyout.Hide();
            var task = taskCommandTarget;
            var dialog = new ContentDialog
            {
                Title = "Are you sure?",
                PrimaryButtonText = "Delete task",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                Content = new DeleteTaskDialog(task),
                XamlRoot = Content.XamlRoot, // https://github.com/microsoft/microsoft-ui-xaml/issues/2504#issuecomment-632352279
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await dinkurService.DeleteTask(task.Id);
                ShowHideTaskListBasedOnCount();
            }
        }
    }
}
