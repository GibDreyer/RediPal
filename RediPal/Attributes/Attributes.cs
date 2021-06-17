using RedipalCore.Objects;
using System;
using System.Collections;

namespace RedipalCore.Attributes
{
    /// <summary>
    /// When set on a property it will NOT be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class RediIgnore : Attribute
    {
    }



    /// <summary>
    /// Will tell the serializer to send the list to redis as json
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [PropertyType(typeof(IList), typeof(IDictionary))]
    public class RediWriteAsJson : Attribute
    {
    }



    /// <summary>
    /// Expires the Object After the specified amount of time. This applies to every nested object under the applied class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class RediExpire : Attribute
    {
        public TimeSpan Time { get; private set; }
        public bool Inherent { get; private set; }

        public RediExpire(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, bool childrenInherent = true)
        {
            Time = new TimeSpan(days, hours, minutes, seconds);
            Inherent = childrenInherent;
        }
    }


    /// <summary>
    /// Sets the default KeySpace that the object will be wrote to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RediKeySpace : Attribute
    {
        public string KeySpace { get; private set; }
        public string[] Append { get; private set; }

        public RediKeySpace(string keySpace, params string[] append)
        {
            KeySpace = keySpace.ToLower();
            Append = append;
        }
    }




    /// <summary>
    /// Sets the default KeySpace that the object will be wrote to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RediSearchSet : Attribute
    {
        public string Set { get; private set; }
        public RediHoldType HoldType { get; private set; }

        public RediSearchSet(string set, RediHoldType holdType = RediHoldType.SortedSet)
        {
            Set = set.ToLower();
            HoldType = holdType;
        }
    }



    /// <summary>
    /// Sets the default KeySpace that the object will be wrote to
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RediSearchScore : Attribute
    {
    }




    /// <summary>
    /// Sets the default KeySpace that the object will be wrote to
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RediIncrementalID : Attribute
    {
        public string SetKey { get; private set; }

        public RediIncrementalID(string key)
        {
            SetKey = key;
        }
    }

  
    /// <summary>
    /// Will Automatically add to this set when wrote
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RediDefaultSet : Attribute
    {
        public string SetKey { get; private set; }

        public RediDefaultSet(string set)
        {
            SetKey = set;
        }
    }


    /// <summary>
    /// Sets the property as the name of the object when writen or read
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RediWriteName : Attribute
    {  }


    /// <summary>
    /// Used to override the Field that is used when saving the data to redis
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RediReName : Attribute
    {
        public string Name { get; private set; }

        public RediReName(string name)
        {
            Name = name;
        }
    }


    /// <summary>
    /// allows an attribute to be aplied to a specific type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class PropertyType : Attribute
    {
        public Type[] Types { get; private set; }

        public PropertyType(params Type[] types)
        {
            Types = types;
        }
    }
}
