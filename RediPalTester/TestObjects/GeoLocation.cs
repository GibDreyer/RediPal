using RedipalCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedipalCore.TestObjects
{
    public class GeoLocation
    {
        public GeoLocation()
        {
            Name = "";
        }
        public GeoLocation(string name)
        {
            Name = name;
        }

        public string Name { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public double? Latitude { get; set; } = null;
        public double? Longitude { get; set; } = null;

        public GeoLocationType Type { get; set; } = GeoLocationType.Unset;

        public Dictionary<string, GeoLocation>? Items { get; set; }

        [RediIgnore]
        public GeoLocation this[string key]
        {
            get => Items[key];
        }
    }

    public enum GeoLocationType
    {
        Unset,
        Storage,
        Operator,
        Warehouse,
        Equipment,
        Transport,
        Facility
    }
}
