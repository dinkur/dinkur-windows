using System;
using System.Collections.Generic;
using Dinkur.Api;
using Dinkur.Types;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using AsyncTask = System.Threading.Tasks.Task;

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

        public async AsyncTask StopActiveTask()
        {
            await tasker.StopActiveTaskAsync(new StopActiveTaskRequest());
        }

        public async AsyncTask StopTask(ulong taskId)
        {
            await tasker.UpdateTaskAsync(new UpdateTaskRequest
            {
                IdOrZero = taskId,
                End = DateTimeOffset.Now.ToTimestamp(),
            });
        }

        public async AsyncTask DeleteTask(ulong taskId)
        {
            await tasker.DeleteTaskAsync(new DeleteTaskRequest
            {
                Id = taskId,
            });
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
