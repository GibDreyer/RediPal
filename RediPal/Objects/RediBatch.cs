using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    public class RediBatch
    {
        private ConcurrentBag<Task> Actions { get; } = new ConcurrentBag<Task>();
        private IBatch Batch { get; set; }

        public RediBatch(IBatch batch)
        {
            Batch = batch;
        }


        public int Count => Actions.Count;


        public Task AddAction(Func<IBatch, Task> action)
        {
            var task = action.Invoke(Batch);
            Actions.Add(task);
            return task;
        }

        public bool AddActions(params Func<IBatch, Task>[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                Actions.Add(actions[i].Invoke(Batch));
            }
            return true;
        }


        public bool Execute()
        {
            try
            {
                if (Actions.IsEmpty)
                {
                    return true;
                }

                if (Batch != null)
                {
                    Batch.Execute();
                    Batch.WaitAll(Actions.ToArray());
                    Actions.Clear();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (Redipal.IFactory is not null)
                {
                    Redipal.IFactory.RediPalInstance.InvokeError(RediError.UnableToWrite, e.Message);
                }

                return false;
            }
        }

        public async Task<bool> ExecuteAsync()
        {
            try
            {
                if (Actions.IsEmpty)
                {
                    return true;
                }

                if (Batch != null)
                {
                    Batch.Execute();
                    await Task.WhenAll(Actions);
                    Actions.Clear();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (Redipal.IFactory is not null)
                {
                    Redipal.IFactory.RediPalInstance.InvokeError(RediError.UnableToWrite, e.Message);
                }

                return false;
            }
        }

        public IBatch GetBatch()
        {
            return Batch;
        }
    }
}
