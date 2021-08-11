using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RedipalCore
{
    [RediIgnore]
    public class Redipal
    {
        // Fields
        private readonly IDatabase db;
        private readonly ConnectionMultiplexer redis;
        internal static Dictionary<Type, List<Delegate>> TypeDefaults2 = new();
        internal static Dictionary<Type, dynamic> TypeDefaults = new();
        internal readonly RediConfig Options;
        // Constructors 

        /// <summary>
        /// Uses an existing redis connection
        /// </summary>
        public Redipal(ConnectionMultiplexer redis, RediConfig? options = null)
        {

            if (options == null)
                options = new RediConfig();

            Options = options;
            SetTaskPool();

            db = redis.GetDatabase();
            if (db == null)
            {
                throw new Exception("Unable to create database");
            }

            IFactory = new RediFactory(db, redis, this);

            Eradicate = new RediEradicater(db, IFactory);
            Read = new RediReader(db, IFactory.TypeDescriptor);
            //ReadAsync = new ReaderAsync(db, options, IFactory.TypeDescriptor);
            Subscribe = new RediSubscriber(redis, db, Read, IFactory);
            Write = new RediWriter(db, IFactory);
            this.redis = redis;
            Search = new RediSearch(db, IFactory.TypeDescriptor, Read);
        }

        /// <summary>
        /// Creates a connection to the redis server internally
        /// </summary>
        public Redipal(string endpoint, RediConfig? options = null, int dataBaseID = -1)
        {

            if (options == null)
                options = new RediConfig();

            Options = options;

            SetTaskPool();

            redis = ConnectionMultiplexer.Connect(endpoint);

            if (redis.IsConnected)
            {
                db = redis.GetDatabase(dataBaseID);
            }
            else
            {
                throw new Exception(redis.GetStatus());
            }

            IFactory = new RediFactory(db, redis, this);

            Eradicate = new RediEradicater(db, IFactory);
            Read = new RediReader(db, IFactory.TypeDescriptor);
            //   ReadAsync = new ReaderAsync(db, options, IFactory.TypeDescriptor);
            Subscribe = new RediSubscriber(redis, db, Read, IFactory);
            Write = new RediWriter(db, IFactory);
            Search = new RediSearch(db, IFactory.TypeDescriptor, Read);
        }

        /// <summary>
        /// Creates a connection to the redis server internally with the given configuration
        /// </summary>
        public Redipal(ConfigurationOptions redisOptions, RediConfig? options = null)
        {
            if (options == null)
                options = new RediConfig();
            Options = options;

            SetTaskPool();

            redis = ConnectionMultiplexer.Connect(redisOptions);
            if (redis.IsConnected)
            {
                db = redis.GetDatabase();
            }
            else
            {
                throw new Exception(redis.GetStatus());
            }

            IFactory = new RediFactory(db, redis, this);

            Read = new RediReader(db, IFactory.TypeDescriptor);
            Eradicate = new RediEradicater(db, IFactory);
            //ReadAsync = new ReaderAsync(db, options, IFactory.TypeDescriptor);
            Subscribe = new RediSubscriber(redis, db, Read, IFactory);
            Write = new RediWriter(db, IFactory);
            Search = new RediSearch(db, IFactory.TypeDescriptor, Read);
        }



        public static void ISetTypeDefaults<T>(RediTypeDefaults<T> rediDefaults) where T : notnull
        {
            if (IFactory != null && IFactory.TypeDescriptor.TryGetDescriptor(typeof(T), out var proccessor, false))
            {
                if (rediDefaults.Redi_WriteName is not null)
                    proccessor.WriteNameProperty = rediDefaults.Redi_WriteName;
                if (rediDefaults.KeySpace is not null)
                    proccessor.keySpace = rediDefaults.KeySpace;
                if (rediDefaults.Redi_Conditionals is not null)
                    proccessor.Conditionals = rediDefaults.Redi_Conditionals;
                if (rediDefaults.Redi_Modifier is not null)
                    proccessor.Modifiers = rediDefaults.Redi_Modifier;
                if (rediDefaults.Redi_SearchScore is not null)
                    proccessor.ScoreProperty = rediDefaults.Redi_SearchScore;
                if (rediDefaults.DefaultSet is not null)
                    proccessor.defaultSet = rediDefaults.DefaultSet;
                if (rediDefaults.Expiration is not null)
                    proccessor.expiration = rediDefaults.Expiration;
                if (rediDefaults.WriteAsJson is not null)
                    proccessor.asJson = rediDefaults.WriteAsJson;
                if (rediDefaults.Ignore is not null)
                    proccessor.ignore = rediDefaults.Ignore;

                if (rediDefaults.Redi_ParameterModifier is not null && proccessor.Properties is not null)
                {
                    foreach (var modifier in rediDefaults.Redi_ParameterModifier)
                    {
                        var property = proccessor.Properties.FirstOrDefault(x => x.PropertyInfo is not null && x.PropertyInfo.Name == modifier.Item1.Name);
                        if (property is not null)
                        {
                            if (proccessor.ParameterModifiers is null)
                                proccessor.ParameterModifiers = new();

                            proccessor.ParameterModifiers.Add((property, modifier.Item2));
                        }
                    }
                }

                var type = typeof(T);
                if (TypeDefaults.ContainsKey(type))
                {
                    TypeDefaults[type] = rediDefaults;
                }
                else
                {
                    TypeDefaults.Add(type, rediDefaults);
                }
            }
        }

        public static RediTypeDefaults<T>? IGetTypeDefaults<T>(T obj) where T : notnull
        {
            if (TypeDefaults.TryGetValue(obj.GetType(), out var value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public static RediTypeDefaults<T>? IGetTypeDefaults<T>()
        {
            if (TypeDefaults.TryGetValue(typeof(T), out var value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public static bool ISetTypeDefaults<T>(Action<RediTypeDefaults<T>> action) where T : notnull
        {
            var defaults = IGetTypeDefaults<T>();

            if (defaults is not null)
            {
                action.Invoke(defaults);

                return true;
            }
            else
            {
                var typeDefaults = new RediTypeDefaults<T>();
                action.Invoke(typeDefaults);

                ISetTypeDefaults(typeDefaults);
                return true;
            }
        }



        public bool SetTypeDefaults<T>(Action<RediTypeDefaults<T>> action) where T : notnull
        {
            return ISetTypeDefaults(action);
        }

        public void SetTypeDefaults<T>(RediTypeDefaults<T> rediDefaults) where T : notnull
        {
            ISetTypeDefaults(rediDefaults);
        }

        public RediTypeDefaults<T>? GetTypeDefaults<T>(T obj) where T : notnull
        {
            return IGetTypeDefaults(obj);
        }

        public RediTypeDefaults<T>? GetTypeDefaults<T>() where T : notnull
        {
            return IGetTypeDefaults<T>();
        }



        // Calls

        public static IRediFactory? IFactory { get; set; }
        public IRediFactory? Factory => IFactory;
        public IRediSubscriber Subscribe { get; set; }
        public IRediWriter Write { get; set; }
        public IRediReader Read { get; set; }
        public IRediSearch Search { get; set; }
        public IRediEradicater Eradicate { get; set; }

        // private IReaderAsync ReadAsync { get; set; }

        public event Action<RediError, string>? OnError;

        internal void InvokeError(RediError type, string message)
        {
            if (OnError is not null)
            {
                OnError.Invoke(type, message);
            }
        }


        public RediBatch CreateBatch()
        {
            return new RediBatch(db.CreateBatch());
        }

        private void SetTaskPool()
        {
            if (Options.UnThrottleCPU)
            {
                ThreadPool.GetMaxThreads(out int worker, out int threads);
                ThreadPool.SetMaxThreads(worker * 3, threads * 3);
                ThreadPool.SetMinThreads(250, 250);
            }
        }
    }

    public enum RediError
    {
        Unknown,
        UnableToWrite,
        UnableToRead,
    }
}
