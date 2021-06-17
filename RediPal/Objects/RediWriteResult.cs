using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    public class RediWriteResult
    {
        public RediWriteResult(TimeSpan? expiration)
        {
            Expiration = expiration;
        }

        public string ID { get; set; } = "";
        public string KeySpace { get; set; } = "";
        public List<HashEntry> Fields { get; set; } = new List<HashEntry>();
        public RediWriteMethods RediWrite { get; set; }
        public bool HasExpiration => Expiration.HasValue;
        public TimeSpan? Expiration { get; set; }
        public List<string>? AppendToSets { get; set; }
        public List<string>? RemoveFromSets { get; set; }
        public bool DeleteExisting { get; set; }
        public List<RediWriteResult>? Nested { get; set; }
        //public RediSearch? RediSearch { get; set; }

        internal PropertyInfo? SearchScoreProperty { get; set; }
        public List<string>? AppendToSearchSets { get; set; }
        public double? SearchScore { get; set; } = null;
        // public string? SearchKey { get; set; }
        //  internal RediHoldType SearchHoldType { get; set; }
    }

    public enum RediWriteMethods
    {
        AsObject,
        AsSet,
        AsList,
        AsString
    }
}
