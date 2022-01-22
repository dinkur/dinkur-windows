using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dinkur.Api;
using Dinkur.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Dinkur.Services
{
    internal sealed class DinkurService
    {
        private readonly Entries.EntriesClient entries;
        private readonly Alerter.AlerterClient alerter;
        public event EventHandler<EntryStreamEventArgs>? EntryStreamEvent;
        public event EventHandler<EntryStreamErrorEventArgs>? EntryStreamError;

        public DinkurService(Entries.EntriesClient entries, Alerter.AlerterClient alerter)
        {
            this.entries = entries;
            this.alerter = alerter;
        }

        private void OnEntryStreamEvent(EntryStreamEventArgs args)
        {
            EntryStreamEvent?.Invoke(this, args);
        }

        private void OnEntryStreamError(EntryStreamErrorEventArgs args)
        {
            EntryStreamError?.Invoke(this, args);
        }

        public async Task StreamEntriesAsEvents(CancellationToken cancellationToken = default)
        {
            var backoffLimitTicks = TimeSpan.FromMinutes(5).Ticks;
            var backoffTimeSpan = TimeSpan.FromSeconds(5);
            var backoffFactor = 1.2f;
            while (!cancellationToken.IsCancellationRequested)
            {
                var lastCheck = DateTime.Now;
                try
                {
                    await foreach (var ev in StreamEntries(cancellationToken))
                    {
                        OnEntryStreamEvent(new EntryStreamEventArgs(ev));
                    }
                }
                catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
                {
                    return;
                }
                catch (Exception ex)
                {
                    OnEntryStreamError(new EntryStreamErrorEventArgs(ex));
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    continue;
                }
                if (DateTime.Now - lastCheck < TimeSpan.FromSeconds(5))
                {
                    OnEntryStreamError(new EntryStreamErrorEventArgs(new ApplicationException("Too frequent requests. Backing off requests.")));
                    await Task.Delay(backoffTimeSpan, cancellationToken);
                    backoffTimeSpan = TimeSpan.FromTicks(Math.Min(backoffLimitTicks, (long)(backoffTimeSpan.Ticks * backoffFactor)));
                }
            }
        }

        public async IAsyncEnumerable<EntryEvent> StreamEntries([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var stream = entries.StreamEntry(new StreamEntryRequest(), cancellationToken: cancellationToken);
            await foreach (var resp in stream.ResponseStream.ReadAllAsync(cancellationToken: cancellationToken))
            {
                if (resp == null || ConvertEventType(resp.Event) is not EventType ev)
                {
                    continue;
                }
                yield return new EntryEvent(new ImmutableEntry(resp.Entry), ev);
            }
        }

        public async Task StopActiveEntry(CancellationToken cancellationToken = default)
        {
            await entries.StopActiveEntryAsync(new StopActiveEntryRequest(), cancellationToken: cancellationToken);
        }

        public async Task StopEntry(ulong entryId, CancellationToken cancellationToken = default)
        {
            await entries.UpdateEntryAsync(new UpdateEntryRequest
            {
                IdOrZero = entryId,
                End = DateTimeOffset.Now.ToTimestamp(),
            }, cancellationToken: cancellationToken);
        }

        public async Task DeleteEntry(ulong entryId, CancellationToken cancellationToken = default)
        {
            await entries.DeleteEntryAsync(new DeleteEntryRequest
            {
                Id = entryId,
            }, cancellationToken: cancellationToken);
        }

        public async Task UpdateEntry(ulong entryId, string? newName, DateTimeOffset? newStart, DateTimeOffset? newEnd, CancellationToken cancellationToken = default)
        {
            await entries.UpdateEntryAsync(new UpdateEntryRequest
            {
                IdOrZero = entryId,
                Name = newName ?? "",
                Start = newStart?.ToTimestamp(),
                End = newEnd?.ToTimestamp(),
            }, cancellationToken: cancellationToken);
        }

        public async Task<ImmutableEntry?> GetActiveEntry(CancellationToken cancellationToken = default)
        {
            var activeEntry = (await entries.GetActiveEntryAsync(new GetActiveEntryRequest(), cancellationToken: cancellationToken)).ActiveEntry;
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
