using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.Interfaces
{
    public interface IRediEradicater
    {
        public bool Field(string key, string field);
        public bool Key(string key);
        public bool Object<T>(string id);
       // public bool List<T>(string key);
       // public bool Dictionary<T>(string key);

        public bool Remove_FromSet(string key, string member);
    }
}
