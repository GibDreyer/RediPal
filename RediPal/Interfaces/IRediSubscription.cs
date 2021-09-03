using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedipalCore.Interfaces
{
    public interface IRediSubscription<T> : IDisposable where T : notnull
    {
        public string SubscriptionID { get; }
        public string KeySpace { get; }
        public string Key { get; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; }

        public event Action<T?>? OnChange;

        public new void Dispose();

        /// <summary>
        /// Builds the object that is subscribed to
        /// </summary>
        /// <returns>An object or value</returns>
        public T? Read();

        public List<Delegate>? Conditions { get; set; }
        public bool AddConditional(Func<T, bool> condition);

        public bool RemoveConditionals();
    }

    public interface IRediSubscriptions<TKey, TValue> : IDisposable where TKey : IConvertible where TValue : notnull
    {
        public Dictionary<TKey, IRediSubscription<TValue>> Subscriptions { get; }

        public string SubscriptionID { get; }
        public DateTime LastUpdated { get; }
        public event Action<TKey, TValue?>? OnValueUpdate;
        public event Action<TKey>? OnRemoved;
        public event Action<TKey, TValue?>? OnAdded;

        public RediReadTask<TKey, TValue> Read();

        public new void Dispose(); 
        public void UnsubscribeMembers();
    }
}
