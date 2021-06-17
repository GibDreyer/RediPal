using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IRediWriter
    {
        // Value

        /// <summary>
        /// Will write the given object and save it to the specified key space. If an IBatch is given it will add the database calls to que and Batch.ExucuteAll must to called to save the data.
        /// </summary>
        public RediWriteResult? Value(IConvertible value, string key, RediWriteOptions? writeOptions = null);
        public RediWriteResult? Value(IConvertible value, string key, params Action<RediWriteOptions>[] writeOptions);

        public RediWriteResult? Field(IConvertible value, string key, string field, RediWriteOptions? writeOptions = null); 
        public RediWriteResult? Field(IConvertible value, string key, string field, params Action<RediWriteOptions>[] writeOptions);


        //public RediWriteResult? Property(IConvertible value, string key, string field, RediWriteOptions? writeOptions = null);


        public RediWriteResult? Fields(RediField rediField, RediWriteOptions? writeOptions = null); 
        public RediWriteResult? Fields(RediField rediField, params Action<RediWriteOptions>[] writeOptions); 

        public RediWriteResult? Fields(RediField[] rediFields, params Action<RediWriteOptions>[] writeOptions);
        public RediWriteResult? Fields(RediField[] rediFields, RediWriteOptions? writeOptions = null);



        // List

        ///// <summary>
        ///// Will write the given object and save it to the specified key space. If an IBatch is given it will add the database calls to que and Batch.ExucuteAll must to called to save the data.
        ///// </summary>
        //public RediBuildResult? Set(IList list, string setKey, string valueKey, RediWriteOptions? writeOptions = null);

        /// <summary>
        /// This will add the objects in the list to
        /// </summary>
        /// Parameters:
        ///   list:
        ///     The list to be wrote.
        ///   setKey:
        ///     The key of the set where the index of the values will be wrote.
        public RediWriteResult? List(IList list, params Action<RediWriteOptions>[] writeOptions);
        public RediWriteResult? List(IList list, string setKey, RediWriteOptions? writeOptions = null);
        public RediWriteResult? List(IList list, string setKey, params Action<RediWriteOptions>[] writeOptions);


        // Object

        /// <summary>
        /// Will write the given object and save it to the specified key space. If an IBatch is given it will add the database calls to que and Batch.ExucuteAll must to called to save the data.
        /// </summary>
        public RediWriteResult? Object<T>(T obj, RediWriteOptions? writeOptions = null) where T : notnull;
        public RediWriteResult? Object<T>(T obj, params Action<RediWriteOptions>[] writeOptions) where T : notnull;



        //Dictionary

        /// <summary>
        /// Write a dictionary to redis. The keys will be added the the set given and the value will be hashed
        /// </summary>
        public RediWriteResult? Dictionary(IDictionary dictionary, params Action<RediWriteOptions>[] writeOptions);
        public RediWriteResult? Dictionary(IDictionary dictionary, string setKeySpace, RediWriteOptions? writeOptions = null);
        public RediWriteResult? Dictionary(IDictionary dictionary, string setKeySpace, params Action<RediWriteOptions>[] writeOptions);

        public RediWriteResult? Property<T, P>(T obj, Expression<Func<T, P>> property) where T : notnull where P : notnull;

        public bool Add_ToSortedSet(string key, long unixTime, string member);
        public bool Add_ToSet(string key, string member);
    }
}
