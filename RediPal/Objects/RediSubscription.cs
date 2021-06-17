using RedipalCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace RedipalCore.Objects
{
    internal class RediObjectSubscription<T> : IDisposable, IRediSubscription<T> where T : notnull
    {
        private readonly IRediReader Reader;
        private readonly RediSubscriberOptions _options;

        internal RediObjectSubscription(IRediReader reader, string keySpace, string key, string subscriptionID, RediSubscriberOptions options)
        {
            _options = options;
            this.Reader = reader;
            this.Key = key.ToLower();
            this.KeySpace = keySpace.ToLower();
            SubscriptionID = subscriptionID;
        }

        public string SubscriptionID { get; }
        public string Key { get; }
        public string KeySpace { get; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public IRediSubscription<P>? Redi_Subscribe<P>(Expression<Func<T, P>> property) where P : notnull
        {
            if (Redipal.IFactory != null)
            {
                return Redipal.IFactory.RediPalInstance.Subscribe.ToProperty(Key, property, new RediSubscriberOptions());
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }


        public event Action<T>? OnChange;


        public List<Delegate>? Conditions { get; set; }

        public bool AddConditional(Func<T, bool> condition)
        {
            if (Conditions is null)
                Conditions = new List<Delegate>();

            Conditions.Add(condition);
            return true;
        }

        public bool RemoveConditionals()
        {
            Conditions = null;
            return true;
        }


        public T? Read()
        {
            var obj = Reader.Object<T>(Key);
            if (obj != null)
            {
                return obj;
            }
            else
            {
                return default;
            }
        }

        internal bool InvokeReloadObject()
        {
            if (new DateTimeOffset(DateTime.Now) - new DateTimeOffset(LastUpdated) > TimeSpan.FromMilliseconds(_options.MaxPublishingInterval))
            {
                LastUpdated = DateTime.Now;
                if (Reader != null && Key != null)
                {
                    var obj = Reader.Object<T>(KeySpace +":"+ Key);
                    if (obj != null)
                    {
                        if (Conditions is null || Conditions.Any(x => x.DynamicInvoke(obj) is bool condition && condition))
                        {
                            InvokeOnChanged(obj);
                            return true;
                        }
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void InvokeOnChanged(T obj)
        {
            if (OnChange != null)
            {
                OnChange.Invoke(obj);
            }
        }

        public void Dispose()
        {
            OnChange = null;

            if (RediSubscriber.Subscriber != null)
            {
                RediSubscriber.Subscriber.Unsubscribe(SubscriptionID);
            }
            if (Redipal.IFactory is not null)
            {
                Redipal.IFactory.RediPalInstance.Subscribe.ActiveRedisConnections.Remove(SubscriptionID);
            }
            RediSubscriber.Subscriptions.Remove(SubscriptionID);
        }
    }

    internal class RediValueSubscription<T> : IDisposable, IRediSubscription<T> where T : notnull
    {
        private readonly Stopwatch stopwatch = new();
        private readonly IRediReader Reader;
        private T? lastValue;

        internal RediValueSubscription(IRediReader reader, string keySpace, string key, string subscriptionID, Delegate invoker)
        {
            Reader = reader;
            stopwatch.Start();
            Key = key.ToLower();
            KeySpace = keySpace.ToLower();
            SubscriptionID = subscriptionID;
            lastValue = Reader.Value<T>(Key);
            Invoker = invoker;
        }

        public string SubscriptionID { get; set; }
        public string KeySpace { get; set; }
        public string Key { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        internal Delegate Invoker { get; set; }
        public event Action<T>? OnChange;

        public List<Delegate>? Conditions { get; set; }

        public bool AddConditional(Func<T, bool> condition)
        {
            if (Conditions is null)
                Conditions = new List<Delegate>();

            Conditions.Add(condition);
            return true;
        }

        public bool RemoveConditionals()
        {
            Conditions = null;
            return true;
        }

        public T? Read()
        {
            var obj = Reader.Value<T>(Key);
            if (obj != null)
            {
                lastValue = obj;
                return obj;
            }
            else
            {
                return default;
            }
        }

        public void Dispose()
        {
            OnChange = null;

            if (RediSubscriber.Subscriber is not null)
            {
                RediSubscriber.Subscriber.Unsubscribe(SubscriptionID);
            }
            if (Redipal.IFactory is not null)
            {
                Redipal.IFactory.RediPalInstance.Subscribe.ActiveRedisConnections.Remove(SubscriptionID);
            }
            RediSubscriber.Subscriptions.Remove(SubscriptionID);
        }

        internal bool InvokeReloadObject(object obj)
        {
            LastUpdated = DateTime.Now;
            if (obj is not null && Invoker is not null)
            {
                var value = Invoker.DynamicInvoke(obj);
                if (Conditions is null || Conditions.Any(x => x.DynamicInvoke(value) is bool condition && condition))
                {
                    if (value is T t && t is not null && (lastValue is null || t.ToString() != lastValue.ToString()))
                    {
                        lastValue = t;
                        InvokeOnChanged(t);
                        return true;
                    }
                }
            }
            return false;
        }

        private void InvokeOnChanged(T obj)
        {
            if (OnChange != null)
            {
                OnChange.Invoke(obj);
            }
        }
    }
}
