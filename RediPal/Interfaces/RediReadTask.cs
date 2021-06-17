using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public class RediReadTask<TKey, TValue> where TKey : IConvertible where TValue : notnull
    {
        private Stopwatch stopwatch = new();

        public RediReadTask(Task<Dictionary<TKey, TValue>?> task, Progress<int> progress)
        {
            Task = task;
            stopwatch.Start();

            Task.ContinueWith(x =>
            {
                if (OnComplete is not null)
                {
                    OnComplete.Invoke(x.Result);
                    stopwatch.Stop();
                }
            });

            progress.ProgressChanged += (s, amount) =>
            {
                if (TotalRead < amount)
                {
                    TotalRead = amount;
                    if (stopwatch.Elapsed.TotalMilliseconds > 50 && OnProggress != null)
                    {
                        stopwatch.Restart();
                        OnProggress.Invoke(TotalRead, TotalItems);
                    }
                }
            };
        }

        public Task<Dictionary<TKey, TValue>?> Task { get; set; }
        public int TotalItems { get; set; }
        public int TotalRead { get; set; }

        public event Action<int, int>? OnProggress;
        public event Action<Dictionary<TKey, TValue>?>? OnComplete;
    }
}
