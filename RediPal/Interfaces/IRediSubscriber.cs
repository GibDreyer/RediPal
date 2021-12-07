using System.Collections.Generic;
using System.Linq.Expressions;
using RedipalCore.Objects;
using System;

namespace RedipalCore.Interfaces
{
    public interface IRediSubscriber
    {
        public string[] GetAllSubscriptions();
        public bool DisposeAllSubscriptions();

        public bool RemoveSubscription(string subscriptionID);

        public List<string> ActiveRedisConnections { get; }

        // Property
        public IRediSubscription<P>? ToProperty<T, P>(string key, Expression<Func<T, P>> property, RediSubscriberOptions options) where T : notnull where P : notnull;

        

        // Object

        /// <summary>
        /// Create a subscription the as an object from the given key
        /// </summary>
        public IRediSubscription<T>? ToObject<T>(string name, RediSubscriberOptions options, string? key = null) where T : notnull;
        public IRediSubscription<T>? ToObject<T>(string name, string? key = null) where T : notnull;

        // public IRediSubscription<T>? ToObject<T>(string name) where T : notnull;



        // List 

        ///// <summary>
        ///// Create a subscription the as a list from the entire set given
        ///// </summary>
        //public IRediSubscription<T>? ToList<T>(string set) where T : notnull;

        ///// <summary>
        /////  Create a subscription the as a list to the given hash Ids
        ///// </summary>
        //public IRediSubscription<T>? ToList<T>(string key, params string[] names) where T : notnull;



        // Dictionary

        /// <summary>
        ///  Create a subscription the as a dictionary from the entire set given
        /// </summary>
        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, string set, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull;
        //public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull;
        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, Action<RediSubscriberOptions>? options = default) where TKey : IConvertible where TValue : notnull;
        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(Action<RediSubscriberOptions>? options = default) where TKey : IConvertible where TValue : notnull;
        //public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull;
        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(Expression<Func<TValue, P>> property, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull;
        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(Expression<Func<TValue, P>> property, string[] keys, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull;
        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(string key, Expression<Func<TValue, P>> property, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull;
        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(string key, Expression<Func<TValue, P>> property, string set, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull;

        /// <summary>
        /// Create a subscription the as a dictionary to the given hash IDs
        /// </summary>
        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, RediSubscriberOptions options, params string[] names) where TKey : IConvertible where TValue : notnull;
        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, params string[] names) where TKey : IConvertible where TValue : notnull;

        /// <summary>
        /// Create a subscription the as a dictionary to the given hash IDs is the form of a message
        /// </summary>
        public IRediSubscriptions<string, string>? ToMessages(string key);
        public IRediSubscriptions<string, string>? ToMessages(string name, params string[] names);
    }
}