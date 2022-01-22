using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dinkur.Controls
{
    public sealed partial class DateTimePicker : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register(nameof(DateTime), typeof(DateTimeOffset?), typeof(DateTimePicker), null);

        private static readonly TimeSpan span1Day = TimeSpan.FromDays(1);
        private static readonly TimeSpan span15Min = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan span5Min = TimeSpan.FromMinutes(5);

        public string? DateLabel { get; set; }
        public string? TimeLabel { get; set; }

        public DateTimeOffset? DateTime
        {
            get => (DateTimeOffset?)GetValue(DateTimeProperty);
            set => SetValue(DateTimeProperty, value);
        }

        public DateTimeOffset? Date
        {
            get => DateTime?.Date;
            set => SetDateTime(value, Time);
        }

        public TimeSpan? Time
        {
            get => DateTime?.TimeOfDay;
            set => SetDateTime(Date, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTimePicker()
        {
            InitializeComponent();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TimeNow_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Time = DateTimeOffset.Now.TimeOfDay;
        }

        private void TimeMinus5Min_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Time = (Time ?? DateTimeOffset.Now.TimeOfDay).Subtract(span5Min);
        }

        private void TimeMinus15Min_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Time = (Time ?? DateTimeOffset.Now.TimeOfDay).Subtract(span15Min);
        }

        private void DateToday_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Date = DateTimeOffset.Now;
        }

        private void DateMinus1Day_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            Date = (Date ?? DateTimeOffset.Now).Subtract(span1Day);
        }

        private static DateTimeOffset? CreateDateTime(DateTimeOffset? date, TimeSpan? timeOfDay)
        {
            if (date == null && timeOfDay == null)
            {
                return null;
            }

            var now = DateTimeOffset.Now;
            var d = date ?? now;
            var t = timeOfDay ?? now.TimeOfDay;
            return new DateTimeOffset(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds, t.Milliseconds, d.Offset);
        }

        private void SetDateTime(DateTimeOffset? date, TimeSpan? timeOfDay)
        {
            SetDateTime(CreateDateTime(date, timeOfDay));
        }

        private void SetDateTime(DateTimeOffset? dateTime)
        {
            var oldDate = DateTime;
            DateTime = dateTime;
            if (oldDate != dateTime)
            {
                OnPropertyChanged(nameof(Date));
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(DateTime));
            }
        }
    }
}
