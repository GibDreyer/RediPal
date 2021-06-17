using Pluralize.NET;
using RedipalCore.Interfaces;
using RedipalCore.Objects;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore
{
    internal class RediEradicater : IRediEradicater
    {
        private readonly IDatabase db;
        private readonly IRediFactory Factory;

        internal RediEradicater(IDatabase db, IRediFactory factory)
        {
            this.db = db;
            Factory = factory;
        }


        public bool Remove_FromSet(string key, string member)
        {
            try
            {
                return db.SetRemove(key, member);
            }
            catch
            {
                return false;
            }
        }

        public bool Field(string key, string field)
        {
            try
            {
                return db.HashDelete(key, field);
            }
            catch
            {
                return false;
            }
        }

        public bool Key(string key)
        {
            try
            {
                return db.KeyDelete(key);
            }
            catch
            {
                return false;
            }
        }

        public bool Object<T>(string id)
        {
            try
            {
                var keys = Factory.TypeDescriptor.GetKeys(typeof(T), id);

                var batch = Factory.CreateBatch();
                if (batch is not null)
                {
                    foreach (var key in keys)
                    {
                        batch.AddAction(x => x.KeyDeleteAsync(key));
                    }

                    return batch.Execute();
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool List<T>(string key)
        {
            throw new NotImplementedException();
        }

        public bool Dictionary<T>(string key)
        {
            try
            {
                RedisValue[]? members = null;
                try
                {
                    members = db.SetMembers(key);
                }
                catch
                {
                    try
                    {
                        members = db.SortedSetRangeByScore(key);
                    }
                    catch
                    {
                        members = db.ListRange(key);
                    }
                }

                if (members is not null)
                {
                    var batch = Factory.CreateBatch();
                    if (batch is not null)
                    {
                        foreach (var member in members)
                        {
                            var keys = Factory.TypeDescriptor.GetKeys(typeof(T), member);

                            foreach (var id in keys)
                            {
                                batch.AddAction(x => x.KeyDeleteAsync(id));
                            }
                        }

                        return batch.Execute();
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
            catch
            {
                return false;
            }
        }
    }
}
