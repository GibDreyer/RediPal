using RediPal.Messages;
using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace RedipalCore
{
    internal class RediDescriptor : ITypeDiscriptor
    {
        private readonly Pluralize.NET.Pluralizer Pluralizer = new();

        public static ConcurrentDictionary<Type, RediTypeProccessor> Descriptors { get; } = new ConcurrentDictionary<Type, RediTypeProccessor>();


        private bool TryGetDescriptor(Type type, out RediTypeProccessor typeProccessor, TimeSpan? timeSpan)
        {
            var proccessor = TryGetDescriptor(type, timeSpan);
            if (proccessor != null)
            {
                proccessor.RestoreDefaults();
                typeProccessor = proccessor;
                return true;
            }
            else
            {
                typeProccessor = new RediTypeProccessor();
                return false;
            }
        }

        public bool TryGetDescriptor(Type type, out RediTypeProccessor typeProccessor, bool asClone = true)
        {
            var proccessor = TryGetDescriptor(type);
            if (proccessor != null)
            {
                proccessor.RestoreDefaults();
                if (asClone)
                {
                    typeProccessor = (RediTypeProccessor)proccessor.Clone();
                }
                else
                {
                    typeProccessor = proccessor;
                }
                return true;
            }
            else
            {
                typeProccessor = new RediTypeProccessor();
                return false;
            }
        }

        public RediTypeProccessor? TryGetDescriptor(Type type, TimeSpan? timeSpan)
        {
            if (Descriptors.TryGetValue(type, out var value))
            {
                value.RestoreDefaults();
                return value;
            }
            else
            {
                return TypeProccesssor(type, timeSpan);
            }
        }

        public RediTypeProccessor? TryGetDescriptor(Type type)
        {
            if (Descriptors.TryGetValue(type, out var value))
            {
                value.RestoreDefaults();
                return value;
            }
            else
            {
                return TypeProccesssor(type);
            }
        }

        public List<string> GetKeys(Type type, string name, string keySpace = "")
        {
            return TryGetDescriptor(type, out RediTypeProccessor? typeProccessor) && (!string.IsNullOrEmpty(typeProccessor.KeySpace) || !string.IsNullOrEmpty(keySpace) || typeProccessor.DisableKeySpace)
                ? Get_Keys(type, typeProccessor.DisableKeySpace ? "" : (typeProccessor.KeySpace ?? keySpace + ":") + name)
                : throw new ArgumentException("No keySpace was given or was applied to the object");
        }

        private List<string> Get_Keys(Type type, string name)
        {
            List<string> keys = new();

            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();

                keys.Add(name);

                if (TryGetDescriptor(type, out RediTypeProccessor? typeProccessor) && typeProccessor.Properties != null)
                {
                    if (!type.IsArray && !type.IsGenericType)
                    {
                        foreach (RediType? subType in typeProccessor.Properties.Where(x => !x.IsPrimitive))
                        {
                            if (subType.PropertyType != null)
                            {
                                keys.AddRange(Get_Keys(subType.PropertyType, name + ":" + subType.Name.ToLower()));
                            }
                        }
                    }
                    else if (Redipal.IFactory is not null)
                    {
                        var listType = type.GetGenericArguments()[0];
                        if (listType is not null && !RediReader.IsPrimitive(listType))
                        {
                            var db = Redipal.IFactory.GetDataBase();
                            if (db is not null)
                            {
                                RedisValue[]? members = null;
                                try
                                {
                                    members = db.SetMembers(name);
                                }
                                catch
                                {
                                    try
                                    {
                                        members = db.SortedSetRangeByScore(name);
                                    }
                                    catch
                                    {
                                        members = db.ListRange(name);
                                    }
                                }

                                if (members is not null)
                                {
                                    foreach (RedisValue member in members)
                                    {
                                        keys.AddRange(Get_Keys(listType, Pluralizer.Singularize(name) + ":" + member));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return keys;
        }

        private RediTypeProccessor? TypeProccesssor(Type objType, TimeSpan? experation = null)
        {
            if (Descriptors.TryGetValue(objType, out var value))
            {
                return value;
            }

            if (objType != null)
            {
                var rediTypeProccessor = new RediTypeProccessor
                {
                    PropertyType = objType
                };

                Descriptors.TryAdd(objType, rediTypeProccessor);


                var inheretExpiration = false;

                if (Attribute.IsDefined(objType, typeof(RediIgnore)))
                {
                    rediTypeProccessor.Ignore = true;
                }
                else
                {
                    if (!IsPrimitive(objType))
                    {
                        if (Attribute.IsDefined(objType, typeof(RediExpire), true))
                        {
                            var attribute = Attribute.GetCustomAttribute(objType, typeof(RediExpire), true);
                            if (attribute != null)
                            {
                                rediTypeProccessor.Expiration = ((RediExpire)attribute).Time;
                                if (((RediExpire)attribute).Inherent)
                                {
                                    inheretExpiration = true;
                                }
                            }
                        }
                        else
                        {
                            rediTypeProccessor.Expiration = experation;
                        }

                        if (Attribute.IsDefined(objType, typeof(RediKeySpace), true))
                        {
                            if (Attribute.GetCustomAttribute(objType, typeof(RediKeySpace), true) is RediKeySpace attribute)
                            {
                                rediTypeProccessor.KeySpace = attribute.KeySpace;

                                if (attribute.DisableKeySpace)
                                {
                                    rediTypeProccessor.DisableKeySpace = true;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(attribute.KeySpace))
                                    {
                                        throw new ArgumentException("No Key space was specified. A key space must be given or disable set to true") { Source = objType.Name };
                                    }
                                    else
                                    {
                                        if (attribute.Append.Length > 0)
                                        {
                                            rediTypeProccessor.AppendToKey = attribute.Append.ToList();
                                        }
                                    }
                                }
                            }
                        }

                        if (Attribute.IsDefined(objType, typeof(RediSearchSet), true))
                        {
                            var attribute = Attribute.GetCustomAttribute(objType, typeof(RediSearchSet), true);
                            if (attribute != null)
                            {
                                if (rediTypeProccessor.Append_ToSearchSets is null)
                                {
                                    rediTypeProccessor.Append_ToSearchSets = new List<string>();
                                }

                                rediTypeProccessor.Append_ToSearchSets.Add(((RediSearchSet)attribute).Set);
                                //rediTypeProccessor.RediSearch = new Objects.RediSearch(((RediSearchSet)attribute).Set)
                                //{
                                //    HoldType = ((RediSearchSet)attribute).HoldType
                                //};
                            }
                        }

                        if (Attribute.IsDefined(objType, typeof(RediDefaultSet), true))
                        {
                            var attribute = Attribute.GetCustomAttribute(objType, typeof(RediDefaultSet), true);
                            if (attribute != null)
                            {
                                rediTypeProccessor.DefaultSet = ((RediDefaultSet)attribute).SetKey;
                            }
                        }

                        if (Attribute.IsDefined(objType, typeof(RediReName)))
                        {
                            var attribute = Attribute.GetCustomAttribute(objType, typeof(RediReName));
                            if (attribute != null)
                            {
                                rediTypeProccessor.Name = ((RediReName)attribute).Name;
                            }
                        }

                        if (Attribute.IsDefined(objType, typeof(RediDefaultID)))
                        {
                            if (Attribute.GetCustomAttribute(objType, typeof(RediDefaultID)) is RediDefaultID attribute)
                            {
                                rediTypeProccessor.WriteName = attribute.ID.ToLower();
                            }
                        }

                        if (Attribute.IsDefined(objType, typeof(RediWriteAsJson)))
                        {
                            rediTypeProccessor.AsJson = true;
                        }
                        else
                        {
                            IEnumerable<PropertyInfo>? properties = null;

                            if (objType.BaseType != null && (objType.BaseType == typeof(RediBase)))
                            {
                                properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                                        .Where(x => x.PropertyType != typeof(PropertyInfo));
                            }
                            else if (objType.BaseType != null && objType.BaseType == typeof(RediMessageBase))
                            {
                                properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                                                       .Where(x => x.PropertyType != typeof(PropertyInfo) && x.DeclaringType == typeof(RediMessageBase));
                            }
                            else
                            {
                                properties = objType.GetProperties()
                                                   .Where(x => x.PropertyType != typeof(PropertyInfo));
                            }


                            if (properties is not null)
                            {
                                foreach (PropertyInfo? property in properties)
                                {
                                    if (property is not null)
                                    {
                                        if (IsPrimitive(property.PropertyType))
                                        {
                                            RediType rediType = new()
                                            {
                                                PropertyType = property.PropertyType,
                                                PropertyInfo = property,
                                                IsPrimitive = true,
                                                CanSet = property.CanWrite,
                                                IsList = objType.IsGenericType
                                            };

                                            if (Attribute.IsDefined(property, typeof(RediWriteAsImage), true))
                                            {
                                                var attribute = Attribute.GetCustomAttribute(property, typeof(RediWriteAsImage), true);
                                                if (attribute is RediWriteAsImage att)
                                                {
                                                    rediType.ImageFormat = att.ImageFormat;
                                                    rediType.CompressionLevel = att.CompressionLevel;
                                                }
                                            }

                                            if (Attribute.IsDefined(property, typeof(RediSearchScore)))
                                            {
                                                rediTypeProccessor.ScoreProperty = property;
                                            }

                                            if (Attribute.IsDefined(property, typeof(RediWriteAsJson)))
                                            {
                                                rediType.AsJson = true;
                                            }

                                            if (Attribute.IsDefined(property, typeof(RediWriteName)))
                                            {
                                                rediTypeProccessor.WriteNameProperty = property;
                                            }
                                            else if (rediTypeProccessor.WriteNameProperty == null && property.Name == nameof(RediBase.Redi_WriteName))
                                            {
                                                rediTypeProccessor.WriteNameProperty = property;
                                                rediType.Ignore = true;
                                            }

                                            if (Attribute.IsDefined(property, typeof(RediIgnore)))
                                            {
                                                rediType.Ignore = true;
                                            }
                                            else
                                            {
                                                if (Attribute.IsDefined(property, typeof(RediReName)))
                                                {
                                                    var attribute = Attribute.GetCustomAttribute(property, typeof(RediReName));
                                                    if (attribute != null)
                                                    {
                                                        rediType.Name = ((RediReName)attribute).Name;
                                                    }
                                                }
                                                else
                                                {
                                                    rediType.Name = property.Name;
                                                }
                                            }
                                            if (rediTypeProccessor.Properties is null)
                                                rediTypeProccessor.Properties = new() { rediType };
                                            else
                                                rediTypeProccessor.Properties.Add(rediType);
                                        }
                                        else
                                        {
                                            var rediType = new RediType
                                            {
                                                PropertyType = property.PropertyType,
                                                PropertyInfo = property,
                                                CanSet = property.CanWrite,
                                            };

                                            if (Attribute.IsDefined(property, typeof(RediIgnore)))
                                            {
                                                rediType.Ignore = true;
                                            }
                                            else
                                            {
                                                if (Attribute.IsDefined(property, typeof(RediWriteAsJson)))
                                                {
                                                    rediType.AsJson = true;
                                                }

                                                if (Attribute.IsDefined(property, typeof(RediReName)))
                                                {
                                                    var attribute = Attribute.GetCustomAttribute(property, typeof(RediReName));
                                                    if (attribute != null)
                                                    {
                                                        rediType.Name = ((RediReName)attribute).Name;
                                                    }
                                                }
                                                else
                                                {
                                                    rediType.Name = property.Name;
                                                }

                                                if (property.PropertyType != objType)
                                                {
                                                    if (inheretExpiration)
                                                    {
                                                        if (TryGetDescriptor(property.PropertyType, out RediTypeProccessor? subProccesor, rediTypeProccessor.Expiration))
                                                        {
                                                            if (subProccesor.AsJson)
                                                            {
                                                                rediType.AsJson = true;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (TryGetDescriptor(property.PropertyType, out var subProccesor))
                                                        {
                                                            if (subProccesor.AsJson)
                                                            {
                                                                rediType.AsJson = true;
                                                            }
                                                        }
                                                    }
                                                    TypeProccesssor(property.PropertyType);
                                                }

                                                if (rediTypeProccessor.SubTypes is null)
                                                {
                                                    rediTypeProccessor.SubTypes = new() { property.PropertyType };
                                                }
                                                else
                                                {
                                                    rediTypeProccessor.SubTypes.Add(property.PropertyType);
                                                }

                                                if (rediTypeProccessor.Properties is null)
                                                {
                                                    rediTypeProccessor.Properties = new() { rediType };
                                                }
                                                else
                                                {
                                                    rediTypeProccessor.Properties.Add(rediType);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        RediType rediType = new()
                        {
                            PropertyType = objType,
                            IsPrimitive = true,
                            IsList = objType.IsGenericType,
                            CanSet = true
                        };

                        if (Attribute.IsDefined(objType, typeof(RediIgnore)))
                        {
                            rediType.Ignore = true;
                        }
                        else
                        {
                            if (Attribute.IsDefined(objType, typeof(RediWriteAsJson)))
                            {
                                rediType.AsJson = true;
                            }

                            if (Attribute.IsDefined(objType, typeof(RediReName)))
                            {
                                var attribute = Attribute.GetCustomAttribute(objType, typeof(RediReName));
                                if (attribute != null)
                                {
                                    rediType.Name = ((RediReName)attribute).Name;
                                }
                            }
                        }
                        if (rediTypeProccessor.Properties == null)
                            rediTypeProccessor.Properties = new() { rediType };
                        else
                            rediTypeProccessor.Properties.Add(rediType);
                    }
                }

                if (!Descriptors.ContainsKey(objType))
                {
                    Descriptors.TryAdd(objType, rediTypeProccessor);
                }

                return rediTypeProccessor;
            }
            return null;
        }

        private static bool IsPrimitive(object? obj)
        {
            if (obj is not null)
            {
                Type? type;

                if (obj is Type t)
                    type = t;
                else if (obj is PropertyInfo property)
                    type = property.PropertyType;
                //else if (obj is IList l)
                //    type = l.GetType().GenericTypeArguments[0];
                else
                    type = obj.GetType();

                if (type.IsPrimitive
                    || type.IsValueType
                    || type == typeof(string)
                    || type == typeof(decimal)
                    || type == typeof(Bitmap)
                    || type == typeof(DateTime)
                    || type == typeof(TimeSpan)
                    || type == typeof(DateTimeOffset)
                    || type == typeof(Guid))
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
    }
}
