using RedipalCore.Objects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RedipalCore.Interfaces
{
    public interface IRediReader
    {
        // Checks

        public bool SetContains(string keySpace, string id);

        /// <summary>
        ///     Increments the number stored at key by increment. If the key does not exist,
        ///     it is set to 0 before performing the operation. An error is returned if the key
        ///     contains a value of the wrong type or contains a string that is not representable
        ///     as integer. This operation is limited to 64 bit signed integers.
        /// </summary>
        public long GetIncrementedID(string key);

        // Object 

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public T? Object<T>(string keySpace, string id) where T : notnull;

        public T? Object<T>(string id) where T : notnull;



        // Value 

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified field
        /// </summary>
        public T? Field<T>(string keySpace, string field) where T : notnull;

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified field
        /// </summary>
        public T? Value<T>(string keySpace) where T : notnull;



        // Property 

        public P? Property<T, P>(string key, Expression<Func<T, P>> property) where T : notnull where P : notnull;



        // List 

        /// <summary>
        /// Creates a list of the given object for each of members in the set specified on the object classes atribute "RediSet".
        /// </summary>
        public List<T>? List<T>() where T : notnull;
        public List<P>? List<T, P>(Expression<Func<T, P>> property) where T : notnull where P : notnull;
        public P? List<T, P>(Expression<Func<T, P>> property, Func<P, P, P> action) where T : notnull where P : IConvertible;
        public bool List<T, P>(Expression<Func<T, P>> property, Action<P, P> action) where T : notnull where P : IConvertible;

        /// <summary>
        /// Creates a List of the given object for each of members in the specified set using the default keyspace.
        /// </summary>
        public List<T>? List<T>(string fromSet) where T : notnull;
        public List<P>? List<T, P>(string fromSet, Expression<Func<T, P>> property) where T : notnull where P : notnull;
        public P? List<T, P>(string fromSet, Expression<Func<T, P>> property, Func<P, P, P> action) where T : notnull where P : IConvertible;
        public bool List<T, P>(string fromSet, Expression<Func<T, P>> property, Action<P, P> action) where T : notnull where P : IConvertible;

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified ids or set.
        /// </summary>
        public List<T>? List<T>(string keySpace, params string[] hashIDs) where T : notnull;
        public List<T>? List<T>(string keySpace, string fromSet) where T : notnull;





        // Dictionary

        /// <summary>
        /// Creates a Dictionary of the given object for each of members in the set specified on the object classes atribute "RediSet".
        /// </summary>
        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(RediReadOptions? readOptions = default) where TKey : IConvertible;
        /// <summary>
        /// Creates a Dictionary of the given object for each of members in the specified set using the default keyspace.
        /// </summary>
        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string fromSet, RediReadOptions? readOptions = default) where TKey : IConvertible;

        /// <summary>
        /// Creates a Dictionary of the given object for the key space for each of the specified ids or set. 
        /// </summary>
        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string[] hashIDs, RediReadOptions? readOptions = default) where TKey : IConvertible;
        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string keySpace, string[] hashIDs, RediReadOptions? readOptions = default) where TKey : IConvertible;
        public Dictionary<TKey, TValue>? Dictionary<TKey, TValue>(string keySpace, string fromSet, RediReadOptions? readOptions = default) where TKey : IConvertible;
    }
}
