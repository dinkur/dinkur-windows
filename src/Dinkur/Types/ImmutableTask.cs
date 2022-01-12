using System;
using Dinkur.Api;

namespace Dinkur.Types
{
    internal record ImmutableTask(ulong Id, string Name, DateTime Start, DateTime? End)
    {
        public ImmutableTask(Task task)
            : this(task.Id, task.Name, task.Start.ToDateTime(), task.End?.ToDateTime())
        {
        }
    }
}
