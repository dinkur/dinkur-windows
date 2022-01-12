using System.Collections.Generic;
using Dinkur.Api;
using Dinkur.Types;
using Grpc.Core;

namespace Dinkur.Services
{
    internal class DinkurService
    {
        private readonly Tasker.TaskerClient tasker;
        private readonly Alerter.AlerterClient alerter;

        public DinkurService(Tasker.TaskerClient tasker, Alerter.AlerterClient alerter)
        {
            this.tasker = tasker;
            this.alerter = alerter;
        }

        public async IAsyncEnumerable<TaskEvent> StreamTasks()
        {
            var stream = tasker.StreamTask(new StreamTaskRequest());
            await foreach (var resp in stream.ResponseStream.ReadAllAsync())
            {
                if (resp == null || ConvertEventType(resp.Event) is not EventType ev)
                {
                    continue;
                }
                yield return new TaskEvent(new ImmutableTask(resp.Task), ev);
            }
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
