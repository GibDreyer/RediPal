using Pluralize.NET;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedipalCore
{
    internal class RediReader : IRediReader
    {
        // Fields
        private readonly IDatabase? db;
        private readonly Pluralizer pluralizer = new();
        internal ITypeDiscriptor Descriptor { get; }

        // Constructor
        internal RediReader(IDatabase? db, ITypeDiscriptor descriptor)
        {
            this.db = db;
            Descriptor = descriptor;
        }



        public long GetIncrementedID(string key)
        {
            if (db != null)
                return db.StringIncrement(key);
            else
                return 0;
        }

        public bool SetContains(string keySpace, string id)
        {
            if (db != null)
            {
                try
                {
                    return db.SetContains(keySpace, id);
                }
                catch
                {
                    var result = db.SortedSetRank(keySpace, id);

                    return result.HasValue;
                }
            }
            else
            {
                return false;
            }
        }



        // Interface Methods

        public T? Object<T>(string keySpace, string id) where T : notnull
        {
            return GetObject<T>(keySpace + ":" + id);
        }

        public T? Object<T>(string id) where T : notnull
        {
            if (Descriptor.TryGetDescriptor(typeof(T), out var discriptor))
            {
                if (!string.IsNullOrEmpty(discriptor.KeySpace))
                {
                    if (id.Contains(discriptor.KeySpace))
                    {
                        return GetObject<T>(id);
                    }
                    else
                    {
                        return GetObject<T>(discriptor.KeySpace + ":" + id);
                    }
                }
                else
                {
                    return GetObject<T>(id);
                }
            }
            return default;
        }

        private T? GetObject<T>(string keySpace) where T : notnull
        {
            var type = typeof(T);
            var instance = CreateInstance(type);

            if (instance != null && db != null && !string.IsNullOrEmpty(keySpace))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var x = db.StringGet(keySpace);
                        return JsonSerializer.Deserialize<T>((string)Convert.ChangeType(x, typeof(string)));
                    }
                    else
                    {
                        var last = keySpace.Split(":").LastOrDefault();
                        if (last is not null)
                        {
                            //var result = Get_AsObject(instance, keySpace.ToLower(), "");
                            var result = Get_AsObject(instance, keySpace.Substring(0, keySpace.Length - (last.Length + 1)).ToLower(), last.ToLower());
                            return (T?)result;
                        }
                        else
                        {
                            return default;
                        }
                    }
                }
            }
            var outInstance = Get_AsObject(instance, keySpace.ToLower(), keySpace.Split(":").LastOrDefault());
            return (T?)outInstance;
        }


        public T? Field<T>(string keySpace, string field) where T : notnull
        {
            if (db == null || string.IsNullOrEmpty(keySpace) || string.IsNullOrEmpty(field))
            {
                return default;
            }

            var a = db.HashGet(keySpace, field);
            if (!a.IsNull)
            {
                return (T)Convert.ChangeType(a, typeof(T));
            }
            else
            {
                return default;
            }
        }

        public T? Value<T>(string keySpace) where T : notnull
        {
            if (db == null || string.IsNullOrEmpty(keySpace))
            {
                return default;
            }

            RedisValue? x = null;

            try
            {
                var field = keySpace.Split(":").LastOrDefault();
                if (field is not null)
                {
                    x = db.HashGet(keySpace.Substring(0, keySpace.Length - (field.Length + 1)), field);
                }
            }
            catch
            {
                x = db.StringGet(keySpace);
            }

            if (x.HasValue)
            {
                return (T)Convert.ChangeType(x, typeof(T));
            }

            return default;
        }


        public List<T>? List<T>() where T : notnull
        {
            var type = typeof(T);
            if (db != null)
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (string.IsNullOrEmpty(typeProccessor.DefaultSet))
                    {
                        throw new ArgumentException("The Attribute 'RediSet' was not set on the object class and 'DefaultSet was not set on the TpyeDefaults'. The Default must be set or a set must be supplied as a method peremater");
                    }

                    var key = typeProccessor.DefaultSet;

                    var instance = new List<T>();

                    var isprimitive = IsPrimitive(type);

                    if (typeProccessor.AsJson || isprimitive)
                    {
                        RedisValue[]? x;
                        try
                        {
                            x = db.ListRange(key);
                        }
                        catch
                        {
                            x = db.SetMembers(key);
                        }

                        if (!isprimitive)
                        {
                            foreach (var item in x)
                            {
                                var obj = JsonSerializer.Deserialize<T>((string)Convert.ChangeType(item, typeof(string)));
                                if (obj != null)
                                {
                                    instance.Add(obj);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in x)
                            {
                                var redisString = ConvertFromRedisType(type, item);
                                if (redisString != null)
                                {
                                    var convert = (T)Convert.ChangeType(redisString, type);
                                    if (convert != null)
                                    {
                                        instance.Add(convert);
                                    }
                                }
                            }
                        }
                        return instance;
                    }
                    else
                    {
                        if (instance != null)
                        {
                            return (List<T>?)Get_AsList(instance, "", key.ToLower());
                        }
                    }
                }
            }
            return default;
        }

        public List<P>? List<T, P>(Expression<Func<T, P>> property) where T : notnull where P : notnull
        {
            var type = typeof(T);
            if (db != null)
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (string.IsNullOrEmpty(typeProccessor.DefaultSet))
                    {
                        throw new ArgumentException("The Attribute 'RediSet' was not set on the object class and 'DefaultSet was not set on the TpyeDefaults'. The Default must be set or a set must be supplied as a method peremater");
                    }
                    else
                    {
                        return List(typeProccessor.DefaultSet, property);
                    }
                }
            }
            return default;
        }


        public List<T>? List<T>(string key) where T : notnull
        {
            var type = typeof(T);
            if (db != null && !string.IsNullOrEmpty(key))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    var instance = new List<T>();

                    var isprimitive = IsPrimitive(type);

                    if (typeProccessor.AsJson || isprimitive)
                    {
                        RedisValue[]? x;
                        try
                        {
                            x = db.ListRange(key);
                        }
                        catch
                        {
                            x = db.SetMembers(key);
                        }

                        if (!isprimitive)
                        {
                            foreach (var item in x)
                            {
                                var obj = JsonSerializer.Deserialize<T>((string)Convert.ChangeType(item, typeof(string)));
                                if (obj != null)
                                {
                                    instance.Add(obj);
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in x)
                            {
                                var redisString = ConvertFromRedisType(type, item);
                                if (redisString != null)
                                {
                                    var convert = (T)Convert.ChangeType(redisString, type);
                                    if (convert != null)
                                    {
                                        instance.Add(convert);
                                    }
                                }
                            }
                        }
                        return instance;
                    }
                    else
                    {
                        if (instance != null)
                        {
                            return (List<T>?)Get_AsList(instance, "", key.ToLower());
                        }
                    }
                }
            }
            return default;
        }

        public List<P>? List<T, P>(string key, Expression<Func<T, P>> property) where T : notnull where P : notnull
        {
            var type = typeof(T);
            if (db != null && !string.IsNullOrEmpty(key))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    var instance = new ConcurrentBag<P>();

                    var isprimitive = IsPrimitive(type);

                    RedisValue[]? x;
                    try
                    {
                        x = db.ListRange(key);
                    }
                    catch
                    {
                        x = db.SetMembers(key);
                    }

                    if (typeProccessor.AsJson || isprimitive)
                    {
                        if (!isprimitive)
                        {
                            var func = property.Compile();

                            foreach (var item in x)
                            {
                                var obj = JsonSerializer.Deserialize<T>((string)Convert.ChangeType(item, typeof(string)));
                                if (obj != null)
                                {
                                    instance.Add(func(obj));
                                }
                            }
                        }
                        else
                        {
                            var func = property.Compile();

                            foreach (var item in x)
                            {
                                var redisString = ConvertFromRedisType(type, item);
                                if (redisString != null)
                                {
                                    var convert = (T)Convert.ChangeType(redisString, type);
                                    if (convert != null)
                                    {
                                        instance.Add(func(convert));
                                    }
                                }
                            }
                        }
                        return instance.ToList();
                    }
                    else
                    {
                        if (instance != null)
                        {
                            if (x.Length > 50)
                            {
                                Parallel.ForEach(x, id =>
                                {
                                    var prop = Property(id, property);
                                    if (prop is not null)
                                    {
                                        instance.Add(prop);
                                    }
                                });
                            }
                            else
                            {
                                foreach (var id in x)
                                {
                                    var prop = Property(id, property);
                                    if (prop is not null)
                                    {
                                        instance.Add(prop);
                                    }
                                }
                            }
                            return instance.ToList();
                        }
                    }
                }
            }
            return default;
        }

        public P? List<T, P>(Expression<Func<T, P>> property, Func<P, P, P> action) where T : notnull where P : IConvertible
        {
            var list = List(property);
            if (list is not null)
            {
                var instance = CreateInstance(typeof(P));
                if (instance is P result)
                {
                    foreach (var item in list)
                    {
                        result = action(result, item);
                    }
                    return result;
                }
            }
            return default;
        }

        public bool List<T, P>(Expression<Func<T, P>> property, Action<P, P> action) where T : notnull where P : IConvertible
        {
            var list = List(property);
            if (list is not null)
            {
                var instance = CreateInstance(typeof(P));
                if (instance is P result)
                {
                    foreach (var item in list)
                    {
                        action(result, item);
                    }
                    return true;
                }
            }
            return false;
        }

        public P? List<T, P>(string fromSet, Expression<Func<T, P>> property, Func<P, P, P> action) where T : notnull where P : IConvertible
        {
            var list = List(fromSet, property);
            if (list is not null)
            {
                var instance = CreateInstance(typeof(P));
                if (instance is P result)
                {
                    foreach (var item in list)
                    {
                        result = action(result, item);
                    }
                    return result;
                }
            }
            return default;
        }

        public bool List<T, P>(string fromSet, Expression<Func<T, P>> property, Action<P, P> action) where T : notnull where P : IConvertible
        {
            var list = List(fromSet, property);
            if (list is not null)
            {
                var instance = CreateInstance(typeof(P));
                if (instance is P result)
                {
                    foreach (var item in list)
                    {
                        action(result, item);
                    }
                    return true;
                }
            }
            return false;
        }

        public List<T>? List<T>(string keySpace, params string[] hashIDs) where T : notnull
        {
            var type = typeof(T);
            if (db != null && !string.IsNullOrEmpty(keySpace))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var x = db.StringGet(keySpace);
                        return JsonSerializer.Deserialize<List<T>>((string)Convert.ChangeType(x, typeof(string)));
                    }
                    else
                    {
                        var instance = new List<T>();
                        if (instance != null)
                        {
                            return (List<T>?)Get_AsList(instance, "", keySpace.ToLower(), hashIDs);
                        }
                    }
                }
            }
            return default;
        }

        public List<T>? List<T>(string keySpace, string fromSet) where T : notnull
        {
            var type = typeof(T);
            if (db != null && !string.IsNullOrEmpty(keySpace))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var x = db.StringGet(keySpace);
                        return JsonSerializer.Deserialize<List<T>>((string)Convert.ChangeType(x, typeof(string)));
                    }
                    else
                    {
                        var instance = new List<T>();
                        if (instance != null)
                        {
                            RedisValue[]? hashes;
                            try
                            {
                                hashes = db.SetMembers(fromSet);
                            }
                            catch
                            {
                                hashes = db.SortedSetRangeByScore(fromSet);
                            }


                            if (hashes.Length > 0)
                            {
                                var array = new string[hashes.Length];

                                for (int i = 0; i < hashes.Length; i++)
                                {
                                    array[i] = hashes[i];
                                }
                                return (List<T>?)Get_AsList(instance, keySpace, keySpace.ToLower(), array);
                            }
                        }
                    }
                }
            }
            return default;
        }



        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(RediReadOptions? readOptions = default) where TKey : IConvertible
        {
            var type = typeof(TValue);

            if (db != null)
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (string.IsNullOrEmpty(typeProccessor.DefaultSet))
                    {
                        throw new ArgumentException("The Attribute 'RediSet' was not set on the object class. The attribute must be set or a set must be supplied");
                    }

                    var set = typeProccessor.DefaultSet;

                    if (typeProccessor.AsJson)
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        var x = db.HashGetAll(set);
                        foreach (var hash in x)
                        {
                            var value = ConvertFromRedisType(typeof(TKey), hash.Name);
                            if (value != null)
                            {
                                var key = Convert.ChangeType(value, typeof(TKey));
                                if (key != null)
                                {
                                    var json = JsonSerializer.Deserialize<TValue>(hash.Value);
                                    if (json != null)
                                    {
                                        instance.Add((TKey)key, json);
                                    }
                                }
                            }
                        }
                        return instance;
                    }
                    else
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        if (instance != null)
                        {
                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", set.ToLower(), rediRead: readOptions);
                        }
                    }
                }
            }
            return null;
        }

        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string set, RediReadOptions? readOptions = default) where TKey : IConvertible
        {
            var type = typeof(TValue);

            if (db != null && !string.IsNullOrEmpty(set))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        var x = db.HashGetAll(set);
                        foreach (var hash in x)
                        {
                            var value = ConvertFromRedisType(typeof(TKey), hash.Name);
                            if (value != null)
                            {
                                var key = Convert.ChangeType(value, typeof(TKey));
                                if (key != null)
                                {
                                    var json = JsonSerializer.Deserialize<TValue>(hash.Value);
                                    if (json != null)
                                    {
                                        instance.Add((TKey)key, json);
                                    }
                                }
                            }
                        }
                        return instance;
                    }
                    else
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        if (instance != null)
                        {
                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", set.ToLower(), rediRead: readOptions);
                        }
                    }
                }
            }
            return null;
        }

        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string keySpace, string[] hashIDs, RediReadOptions? readOptions = default) where TKey : IConvertible
        {
            var type = typeof(TValue);
            if (db != null && !string.IsNullOrEmpty(keySpace))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var x = db.StringGet(keySpace);
                        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>((string)Convert.ChangeType(x, typeof(string)));
                    }
                    else
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        if (instance != null)
                        {
                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", keySpace.ToLower(), hashIDs, rediRead: readOptions);
                        }
                    }
                }
            }
            return default;
        }

        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string[] hashIDs, RediReadOptions? readOptions = default) where TKey : IConvertible
        {
            var instance = new Dictionary<TKey, TValue>();
            if (instance != null)
            {
                return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", "", hashIDs, rediRead: readOptions);
            }
            return default;
        }

        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string keySpace, string fromSet, RediReadOptions? readOptions = default) where TKey : IConvertible
        {
            var type = typeof(TValue);
            if (db != null && !string.IsNullOrEmpty(keySpace))
            {
                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
                {
                    if (typeProccessor.AsJson)
                    {
                        var x = db.StringGet(keySpace);
                        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>((string)Convert.ChangeType(x, typeof(string)));
                    }
                    else
                    {
                        var instance = new Dictionary<TKey, TValue>();
                        if (instance != null)
                        {
                            //RedisValue[]? hashes;
                            //try
                            //{
                            //    hashes = db.SetMembers(fromSet);
                            //}
                            //catch
                            //{
                            //    hashes = db.ListRange(fromSet);
                            //}

                            //if (hashes.Length > 0)
                            //{
                            //    var array = new string[hashes.Length];

                            //    for (int i = 0; i < hashes.Length; i++)
                            //    {
                            //        array[i] = hashes[i];
                            //    }

                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, keySpace.ToLower(), fromSet, rediRead: readOptions);
                            //}
                        }
                    }
                }
            }
            return default;
        }


        public P? Property<T, P>(string key, Expression<Func<T, P>> property) where T : notnull where P : notnull
        {
            if (db != null && Descriptor.TryGetDescriptor(typeof(T), out var discriptor))
            {
                if (!string.IsNullOrEmpty(discriptor.KeySpace))
                {
                    var test = property.Body.ToString();
                    key += test.Replace(test.Split(".").First(), "").Replace(".", ":").ToLower();

                    var propType = typeof(P);

                    if (IsPrimitive(propType))
                    {
                        var propName = key.Split(":").LastOrDefault();
                        if (propName != null)
                        {
                            var a = db.HashGet(discriptor.KeySpace + ":" + key.Replace(":" + propName, ""), propName);
                            if (!a.IsNull)
                            {
                                var redisString = ConvertFromRedisType(propType, a);
                                if (redisString != null)
                                {
                                    return (P)Convert.ChangeType(redisString, typeof(P));
                                }
                            }
                        }
                    }
                    else
                    {
                        var instance = CreateInstance(propType);

                        var result = Get_Instance(instance, discriptor.KeySpace + ":" + key);
                        if (result is not null)
                        {
                            return (P)result;
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




        // Interal Methods

        private object? Get_Instance(object? instance, string keySpace, params string[] hashIDs)
        {
            if (instance is IList)
            {
                instance = Get_AsList(instance, "", keySpace, hashIDs);
            }
            else if (instance is IDictionary)
            {
                instance = Get_AsDictionary(instance, "", keySpace, hashIDs);
            }
            else
            {
                if (hashIDs.Length == 1)
                {
                    instance = Get_AsObject(instance, keySpace, hashIDs[0]);
                }
                else
                {
                    var last = keySpace.Split(":").LastOrDefault();
                    if (last is not null)
                    {
                        instance = Get_AsObject(instance, keySpace.Substring(0, keySpace.Length - (last.Length + 1)), last);
                    }
                }
            }

            return instance;
        }


        private object? Get_AsDictionary(object? instance, string keySpace, string fromSet, string[]? hashIDs = default, RediReadOptions? rediRead = default)
        {
            if (db != null && instance != null)
            {
                fromSet = fromSet.ToLower();

                if (instance is IDictionary dictionary)
                {
                    var type = dictionary.GetType();
                    var keyType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
                    var valueType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[1];

                    var totalElements = dictionary.Count;
                    var totalRead = 0;

                    var dicts = new ConcurrentDictionary<object, object?>();

                    if (valueType != null && keyType != null)
                    {
                        var isPrimitive = IsPrimitive(valueType);

                        var singleSpace = pluralizer.Singularize(fromSet);
                        if (Descriptor.TryGetDescriptor(valueType, out var typeProccessor))
                        {
                            if (hashIDs is null || hashIDs.Length == 0)
                            {
                                if (isPrimitive)
                                {
                                    var x = db.HashGetAll(fromSet);
                                    if (x.Length == 0)
                                    {
                                        return null;
                                    }

                                    for (int i = 0; i < x.Length; i++)
                                    {
                                        var elm = x[i];

                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, elm.Name), keyType);
                                        var value = Convert.ChangeType(ConvertFromRedisType(valueType, elm.Value), valueType);
                                        if (key != null && value != null)
                                        {
                                            dicts.TryAdd(key, value);
                                        }
                                    }
                                }
                                else
                                {
                                    if (typeProccessor.AsJson)
                                    {
                                        var x = db.HashGetAll(fromSet);

                                        foreach (var entry in x)
                                        {
                                            var key = Convert.ChangeType(ConvertFromRedisType(keyType, entry.Name), keyType);
                                            var value = JsonSerializer.Deserialize(entry.Value, valueType);
                                            if (key != null && value != null)
                                            {
                                                dicts.TryAdd(key, value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        RedisValue[] x;
                                        try
                                        {
                                            x = db.SetMembers(fromSet);
                                        }
                                        catch
                                        {
                                            x = db.SortedSetRangeByScore(fromSet);
                                        }

                                        if (x.Length == 0)
                                        {
                                            return null;
                                        }

                                        if (x.Length > 3) // this one
                                        {
                                            var array = new string[x.Length];

                                            for (int i = 0; i < x.Length; i++)
                                            {
                                                array[i] = x[i].ToString();
                                            }

                                            Parallel.ForEach(array, SetID);
                                        }
                                        else
                                        {
                                            foreach (var id in x)
                                            {
                                                SetID(id);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (isPrimitive)
                                {
                                    foreach (var id in hashIDs)
                                    {
                                        var a = db.StringGet(singleSpace + ":" + id.ToLower());
                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);
                                        var value = Convert.ChangeType(ConvertFromRedisType(keyType, a), valueType);
                                        if (key != null && value != null)
                                        {
                                            dicts.TryAdd(key, value);
                                        }
                                    }
                                }
                                else
                                {
                                    if (hashIDs.Length > 3)
                                    {
                                        Parallel.ForEach(hashIDs, id =>
                                        {
                                            SetID(id.ToLower());
                                        });
                                    }
                                    else
                                    {
                                        foreach (var id in hashIDs)
                                        {
                                            SetID(id.ToLower());
                                        }
                                    }
                                }
                            }

                            void SetID(string id)
                            {
                                if (rediRead is not null)
                                {
                                    if (rediRead.Progress is not null)
                                    {
                                        rediRead.Progress.Report(totalRead++);
                                    }
                                }

                                if (typeProccessor.AsJson)
                                {
                                    var a = db.StringGet(singleSpace + ":" + id);

                                    var json = JsonSerializer.Deserialize(a, valueType);
                                    var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);

                                    if (json != null && key != null)
                                    {
                                        var valueString = key;
                                        if (valueString != null)
                                        {
                                            dicts.TryAdd(valueString, json);
                                        }
                                    }
                                }
                                else
                                {
                                    var newInstance = CreateInstance(valueType);
                                    if (newInstance != null)
                                    {
                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);
                                        if (key != null)
                                        {
                                            if (!string.IsNullOrEmpty(typeProccessor.KeySpace) && string.IsNullOrEmpty(keySpace))
                                            {
                                                var result = Get_Instance(newInstance, typeProccessor.KeySpace.ToLower(), id);
                                                if (result is not null)
                                                {
                                                    dicts.TryAdd(key, result);
                                                }
                                                else
                                                {
                                                    RemoveFromSet(fromSet, id);
                                                }
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(keySpace))
                                                {
                                                    var result = Get_Instance(newInstance, singleSpace, id);
                                                    if (result is not null)
                                                    {
                                                        dicts.TryAdd(key, result);
                                                    }
                                                    else
                                                    {
                                                        RemoveFromSet(fromSet, id);
                                                    }
                                                }
                                                else
                                                {
                                                    var result = Get_Instance(newInstance, keySpace, id);
                                                    if (result is not null)
                                                    {
                                                        dicts.TryAdd(key, result);
                                                    }
                                                    else
                                                    {
                                                        RemoveFromSet(fromSet, id);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        foreach (var item in dicts)
                        {
                            dictionary.Add(item.Key, item.Value);
                        }
                        return dictionary;
                    }
                }
            }

            void RemoveFromSet(string fromSet, string id)
            {
                if (rediRead is not null && rediRead.DeleteUnfound)
                {
                    if (db is not null)
                    {
                        try
                        {
                            db.SetRemove(fromSet, id);
                        }
                        catch
                        {
                            db.SortedSetRemove(fromSet, id);
                        }
                    }
                }
            }
            return null;
        }

        private object? Get_AsList(object? instance, string keySpace, string fromSet, params string[] hashIDs)
        {
            if (db != null && instance != null)
            {
                fromSet = fromSet.ToLower();

                if (instance is IList list)
                {
                    var type = list.GetType();
                    var listType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

                    if (listType != null)
                    {
                        var isPrimitive = IsPrimitive(listType);

                        var singleSpace = pluralizer.Singularize(fromSet);
                        if (Descriptor.TryGetDescriptor(listType, out var typeProccessor))
                        {
                            var conList = new ConcurrentBag<object?>();

                            if (hashIDs.Length == 0)
                            {
                                var setKey = pluralizer.Pluralize(fromSet);

                                if (isPrimitive)
                                {
                                    var x = new RedisValue[1];

                                    try
                                    {
                                        x = db.SetMembers(setKey);
                                    }
                                    catch
                                    {
                                        x = db.ListRange(setKey);
                                    }

                                    if (x.Length == 0)
                                    {
                                        return null;
                                    }
                                    if (list.GetType().IsArray)
                                    {
                                        if (list is Array array && x is Array xArray)
                                        {
                                            if (CreateInstance(list.GetType(), xArray.Length) is Array newArraySize)
                                            {
                                                array = newArraySize;
                                                for (int i = 0; i < xArray.Length; i++)
                                                {
                                                    if (xArray.Length <= i || array.Length <= i)
                                                    {
                                                        break;
                                                    }

                                                    var stringValue = ConvertFromRedisType(listType, xArray.GetValue(i));
                                                    var value = Convert.ChangeType(stringValue, listType);
                                                    if (stringValue != null && value != null)
                                                    {
                                                        array.SetValue(value, i);
                                                    }
                                                }

                                                list = array;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var id in x)
                                        {
                                            var value = Convert.ChangeType(ConvertFromRedisType(listType, id), listType);
                                            if (value != null)
                                            {
                                                conList.Add(value);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!typeProccessor.AsJson)
                                    {
                                        var x = new RedisValue[1];

                                        try
                                        {
                                            x = db.SetMembers(setKey);
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                x = db.ListRange(setKey);
                                            }
                                            catch
                                            {
                                                x = db.SortedSetRangeByScore(setKey);
                                            }
                                        }


                                        if (x.Length == 0)
                                        {
                                            return null;
                                        }
                                        if (x.Length > 3)
                                        {
                                            Parallel.ForEach(x, id =>
                                            {
                                                SetID(id);
                                            });
                                        }
                                        else
                                        {
                                            foreach (var id in x)
                                            {
                                                SetID(id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var x = db.ListRange(setKey);
                                        if (x.Length == 0)
                                        {
                                            return null;
                                        }
                                        foreach (var id in x)
                                        {
                                            if (!string.IsNullOrEmpty(id))
                                            {
                                                var jsonOBJ = JsonSerializer.Deserialize(id, listType);
                                                if (jsonOBJ != null)
                                                {
                                                    conList.Add(jsonOBJ);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (isPrimitive)
                                {
                                    foreach (var id in hashIDs)
                                    {
                                        var a = db.StringGet(singleSpace + ":" + id.ToLower());
                                        var value = Convert.ChangeType(a, listType);
                                        if (value != null)
                                        {
                                            conList.Add(value);
                                        }
                                    }
                                }
                                else
                                {
                                    if (hashIDs.Length > 3)
                                    {
                                        Parallel.ForEach(hashIDs, id =>
                                        {
                                            SetID(id.ToLower());
                                        });
                                    }
                                    else
                                    {
                                        foreach (var id in hashIDs)
                                        {
                                            SetID(id.ToLower());
                                        }
                                    }
                                }
                            }

                            void SetID(string id)
                            {
                                if (typeProccessor.AsJson)
                                {
                                    var a = db.StringGet(singleSpace + ":" + id);

                                    var json = JsonSerializer.Deserialize(a, listType);
                                    if (json != null)
                                    {
                                        conList.Add(json);
                                    }
                                }
                                else
                                {
                                    var newInstance = CreateInstance(listType);
                                    if (newInstance != null)
                                    {
                                        if (!string.IsNullOrEmpty(typeProccessor.KeySpace) && string.IsNullOrEmpty(keySpace))
                                        {
                                            var result = Get_Instance(newInstance, typeProccessor.KeySpace.ToLower(), id);
                                            if (result is not null)
                                            {
                                                conList.Add(result);
                                            }
                                            else
                                            {
                                                RemoveFromSet(fromSet, id);
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(keySpace))
                                            {
                                                var result = Get_Instance(newInstance, singleSpace, id);
                                                if (result is not null)
                                                {
                                                    conList.Add(result);
                                                }
                                                else
                                                {
                                                    RemoveFromSet(fromSet, id);
                                                }
                                            }
                                            else
                                            {
                                                var result = Get_Instance(newInstance, keySpace, id);
                                                if (result is not null)
                                                {
                                                    conList.Add(result);
                                                }
                                                else
                                                {
                                                    RemoveFromSet(fromSet, id);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var obj in conList)
                            {
                                list.Add(obj);
                            }

                            return list;
                        }
                    }
                }
                void RemoveFromSet(string fromSet, string id)
                {
                    if (db is not null)
                    {
                        try
                        {
                            db.SetRemove(fromSet, id);
                        }
                        catch
                        {
                            db.SortedSetRemove(fromSet, id);
                        }
                    }
                }
            }
            return default;
        }

        private object? Get_AsObject(object? instance, string keySpace, string? id)
        {
            var results = new List<bool>();

            if (db != null && instance != null)
            {
                if (Descriptor.TryGetDescriptor(instance.GetType(), out var typeProccessor))
                {
                    keySpace = keySpace.ToLower();

                    if (instance is RediBase rediBase)
                    {
                        rediBase.Redi_WriteName = id;
                    }

                    if (string.IsNullOrEmpty(keySpace))
                    {
                        if (typeProccessor.KeySpace is not null)
                        {
                            keySpace = typeProccessor.KeySpace;
                        }
                        else if (typeProccessor.DefaultSet is not null)
                        {
                            keySpace = pluralizer.Singularize(typeProccessor.DefaultSet);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    var appendToKey = "";

                    if (typeProccessor.AppendToKey != null)
                    {
                        for (int i = 0; i < typeProccessor.AppendToKey.Count; i++)
                        {
                            //var last = keySpace.Split(":").LastOrDefault();
                            var last = id.Split(":").LastOrDefault();
                            var keyToAppend = typeProccessor.AppendToKey[i].ToLower();
                            if (last is not null && last.ToLower() != keyToAppend && id != keyToAppend)
                            {
                                appendToKey += ":" + keyToAppend;
                                //keySpace += ":" + keyToAppend;
                            }
                        }
                    }


                    if (id != null && typeProccessor.WriteNameProperty != null)
                    {
                        if (typeProccessor.WriteNameProperty.CanWrite && typeProccessor.WriteNameProperty.Name == nameof(RediBase.Redi_WriteName))
                        {
                            typeProccessor.WriteNameProperty.SetValue(instance, id);
                        }
                    }

                    if (typeProccessor.Properties != null)
                    {
                        var primitiveProperties = typeProccessor.Properties.Where(x => x.IsPrimitive && !x.Ignore && x.CanSet);
                        var nestedProperties = typeProccessor.Properties.Where(x => !x.IsPrimitive && !x.Ignore && x.CanSet);

                        if (primitiveProperties.Any())
                        {
                            try
                            {
                                var x = db.HashGetAll(keySpace + ":" + id + appendToKey);
                                if (x.Length > 0)
                                {
                                    //var accessors = TypeAccessor.Create(typeProccessor.PropertyType);
                                    results.Add(true);
                                    foreach (var hash in x)
                                    {
                                        var property = primitiveProperties.FirstOrDefault(x => x.Name.ToLower() == hash.Name.ToString().ToLower());
                                        if (property != null && property.PropertyInfo != null && property.PropertyType != null)
                                        {
                                            var redisConvert = ConvertFromRedisType(property.PropertyType, hash.Value);
                                            ///// accessors[instance, property.Name] = Convert.ChangeType(redisConvert, property.PropertyType);
                                            try
                                            {
                                                property.PropertyInfo.SetValue(instance, Convert.ChangeType(redisConvert, property.PropertyType), null);
                                            }
                                            catch
                                            {
                                                property.PropertyInfo.SetValue(instance, redisConvert, null);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    results.Add(false);
                                }
                            }
                            catch
                            {
                                var x = db.StringGet(keySpace + ":" + id);
                                if (!string.IsNullOrEmpty(x) && typeProccessor.PropertyType is not null)
                                {
                                    var jsonObj = JsonSerializer.Deserialize(x, typeProccessor.PropertyType);
                                    if (jsonObj != null)
                                    {
                                        return jsonObj;
                                    }
                                }
                            }
                        }


                        if (nestedProperties.Any())
                        {
                            //foreach (var item in nestedProperties)
                            //{
                            //    SetNest(item);
                            //}

                            Parallel.ForEach(nestedProperties, SetNest);


                            void SetNest(RediType property)
                            {
                                if (property.PropertyType != null && property.PropertyInfo != null)
                                {
                                    if (property.AsJson)
                                    {
                                        var x = db.StringGet(keySpace + ":" + id + ":" + property.Name.ToLower());
                                        if (!string.IsNullOrEmpty(x))
                                        {
                                            var jsonObj = JsonSerializer.Deserialize(x, property.PropertyType);
                                            if (jsonObj != null)
                                            {
                                                property.PropertyInfo.SetValue(instance, jsonObj);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (property.PropertyInfo != null)
                                        {
                                            object? nestedObject = null;

                                            if (property.PropertyType.IsArray)
                                            {
                                                if (property.PropertyInfo.GetValue(instance) is Array array && array.Length > 0)
                                                {
                                                    nestedObject = array;
                                                }
                                            }
                                            else
                                            {
                                                nestedObject = CreateInstance(property.PropertyType);
                                            }

                                            if (nestedObject != null)
                                            {
                                                var result = Get_Instance(nestedObject, keySpace + ":" + id + ":" + property.Name.ToLower());

                                                results.Add(result != null);

                                                property.PropertyInfo.SetValue(instance, result);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (results.Any(x => x))
            {
                return instance;
            }
            else
            {
                return null;
            }
        }




        internal List<string> GetKeys(Type type, string currentKeySpace = "")
        {
            var keys = new List<string>();

            if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
            {
                var ActiveSpace = (string.IsNullOrEmpty(currentKeySpace) ? (currentKeySpace + ":" + typeProccessor.Name) : typeProccessor.Name);
                keys.Add(ActiveSpace);

                if (typeProccessor.SubTypes is not null)
                {
                    foreach (var subType in typeProccessor.SubTypes)
                    {
                        GetKeys(subType, ActiveSpace).ForEach(x => keys.Add(x));
                    }
                }
            }

            return keys;
        }

        internal static bool IsPrimitive(object? obj)
        {
            if (obj != null)
            {
                Type? type;

                if (obj is Type t)
                    type = t;
                else if (obj is PropertyInfo property)
                    type = property.PropertyType;
                else
                    type = obj.GetType();

                if (type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset) || type == typeof(Guid))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal static string[] GetRedisSearchQuery(string keySpace)
        {
            string query = $"{keySpace} * NOCONTENT limit 0 3000";
            return query.Split(" ");
        }

        internal static List<string> GetKeyList(RedisResult[]? results)
        {
            var list = new List<string>();

            if (results != null)
            {
                foreach (var result in results)
                {
                    var st = result.ToString();
                    if (st != null)
                    {
                        var parsed = st.Split(":");
                        if (parsed.Length > 1)
                        {
                            list.Add(parsed.Last());
                        }
                    }
                }
            }

            return list;
        }

        internal static object? ConvertFromRedisType(Type t, object? obj)
        {
            if (obj != null)
            {
                if (t == typeof(DateTime))
                {
                    var value = Convert.ChangeType(obj, typeof(long));
                    if (value != null)
                    {
                        try
                        {
                            return DateTimeOffset.FromUnixTimeSeconds((long)value).DateTime.ToLocalTime();
                        }
                        catch
                        {
                            return new DateTime((long)value).ToLocalTime();
                        }
                    }
                }
                else if (t == typeof(DateTimeOffset))
                {
                    var value = Convert.ChangeType(obj, typeof(long));
                    if (value != null)
                    {
                        return DateTimeOffset.FromUnixTimeSeconds((long)value).ToLocalTime();
                    }
                }
                else if (t == typeof(string))
                {
                    var s = obj.ToString();
                    if (s != null)
                    {
                        return s;
                    }
                }
                else
                {
                    var nullable = Nullable.GetUnderlyingType(t);
                    if (nullable is not null)
                    {
                        var s = Convert.ChangeType(obj, nullable);
                        if (s != null)
                        {
                            return s;
                        }
                    }
                }
            }
            return obj;
        }

        internal static object? CreateInstance(Type t, int length = 1)
        {
            if (t == typeof(string))
                return Activator.CreateInstance(typeof(string), new object[] { "".ToCharArray() });
            else if (t.IsArray)
                return Activator.CreateInstance(t, new object[] { length });
            else
                return Activator.CreateInstance(t);
        }
    }
}
