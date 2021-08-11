using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using RedipalCore.TestObjects;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RedipalCore
{
    internal class RediSubscriber : IRediSubscriber
    {
        // Fields
        internal static Dictionary<string, dynamic> Subscriptions { get; } = new Dictionary<string, dynamic>();
        private readonly IDatabase db;
        private readonly IRediReader Reader;
        private readonly Pluralize.NET.Pluralizer Pluralizer = new();
        internal IRediFactory Extensions { get; }

        // Constructor
        internal RediSubscriber(ConnectionMultiplexer redis, IDatabase database, IRediReader deserializer, IRediFactory descriptor)
        {
            db = database;

            if (redis != null)
            {
                Subscriber = redis.GetSubscriber();
            }
            Extensions = descriptor;
            this.Reader = deserializer;
            ActiveRedisConnections = new();
        }




        // Properties
        public static ISubscriber? Subscriber { get; set; }

        public List<string> ActiveRedisConnections { get; private set; }

        // Commands
        public string[] GetAllSubscriptions()
        {
            return Subscriptions.Select(x => x.Key).ToArray();
        }

        public bool DisposeAllSubscriptions()
        {
            var subscriptions = GetAllSubscriptions();

            foreach (var subscription in subscriptions)
            {
                if (!RemoveSubscription(subscription))
                {
                    return false;
                }
            }
            return true;
        }

        public bool RemoveSubscription(string key)
        {
            if (Subscriptions.TryGetValue(key, out var subscription))
            {
                try
                {
                    subscription.Dispose();
                    return true;
                }
                catch { }
            }
            return false;
        }



        public IRediSubscription<T>? ToObject<T>(string key, string hash) where T : notnull
        {
            return ToObject<T>(key + ":" + hash);
        }

        public IRediSubscription<T>? ToObject<T>(string key) where T : notnull
        {
            if (db != null)
            {
                if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(T), out var discriptor))
                {
                    if (!string.IsNullOrEmpty(discriptor.KeySpace))
                    {
                        if (Subscriber != null)
                        {
                            var appendToKey = "";

                            if (discriptor.AppendToKey != null)
                            {
                                for (int i = 0; i < discriptor.AppendToKey.Count; i++)
                                {
                                    var last = key.ToString().Split(":").LastOrDefault();
                                    var keyToAppend = discriptor.AppendToKey[i].ToLower();
                                    if (last is not null && last.ToLower() != keyToAppend && key != keyToAppend)
                                    {
                                        appendToKey += ":" + keyToAppend;
                                    }
                                }
                            }

                            var subscriptionID = $"__keyspace@{db.Database}__:" + discriptor.KeySpace + ":" + key + appendToKey;
                            if (Subscriptions.ContainsKey(subscriptionID))
                            {
                                if (Subscriptions[subscriptionID] is RediObjectSubscription<T> value)
                                {
                                    return value;
                                }
                            }

                            var rediSubbed = new RediObjectSubscription<T>(Reader, discriptor.KeySpace, key, subscriptionID, new RediSubscriberOptions());
                            var createResult = rediSubbed.InvokeReloadObject();

                            ActiveRedisConnections.Add(subscriptionID);
                            _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
                            {
                                rediSubbed.InvokeReloadObject();
                            });
                            Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);

                            return rediSubbed;
                        }
                    }
                }
            }
            return default;
        }



        //public IRediSubscription<T>? ToList<T>(string key) where T : notnull
        //{
        //    if (db != null)
        //    {
        //        if (Subscriber != null)
        //        {
        //            var result = new RediListSubscription<T>();

        //            var subList = new List<RediObjectSubscription<T>>();

        //            var singleized = Pluralizer.Singularize(key);

        //            if (!RediWriter.IsPrimitive(typeof(T)))
        //            {
        //                var hashes = db.SetMembers(key);

        //                if (hashes.Length > 0)
        //                {
        //                    foreach (var hash in hashes)
        //                    {
        //                        if (Subscriptions.ContainsKey(singleized + ":" + hash))
        //                        {
        //                            if (Subscriptions[singleized + ":" + hash] is RediObjectSubscription<T> value)
        //                            {
        //                                subList.Add(value);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var subscriptionID = $"__keyspace@{db.Database}__:" + singleized + ":" + hash;

        //                            var rediSubbed = new RediObjectSubscription<T>(Reader, singleized, hash, subscriptionID, new RediSubscriberOptions());

        //                            _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
        //                              {
        //                                  rediSubbed.InvokeReloadObject();
        //                              });

        //                            subList.Add(rediSubbed);
        //                            Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);
        //                        }
        //                    }

        //                    subList.ForEach(x =>
        //                    {
        //                        x.OnChange += (s) =>
        //                        {
        //                            result.InvokeOnChanged(s);
        //                        };
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                var subscriptionID = $"__keyspace@{db.Database}__:" + singleized;

        //                _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
        //                {
        //                    result.InvokeOnRemoved();
        //                });
        //            }
        //            return result;
        //        }
        //    }

        //    return null;
        //}

        //public IRediSubscription<T>? ToList<T>(string key, params string[] hashes) where T : notnull
        //{
        //    if (db != null)
        //    {
        //        if (Subscriber != null)
        //        {
        //            var result = new RediListSubscription<T>();

        //            var subList = new List<RediObjectSubscription<T>>();

        //            if (hashes.Length > 0)
        //            {
        //                var singleized = Pluralizer.Singularize(key);

        //                foreach (var hash in hashes)
        //                {
        //                    if (Subscriptions.ContainsKey(singleized + ":" + hash))
        //                    {
        //                        if (Subscriptions[singleized + ":" + hash] is RediObjectSubscription<T> value)
        //                        {
        //                            subList.Add(value);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var subscriptionID = $"__keyspace@{db.Database}__:" + singleized + ":" + hash;

        //                        var rediSubbed = new RediObjectSubscription<T>(Reader, singleized, hash, subscriptionID, new RediSubscriberOptions());

        //                        _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
        //                          {
        //                              rediSubbed.InvokeReloadObject();
        //                          });

        //                        subList.Add(rediSubbed);

        //                        Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);
        //                    }
        //                }

        //                subList.ForEach(x =>
        //                {
        //                    x.OnChange += (s) =>
        //                    {
        //                        result.InvokeOnChanged(s);
        //                    };
        //                });

        //                return result;
        //            }
        //        }
        //    }

        //    return null;
        //}



        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(Action<RediSubscriberOptions>? options = default) where TKey : IConvertible where TValue : notnull
        {
            var setOptions = new RediSubscriberOptions();

            if (options is not null)
            {
                options(setOptions);
                return ToDictionary<TKey, TValue>(setOptions);
            }
            else
            {
                return ToDictionary<TKey, TValue>(setOptions);
            }
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull
        {
            if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(TValue), out var proccessor))
            {
                if (proccessor.DefaultSet is not null && proccessor.KeySpace is not null)
                {
                    return ToDictionary<TKey, TValue>(proccessor.KeySpace, proccessor.DefaultSet, options);
                }
            }
            return null;
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string set, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull
        {
            return ToDictionary<TKey, TValue>(set, set, options);
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string set, Action<RediSubscriberOptions>? options = default) where TKey : IConvertible where TValue : notnull
        {
            var setOptions = new RediSubscriberOptions();
            if (options is not null)
                options(setOptions);

            return ToDictionary<TKey, TValue>(set, set, setOptions);
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string keySpace, string set, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull
        {
            if (db != null)
            {
                bool noOptions = false;
                if (options is null)
                {
                    options = new();
                    noOptions = true;
                }

                if (Subscriber != null)
                {
                    var setSubscriptionID = $"__keyspace@{db.Database}__:" + set;

                    if (Subscriptions.TryGetValue(setSubscriptionID, out var subValue) && subValue is IRediSubscriptions<TKey, TValue> sub)
                    {
                        return sub;
                    }


                    var result = new RediDictionarySubscription<TKey, TValue>(setSubscriptionID);

                    var subList = new List<RediObjectSubscription<TValue>>();

                    RedisValue[]? keys;

                    try
                    {
                        keys = db.SetMembers(set);
                    }
                    catch
                    {
                        keys = db.SortedSetRangeByScore(set);
                    }

                    var singleized = Pluralizer.Singularize(keySpace);

                    if (keys.Length > 0)
                    {
                        if (noOptions && keys.Length > 250)
                        {
                            options.SubscribeToSetMembers = false;
                        }

                        for (int i = 0; i < keys.Length; i++)
                        {
                            result.SetKeys.Add(keys[i].ToString());
                        }

                        if (options.SubscribeToSetMembers)
                        {
                            if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(TValue), out var discriptor))
                            {
                                if (!string.IsNullOrEmpty(discriptor.KeySpace))
                                {
                                    foreach (var key in keys.Take(options.MaxMembers))
                                    {

                                        var appendToKey = "";

                                        if (discriptor.AppendToKey != null)
                                        {
                                            for (int i = 0; i < discriptor.AppendToKey.Count; i++)
                                            {
                                                var last = key.ToString().Split(":").LastOrDefault();
                                                var keyToAppend = discriptor.AppendToKey[i].ToLower();
                                                if (last is not null && last.ToLower() != keyToAppend && key != keyToAppend)
                                                {
                                                    appendToKey += ":" + keyToAppend;
                                                }
                                            }
                                        }

                                        var subscriptionID = $"__keyspace@{db.Database}__:" + discriptor.KeySpace + ":" + key + appendToKey;

                                        if (Subscriptions.ContainsKey(subscriptionID))
                                        {
                                            if (Subscriptions[subscriptionID] is RediObjectSubscription<TValue> value)
                                            {
                                                subList.Add(value);
                                            }
                                        }
                                        else
                                        {
                                            var rediSubbed = new RediObjectSubscription<TValue>(Reader, discriptor.KeySpace, key, subscriptionID, options);

                                            ActiveRedisConnections.Add(subscriptionID);
                                            _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
                                              {
                                                  rediSubbed.InvokeReloadObject();
                                              });

                                            Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);

                                            subList.Add(rediSubbed);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    result.Subscriptions = new Dictionary<TKey, IRediSubscription<TValue>>();

                    if (options.WatchForRemove || options.WatchForAdd)
                    {
                        if (Subscriptions.TryGetValue(setSubscriptionID, out var existingSub))
                        {
                            if (existingSub is RediDictionarySubscription<TKey, TValue> existing)
                            {
                                if (options.WatchForRemove)
                                    existing.OnRemoved += (s) => result.InvokeOnRemoved(s);
                                if (options.WatchForAdd)
                                    existing.OnValueUpdate += (s, e) => result.InvokeOnAdded(s, e);
                            }
                        }
                        else
                        {
                            ActiveRedisConnections.Add(setSubscriptionID);
                            Subscriber.Subscribe(setSubscriptionID, (s, e) =>
                            {
                                try
                                {
                                    RedisValue[]? keys;
                                    try
                                    {
                                        keys = db.SetMembers(set);
                                    }
                                    catch
                                    {
                                        keys = db.SortedSetRangeByScore(set);
                                    }

                                    if (keys != null)
                                    {
                                        var listOfKeys = keys.Select(x => x.ToString()).ToList();

                                        var needsAdded = listOfKeys.Where(x => !result.SetKeys.Contains(x)).ToList();
                                        var removed = result.SetKeys.Where(x => x != null && !listOfKeys.Contains(x)).ToList();

                                        if (removed.Any())
                                        {
                                            foreach (var key in removed.ToList())
                                            {
                                                var subscription = result.Subscriptions.FirstOrDefault(x => x.Key.ToString() == key);
                                                if (subscription.Value != null)
                                                {
                                                    result.InvokeOnRemoved(subscription.Key);
                                                    result.Subscriptions.Remove(subscription.Key);
                                                    try
                                                    {
                                                        result.SetKeys.Remove(key);
                                                    }
                                                    catch { }
                                                    subscription.Value.Dispose();
                                                }
                                            }
                                        }

                                        if (needsAdded.Any())
                                        {
                                            foreach (var key in needsAdded.ToList())
                                            {
                                                result.SetKeys.Add(key);

                                                var keyConvert = (TKey)Convert.ChangeType(key, typeof(TKey));

                                                var obj = Reader.Object<TValue>(key);
                                                if (obj != null)
                                                {
                                                    result.InvokeOnAdded(keyConvert, obj);
                                                }
                                                else
                                                {
                                                    result.InvokeOnAdded(keyConvert, default);
                                                }

                                                var subscriptionID = $"__keyspace@{db.Database}__:" + singleized + ":" + key;

                                                RediObjectSubscription<TValue>? rediSubbed = null;

                                                if (Subscriptions.ContainsKey(subscriptionID) && Subscriptions[subscriptionID] is RediObjectSubscription<TValue> value)
                                                {
                                                    rediSubbed = value;
                                                }
                                                else
                                                {
                                                    rediSubbed = new RediObjectSubscription<TValue>(Reader, singleized, key, subscriptionID, options);
                                                }

                                                if (options.SubscribeToSetMembers)
                                                {
                                                    if (rediSubbed != null)
                                                    {
                                                        ActiveRedisConnections.Add(subscriptionID);
                                                        _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
                                                        {
                                                            rediSubbed.InvokeReloadObject();
                                                        });

                                                        rediSubbed.OnChange += (v) =>
                                                        {
                                                            result.InvokeOnChanged(keyConvert, v);
                                                        };


                                                        if (!Subscriptions.ContainsKey(rediSubbed.SubscriptionID))
                                                        {
                                                            Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);
                                                        }
                                                    }
                                                }
                                                if (rediSubbed is not null && !result.Subscriptions.ContainsKey(keyConvert))
                                                {
                                                    result.Subscriptions.Add(keyConvert, rediSubbed);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ea)
                                {
                                    Console.WriteLine(ea);
                                }
                            });

                            Subscriptions.Add(result.SubscriptionID, result);
                        }
                    }

                    if (subList.Count > 0)
                    {
                        subList.ForEach(x =>
                        {
                            var objKey = (TKey)Convert.ChangeType(x.Key, typeof(TKey));
                            result.Subscriptions.Add(objKey, x);
                            x.OnChange += (v) =>
                            {
                                result.InvokeOnChanged(objKey, v);
                            };
                        });
                    }

                    return result;
                }
            }

            return null;
        }

        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(Expression<Func<TValue, P>> property, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull
        {
            if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(TValue), out var proccessor))
            {
                if (proccessor.DefaultSet is not null && proccessor.KeySpace is not null)
                {
                    return ToDictionary<TKey, TValue, P>(proccessor.KeySpace, property, proccessor.DefaultSet, options);
                }
            }
            return null;
        }

        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(string keySpace, Expression<Func<TValue, P>> property, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull
        {
            if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(TValue), out var proccessor))
            {
                if (proccessor.KeySpace is not null)
                {
                    return ToDictionary<TKey, TValue, P>(proccessor.KeySpace, property, keySpace, options);
                }
            }
            return null;
        }

        public IRediSubscriptions<TKey, P>? ToDictionary<TKey, TValue, P>(string keySpace, Expression<Func<TValue, P>> property, string set, RediSubscriberOptions? options = default) where TKey : IConvertible where TValue : notnull where P : notnull
        {
            if (db != null)
            {
                bool noOptions = false;
                if (options is null)
                {
                    options = new();
                    noOptions = true;
                }

                if (Subscriber != null)
                {
                    var setSubscriptionID = $"__keyspace@{db.Database}__:" + set;

                    var result = new RediDictionarySubscription<TKey, P>(setSubscriptionID);

                    var subList = new List<IRediSubscription<P>>();

                    RedisValue[]? keys;

                    try
                    {
                        keys = db.SetMembers(set);
                    }
                    catch
                    {
                        keys = db.SortedSetRangeByScore(set);
                    }

                    var singleized = Pluralizer.Singularize(keySpace);

                    if (keys.Length > 0)
                    {
                        if (noOptions && keys.Length > 250)
                        {
                            options.SubscribeToSetMembers = false;
                        }

                        for (int i = 0; i < keys.Length; i++)
                        {
                            result.SetKeys.Add(keys[i].ToString());
                        }

                        if (options.SubscribeToSetMembers)
                        {
                            foreach (var key in keys.Take(options.MaxMembers))
                            {
                                var keyConvert = (TKey)Convert.ChangeType(key, typeof(TKey));
                                var rediSubbed = ToProperty(key, property, options);
                                if (rediSubbed is not null)
                                {
                                    subList.Add(rediSubbed);
                                }
                            }
                        }
                    }
                    result.Subscriptions = new Dictionary<TKey, IRediSubscription<P>>();

                    ActiveRedisConnections.Add(setSubscriptionID);
                    Subscriber.Subscribe(setSubscriptionID, (s, e) =>
                    {
                        try
                        {
                            RedisValue[]? keys;
                            try
                            {
                                keys = db.SetMembers(set);
                            }
                            catch
                            {
                                keys = db.SortedSetRangeByScore(set);
                            }

                            if (keys != null)
                            {
                                var listOfKeys = keys.Select(x => x.ToString()).ToList();

                                var needsAdded = listOfKeys.Where(x => !result.SetKeys.Contains(x)).ToList();
                                var removed = result.SetKeys.Where(x => x != null && !listOfKeys.Contains(x)).ToList();

                                if (removed.Any())
                                {
                                    foreach (var key in removed.ToList())
                                    {
                                        var subscription = result.Subscriptions.FirstOrDefault(x => x.Key.ToString() == key);
                                        if (subscription.Value != null)
                                        {
                                            result.InvokeOnRemoved(subscription.Key);
                                            result.Subscriptions.Remove(subscription.Key);
                                            try
                                            {
                                                result.SetKeys.Remove(key);
                                            }
                                            catch { }
                                            subscription.Value.Dispose();
                                        }
                                    }
                                }

                                if (needsAdded.Any())
                                {
                                    foreach (var key in needsAdded.ToList())
                                    {
                                        result.SetKeys.Add(key);

                                        var keyConvert = (TKey)Convert.ChangeType(key, typeof(TKey));

                                        var obj = Reader.Property(key, property);
                                        if (obj != null)
                                        {
                                            result.InvokeOnAdded(keyConvert, obj);
                                        }
                                        else
                                        {
                                            result.InvokeOnAdded(keyConvert, default);
                                        }

                                        if (options.SubscribeToSetMembers)
                                        {
                                            var newSub = !Subscriptions.ContainsKey(setSubscriptionID);
                                            var rediSubbed = ToProperty(key, property, options);
                                            if (rediSubbed is not null)
                                            {
                                                var objKey = (TKey)Convert.ChangeType(key, typeof(TKey));

                                                //if (!newSub)
                                                //{
                                                rediSubbed.OnChange += (value) =>
                                                {
                                                    result.InvokeOnChanged(objKey, value);
                                                };
                                                //}

                                                result.Subscriptions.Add(objKey, rediSubbed);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ea)
                        {
                            Console.WriteLine(ea);
                        }
                    });

                    if (subList.Count > 0)
                    {
                        subList.ForEach(x =>
                        {
                            var objKey = (TKey)Convert.ChangeType(x.Key, typeof(TKey));
                            result.Subscriptions.Add(objKey, x);
                            x.OnChange += (v) =>
                            {
                                result.InvokeOnChanged(objKey, v);
                            };
                        });
                    }

                    return result;
                }
            }

            return null;
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, params string[] hashes) where TKey : IConvertible where TValue : notnull
        {
            return ToDictionary<TKey, TValue>(key, new RediSubscriberOptions(), hashes);
        }

        public IRediSubscriptions<TKey, TValue>? ToDictionary<TKey, TValue>(string key, RediSubscriberOptions options, params string[] hashes) where TKey : IConvertible where TValue : notnull
        {
            if (db != null)
            {
                if (Subscriber != null)
                {
                    var result = new RediDictionarySubscription<TKey, TValue>("");

                    var subList = new List<RediObjectSubscription<TValue>>();

                    if (hashes.Length > 0)
                    {
                        var singulized = Pluralizer.Singularize(key);

                        foreach (var item in hashes)
                        {
                            result.SetKeys.Add(item);
                            var subscriptionID = $"__keyspace@{db.Database}__:" + singulized + ":" + item;

                            if (Subscriptions.ContainsKey(subscriptionID))
                            {
                                if (Subscriptions[subscriptionID] is RediObjectSubscription<TValue> value)
                                {
                                    subList.Add(value);
                                }
                            }
                            else
                            {
                                var rediSubbed = new RediObjectSubscription<TValue>(Reader, singulized, item, subscriptionID, options);

                                ActiveRedisConnections.Add(subscriptionID);
                                _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
                                {
                                    rediSubbed.InvokeReloadObject();
                                });

                                subList.Add(rediSubbed);
                                Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);
                            }
                        }

                        subList.ForEach(x =>
                        {
                            var objKey = (TKey)Convert.ChangeType(x.Key, typeof(TKey));
                            result.Subscriptions.Add(objKey, x);
                            x.OnChange += (v) =>
                            {
                                result.InvokeOnChanged(objKey, v);
                            };
                        });

                        return result;
                    }
                }
            }
            return null;
        }



        public IRediSubscriptions<string, string>? ToMessages(string key)
        {
            if (db is not null)
            {
                var ids = db.SetMembers(key);
                if (ids is not null)
                {
                    return ToMessages(ids.ToStringArray());
                }
            }
            return null;
        }
        public IRediSubscriptions<string, string>? ToMessages(string name, params string[] names)
        {
            return ToMessages(name, names);
        }
        private IRediSubscriptions<string, string>? ToMessages(params string[] names)
        {
            if (db != null)
            {

                if (Subscriber != null)
                {
                    var result = new RediDictionarySubscription<string, string>("")
                    {
                        IsMessages = true
                    };

                    var subList = new List<RediObjectSubscription<string>>();

                    if (names.Length > 0)
                    {
                        var singulized = "status-message";

                        foreach (var item in names)
                        {
                            result.SetKeys.Add(item);
                            var subscriptionID = $"__keyspace@{db.Database}__:" + singulized + ":" + item;

                            if (Subscriptions.ContainsKey(subscriptionID))
                            {
                                if (Subscriptions[subscriptionID] is RediObjectSubscription<string> value)
                                {
                                    subList.Add(value);
                                }
                            }
                            else
                            {
                                var rediSubbed = new RediObjectSubscription<string>(Reader, singulized, item, subscriptionID, new RediSubscriberOptions() )
                                {
                                    IsMessage = true
                                };

                                ActiveRedisConnections.Add(subscriptionID);
                                _ = Subscriber.SubscribeAsync(subscriptionID, (s, e) =>
                                {
                                    rediSubbed.InvokeReloadObject();
                                });

                                subList.Add(rediSubbed);
                                Subscriptions.Add(rediSubbed.SubscriptionID, rediSubbed);
                            }
                        }

                        subList.ForEach(x =>
                        {
                            var objKey = (string)Convert.ChangeType(x.Key, typeof(string));
                            result.Subscriptions.Add(objKey, x);
                            x.OnChange += (v) =>
                            {
                                result.InvokeOnChanged(objKey, v);
                            };
                        });

                        return result;
                    }
                }
            }
            return null;

        }



        public IRediSubscription<P>? ToProperty<T, P>(string key, Expression<Func<T, P>> property, RediSubscriberOptions options) where T : notnull where P : notnull
        {
            if (db != null && Extensions.TypeDescriptor.TryGetDescriptor(typeof(T), out var discriptor))
            {
                var test = property.Body.ToString();

                if (!string.IsNullOrEmpty(discriptor.KeySpace))
                {
                    if (test.Split(".").Length > 2)
                    {
                        throw new ArgumentException("Only root level properties are supported");
                    }

                    key = discriptor.KeySpace + ":" + key + test.Replace(test.Split(".").First(), "").Replace(".", ":").ToLower();

                    var propType = typeof(P);
                    var propName = key.Split(":").LastOrDefault();

                    if (propName != null && Subscriber != null)
                    {
                        if (RediWriter.IsPrimitive(propType))
                        {
                            var objKey = key.Replace(":" + propName, "").Split(":").LastOrDefault();
                            if (objKey is not null)
                            {
                                var subscriptionID = $"__keyspace@{db.Database}__:{discriptor.KeySpace}:{objKey}";

                                if (Subscriptions.TryGetValue(subscriptionID, out var sub))
                                {
                                    if (sub is IRediSubscription<T> subOut)
                                    {
                                        var rediSubbed = new RediValueSubscription<P>(Reader, discriptor.KeySpace, key, subscriptionID, property.Compile());
                                        subOut.OnChange += (value) =>
                                        {
                                            rediSubbed.InvokeReloadObject(value);
                                        };
                                        return rediSubbed;
                                    }
                                }
                                else
                                {
                                    var rediObjSubbed = new RediObjectSubscription<T>(Reader, discriptor.KeySpace, objKey, subscriptionID, options);
                                    var rediSubbed = new RediValueSubscription<P>(Reader, discriptor.KeySpace, objKey, subscriptionID, property.Compile());

                                    ActiveRedisConnections.Add(subscriptionID);
                                    Subscriber.Subscribe(subscriptionID, (s, e) =>
                                    {
                                        rediObjSubbed.InvokeReloadObject();
                                    });

                                    rediObjSubbed.OnChange += (value) =>
                                    {
                                        rediSubbed.InvokeReloadObject(value);
                                    };

                                    Subscriptions.Add(rediObjSubbed.SubscriptionID, rediObjSubbed);
                                    return rediSubbed;
                                }
                            }
                        }
                        else
                        {
                            return ToObject<P>(key.Replace(":" + propName, ""), propName);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("To use the read method without supplying a keyspace you must set the 'RediKeySpace('')' Attribute on the object class");
                }
            }
            return default;
        }
    }
}
