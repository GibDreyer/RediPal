using System.Drawing.Imaging;
using System;

namespace RedipalCore.Interfaces
{
    public interface IRediTypeProccessor
    {
        public string? DefaultSet { get; set; }
        public string? DefaultID { get; set; }
        public string? KeySpace { get; set; }
        public bool DisableKeySpace { get; set; }
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
    
    public interface IRediType
    {
        public bool Ignore { get; set; }
        public bool AsJson { get; set; }
        public TimeSpan? Expiration { get; set; }

        public ImageFormat? ImageFormat { get; set; }
        public long? CompressionLevel { get; set; }

        public void AppendToSet(string set, bool isSearchable = false);
        public void RemoveFromSet(string set);

        public void IncrementKey(string key);

        public void DecrementKey(string key);

        public void DeleteKey(string key);
    }
}
