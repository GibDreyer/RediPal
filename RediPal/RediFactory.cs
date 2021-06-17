using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;

namespace RedipalCore
{
    internal class RediFactory : IRediFactory
    {
        public IDatabase? db;
        private readonly ConnectionMultiplexer redis;

        internal RediFactory(IDatabase dbo, ConnectionMultiplexer redis, Redipal redipal)
        {
            db = dbo;
            this.redis = redis;
            TypeDescriptor = new RediDescriptor();
            RediPalInstance = redipal;
        }

        public Redipal RediPalInstance { get; }


        public ITypeDiscriptor TypeDescriptor { get; set; }




        public IDatabase? GetDataBase()
        {
            return db;
        }

        public int GetDataBaseIndex()
        {
            if (db != null)
            {
                return db.Database;
            }
            else
            {
                return default;
            }
        }

        public ConnectionMultiplexer GetRedisConnection()
        {
            return redis;
        }

        public RediBatch? CreateBatch()
        {
            if (db is not null)
                return new RediBatch(db.CreateBatch());
            else
                return null;
        }
    }
}
