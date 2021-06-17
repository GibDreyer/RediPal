using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IRediFactory
    {
        public IDatabase? GetDataBase();
        public int GetDataBaseIndex();
        public ConnectionMultiplexer GetRedisConnection();
        public RediBatch? CreateBatch();

        public Redipal RediPalInstance { get; }

        public ITypeDiscriptor TypeDescriptor { get; set; }
    }
}
