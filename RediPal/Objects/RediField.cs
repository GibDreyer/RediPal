using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Objects
{
    public class RediField
    {
        public RediField()
        {
        }

        public RediField(string key, params HashEntry[] fields)
        {
            Key = key;
            Fields = fields;
        }

        public string? Key { get; set; }
        public HashEntry[]? Fields { get; set; }
    }
}
