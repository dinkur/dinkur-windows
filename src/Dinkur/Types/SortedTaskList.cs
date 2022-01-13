using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Dinkur.Types
{
    internal class SortedTaskList : IReadOnlyList<ImmutableTask>, INotifyCollectionChanged, IList
    {
        private static readonly ImmutableTaskStartComparer comparer = new();

        private readonly List<ImmutableTask> tasks = new();
        private readonly HashSet<ulong> taskIds = new();

        public ImmutableTask this[int index] => tasks[index];

        public int Count => tasks.Count;

        public bool IsFixedSize { get; } = false;
        public bool IsReadOnly { get; } = true;
        public bool IsSynchronized { get; } = false;
        public object SyncRoot => this;

        object IList.this[int index]
        {
            get => tasks[index];
            set => throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SortedTaskList()
        {
        }

        public SortedTaskList(IEnumerable<ImmutableTask> tasks)
        {
            AddRange(tasks);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        public void Clear()
        {
            tasks.Clear();
            taskIds.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void AddOrUpdateById(ImmutableTask task)
        {
            if (taskIds.Contains(task.Id))
            {
                // Update
                var idx = tasks.FindIndex(t => t.Id == task.Id);
                var old = tasks[idx];
                tasks[idx] = task;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, task, old, idx));
            }
            else
            {
                // New
                var idx = tasks.BinarySearch(task, comparer);
                if (idx < 0)
                {
                    idx = ~idx;
                }
                tasks.Insert(idx, task);
                taskIds.Add(task.Id);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, task, idx));
            }
        }

        public void AddRange(IEnumerable<ImmutableTask> tasks)
        {
            foreach (var task in tasks)
            {
                AddOrUpdateById(task);
            }
        }

        public bool RemoveById(ulong id)
        {
            var idx = tasks.FindIndex(x => x.Id == id);
            if (idx == -1)
            {
                return false;
            }

            var task = tasks[idx];
            tasks.RemoveAt(idx);
            taskIds.Remove(id);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, task, idx));
            return true;
        }

        public IEnumerator<ImmutableTask> GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            if (value is not ImmutableTask task)
            {
                throw new ArgumentException($"Expected {nameof(ImmutableTask)}, but got {value?.GetType().Name??"<null>"}.", nameof(value));
            }
            return tasks.Contains(task);
        }

        public int IndexOf(object value)
        {
            if (value is not ImmutableTask task)
            {
                throw new ArgumentException($"Expected {nameof(ImmutableTask)}, but got {value?.GetType().Name??"<null>"}.", nameof(value));
            }
            return tasks.IndexOf(task);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)tasks).CopyTo(array, index);
        }

        private sealed class ImmutableTaskStartComparer : IEqualityComparer<ImmutableTask>, IComparer<ImmutableTask>
        {
            public int Compare(ImmutableTask x, ImmutableTask y)
            {
                return x.Start.CompareTo(y.Start);
            }

            public bool Equals(ImmutableTask x, ImmutableTask y)
            {
                return x.Start == y.Start;
            }

            public int GetHashCode([DisallowNull] ImmutableTask obj)
            {
                return obj.Start.GetHashCode();
            }
        }
    }
}
