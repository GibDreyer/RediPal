using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Pluralize.NET;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RedipalCore
{
    internal class RediWriter : IRediWriter
    {
        private readonly IDatabase db;
        private readonly Pluralizer pluralizer;
        private readonly ITypeDiscriptor Descriptor;
        private readonly IRediFactory Extensions;

        internal RediWriter(IDatabase db, IRediFactory extensions)
        {
            this.db = db;
            pluralizer = new Pluralizer();
            Descriptor = extensions.TypeDescriptor;
            Extensions = extensions;
        }



        // Interface Methods

        public RediWriteResult? Value(IConvertible value, string key, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Value(value, key, writer);
        }
        public RediWriteResult? Value(IConvertible value, string key, RediWriteOptions? writeOptions)
        {
            if (db == null)
            {
                throw new Exception("Unable to connect to the DataBase");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The key given was null or empty");
            }
            if (value == null)
            {
                throw new Exception("The value given was null");
            }

            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            var master = Build_Value(value, key.ToLower(), writeOptions);

            if (master != null)
            {
                SetNested(master, writeOptions);

                if (exucute)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }



        public RediWriteResult? Field(IConvertible value, string key, string field, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Field(value, key, field, writer);
        }
        public RediWriteResult? Field(IConvertible value, string key, string field, RediWriteOptions? writeOptions)
        {
            if (db == null)
            {
                throw new Exception("Unable to connect to the DataBase");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The key given was null or empty");
            }
            if (string.IsNullOrEmpty(field))
            {
                throw new Exception("The fieldID given was null or empty");
            }
            if (value == null)
            {
                throw new Exception("The value given was null");
            }

            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            var master = Build_Value(value, key.ToLower(), field.ToLower(), writeOptions);

            if (master != null)
            {
                SetNested(master, writeOptions);

                if (exucute)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }


        public RediWriteResult? Fields(RediField rediField, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Fields(rediField, writer);
        }
        public RediWriteResult? Fields(RediField rediField, RediWriteOptions? writeOptions = null)
        {
            if (db == null)
            {
                return default;
            }
            if (rediField == null || string.IsNullOrEmpty(rediField.Key) || rediField.Fields == null)
            {
                return default;
            }

            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }

                exucute = true;
            }

            var master = new RediWriteResult(writeOptions.Expiration)
            {
                KeySpace = rediField.Key,
                DeleteExisting = writeOptions.DeleteExisting,
                RediWrite = RediWriteMethods.AsObject,
                Fields = rediField.Fields.ToList()
            };

            if (master != null)
            {
                SetNested(master, writeOptions);

                if (exucute && writeOptions.RediBatch is not null)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }

        public RediWriteResult? Fields(RediField[] rediFields, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Fields(rediFields, writer);
        }
        public RediWriteResult? Fields(RediField[] rediFields, RediWriteOptions? writeOptions)
        {
            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            var master = new RediWriteResult(writeOptions.Expiration)
            {
                Nested = new List<RediWriteResult>()
            };

            foreach (var field in rediFields)
            {
                var result = Fields(field, writeOptions);
                if (result != null)
                {
                    master.Nested.Add(result);
                }
            }

            if (master != null)
            {
                if (exucute)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }


        public RediWriteResult? Object<T>(T obj, params Action<RediWriteOptions>[] writeOptions) where T : notnull
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Object(obj, writer);
        }

        public RediWriteResult? Object<T>(T obj, RediWriteOptions? writeOptions) where T : notnull
        {
            if (obj is null)
            {
                throw new ArgumentException("Unable to connect to the DataBase");
            }
            if (db == null)
            {
                throw new Exception("Unable to connect to the DataBase");
            }
            if (obj is IDictionary)
            {
                throw new ArgumentException("A dictionary was given when an object was expected");
            }
            if (obj is IList)
            {
                throw new ArgumentException("A list was given when an object was expected");
            }
            if (IsPrimitive(obj))
            {
                throw new ArgumentException("A primitive type was given when an object was expected");
            }


            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            if (writeOptions != null)
            {
                if (Descriptor.TryGetDescriptor(obj.GetType(), out var discriptor))
                {
                    if (string.IsNullOrEmpty(writeOptions.ID))
                    {
                        if (obj is RediBase rediBase && rediBase.Redi_WriteName != null)
                        {
                            writeOptions.ID = rediBase.Redi_WriteName;
                        }
                        else
                        {
                            if (discriptor.WriteNameProperty is not null)
                            {
                                var value = discriptor.WriteNameProperty.GetValue(obj);
                                if (value is not null && IsPrimitive(value))
                                {
                                    var redisName = GetStringValue(value);
                                    if (string.IsNullOrEmpty(redisName))
                                    {
                                        throw new ArgumentException("RediWriteName is applied to a property that is not convertable to a redis type");
                                    }
                                    else
                                    {
                                        writeOptions.ID = redisName;
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException("RediWriteName is applied to a property that is not convertable to a redis type");
                                }
                            }
                            else
                            {
                                throw new ArgumentException("RediWriteName is not applied to a property on the object so an ID must be supplied");
                            }
                        }
                    }

                    if (writeOptions.KeySpace == null || string.IsNullOrEmpty(writeOptions.KeySpace))
                    {
                        if (string.IsNullOrEmpty(discriptor.KeySpace))
                        {
                            throw new ArgumentException("RediKeySpace is not applied to a object class so an ID must be supplied or the attribute must be set");
                        }
                        else
                        {
                            writeOptions.KeySpace = discriptor.KeySpace;
                        }
                    }
                }

                var master = Build_Object(obj, writeOptions.KeySpace, writeOptions.ID, writeOptions);

                if (master != null)
                {
                    SetNested(master, writeOptions);

                    if (exucute)
                    {
                        writeOptions.RediBatch.Execute();
                    }

                    return master;
                }
            }
            return null;
        }



        //TODO: add the write the keys to a different list the the objects
        public RediWriteResult? Dictionary(IDictionary dictionary, string setKeySpace, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return Dictionary(dictionary, setKeySpace, writer);
        }
        public RediWriteResult? Dictionary(IDictionary dictionary, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            if (Descriptor.TryGetDescriptor(dictionary.GetType().GetGenericArguments()[1], out var proccessor))
            {
                if (proccessor.DefaultSet is not null)
                {
                    return Dictionary(dictionary, proccessor.DefaultSet, writer);
                }
            }
            return null;
        }
        public RediWriteResult? Dictionary(IDictionary dictionary, string setKeySpace, RediWriteOptions? writeOptions)
        {
            if (db == null)
            {
                throw new Exception("Unable to connect to the DataBase");
            }
            if (string.IsNullOrEmpty(setKeySpace))
            {
                throw new Exception("The setKeySpace given was null or empty");
            }
            if (dictionary == null || dictionary.Count == 0)
            {
                throw new Exception("The dictionary given was null or empty");
            }

            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            var master = Build_Dictionary(dictionary, setKeySpace.ToLower(), writeOptions);

            if (master != null)
            {
                SetNested(master, writeOptions);
                if (exucute)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }


        public RediWriteResult? List(IList list, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }
            if (Descriptor.TryGetDescriptor(list.GetType().GetGenericArguments()[0], out var proccessor))
            {
                if (proccessor.DefaultSet is not null)
                {
                    return List(list, proccessor.DefaultSet, writer);
                }
            }
            return null;
        }
        public RediWriteResult? List(IList list, string key, params Action<RediWriteOptions>[] writeOptions)
        {
            var writer = new RediWriteOptions();
            foreach (var option in writeOptions)
            {
                option.Invoke(writer);
            }

            return List(list, key, writer);
        }
        public RediWriteResult? List(IList list, string key, RediWriteOptions? writeOptions)
        {
            if (db == null)
            {
                throw new Exception("Unable to connect to the DataBase");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("The key given was null or empty");
            }
            if (list == null || list.Count == 0)
            {
                throw new Exception("The list given was null or empty");
            }

            var exucute = false;
            if (writeOptions == null || writeOptions.RediBatch == null)
            {
                if (writeOptions == null)
                {
                    writeOptions = new RediWriteOptions
                    {
                        RediBatch = Extensions.CreateBatch()
                    };
                }
                else
                {
                    writeOptions.RediBatch = Extensions.CreateBatch();
                }
                exucute = true;
            }

            if (writeOptions.RediBatch == null)
            {
                return default;
            }

            var master = Build_List(list, key.ToLower(), writeOptions);

            if (master != null)
            {
                SetNested(master, writeOptions);

                if (exucute)
                {
                    writeOptions.RediBatch.Execute();
                }

                return master;
            }
            return null;
        }



        public RediWriteResult? Property<T, P>(T obj, Expression<Func<T, P>> property) where T : notnull where P : notnull
        {
            if (Extensions.TypeDescriptor.TryGetDescriptor(typeof(T), out var discriptor))
            {
                if (!string.IsNullOrEmpty(discriptor.KeySpace))
                {
                    if (discriptor.WriteNameProperty is not null)
                    {
                        var nameValue = discriptor.WriteNameProperty.GetValue(obj);
                        if (nameValue is not null && IsPrimitive(nameValue))
                        {
                            var redisName = GetStringValue(nameValue);
                            if (!string.IsNullOrEmpty(redisName))
                            {
                                var body = property.Body.ToString();
                                var key = discriptor.KeySpace + ":" + redisName + body.Replace(body.Split(".").First(), "").Replace(".", ":").ToLower();

                                var propName = key.Split(":").LastOrDefault();

                                var compile = property.Compile();
                                if (compile != null && propName != null)
                                {
                                    var value = compile.Invoke(obj);
                                    if (value != null)
                                    {
                                        if (value is IConvertible convertible)
                                        {
                                            key = key.Replace(":" + propName, "");
                                            return Field(convertible, key, propName);
                                        }
                                        else if (value is IDictionary dictionary)
                                        {
                                            return Dictionary(dictionary, pluralizer.Pluralize(key));
                                        }
                                        else if (value is IList list)
                                        {
                                            return List(list, pluralizer.Pluralize(key));
                                        }
                                        else
                                        {
                                            key = key.Replace(":" + propName, "");
                                            return Object(value, x => x.KeySpace = key, x => x.ID = propName);
                                        }
                                    }
                                }
                            }
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




        //tested and working
        internal static RediWriteResult? Build_Value(IConvertible value, string key, string field, RediWriteOptions? options = null)
        {
            if (options == null)
            {
                options = new RediWriteOptions();
            }

            var master = new RediWriteResult(options.Expiration)
            {
                KeySpace = key,
                DeleteExisting = options.DeleteExisting,
                RediWrite = RediWriteMethods.AsObject
            };

            if (options.SetsToAppend != null)
            {
                master.AppendToSets = new List<string>();
                foreach (var set in options.SetsToAppend)
                {
                    master.AppendToSets.Add(set);
                }
                options.SetsToAppend = null;
            }

            if (IsPrimitive(value))
            {
                var stringValue = GetStringValue(value);
                if (stringValue != null)
                {
                    master.Fields.Add(new HashEntry(field, stringValue));
                }
                else
                {
                    return default;
                }
                return master;
            }
            else
            {
                return default;
            }
        }

        internal static RediWriteResult? Build_Value(IConvertible value, string key, RediWriteOptions? options = null)
        {
            if (options == null)
            {
                options = new RediWriteOptions();
            }

            var name = key.Split(":").LastOrDefault();

            var master = new RediWriteResult(options.Expiration)
            {
                KeySpace = key,
                ID = name ?? key,
                DeleteExisting = options.DeleteExisting,
                RediWrite = RediWriteMethods.AsString
            };

            if (options.SetsToAppend != null)
            {
                master.AppendToSets = new List<string>();
                foreach (var set in options.SetsToAppend)
                {
                    master.AppendToSets.Add(set);
                }
                options.SetsToAppend = null;
            }

            if (IsPrimitive(value))
            {
                var stringValue = GetStringValue(value);
                if (stringValue != null)
                {
                    master.Fields.Add(new HashEntry("", stringValue));
                }
                else
                {
                    return default;
                }
                return master;
            }
            else
            {
                return default;
            }
        }

        internal RediWriteResult? Build_List(IList list, string key, RediWriteOptions? options = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var setSpace = pluralizer.Pluralize(key.ToLower());
                var keySpace = pluralizer.Singularize(key.ToLower());

                Type? listType;
                if (list.GetType().IsArray)
                    listType = list.GetType().GetElementType();
                else
                    listType = list.GetType().GenericTypeArguments[0];

                if (listType != null && Descriptor.TryGetDescriptor(listType, out var typeProccessor))
                {
                    if (options == null)
                    {
                        options = new RediWriteOptions();
                    }
                    var master = new RediWriteResult(options.Expiration)
                    {
                        KeySpace = setSpace,
                        DeleteExisting = options.DeleteExisting,
                        RediWrite = RediWriteMethods.AsList
                    };


                    if (!string.IsNullOrEmpty(typeProccessor.DefaultSet))
                    {
                        if (master.AppendToSets == null)
                        {
                            master.AppendToSets = new List<string>();
                        }
                        master.AppendToSets.Add(typeProccessor.DefaultSet);
                    }

                    if (options.SetsToAppend != null)
                    {
                        if (master.AppendToSets == null)
                        {
                            master.AppendToSets = new List<string>();
                        }

                        foreach (var set in options.SetsToAppend)
                        {
                            master.AppendToSets.Add(set);
                        }
                        options.SetsToAppend = null;
                    }

                    if (typeProccessor.Expiration != null)
                    {
                        master.Expiration = typeProccessor.Expiration;
                    }

                    if (master.Nested == null)
                        master.Nested = new List<RediWriteResult>();

                    if (list.Count > 0)
                    {
                        if (typeProccessor.AsJson)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                var elem = list[i];

                                var value = JsonSerializer.Serialize(elem);
                                if (value != null)
                                {
                                    master.Fields.Add(new HashEntry(i, value));
                                }
                            }
                        }
                        else if (list is IList<IConvertible> || IsPrimitive(listType))
                        {
                            master.RediWrite = RediWriteMethods.AsSet;
                            for (int i = 0; i < list.Count; i++)
                            {
                                var elem = list[i];

                                var value = GetStringValue(elem);
                                if (value != null)
                                {
                                    master.Fields.Add(new HashEntry(0, value));
                                }
                            }
                        }
                        else
                        {
                            master.Fields = GetHashs(list.Count);
                            master.RediWrite = RediWriteMethods.AsSet;

                            for (int i = 0; i < list.Count; i++)
                            {
                                var elem = list[i];

                                if (elem != null)
                                {
                                    options.DeleteExisting = true;
                                    var result = Build_Object(elem, keySpace, i.ToString(), options);
                                    if (result != null)
                                    {
                                        if (result != null)
                                        {
                                            master.Nested.Add(result);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        master.DeleteExisting = true;
                    }

                    return master;
                }
            }
            return default;

            static List<HashEntry> GetHashs(int count)
            {
                var entries = new List<HashEntry>();
                for (int i = 0; i < count; i++)
                {
                    entries.Add(new HashEntry(0, i));
                }
                return entries;
            }
        }

        internal RediWriteResult? Build_Dictionary(IDictionary dict, string key, RediWriteOptions? options = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var setSpace = pluralizer.Pluralize(key.ToLower());
                var keySpace = pluralizer.Singularize(key.ToLower());

                if (dict != null)
                {
                    var keyType = dict.GetType().GetGenericArguments()[0];
                    var valueType = dict.GetType().GetGenericArguments()[1];

                    if (Descriptor.TryGetDescriptor(valueType, out var typeProccessor))
                    {
                        if (options == null)
                        {
                            options = new RediWriteOptions();
                        }

                        var master = new RediWriteResult(options.Expiration)
                        {
                            KeySpace = setSpace,
                            DeleteExisting = options.DeleteExisting
                        };

                        if (!string.IsNullOrEmpty(typeProccessor.DefaultSet))
                        {
                            if (master.AppendToSets == null)
                                master.AppendToSets = new List<string>();

                            master.AppendToSets.Add(typeProccessor.DefaultSet);
                        }

                        if (options.SetsToAppend != null)
                        {
                            if (master.AppendToSets == null)
                                master.AppendToSets = new List<string>();

                            foreach (var set in options.SetsToAppend)
                            {
                                master.AppendToSets.Add(set);
                            }

                            options.SetsToAppend = null;
                        }

                        if (IsPrimitive(keyType))
                        {
                            var keys = new object[dict.Keys.Count];
                            dict.Keys.CopyTo(keys, 0);

                            var values = new object[dict.Values.Count];
                            dict.Values.CopyTo(values, 0);

                            var dictionary = new Dictionary<string, object>();

                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (i < keys.Length && i < values.Length)
                                {
                                    if (values[i] != null)
                                    {
                                        var stringValue = GetStringValue(keys[i]);
                                        if (!string.IsNullOrEmpty(stringValue))
                                        {
                                            dictionary.Add(stringValue, values[i]);
                                        }
                                    }
                                }
                            }

                            if (dictionary.Any())
                            {
                                if (typeProccessor != null)
                                {
                                    if (typeProccessor.Expiration != null)
                                    {
                                        master.Expiration = typeProccessor.Expiration;
                                    }

                                    if (master.Nested == null)
                                        master.Nested = new List<RediWriteResult>();

                                    var isPrimitive = IsPrimitive(dictionary.First().Value);

                                    if (typeProccessor.AsJson)
                                    {
                                        master.RediWrite = RediWriteMethods.AsObject;
                                        master.DeleteExisting = true;
                                        foreach (var elem in dictionary)
                                        {
                                            var value = JsonSerializer.Serialize(elem.Value);
                                            if (value != null)
                                            {
                                                master.Fields.Add(new HashEntry(elem.Key, value));
                                            }
                                        }
                                    }
                                    else if (isPrimitive)
                                    {
                                        master.RediWrite = RediWriteMethods.AsObject;
                                        master.DeleteExisting = true;
                                        foreach (var elem in dictionary)
                                        {
                                            var value = GetStringValue(elem.Value);
                                            if (value != null)
                                            {
                                                master.Fields.Add(new HashEntry(elem.Key, value));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        master.Fields = GetHashs(dictionary.Keys);
                                        master.RediWrite = RediWriteMethods.AsSet;

                                        foreach (var elem in dictionary)
                                        {
                                            options.DeleteExisting = true;
                                            var result = Build_Object(elem.Value, keySpace, elem.Key.ToLower(), options);
                                            if (result != null)
                                            {
                                                if (result != null)
                                                {
                                                    master.Nested.Add(result);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                master.DeleteExisting = true;
                            }
                        }

                        return master;
                    }


                    static List<HashEntry> GetHashs(ICollection<string> keys)
                    {
                        var entries = new List<HashEntry>();
                        foreach (var key in keys)
                        {
                            entries.Add(new HashEntry(0, key));
                        }
                        return entries;
                    }
                }
            }

            return null;
        }

        internal RediWriteResult? Build_Object(object obj, string? key, string? hash, RediWriteOptions? options = null)
        {
            if (options == null)
            {
                options = new RediWriteOptions();
            }

            if (obj is not null && Descriptor.TryGetDescriptor(obj.GetType(), out var discriptor))
            {
                discriptor.RunConditionals(obj);

                if (discriptor.Ignore)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(discriptor.DefaultSet))
                {
                    options.AppendToSet(discriptor.DefaultSet);
                }

                if (string.IsNullOrEmpty(hash) && discriptor.WriteNameProperty is not null)
                {
                    var value = discriptor.WriteNameProperty.GetValue(obj);
                    if (value is not null && IsPrimitive(value))
                    {
                        var redisName = GetStringValue(value);
                        if (!string.IsNullOrEmpty(redisName))
                        {
                            hash = redisName.ToLower();
                        }
                    }
                }

                if (hash != null)
                {


                    hash = hash.ToLower();

                    key = discriptor.KeySpace is not null && string.IsNullOrEmpty(key) ? discriptor.KeySpace : key;

                    if (discriptor.Append_PostID is not null)
                    {
                        foreach (var item in discriptor.Append_PostID)
                        {
                            hash += item;
                        }
                    }

                    if (discriptor.Append_PreID is not null)
                    {
                        var pres = "";
                        foreach (var item in discriptor.Append_PreID)
                        {
                            hash += item;
                        }
                        hash = pres + hash;
                    }

                    var master = new RediWriteResult(options.Expiration)
                    {
                        KeySpace = (key + ":" + hash).ToLower(),
                        ID = hash,
                        DeleteExisting = options.DeleteExisting
                    };

                    if (discriptor.Append_ToSearchSets != null && discriptor.ScoreProperty != null)
                    {
                        master.SearchScoreProperty = discriptor.ScoreProperty;
                        var value = discriptor.ScoreProperty.GetValue(obj);
                        if (value != null)
                        {
                            master.SearchScore = GetScoreValue(value);
                        }

                        if (master.AppendToSearchSets == null)
                            master.AppendToSearchSets = new List<string>();

                        discriptor.Append_ToSearchSets.ForEach(x => master.AppendToSearchSets.Add(x));
                    }

                    if (discriptor.RemoveFromSets != null)
                    {
                        if (master.RemoveFromSets == null)
                            master.RemoveFromSets = new List<string>();

                        discriptor.RemoveFromSets.ForEach(x => master.RemoveFromSets.Add(x));
                    }

                    if (discriptor.AppendToSets != null)
                    {
                        if (master.AppendToSets == null)
                            master.AppendToSets = new List<string>();

                        discriptor.AppendToSets.ForEach(x => master.AppendToSets.Add(x));
                    }

                    if (discriptor.AppendToKey != null && discriptor.AppendToKey.Count > 0)
                    {
                        for (int i = 0; i < discriptor.AppendToKey.Count; i++)
                        {
                            master.KeySpace += ":" + discriptor.AppendToKey[i].ToLower();
                        }
                    }



                    if (options.SetsToAppend is not null)
                    {
                        master.AppendToSets = new List<string>();
                        foreach (var set in options.SetsToAppend)
                        {
                            master.AppendToSets.Add(set);
                        }
                        if (discriptor.AppendToSets is not null)
                        {
                            discriptor.AppendToSets.ForEach(x => master.AppendToSets.Add(x.ToLower()));
                        }

                        options.SetsToAppend = null;
                    }

                    if (discriptor.Expiration is not null)
                    {
                        master.Expiration = discriptor.Expiration;
                    }

                    if (discriptor.AsJson)
                    {
                        master.Fields.Add(new HashEntry(discriptor.Name.ToLower(), JsonSerializer.Serialize(obj)));
                        master.RediWrite = RediWriteMethods.AsString;
                    }
                    else
                    {
                        if (discriptor.Properties != null)
                        {
                            foreach (var property in discriptor.Properties)
                            {
                                if (property != null && property.PropertyInfo != null && !property.Ignore)
                                {
                                    var propValue = property.PropertyInfo.GetValue(obj);

                                    if (propValue is IList elems)
                                    {
                                        if (property.AsJson)
                                        {
                                            var nestedJson = new RediWriteResult(options.Expiration)
                                            {
                                                KeySpace = master.KeySpace + ":" + property.Name.ToLower(),
                                                ID = hash,
                                                RediWrite = RediWriteMethods.AsString
                                            };

                                            nestedJson.Fields.Add(new HashEntry(0, JsonSerializer.Serialize(elems)));

                                            if (master.Nested != null)
                                                master.Nested.Add(nestedJson);
                                            else
                                                master.Nested = new List<RediWriteResult> { nestedJson };
                                        }
                                        else
                                        {
                                            options.DeleteExisting = true;
                                            var result = Build_List(elems, master.KeySpace + ":" + property.Name.ToLower(), options);
                                            if (result != null)
                                            {
                                                if (master.Nested != null)
                                                    master.Nested.Add(result);
                                                else
                                                    master.Nested = new List<RediWriteResult> { result };
                                            }
                                        }
                                    }
                                    else if (propValue is IDictionary dicts)
                                    {
                                        if (property.AsJson)
                                        {
                                            var nestedJson = new RediWriteResult(options.Expiration)
                                            {
                                                KeySpace = master.KeySpace + ":" + property.Name.ToLower(),
                                                ID = hash,
                                                RediWrite = RediWriteMethods.AsString
                                            };

                                            nestedJson.Fields.Add(new HashEntry(0, JsonSerializer.Serialize(dicts)));

                                            if (master.Nested != null)
                                                master.Nested.Add(nestedJson);
                                            else
                                                master.Nested = new List<RediWriteResult> { nestedJson };
                                        }
                                        else
                                        {
                                            options.DeleteExisting = true;
                                            var result = Build_Dictionary(dicts, master.KeySpace + ":" + property.Name.ToLower(), options);
                                            if (result != null)
                                            {
                                                if (master.Nested != null)
                                                    master.Nested.Add(result);
                                                else
                                                    master.Nested = new List<RediWriteResult> { result };
                                            }
                                        }
                                    }
                                    else if (IsPrimitive(propValue))
                                    {
                                        var value = GetStringValue(propValue);
                                        if (value != null)
                                        {
                                            master.Fields.Add(new HashEntry(property.Name.ToLower(), value));
                                        }
                                        else
                                        {
                                            master.Fields.Add(new HashEntry(property.Name.ToLower(), ""));
                                        }
                                    }
                                    else
                                    {
                                        if (propValue != null)
                                        {
                                            if (property.AsJson)
                                            {
                                                var nest = new RediWriteResult(options.Expiration)
                                                {
                                                    RediWrite = RediWriteMethods.AsString,
                                                    DeleteExisting = true,
                                                    KeySpace = master.KeySpace + ":" + property.Name.ToLower(),
                                                    ID = property.Name.ToLower()
                                                };
                                                nest.Fields.Add(new HashEntry(property.Name.ToLower(), JsonSerializer.Serialize(propValue)));

                                                if (master.Nested != null)
                                                    master.Nested.Add(nest);
                                                else
                                                    master.Nested = new List<RediWriteResult> { nest };
                                            }
                                            else
                                            {
                                                var result = Build_Object(propValue, master.KeySpace.ToLower(), property.Name.ToLower(), options);
                                                if (result != null)
                                                {
                                                    if (master.Nested != null)
                                                        master.Nested.Add(result);
                                                    else
                                                        master.Nested = new List<RediWriteResult> { result };
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return master;
                }
            }
            return null;
        }


        private RediResult SetNested(RediWriteResult space, RediWriteOptions rediWrite)
        {
            if (string.IsNullOrEmpty(space.KeySpace))
            {
                return new RediResult { Success = false, Message = "KeySpace was not set" };
            }
            if (rediWrite.RediBatch == null)
            {
                return new RediResult { Success = false, Message = "DataBase was closed or not valid" };
            }

            if (rediWrite.SetsToRemove != null)
            {
                foreach (var set in rediWrite.SetsToRemove)
                {
                    try
                    {
                        rediWrite.RediBatch.AddAction(x => x.SetRemoveAsync(set.ToLower(), space.ID));
                    }
                    catch { }
                }
            }

            if (rediWrite.SearchableSetsToAppend != null && space.SearchScore.HasValue)
            {
                foreach (var set in rediWrite.SearchableSetsToAppend)
                {
                    try
                    {
                        rediWrite.RediBatch.AddAction(x => x.SortedSetAddAsync(set.ToLower(), space.ID, space.SearchScore.Value));
                    }
                    catch { }
                }
            }

            if (space.AppendToSearchSets != null && space.SearchScore.HasValue)
            {
                foreach (var set in space.AppendToSearchSets)
                {
                    try
                    {
                        rediWrite.RediBatch.AddAction(x => x.SortedSetAddAsync(set.ToLower(), space.ID, space.SearchScore.Value));
                    }
                    catch { }
                }
            }

            if (space.RemoveFromSets is not null)
            {
                foreach (var set in space.RemoveFromSets)
                {
                    rediWrite.RediBatch.AddAction(x => x.SetRemoveAsync(set.ToLower(), space.ID));
                }
            }
            if (space.AppendToSets is not null)
            {
                foreach (var set in space.AppendToSets)
                {
                    rediWrite.RediBatch.AddAction(x => x.SetAddAsync(set.ToLower(), space.ID));
                }
            }

            if (space.Fields.Count > 0)
            {
                if (space.RediWrite == RediWriteMethods.AsSet)
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    if (space.DeleteExisting)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyDeleteAsync(space.KeySpace));
                    }

                    var keys = space.Fields.Select(x => x.Value).ToArray(); // took 20 mm

                    rediWrite.RediBatch.AddAction(x => x.SetAddAsync(space.KeySpace, keys));

                    if (space.AppendToSets != null && space.AppendToSets.Count > 0)
                    {
                        foreach (var set in space.AppendToSets)
                        {
                            rediWrite.RediBatch.AddAction(x => x.SetAddAsync(set.ToLower(), keys.ToArray()));
                        }
                    }

                    if (space.HasExpiration)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyExpireAsync(space.KeySpace, space.Expiration));
                    }
                }
                if (space.RediWrite == RediWriteMethods.AsList)
                {
                    if (space.DeleteExisting)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyDeleteAsync(space.KeySpace));
                    }

                    var keys = space.Fields.Select(x => x.Value).ToArray();

                    rediWrite.RediBatch.AddAction(x => x.ListRightPushAsync(space.KeySpace, keys));

                    if (space.AppendToSets != null && space.AppendToSets.Count > 0)
                    {
                        foreach (var set in space.AppendToSets)
                        {
                            rediWrite.RediBatch.AddAction(x => x.SetAddAsync(set.ToLower(), keys.ToArray()));
                        }
                    }

                    if (space.HasExpiration)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyExpireAsync(space.KeySpace, space.Expiration));
                    }
                }
                else if (space.RediWrite == RediWriteMethods.AsObject)  // took 260
                {
                    if (space.DeleteExisting)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyDeleteAsync(space.KeySpace));
                    }
                    var array = space.Fields.ToArray(); // 92 mm

                    rediWrite.RediBatch.AddAction(x => x.HashSetAsync(space.KeySpace, array));

                    if (space.AppendToSets != null && space.AppendToSets.Count > 0)
                    {
                        foreach (var set in space.AppendToSets)
                        {
                            rediWrite.RediBatch.AddAction(x => x.SetAddAsync(set.ToLower(), space.ID));
                        }
                    }

                    if (space.HasExpiration)
                    {
                        rediWrite.RediBatch.AddAction(x => x.KeyExpireAsync(space.KeySpace, space.Expiration));
                    }
                }
                else if (space.RediWrite == RediWriteMethods.AsString)
                {
                    foreach (var field in space.Fields)
                    {
                        rediWrite.RediBatch.AddAction(x => x.StringSetAsync(space.KeySpace, field.Value, space.Expiration));
                    }
                }
            }
            else
            {
                if (space.DeleteExisting)
                {
                    rediWrite.RediBatch.AddAction(x => x.KeyDeleteAsync(space.KeySpace));
                }
            }


            if (space.Nested != null)
            {
                foreach (var nest in space.Nested)
                {
                    var result = SetNested(nest, rediWrite);
                    if (!result.Success)
                    {
                        Console.WriteLine(result.Message);
                    }
                }
            }

            return new RediResult { Success = true, Message = "" };
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

        internal static string? GetStringValue(object? t)
        {
            if (t != null)
            {
                if (t is Enum)
                {
                    return ((int)t).ToString();
                }
                else if (t is bool tB)
                {
                    return tB ? "1" : "0";
                }
                else if (t is DateTime dateTime)
                {
                    return ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString();
                }
                else if (t is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.ToUnixTimeSeconds().ToString();
                }
                else
                {
                    var value = t.ToString();
                    if (value != null)
                    {
                        return value;
                    }
                    else
                    {
                        return value;
                    }
                }
            }
            else
            {
                return null;
            }
        }



        internal static double? GetScoreValue(object? t)
        {
            if (t != null)
            {
                if (t is DateTime dateTime)
                {
                    return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
                }
                else if (t is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.ToUnixTimeSeconds();
                }
                else if (t is long longValue)
                {
                    return longValue;
                }
                else if (t is double doubleValue)
                {
                    return doubleValue;
                }
                else if (t is int intValue)
                {
                    return intValue;
                }
                else if (int.TryParse(t.ToString(), out var parsed))
                {
                    return parsed;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }








        public bool Add_ToSet(string key, string member)
        {
            return db.SetAdd(key, member);
        }

        public bool Add_ToSortedSet(string key, long unixTime, string member)
        {
            return db.SortedSetAdd(key, member, unixTime);
        }



    }
}
