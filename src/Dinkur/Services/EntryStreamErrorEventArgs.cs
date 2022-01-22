using System;
using Dinkur.Types;

namespace Dinkur.Services
{
    internal class EntryStreamErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public EntryStreamErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
