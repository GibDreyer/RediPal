using RedipalCore.Objects;
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;

namespace RedipalCore.Attributes
{
    /// <summary>
    /// When set on a property it will NOT be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class RediIgnore : Attribute { }



    /// <summary>
    /// Will tell the serializer to send the list to redis as json
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [PropertyType(typeof(IList), typeof(IDictionary))]
    public class RediWriteAsJson : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    [PropertyType(typeof(Bitmap))]
    public class RediWriteAsImage : Attribute
    {
        public ImageFormat ImageFormat { get; private set; }
        public long CompressionLevel { get; private set; }

        public RediWriteAsImage(Redi_ImageFormat imageFormat, int compressionLevel = 100)
        {
            CompressionLevel = Math.Min(100, Math.Max(compressionLevel, 0));

            ImageFormat = imageFormat switch
            {
                Redi_ImageFormat.Bmp => ImageFormat.Bmp,
                Redi_ImageFormat.Emf => ImageFormat.Emf,
                Redi_ImageFormat.Exif => ImageFormat.Exif,
                Redi_ImageFormat.Gif => ImageFormat.Gif,
                Redi_ImageFormat.Icon => ImageFormat.Icon,
                Redi_ImageFormat.Jpeg => ImageFormat.Jpeg,
                Redi_ImageFormat.MemoryBmp => ImageFormat.MemoryBmp,
                Redi_ImageFormat.Png => ImageFormat.Png,
                Redi_ImageFormat.Tiff => ImageFormat.Tiff,
                Redi_ImageFormat.Wmf => ImageFormat.Wmf,
                _ => ImageFormat.Png,
            };
        }
    }

    public enum Redi_ImageFormat
    {
        Bmp,
        Emf,
        Exif,
        Gif,
        Icon,
        Jpeg,
        MemoryBmp,
        Png,
        Tiff,
        Wmf
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class RediLayer : Attribute
    {
        public string[] Layers { get; private set; }

        public RediLayer(params string[] layers)
        {
            Layers = layers;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RediMessageIndex : Attribute
    {
        internal ushort Index { get; private set; }

        public RediMessageIndex(ushort index)
        {
            Index = index;
        }
    }



    /// <summary>
    /// Expires the Object After the specified amount of time. This applies to every nested object under the applied class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class RediDefaultSet : Attribute
    {
        public string SetKey { get; private set; }

        public RediDefaultSet(string set)
        {
            SetKey = set;
        }
    }

    /// <summary>
    /// Will Automatically write and read the Object with the given ID. This should not be used if unless the instance of the class or struct is singular
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class RediDefaultID : Attribute
    {
        public string ID { get; private set; }

        public RediDefaultID(string set)
        {
            ID = set;
        }
    }


    /// <summary>
    /// Sets the property as the name of the object when written or read
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RediWriteName : Attribute
    { }


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
    /// allows an attribute to be applied to a specific type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class PropertyType : Attribute
    {
        public Type[] Types { get; private set; }

        public PropertyType(params Type[] types)
        {
            Types = types;
        }
    }
}
