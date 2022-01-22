using System;
using Dinkur.Types;

namespace Dinkur.Services
{
    internal class EntryStreamEventArgs : EventArgs
    {
        public EntryEvent EntryEvent { get; }

        public EntryStreamEventArgs(EntryEvent entryEvent)
        {
            EntryEvent = entryEvent;
        }
    }
}
