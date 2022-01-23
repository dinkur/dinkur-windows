using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Dinkur.Types
{
    public class SortedEntryList : IReadOnlyList<ImmutableEntry>, INotifyCollectionChanged, IList
    {
        private static readonly ImmutableEntryStartComparer comparer = new();

        private readonly List<ImmutableEntry> entries = new();
        private readonly HashSet<ulong> entryIds = new();

        public ImmutableEntry this[int index] => entries[index];

        public int Count => entries.Count;

        public bool IsFixedSize { get; } = false;
        public bool IsReadOnly { get; } = true;
        public bool IsSynchronized { get; } = false;
        public object SyncRoot => this;

        object? IList.this[int index]
        {
            get => entries[index];
            set => throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public SortedEntryList()
        {
        }

        public SortedEntryList(IEnumerable<ImmutableEntry> entries)
        {
            AddRange(entries);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public void Clear()
        {
            entries.Clear();
            entryIds.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddOrUpdateById(ImmutableEntry entry)
        {
            if (entryIds.Contains(entry.Id))
            {
                // Update
                var idx = entries.FindIndex(t => t.Id == entry.Id);
                var old = entries[idx];
                entries[idx] = entry;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, entry, old, idx));
            }
            else
            {
                // New
                var idx = entries.BinarySearch(entry, comparer);
                if (idx < 0)
                {
                    idx = ~idx;
                }
                entries.Insert(idx, entry);
                entryIds.Add(entry.Id);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry, idx));
            }
        }

        public void AddRange(IEnumerable<ImmutableEntry> entries)
        {
            foreach (var entry in entries)
            {
                AddOrUpdateById(entry);
            }
        }

        public bool RemoveById(ulong id)
        {
            var idx = entries.FindIndex(x => x.Id == id);
            if (idx == -1)
            {
                return false;
            }

            var entry = entries[idx];
            entries.RemoveAt(idx);
            entryIds.Remove(id);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entry, idx));
            return true;
        }

        public IEnumerator<ImmutableEntry> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        public int Add(object? value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object? value)
        {
            if (value is not ImmutableEntry entry)
            {
                throw new ArgumentException($"Expected {nameof(ImmutableEntry)}, but got {value?.GetType().Name??"<null>"}.", nameof(value));
            }
            return entries.Contains(entry);
        }

        public int IndexOf(object? value)
        {
            if (value is not ImmutableEntry entry)
            {
                throw new ArgumentException($"Expected {nameof(ImmutableEntry)}, but got {value?.GetType().Name??"<null>"}.", nameof(value));
            }
            return entries.IndexOf(entry);
        }

        public void Insert(int index, object? value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object? value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)entries).CopyTo(array, index);
        }

        private sealed class ImmutableEntryStartComparer : IEqualityComparer<ImmutableEntry>, IComparer<ImmutableEntry>
        {
            public int Compare(ImmutableEntry? x, ImmutableEntry? y)
            {
                return (x, y) switch
                {
                    (null, null) => 0,
                    (null, _) => -1,
                    (_, null) => 1,
                    _ => x.Start.CompareTo(y.Start),
                };
            }

            public bool Equals(ImmutableEntry? x, ImmutableEntry? y)
            {
                return x?.Start == y?.Start;
            }

            public int GetHashCode([DisallowNull] ImmutableEntry obj)
            {
                return obj.Start.GetHashCode();
            }
        }
    }
}
