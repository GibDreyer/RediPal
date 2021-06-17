using System;
using System.Collections.Generic;
using System.Linq;

namespace RedipalCore.Objects
{
    public class RediReadOptions
    {
        internal List<string>? Append_PreKey { get; set; }
        internal List<string>? Append_PreID { get; set; }
        internal List<string>? Append_PreSet { get; set; }

        internal List<string>? Append_PostKey { get; set; }
        internal List<string>? Append_PostID { get; set; }
        internal List<string>? Append_PostSet { get; set; }

        public bool DeleteUnfound { get; set; } = false;

        public IProgress<int>? Progress { get; set; }

        public void AppendPreKey(string key, bool addColon = true)
        {
            if (Append_PreKey is null)
                Append_PreKey = new List<string>();

            if (addColon)
            {
                Append_PreKey.Add(key + ":");
            }
            else
            {
                Append_PreKey.Add(key);
            }
        }

        public void AppendPreID(string key, bool addColon = true)
        {
            if (Append_PreID is null)
                Append_PreID = new List<string>();

            if (addColon)
            {
                Append_PreID.Add( key + ":");
            }
            else
            {
                Append_PreID.Add(key);
            }
        }

        public void AppendPreSet(string key, bool addColon = true)
        {
            if (Append_PreSet is null)
                Append_PreSet = new List<string>();

            if (addColon)
            {
                Append_PreSet.Add(key + ":");
            }
            else
            {
                Append_PreSet.Add(key);
            }
        }

        public void AppendPostKey(string key, bool addColon = true)
        {
            if (Append_PostKey is null)
                Append_PostKey = new List<string>();

            if (addColon)
            {
                Append_PostKey.Add(":" + key);
            }
            else
            {
                Append_PostKey.Add(key);
            }
        }

        public void AppendPostID(string key, bool addColon = true)
        {
            if (Append_PostID is null)
                Append_PostID = new List<string>();

            if (addColon)
            {
                Append_PostID.Add(":" + key);
            }
            else
            {
                Append_PostID.Add(key);
            }
        }

        public void AppendPostSet(string key, bool addColon = true)
        {
            if (Append_PostSet is null)
                Append_PostSet = new List<string>();

            if (addColon)
            {
                Append_PostSet.Add(":" + key);
            }
            else
            {
                Append_PostSet.Add(key);
            }
        }
    }
}
