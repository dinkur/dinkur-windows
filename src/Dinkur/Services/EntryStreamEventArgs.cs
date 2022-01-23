using System;
using Dinkur.Types;

namespace Dinkur.Services
{
    public class EntryStreamEventArgs : EventArgs
    {
        public ImmutableEntry Entry { get; }
        public EventType EventType { get; }

        public EntryStreamEventArgs(EntryEvent entryEvent)
        {
            Entry = entryEvent.Entry;
            EventType = entryEvent.EventType;
        }
    }
}
