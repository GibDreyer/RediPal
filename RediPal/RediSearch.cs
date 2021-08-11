using Pluralize.NET;
using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore
{
    public class RediSearch : IRediSearch
    {
        private readonly IDatabase? db;
        internal ITypeDiscriptor Descriptor { get; }
        private readonly IRediReader reader;

        // Constructor
        internal RediSearch(IDatabase? db, ITypeDiscriptor descriptor, IRediReader reader)
        {
            this.db = db;
            Descriptor = descriptor;
            this.reader = reader;
        }

        public Dictionary<TKey, TValue>? AsDictionary<TKey, TValue>(string fromSet, DateTime start, DateTime? end = null) where TKey : IConvertible where TValue : notnull
        {
            if (db != null)
            {
                var type = typeof(TValue);

                if (Redipal.IFactory != null && Redipal.IFactory.TypeDescriptor.TryGetDescriptor(type, out var proccessor))
                {
                    if (!string.IsNullOrEmpty(fromSet) && !string.IsNullOrEmpty(proccessor.KeySpace))
                    {
                        var startTime = new DateTimeOffset(start).ToUnixTimeSeconds();

                        RedisValue[]? keys;
                        if (!end.HasValue)
                        {
                            keys = db.SortedSetRangeByScore(fromSet, startTime);
                        }
                        else
                        {
                            var endTime = new DateTimeOffset(end.Value).ToLocalTime().ToUnixTimeSeconds();
                            keys = db.SortedSetRangeByScore(fromSet, startTime, endTime);
                        }

                        if (keys != null && keys.Length > 0)
                        {
                            var stringKeys = new string[keys.Length];

                            for (int i = 0; i < keys.Length; i++)
                            {
                                stringKeys[i] = keys[i].ToString();
                            }
                            return reader.Dictionary<TKey, TValue>(fromSet, stringKeys);
                        }
                    }
                }
            }
            return null;
        }

        public Dictionary<TKey, TValue>? AsDictionary<TKey, TValue>(DateTime start, DateTime? end = null) where TKey : IConvertible where TValue : notnull
        {
            if (db != null)
            {
                var type = typeof(TValue);

                if (Redipal.IFactory != null && Redipal.IFactory.TypeDescriptor.TryGetDescriptor(type, out var proccessor))
                {
                    if (!string.IsNullOrEmpty(proccessor.DefaultSet) && !string.IsNullOrEmpty(proccessor.KeySpace))
                    {
                        var startTime = new DateTimeOffset(start).ToUnixTimeSeconds();

                        RedisValue[]? keys;
                        if (!end.HasValue)
                        {
                            keys = db.SortedSetRangeByScore(proccessor.DefaultSet, startTime);
                        }
                        else
                        {
                            var endTime = new DateTimeOffset(end.Value).ToLocalTime().ToUnixTimeSeconds();
                            keys = db.SortedSetRangeByScore(proccessor.DefaultSet, startTime, endTime);
                        }

                        if (keys != null && keys.Length > 0)
                        {
                            var stringKeys = new string[keys.Length];

                            for (int i = 0; i < keys.Length; i++)
                            {
                                stringKeys[i] = keys[i].ToString();
                            }

                            return reader.Dictionary<TKey, TValue>(stringKeys);
                            // return reader.Dictionary<TKey, TValue>(keyspace, stringKeys);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("No Default set could be determained. please previde one in the method call or set the RediDefaultSet Attribute on the object");
                    }
                }
            }
            return null;
        }
    }
}
