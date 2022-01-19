using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dinkur.Api;
using Dinkur.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Dinkur.Services
{
    internal class DinkurService
    {
        private readonly Entries.EntriesClient entries;
        private readonly Alerter.AlerterClient alerter;

        public DinkurService(Entries.EntriesClient entries, Alerter.AlerterClient alerter)
        {
            this.entries = entries;
            this.alerter = alerter;
        }

        public async IAsyncEnumerable<EntryEvent> StreamEntrys()
        {
            var stream = entries.StreamEntry(new StreamEntryRequest());
            await foreach (var resp in stream.ResponseStream.ReadAllAsync())
            {
                if (resp == null || ConvertEventType(resp.Event) is not EventType ev)
                {
                    continue;
                }
                yield return new EntryEvent(new ImmutableEntry(resp.Entry), ev);
            }
        }

        public async Task StopActiveEntry()
        {
            await entries.StopActiveEntryAsync(new StopActiveEntryRequest());
        }

        public async Task StopEntry(ulong entryId)
        {
            await entries.UpdateEntryAsync(new UpdateEntryRequest
            {
                IdOrZero = entryId,
                End = DateTimeOffset.Now.ToTimestamp(),
            });
        }

        public async Task DeleteEntry(ulong entryId)
        {
            await entries.DeleteEntryAsync(new DeleteEntryRequest
            {
                Id = entryId,
            });
        }

        public async Task UpdateEntry(ulong entryId, string? newName, DateTimeOffset? newStart, DateTimeOffset? newEnd)
        {
            await entries.UpdateEntryAsync(new UpdateEntryRequest
            {
                IdOrZero = entryId,
                Name = newName ?? "",
                Start = newStart?.ToTimestamp(),
                End = newEnd?.ToTimestamp(),
            });
        }

        public async Task<ImmutableEntry?> GetActiveEntry()
        {
            var activeEntry = (await entries.GetActiveEntryAsync(new GetActiveEntryRequest())).ActiveEntry;
            return activeEntry == null ? null : new ImmutableEntry(activeEntry);
        }

        private static EventType? ConvertEventType(Event ev) =>
            ev switch
            {
                Event.Created => EventType.Created,
                Event.Updated => EventType.Updated,
                Event.Deleted => EventType.Deleted,
                _ => null,
            };

    }
}
