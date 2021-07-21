using RedipalCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.TestObjects
{
    public class GeoLocation2
    {
        public GeoLocation2()
        {
            Name = "";
        }
        public GeoLocation2(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public bool Enabled { get; set; } = true;

        public Dictionary<string, GeoLocation2>? Children { get; set; }
    }
}
