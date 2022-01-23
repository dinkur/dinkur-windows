using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dinkur.Types;

namespace Dinkur.Models
{
    public class EntryModel : INotifyPropertyChanged
    {
        private string _name;
        private DateTimeOffset? _end;
        private DateTimeOffset _start;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ulong Id { get; }
        public string Name
        {
            get => _name;
            set => ChangeProperty(ref _name, value);
        }

        public DateTimeOffset Start
        {
            get => _start;
            set => ChangeProperty(ref _start, value);
        }

        public DateTimeOffset? End
        {
            get => _end;
            set => ChangeProperty(ref _end, value);
        }

        public EntryModel(ulong id, string name, DateTimeOffset start, DateTimeOffset? end)
        {
            Id = id;
            _name = name;
            _start = start;
            _end = end;
        }

        public EntryModel(ImmutableEntry entry)
            : this(entry.Id, entry.Name, entry.Start, entry.End)
        {
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChangeProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
            where T : IEquatable<T>
        {
            if (!oldValue.Equals(newValue))
            {
                oldValue = newValue;
                OnPropertyChanged(propertyName);
            }
        }

        private void ChangeProperty<T>(ref T? oldValue, T? newValue, [CallerMemberName] string? propertyName = null)
            where T : struct, IEquatable<T>
        {
            if (!oldValue.Equals(newValue))
            {
                oldValue = newValue;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
