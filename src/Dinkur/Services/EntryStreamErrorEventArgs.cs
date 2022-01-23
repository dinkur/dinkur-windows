using System;
using Dinkur.Types;

namespace Dinkur.Services
{
    public class EntryStreamErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public EntryStreamErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
