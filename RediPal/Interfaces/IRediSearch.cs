using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IRediSearch
    {
        public Dictionary<TKey, TValue>? AsDictionary<TKey, TValue>(DateTime start, DateTime? end = null) where TKey : IConvertible where TValue : notnull;
        public Dictionary<TKey, TValue>? AsDictionary<TKey, TValue>(string fromSet, DateTime start, DateTime? end = null) where TKey : IConvertible where TValue : notnull;
    }
}
