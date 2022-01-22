using System;
using Dinkur.Types;

namespace Dinkur.Services
{
    public class EntryStreamEventArgs : EventArgs
    {
        public EntryEvent EntryEvent { get; }

        public EntryStreamEventArgs(EntryEvent entryEvent)
        {
            EntryEvent = entryEvent;
        }
    }
}
