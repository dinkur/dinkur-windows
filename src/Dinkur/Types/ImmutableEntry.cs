using System;
using Dinkur.Api;

namespace Dinkur.Types
{
    internal record ImmutableEntry(ulong Id, string Name, DateTimeOffset Start, DateTimeOffset? End)
    {
        public ImmutableEntry(Entry entry)
            : this(entry.Id, entry.Name, entry.Start.ToDateTimeOffset().LocalDateTime, entry.End?.ToDateTimeOffset().LocalDateTime)
        {
        }
    }
}
