using RedipalCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    //internal class RediListSubscription<T> : IDisposable, IRediSubscription<T> where T : notnull
    //{
    //    public IRediSubscription<T>[]? Subscriptions { get; }

    //    public string SubscriptionID { get; set; } = "";
    //    public string SpaceKey { get; } = "";
    //    public string Key { get; set; } = "";
    //    public DateTime LastUpdated { get; set; }

    //    string IRediSubscription<T>.KeySpace => throw new NotImplementedException();

    //    public event Action<T>? OnChange;
    //    public event Action<T>? OnAdded;
    //    public event Action? OnRemoved;

    //    public T? Read()
    //    {
    //        return default;
    //    }

    //    internal void InvokeOnChanged(T obj)
    //    {
    //        if (OnChange != null)
    //        {
    //            OnChange.Invoke(obj);
    //        }
    //    }

    //    internal void InvokeOnAdded(T obj)
    //    {
    //        if (OnAdded != null)
    //        {
    //            OnAdded.Invoke(obj);
    //        }
    //    }

    //    internal void InvokeOnRemoved()
    //    {
    //        if (OnRemoved != null)
    //        {
    //            OnRemoved.Invoke();
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        OnChange = null;

    //        if (Subscriptions != null)
    //        {
    //            foreach (var sub in Subscriptions)
    //            {
    //                sub.Dispose();
    //            }
    //        }
    //    }
    //}

    internal class RediDictionarySubscription<TKey, TValue> : IDisposable, IRediSubscriptions<TKey, TValue> where TKey : IConvertible where TValue : notnull
    {
        internal RediDictionarySubscription(string subscriptionID)
        {
            SubscriptionID = subscriptionID;
            Subscriptions = new Dictionary<TKey, IRediSubscription<TValue>>();
        }

        internal List<string> SetKeys { get; } = new List<string>();

        public Dictionary<TKey, IRediSubscription<TValue>> Subscriptions { get; internal set; }

        internal bool IsMessages { get; set; }
        internal bool IsUnsetKeySpace { get; set; }
        public string SubscriptionID { get; }
        public string MemberKeySpace { get; internal set; } = "";

        public DateTime LastUpdated { get; private set; }

        public event Action<TKey, TValue?>? OnValueUpdate;
        public event Action<TKey>? OnRemoved;
        public event Action<TKey, TValue?>? OnAdded;

        public RediReadTask<TKey, TValue> Read()
        {
            var progressIndicator = new Progress<int>();

            RediReadOptions readOptions = new()
            {
                Progress = progressIndicator,
            };

            if (IsMessages)
            {
                RediReadTask<TKey, TValue>? reader = new(Task.Run(() => GetMessages()), progressIndicator)
                {
                    TotalItems = Subscriptions.Count
                };

                return reader;

                Dictionary<TKey, TValue>? GetMessages()
                {
                    var result = Redipal.IFactory!.RediPalInstance.Read.Messages(SetKeys.ToArray());
                    if (result is not null)
                    {
                        var d = new Dictionary<TKey, TValue>();
                        foreach (var item in result)
                        {
                            if (item.Key is TKey key && item.Value is TValue value)
                            {
                                d.Add(key, value);
                            }
                        }
                        return d;
                    }
                    else
                    {
                        return default;
                    }
                }
            }
            else
            {
                RediReadTask<TKey, TValue>? reader = new(Task.Run(() => Redipal.IFactory!.RediPalInstance.Read.Dictionary<TKey, TValue>(MemberKeySpace, SetKeys.ToArray(), readOptions)), progressIndicator)
                {
                    TotalItems = Subscriptions.Count
                };

                return reader;
            }
        }

        public void UnsubscribeMembers()
        {
            if (Subscriptions != null)
            {
                foreach (var sub in Subscriptions)
                {
                    sub.Value.Dispose();
                }
            }
        }

        internal void InvokeOnChanged(TKey key, TValue? value)
        {
            if (OnValueUpdate != null)
            {
                OnValueUpdate.Invoke(key, value);
            }
        }

        internal void InvokeOnRemoved(TKey key)
        {
            if (OnRemoved != null)
            {
                OnRemoved.Invoke(key);
            }
        }

        internal void InvokeOnAdded(TKey key, TValue? value)
        {
            if (OnAdded != null)
            {
                OnAdded.Invoke(key, value);
            }
        }

        public void Dispose()
        {
            OnValueUpdate = null;
            OnAdded = null;
            OnRemoved = null;

            if (Subscriptions != null)
            {
                foreach (var sub in Subscriptions)
                {
                    sub.Value.Dispose();
                }
            }

            if (RediSubscriber.Subscriber != null)
            {
                RediSubscriber.Subscriber.Unsubscribe(SubscriptionID);
            }
            RediSubscriber.Subscriptions.Remove(SubscriptionID);
        }
    }
}
