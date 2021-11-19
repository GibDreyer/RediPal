using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace RedipalCore
{
    public static class RediExtensions
    {
        public static long? Redi_GetObjectID(this IRediExtensions obj, string key = "")
        {
            if (Redipal.IFactory != null)
            {
                if (string.IsNullOrEmpty(key))
                {
                    var type = obj.GetType();
                    if (Attribute.IsDefined(type, typeof(RediIncrementalID)))
                    {
                        var attribute = Attribute.GetCustomAttribute(type, typeof(RediIncrementalID));
                        if (attribute != null)
                        {
                            var id = ((RediIncrementalID)attribute).SetKey;
                            if (!string.IsNullOrEmpty(id))
                            {
                                key = id;
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(key))
                {
                    return Redipal.IFactory.RediPalInstance.Read.GetIncrementedID(key);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        // Add reload methods
        /// <summary>
        /// Reloads the object from the given ID
        /// </summary>
        public static bool Redi_LoadAs<T>(this T obj, ref T outObj, string id) where T : IRediExtensions
        {
            if (Redipal.IFactory != null)
            {
                var result = Redipal.IFactory.RediPalInstance.Read.Object<T>(id);

                if (result is not null)
                {
                    outObj = result;
                    return true;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
            return false;
        }

        /// <summary>
        /// Returns a new instance from the ID given
        /// </summary>
        public static T? Redi_CreateNewAs<T>(this T obj, string id) where T : IRediExtensions
        {
            if (Redipal.IFactory != null)
            {
                return Redipal.IFactory.RediPalInstance.Read.Object<T>(id);
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// Deletes the object and all its children permanently
        /// </summary>
        public static bool Redi_Eradicate<T>(this T obj) where T : IRediExtensions
        {
            if (Redipal.IFactory != null && obj is RediBase rediBase && rediBase.Redi_WriteName is not null)
            {
                return Redipal.IFactory.RediPalInstance.Eradicate.Object<T>(rediBase.Redi_WriteName);
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// returns the most recent version of the current object
        /// </summary>
        public static T? Redi_Reload<T>(this T obj) where T : IRediExtensions
        {
            if (Redipal.IFactory != null && obj is RediBase rediBase && rediBase.Redi_WriteName is not null)
            {
                return Redipal.IFactory.RediPalInstance.Read.Object<T>(rediBase.Redi_WriteName);
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// Reloads the object to the most recent version
        /// </summary>
        public static bool Redi_Reload<T>(this T obj, ref T? objOut) where T : IRediExtensions
        {
            if (Redipal.IFactory != null && obj is RediBase rediBase && rediBase.Redi_WriteName is not null)
            {
                var result = Redipal.IFactory.RediPalInstance.Read.Object<T>(rediBase.Redi_WriteName);
                if (result is not null)
                {
                    objOut = result;
                    return true;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
            return false;
        }

        /// <summary>
        /// returns a subscription to the current object
        /// </summary>
        public static IRediSubscription<T>? Redi_Subscribe<T>(this T obj) where T : IRediExtensions
        {
            if (Redipal.IFactory != null && obj is RediBase rediBase && rediBase.Redi_WriteName is not null)
            {
                if (Redipal.IFactory.TypeDescriptor.TryGetDescriptor(typeof(T), out var proccessor) && proccessor.KeySpace is not null)
                {
                    return Redipal.IFactory.RediPalInstance.Subscribe.ToObject<T>(proccessor.KeySpace, rediBase.Redi_WriteName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// returns a subscription to the property of the object
        /// </summary>
        public static IRediSubscription<P>? Redi_Subscribe<T, P>(this T obj, Expression<Func<T, P>> property) where T : IRediExtensions where P : notnull
        {
            if (Redipal.IFactory != null && obj is RediBase rediBase && rediBase.Redi_WriteName is not null)
            {
                if (Redipal.IFactory.TypeDescriptor.TryGetDescriptor(typeof(T), out var proccessor) && proccessor.KeySpace is not null)
                {
                    return Redipal.IFactory.RediPalInstance.Subscribe.ToProperty(rediBase.Redi_WriteName, property, new Objects.RediSubscriberOptions());
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// Writes the object 
        /// </summary>
        public static bool Redi_Write<T>(this T obj) where T : IRediExtensions
        {
            if (Redipal.IFactory != null)
            {
                var result = Redipal.IFactory.RediPalInstance.Write.Object(obj);
                if (result is not null)
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
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        /// <summary>
        /// Writes only the property given 
        /// </summary>
        public static bool Redi_Write<T, P>(this T obj, Expression<Func<T, P>> property) where T : IRediExtensions where P : notnull
        {
            if (Redipal.IFactory != null && Redipal.IFactory.TypeDescriptor.TryGetDescriptor(typeof(T), out var discriptor))
            {
                if (!string.IsNullOrEmpty(discriptor.KeySpace) || discriptor.DisableKeySpace)
                {
                    if (discriptor.WriteNameProperty is not null || !string.IsNullOrEmpty(discriptor.WriteName))
                    {
                        var nameValue = discriptor.WriteNameProperty?.GetValue(obj) ?? discriptor.WriteName;
                        if (nameValue is not null && RediWriter.IsPrimitive(nameValue))
                        {
                            var redisName = RediWriter.GetStringValue(nameValue);
                            if (!string.IsNullOrEmpty(redisName))
                            {
                                var body = property.Body.ToString();
                                var key = (!discriptor.DisableKeySpace ? discriptor.KeySpace + ":" : "") + redisName + body.Replace(body.Split(".").First() + ".", ":").Replace(".", ":").ToLower();

                                var propName = key.Split(":").LastOrDefault();

                                var compile = property.Compile();
                                if (compile != null && propName != null)
                                {
                                    var value = compile.Invoke(obj);
                                    if (value != null)
                                    {
                                        if (value is IConvertible convertible)
                                        {
                                            var appendToKey = "";

                                            if (discriptor.AppendToKey != null)
                                            {
                                                for (int i = 0; i < discriptor.AppendToKey.Count; i++)
                                                {
                                                    var last = key.Split(":").LastOrDefault();
                                                    var keyToAppend = discriptor.AppendToKey[i].ToLower();
                                                    if (last is not null && last.ToLower() != keyToAppend && key != keyToAppend)
                                                    {
                                                        appendToKey += ":" + keyToAppend;
                                                    }
                                                }
                                            }


                                            key = key.Replace(":" + propName, "");
                                            var result = Redipal.IFactory.RediPalInstance.Write.Field(convertible, key + appendToKey, propName);
                                            if (result is not null)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else if (value is IDictionary dictionary)
                                        {
                                            var result = Redipal.IFactory.RediPalInstance.Write.Dictionary(dictionary, new Pluralize.NET.Pluralizer().Pluralize(key));
                                            if (result is not null)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else if (value is IList list)
                                        {
                                            var result = Redipal.IFactory.RediPalInstance.Write.List(list, new Pluralize.NET.Pluralizer().Pluralize(key));
                                            if (result is not null)
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
                                            key = key.Replace(":" + propName, "");
                                            var result = Redipal.IFactory.RediPalInstance.Write.Object(value, x => x.KeySpace = key, x => x.ID = propName);
                                            if (result is not null)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
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
                    throw new ArgumentException("To use the read method without supplying a keyspace you must set the 'RediKeySpace('')' Attribute on the object class");
                }
            }
            return false;
        }

        public static bool Redi_Write<T, P>(this T obj, Action<T> action, Expression<Func<T, P>> property) where T : IRediExtensions where P : notnull
        {
            action(obj);
            return obj.Redi_Write(property);
        }

        public static bool Redi_WriteAs<T, P, X>(this X inst, string id, Action<T> action, Expression<Func<T, P>> property) where X : IRediExtensions where T : IRediExtensions where P : notnull
        {
            var instance = RediReader.CreateInstance(typeof(T));

            if (instance is RediBase extensions)
            {
                extensions.Redi_WriteName = id;
                if (extensions is T obj)
                {
                    action(obj);
                    return obj.Redi_Write(property);
                }
            }
            return false;
        }
    }
}
