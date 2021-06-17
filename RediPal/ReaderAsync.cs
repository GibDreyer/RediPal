//using Pluralize.NET;
//using RediPal.Attributes;
//using RediPal.Interfaces;
//using RediPal.Objects;
//using StackExchange.Redis;
//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;

//namespace RediPal
//{
//    internal class ReaderAsync : IReaderAsync
//    {
//        // Fields
//        private readonly IDatabase? db;
//        private readonly RediConfig options;
//        private readonly Pluralizer pluralizer = new Pluralizer();
//        internal ITypeDiscriptor Descriptor { get; }

//        // Constructor
//        internal ReaderAsync(IDatabase? db, RediConfig options, ITypeDiscriptor descriptor)
//        {
//            this.db = db;
//            this.options = options;
//            Descriptor = descriptor;
//        }



//        public long GetIncrementedID(string key)
//        {
//            if (db != null)
//                return db.StringIncrement(key);
//            else
//                return 0;
//        }

//        public bool SetContains(string keySpace, string id)
//        {
//            if (db != null)
//            {
//                try
//                {
//                    return db.SetContains(keySpace, id);
//                }
//                catch
//                {
//                    var result = db.SortedSetRank(keySpace, id);

//                    return result.HasValue;
//                }
//            }
//            else
//            {
//                return false;
//            }
//        }



//        // Interface Methods

//        public async Task<T?> Object<T>(string keySpace, string id) where T : notnull
//        {
//            return await GetObject<T>(keySpace + ":" + id);
//        }

//        public async Task<T?> Object<T>(string id) where T : notnull
//        {
//            if (Descriptor.TryGetDescriptor(typeof(T), out var discriptor))
//            {
//                if (!string.IsNullOrEmpty(discriptor.KeySpace))
//                {
//                    if (id.Contains(discriptor.KeySpace))
//                    {
//                        return await GetObject<T>(id);
//                    }
//                    else
//                    {
//                        return await GetObject<T>(discriptor.KeySpace + ":" + id);
//                    }
//                }
//                else
//                {
//                    return await GetObject<T>(id);
//                }
//            }
//            return default;
//        }

//        private async Task<T?> GetObject<T>(string keySpace) where T : notnull
//        {
//            var type = typeof(T);
//            var instance = CreateInstance(type);

//            if (instance != null && db != null && !string.IsNullOrEmpty(keySpace))
//            {
//            var batch = db.CreateBatch();

//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var x = db.StringGet(keySpace);
//                        return JsonSerializer.Deserialize<T>((string)Convert.ChangeType(x, typeof(string)));
//                    }
//                    else
//                    {
//                        var result = Get_AsObject(instance, keySpace.ToLower(), keySpace.Split(":").LastOrDefault(), batch);
//                        return (T?)result;
//                    }
//                }
//                return (T?)Get_AsObject(instance, keySpace.ToLower(), keySpace.Split(":").LastOrDefault(), batch);
//            }
//            return default;
//        }


//        public async Task<List<T>?> List<T>() where T : notnull
//        {
//            var type = typeof(T);
//            if (db != null)
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (string.IsNullOrEmpty(typeProccessor.DefaultSet))
//                    {
//                        throw new ArgumentException("The Attribute 'RediSet' was not set on the object class. The attribute must be set or a set must be supplied");
//                    }

//                    var key = typeProccessor.DefaultSet;

//                    var instance = new List<T>();

//                    var isprimitive = IsPrimitive(type);

//                    if (typeProccessor.AsJson || isprimitive)
//                    {
//                        RedisValue[]? x;
//                        try
//                        {
//                            x = db.ListRange(key);
//                        }
//                        catch
//                        {
//                            x = db.SetMembers(key);
//                        }

//                        if (!isprimitive)
//                        {
//                            foreach (var item in x)
//                            {
//                                var obj = JsonSerializer.Deserialize<T>((string)Convert.ChangeType(item, typeof(string)));
//                                if (obj != null)
//                                {
//                                    instance.Add(obj);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            foreach (var item in x)
//                            {
//                                var redisString = ConvertFromRedisType(type, item);
//                                if (redisString != null)
//                                {
//                                    var convert = (T)Convert.ChangeType(redisString, type);
//                                    if (convert != null)
//                                    {
//                                        instance.Add(convert);
//                                    }
//                                }
//                            }
//                        }
//                        return instance;
//                    }
//                    else
//                    {
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (List<T>?)Get_AsList(instance, "", key.ToLower(), batch);
//                        }
//                    }
//                }
//            }
//            return default;
//        }

//        public async Task<List<T>?> List<T>(string key) where T : notnull
//        {
//            var type = typeof(T);
//            if (db != null && !string.IsNullOrEmpty(key))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    var instance = new List<T>();

//                    var isprimitive = IsPrimitive(type);

//                    if (typeProccessor.AsJson || isprimitive)
//                    {
//                        RedisValue[]? x;
//                        try
//                        {
//                            x = db.ListRange(key);
//                        }
//                        catch
//                        {
//                            x = db.SetMembers(key);
//                        }

//                        if (!isprimitive)
//                        {
//                            foreach (var item in x)
//                            {
//                                var obj = JsonSerializer.Deserialize<T>((string)Convert.ChangeType(item, typeof(string)));
//                                if (obj != null)
//                                {
//                                    instance.Add(obj);
//                                }
//                            }
//                        }
//                        else
//                        {
//                            foreach (var item in x)
//                            {
//                                var redisString = ConvertFromRedisType(type, item);
//                                if (redisString != null)
//                                {
//                                    var convert = (T)Convert.ChangeType(redisString, type);
//                                    if (convert != null)
//                                    {
//                                        instance.Add(convert);
//                                    }
//                                }
//                            }
//                        }
//                        return instance;
//                    }
//                    else
//                    {
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (List<T>?)Get_AsList(instance, "", key.ToLower(), batch);
//                        }
//                    }
//                }
//            }
//            return default;
//        }

//        public async Task<List<T>?> List<T>(string keySpace, params string[] hashIDs) where T : notnull
//        {
//            var type = typeof(T);
//            if (db != null && !string.IsNullOrEmpty(keySpace))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var x = db.StringGet(keySpace);
//                        return JsonSerializer.Deserialize<List<T>>((string)Convert.ChangeType(x, typeof(string)));
//                    }
//                    else
//                    {
//                        var instance = new List<T>();
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (List<T>?)Get_AsList(instance, "", keySpace.ToLower(), batch, hashIDs);
//                        }
//                    }
//                }
//            }
//            return default;
//        }

//        public async Task<List<T>?> List<T>(string keySpace, string fromSet) where T : notnull
//        {
//            var type = typeof(T);
//            if (db != null && !string.IsNullOrEmpty(keySpace))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var x = db.StringGet(keySpace);
//                        return JsonSerializer.Deserialize<List<T>>((string)Convert.ChangeType(x, typeof(string)));
//                    }
//                    else
//                    {
//                        var instance = new List<T>();
//                        if (instance != null)
//                        {
//                            RedisValue[]? hashes;
//                            try
//                            {
//                                hashes = db.SetMembers(fromSet);
//                            }
//                            catch
//                            {
//                                hashes = db.ListRange(fromSet);
//                            }


//                            if (hashes.Length > 0)
//                            {
//                                var array = new string[hashes.Length];

//                                for (int i = 0; i < hashes.Length; i++)
//                                {
//                                    array[i] = hashes[i];
//                                }
//                                var batch = db.CreateBatch();
//                                return (List<T>?)Get_AsList(instance, keySpace, keySpace.ToLower(), batch, array);
//                            }
//                        }
//                    }
//                }
//            }
//            return default;
//        }



//        public async Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>() where TKey : IConvertible
//        {
//            var type = typeof(TValue);

//            if (db != null)
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (string.IsNullOrEmpty(typeProccessor.DefaultSet))
//                    {
//                        throw new ArgumentException("The Attribute 'RediSet' was not set on the object class. The attribute must be set or a set must be supplied");
//                    }

//                    var set = typeProccessor.DefaultSet;

//                    if (typeProccessor.AsJson)
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        var x = db.HashGetAll(set);
//                        foreach (var hash in x)
//                        {
//                            var value = ConvertFromRedisType(typeof(TKey), hash.Name);
//                            if (value != null)
//                            {
//                                var key = Convert.ChangeType(value, typeof(TKey));
//                                if (key != null)
//                                {
//                                    var json = JsonSerializer.Deserialize<TValue>(hash.Value);
//                                    if (json != null)
//                                    {
//                                        instance.Add((TKey)key, json);
//                                    }
//                                }
//                            }
//                        }
//                        return instance;
//                    }
//                    else
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", set.ToLower(), batch);
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        public async Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(string set) where TKey : IConvertible
//        {
//            var type = typeof(TValue);

//            if (db != null && !string.IsNullOrEmpty(set))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        var x = db.HashGetAll(set);
//                        foreach (var hash in x)
//                        {
//                            var value = ConvertFromRedisType(typeof(TKey), hash.Name);
//                            if (value != null)
//                            {
//                                var key = Convert.ChangeType(value, typeof(TKey));
//                                if (key != null)
//                                {
//                                    var json = JsonSerializer.Deserialize<TValue>(hash.Value);
//                                    if (json != null)
//                                    {
//                                        instance.Add((TKey)key, json);
//                                    }
//                                }
//                            }
//                        }
//                        return instance;
//                    }
//                    else
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        if (instance != null && Redipal.IFactory != null)
//                        {
//                            var batch = db.CreateBatch();
//                            var result = (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", set.ToLower(), batch);
//                            batch.Execute();
//                            return result;
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        public async Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(string keySpace, params string[] hashIDs) where TKey : IConvertible
//        {
//            var type = typeof(TValue);
//            if (db != null && !string.IsNullOrEmpty(keySpace))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var x = db.StringGet(keySpace);
//                        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>((string)Convert.ChangeType(x, typeof(string)));
//                    }
//                    else
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", keySpace.ToLower(), batch, hashIDs);
//                        }
//                    }
//                }
//            }
//            return default;
//        }

//        public async Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(params string[] hashIDs) where TKey : IConvertible
//        {
//            var instance = new Dictionary<TKey, TValue>();
//            if (instance != null && db is not null)
//            {
//                var batch = db.CreateBatch();
//                return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, "", "", batch, hashIDs);
//            }
//            return default;
//        }

//        public async Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(string keySpace, string fromSet) where TKey : IConvertible
//        {
//            var type = typeof(TValue);
//            if (db != null && !string.IsNullOrEmpty(keySpace))
//            {
//                if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//                {
//                    if (typeProccessor.AsJson)
//                    {
//                        var x = db.StringGet(keySpace);
//                        return JsonSerializer.Deserialize<Dictionary<TKey, TValue>>((string)Convert.ChangeType(x, typeof(string)));
//                    }
//                    else
//                    {
//                        var instance = new Dictionary<TKey, TValue>();
//                        if (instance != null)
//                        {
//                            var batch = db.CreateBatch();
//                            return (Dictionary<TKey, TValue>?)Get_AsDictionary(instance, keySpace.ToLower(), fromSet, batch);
//                        }
//                    }
//                }
//            }
//            return default;
//        }




//        // Interal Methods

//        private object? Get_Instance(object? instance, string keySpace, IBatch batch, params string[] hashIDs)
//        {
//            if (instance is IList)
//            {
//                instance = Get_AsList(instance, "", keySpace, batch, hashIDs);
//            }
//            else if (instance is IDictionary)
//            {
//                instance = Get_AsDictionary(instance, "", keySpace, batch, hashIDs);
//            }
//            else
//            {
//                if (hashIDs.Length == 1)
//                {
//                    instance = Get_AsObject(instance, keySpace + ":" + hashIDs[0], hashIDs[0], batch);
//                }
//                else
//                {
//                    instance = Get_AsObject(instance, keySpace, keySpace.Split(":").LastOrDefault(), batch);
//                }
//            }

//            return instance;
//        }


//        private object? Get_AsDictionary(object? instance, string keySpace, string fromSet, IBatch batch, params string[] hashIDs)
//        {
//            if (db != null && instance != null)
//            {
//                fromSet = fromSet.ToLower();

//                if (instance is IDictionary dictionary)
//                {
//                    var type = dictionary.GetType();
//                    var keyType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
//                    var valueType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[1];

//                    var dicts = new ConcurrentDictionary<object, object?>();

//                    if (valueType != null && keyType != null)
//                    {
//                        var isPrimitive = IsPrimitive(valueType);

//                        var singleSpace = pluralizer.Singularize(fromSet);
//                        if (Descriptor.TryGetDescriptor(valueType, out var typeProccessor))
//                        {
//                            if (hashIDs.Length == 0)
//                            {
//                                if (isPrimitive)
//                                {
//                                    var x = db.HashGetAll(fromSet);
//                                    if (x.Length == 0)
//                                    {
//                                        return null;
//                                    }

//                                    for (int i = 0; i < x.Length; i++)
//                                    {
//                                        var elm = x[i];

//                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, elm.Name), keyType);
//                                        var value = Convert.ChangeType(ConvertFromRedisType(valueType, elm.Value), valueType);
//                                        if (key != null && value != null)
//                                        {
//                                            dicts.TryAdd(key, value);
//                                        }
//                                    }
//                                }
//                                else
//                                {

//                                    if (typeProccessor.AsJson)
//                                    {
//                                        var x = db.HashGetAll(fromSet);

//                                        foreach (var entry in x)
//                                        {
//                                            var key = Convert.ChangeType(ConvertFromRedisType(keyType, entry.Name), keyType);
//                                            var value = JsonSerializer.Deserialize(entry.Value, valueType);
//                                            if (key != null && value != null)
//                                            {
//                                                dicts.TryAdd(key, value);
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        RedisValue[] x;
//                                        try
//                                        {
//                                            x = db.SetMembers(fromSet);
//                                        }
//                                        catch
//                                        {
//                                            x = db.SortedSetRangeByScore(fromSet);
//                                        }

//                                        if (x.Length == 0)
//                                        {
//                                            return null;
//                                        }

//                                        if (x.Length > 3) // this one
//                                        {
//                                            var array = new string[x.Length];

//                                            for (int i = 0; i < x.Length; i++)
//                                            {
//                                                array[i] = x[i].ToString();
//                                            }

//                                            Parallel.ForEach(array, SetID);
//                                        }
//                                        else
//                                        {
//                                            foreach (var id in x)
//                                            {
//                                                SetID(id);
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                if (isPrimitive)
//                                {
//                                    foreach (var id in hashIDs)
//                                    {
//                                        var a = db.StringGet(singleSpace + ":" + id.ToLower());
//                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);
//                                        var value = Convert.ChangeType(ConvertFromRedisType(keyType, a), valueType);
//                                        if (key != null && value != null)
//                                        {
//                                            dicts.TryAdd(key, value);
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    if (hashIDs.Length > 3)
//                                    {
//                                        Parallel.ForEach(hashIDs, id =>
//                                        {
//                                            SetID(id.ToLower());
//                                        });
//                                    }
//                                    else
//                                    {
//                                        foreach (var id in hashIDs)
//                                        {
//                                            SetID(id.ToLower());
//                                        }
//                                    }
//                                }
//                            }

//                            void SetID(string id)
//                            {
//                                if (typeProccessor.AsJson)
//                                {
//                                    var a = db.StringGet(singleSpace + ":" + id);

//                                    var json = JsonSerializer.Deserialize(a, valueType);
//                                    var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);

//                                    if (json != null && key != null)
//                                    {
//                                        var valueString = key;
//                                        if (valueString != null)
//                                        {
//                                            dicts.TryAdd(valueString, json);
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    var newInstance = CreateInstance(valueType);
//                                    if (newInstance != null)
//                                    {
//                                        var key = Convert.ChangeType(ConvertFromRedisType(keyType, id), keyType);
//                                        if (key != null)
//                                        {
//                                            if (!string.IsNullOrEmpty(typeProccessor.KeySpace) && string.IsNullOrEmpty(keySpace))
//                                            {
//                                                dicts.TryAdd(key, Get_Instance(newInstance, typeProccessor.KeySpace.ToLower(), batch, id));
//                                            }
//                                            else
//                                            {
//                                                if (string.IsNullOrEmpty(keySpace))
//                                                {
//                                                    dicts.TryAdd(key, Get_Instance(newInstance, singleSpace, batch, id));
//                                                }
//                                                else
//                                                {
//                                                    dicts.TryAdd(key, Get_Instance(newInstance, keySpace, batch, id));
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }

//                            foreach (var item in dicts)
//                            {
//                                dictionary.Add(item.Key, item.Value);
//                            }
//                            return dictionary;
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        private object? Get_AsList(object? instance, string keySpace, string fromSet, IBatch batch, params string[] hashIDs)
//        {
//            if (db != null && instance != null)
//            {
//                fromSet = fromSet.ToLower();

//                if (instance is IList list)
//                {
//                    var type = list.GetType();
//                    var listType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

//                    if (listType != null)
//                    {
//                        var isPrimitive = IsPrimitive(listType);

//                        var singleSpace = pluralizer.Singularize(fromSet);
//                        if (Descriptor.TryGetDescriptor(listType, out var typeProccessor))
//                        {
//                            var conList = new ConcurrentBag<object?>();

//                            if (hashIDs.Length == 0)
//                            {
//                                var setKey = pluralizer.Pluralize(fromSet);
//                                if (isPrimitive)
//                                {
//                                    var x = new RedisValue[1];

//                                    try
//                                    {
//                                        x = db.SetMembers(setKey);
//                                    }
//                                    catch
//                                    {
//                                        x = db.ListRange(setKey);
//                                    }

//                                    if (x.Length == 0)
//                                    {
//                                        return null;
//                                    }
//                                    if (list.GetType().IsArray)
//                                    {
//                                        if (list is Array array && x is Array xArray)
//                                        {
//                                            for (int i = 0; i < xArray.Length; i++)
//                                            {
//                                                if (xArray.Length < i || array.Length < i)
//                                                {
//                                                    break;
//                                                }

//                                                var stringValue = ConvertFromRedisType(listType, xArray.GetValue(i));
//                                                var value = Convert.ChangeType(stringValue, listType);
//                                                if (stringValue != null && value != null)
//                                                {
//                                                    array.SetValue(value, i);
//                                                }
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        foreach (var id in x)
//                                        {
//                                            var value = Convert.ChangeType(ConvertFromRedisType(listType, id), listType);
//                                            if (value != null)
//                                            {
//                                                conList.Add(value);
//                                            }
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    if (!typeProccessor.AsJson)
//                                    {
//                                        var x = new RedisValue[1];

//                                        try
//                                        {
//                                            x = db.SetMembers(setKey);
//                                        }
//                                        catch
//                                        {
//                                            x = db.ListRange(setKey);
//                                        }


//                                        if (x.Length == 0)
//                                        {
//                                            return null;
//                                        }
//                                        if (x.Length > 3)
//                                        {
//                                            Parallel.ForEach(x, id =>
//                                            {
//                                                SetID(id);
//                                            });
//                                        }
//                                        else
//                                        {
//                                            foreach (var id in x)
//                                            {
//                                                SetID(id);
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        var x = db.ListRange(setKey);
//                                        if (x.Length == 0)
//                                        {
//                                            return null;
//                                        }
//                                        foreach (var id in x)
//                                        {
//                                            if (!string.IsNullOrEmpty(id))
//                                            {
//                                                var jsonOBJ = JsonSerializer.Deserialize(id, listType);
//                                                if (jsonOBJ != null)
//                                                {
//                                                    conList.Add(jsonOBJ);
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                if (isPrimitive)
//                                {
//                                    foreach (var id in hashIDs)
//                                    {
//                                        var a = db.StringGet(singleSpace + ":" + id.ToLower());
//                                        var value = Convert.ChangeType(a, listType);
//                                        if (value != null)
//                                        {
//                                            conList.Add(value);
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    if (hashIDs.Length > 3)
//                                    {
//                                        Parallel.ForEach(hashIDs, id =>
//                                        {
//                                            SetID(id.ToLower());
//                                        });
//                                    }
//                                    else
//                                    {
//                                        foreach (var id in hashIDs)
//                                        {
//                                            SetID(id.ToLower());
//                                        }
//                                    }
//                                }
//                            }

//                            void SetID(string id)
//                            {
//                                if (typeProccessor.AsJson)
//                                {
//                                    var a = db.StringGet(singleSpace + ":" + id);

//                                    var json = JsonSerializer.Deserialize(a, listType);
//                                    if (json != null)
//                                    {
//                                        conList.Add(json);
//                                    }
//                                }
//                                else
//                                {
//                                    var newInstance = CreateInstance(listType);
//                                    if (newInstance != null)
//                                    {
//                                        if (!string.IsNullOrEmpty(typeProccessor.KeySpace) && string.IsNullOrEmpty(keySpace))
//                                        {
//                                            conList.Add(Get_Instance(newInstance, typeProccessor.KeySpace.ToLower(), batch, id));
//                                        }
//                                        else
//                                        {
//                                            if (string.IsNullOrEmpty(keySpace))
//                                            {
//                                                conList.Add(Get_Instance(newInstance, singleSpace, batch, id));
//                                            }
//                                            else
//                                            {
//                                                conList.Add(Get_Instance(newInstance, keySpace, batch, id));
//                                            }
//                                        }
//                                    }
//                                }
//                            }

//                            foreach (var obj in conList)
//                            {
//                                list.Add(obj);
//                            }

//                            return list;
//                        }
//                    }
//                }
//            }
//            return default;
//        }

//        private object? Get_AsObject(object? instance, string keySpace, string? id, IBatch batch)
//        {
//            var results = new ConcurrentBag<bool>();

//            if (db != null && instance != null)
//            {
//                if (Descriptor.TryGetDescriptor(instance.GetType(), out var typeProccessor))
//                {
//                    keySpace = keySpace.ToLower();

//                    if (instance is RediBase rediBase)
//                    {
//                        rediBase.Redi_WriteName = id;
//                    }

//                    if (string.IsNullOrEmpty(keySpace))
//                    {
//                        if (typeProccessor.KeySpace is not null)
//                        {
//                            keySpace = typeProccessor.KeySpace;
//                        }
//                        else if (typeProccessor.DefaultSet is not null)
//                        {
//                            keySpace = pluralizer.Singularize(typeProccessor.DefaultSet);
//                        }
//                        else
//                        {
//                            return null;
//                        }
//                    }


//                    if (typeProccessor.AppendToKey != null)
//                    {
//                        for (int i = 0; i < typeProccessor.AppendToKey.Count; i++)
//                        {
//                            var keyToAppend = typeProccessor.AppendToKey[i];
//                            keySpace += ":" + keyToAppend;
//                        }
//                    }

//                    if (id != null && typeProccessor.WriteNameProperty != null)
//                    {
//                        if (typeProccessor.WriteNameProperty.CanWrite && typeProccessor.WriteNameProperty.Name == nameof(RediBase.Redi_WriteName))
//                        {
//                            typeProccessor.WriteNameProperty.SetValue(instance, id);
//                        }
//                    }

//                    if (typeProccessor.Properties != null)
//                    {
//                        var primitiveProperties = typeProccessor.Properties.Where(x => x.IsPrimitive && !x.Ignore && x.CanSet);
//                        var nestedProperties = typeProccessor.Properties.Where(x => !x.IsPrimitive && !x.Ignore && x.CanSet);

//                        if (primitiveProperties.Any())
//                        {
//                            try
//                            {
//                                db.HashGetAllAsync(keySpace).ContinueWith(x =>
//                                //var task = batch.HashGetAllAsync(keySpace).ContinueWith(x =>
//                                {
//                                    if (x.Result.Length > 0)
//                                    {
//                                        results.Add(true);
//                                        foreach (var hash in x.Result)
//                                        {
//                                            var property = primitiveProperties.FirstOrDefault(x => x.Name.ToLower() == hash.Name.ToString().ToLower());
//                                            if (property != null && property.PropertyInfo != null && property.PropertyType != null)
//                                            {
//                                                var redisConvert = ConvertFromRedisType(property.PropertyType, hash.Value);
//                                                property.PropertyInfo.SetValue(instance, Convert.ChangeType(redisConvert, property.PropertyType));
//                                            }
//                                        }
//                                    }
//                                });
//                            }
//                            catch
//                            {
//                                var x = db.StringGet(keySpace);
//                                if (!string.IsNullOrEmpty(x) && typeProccessor.PropertyType is not null)
//                                {
//                                    var jsonObj = JsonSerializer.Deserialize(x, typeProccessor.PropertyType);
//                                    if (jsonObj != null)
//                                    {
//                                        return jsonObj;
//                                    }
//                                }
//                            }
//                        }


//                        if (nestedProperties.Any())
//                        {
//                            //foreach (var item in nestedProperties)
//                            //{
//                            //    SetNest(item);
//                            //}

//                            Parallel.ForEach(nestedProperties, SetNest);


//                            void SetNest(RediType property)
//                            {
//                                if (property.PropertyType != null && property.PropertyInfo != null)
//                                {
//                                    if (property.AsJson)
//                                    {
//                                        var x = db.StringGet(keySpace + ":" + property.Name.ToLower());
//                                        if (!string.IsNullOrEmpty(x))
//                                        {
//                                            var jsonObj = JsonSerializer.Deserialize(x, property.PropertyType);
//                                            if (jsonObj != null)
//                                            {
//                                                property.PropertyInfo.SetValue(instance, jsonObj);
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                        if (property.PropertyInfo != null)
//                                        {
//                                            object? nestedObject = null;

//                                            if (property.PropertyType.IsArray)
//                                            {
//                                                if (property.PropertyInfo.GetValue(instance) is Array array && array.Length > 0)
//                                                {
//                                                    nestedObject = array;
//                                                }
//                                            }
//                                            else
//                                            {
//                                                nestedObject = CreateInstance(property.PropertyType);
//                                            }

//                                            if (nestedObject != null && Redipal.IFactory is not null)
//                                            {
//                                                var batch = db.CreateBatch();
//                                                var result = Get_Instance(nestedObject, keySpace + ":" + property.Name.ToLower(), batch);

//                                                results.Add(result != null);

//                                                property.PropertyInfo.SetValue(instance, result);
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            if (results.Any(x => x))
//            {
//                return instance;
//            }
//            else
//            {
//                return null;
//            }
//        }




//        internal List<string> GetKeys(Type type, string currentKeySpace = "")
//        {
//            var keys = new List<string>();

//            if (Descriptor.TryGetDescriptor(type, out var typeProccessor))
//            {
//                var ActiveSpace = (string.IsNullOrEmpty(currentKeySpace) ? (currentKeySpace + ":" + typeProccessor.Name) : typeProccessor.Name);
//                keys.Add(ActiveSpace);

//                if (typeProccessor.SubTypes is not null)
//                {
//                    foreach (var subType in typeProccessor.SubTypes)
//                    {
//                        GetKeys(subType, ActiveSpace).ForEach(x => keys.Add(x));
//                    }
//                }
//            }

//            return keys;
//        }

//        internal static bool IsPrimitive(object? obj)
//        {
//            if (obj != null)
//            {
//                Type? type;

//                if (obj is Type t)
//                    type = t;
//                else if (obj is PropertyInfo property)
//                    type = property.PropertyType;
//                else
//                    type = obj.GetType();

//                if (type.IsPrimitive || type.IsValueType || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset) || type == typeof(Guid))
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            else
//            {
//                return false;
//            }
//        }

//        internal static string[] GetRedisSearchQuery(string keySpace)
//        {
//            string query = $"{keySpace} * NOCONTENT limit 0 3000";
//            return query.Split(" ");
//        }

//        internal static List<string> GetKeyList(RedisResult[]? results)
//        {
//            var list = new List<string>();

//            if (results != null)
//            {
//                foreach (var result in results)
//                {
//                    var st = result.ToString();
//                    if (st != null)
//                    {
//                        var parsed = st.Split(":");
//                        if (parsed.Length > 1)
//                        {
//                            list.Add(parsed.Last());
//                        }
//                    }
//                }
//            }

//            return list;
//        }

//        internal static object? ConvertFromRedisType(Type t, object? obj)
//        {
//            if (obj != null)
//            {
//                if (t == typeof(DateTime))
//                {
//                    var value = Convert.ChangeType(obj, typeof(long));
//                    if (value != null)
//                    {
//                        try
//                        {
//                            return DateTimeOffset.FromUnixTimeSeconds((long)value).DateTime;
//                        }
//                        catch
//                        {
//                            return new DateTime((long)value);
//                        }
//                    }
//                }
//                else if (t == typeof(DateTimeOffset))
//                {
//                    var value = Convert.ChangeType(obj, typeof(long));
//                    if (value != null)
//                    {
//                        return DateTimeOffset.FromUnixTimeSeconds((long)value);
//                    }
//                }
//                else if (t == typeof(string))
//                {
//                    var s = obj.ToString();
//                    if (s != null)
//                    {
//                        return s;
//                    }
//                }
//            }
//            return obj;
//        }

//        internal static object? CreateInstance(Type t)
//        {
//            try
//            {
//                if (t == typeof(string))
//                    return Activator.CreateInstance(typeof(string), new object[] { "value".ToCharArray() });
//                else
//                    return Activator.CreateInstance(t);
//            }
//            catch { return default; }
//        }
//    }
//}
