using System;
using Dinkur.Api;

namespace Dinkur.Types
{
    internal record ImmutableTask(ulong Id, string Name, DateTimeOffset Start, DateTimeOffset? End)
    {
        public ImmutableTask(Task task)
            : this(task.Id, task.Name, task.Start.ToDateTimeOffset().LocalDateTime, task.End?.ToDateTimeOffset().LocalDateTime)
        {
        }
    }
}
