using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IRediType
    {
        public string? DefaultSet { get; set; }
        public string? KeySpace { get; set; }
        public TimeSpan? Expiration { get; set; }
        public bool Ignore { get; set; }
        public bool AsJson { get; set; }

        public void AppendPostID(string id, bool addColon = true);
        public void AppendPreID(string id, bool addColon = true);
        public void AppendToSet(string set, bool isSearchable = false);
        public void RemoveFromSet(string set);

        public void IncrementKey(string key);



        public void DecrementKey(string key);
        
        public void DeleteKey(string key);
    }
}
