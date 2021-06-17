using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IReaderAsync
    {

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public Task<T?> Object<T>(string key, string hash) where T : notnull;

        public Task<T?> Object<T>(string key) where T : notnull;


        // List 

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public Task<List<T>?> List<T>(string setKey) where T : notnull;

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public Task<List<T>?> List<T>(string key, params string[] hashes) where T : notnull;



        // Dictionary

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public  Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(string setKey) where TKey : IConvertible;

        /// <summary>
        /// Creates a list of the given object for the key space for each of the specified fields. If no Objects can be created null will be returned
        /// </summary>
        public Task<Dictionary<TKey, TValue>?> Dictionary<TKey, TValue>(string key, params string[] hashes) where TKey : IConvertible;
    }
}
