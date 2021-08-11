using RedipalCore.Attributes;
using RedipalCore.Interfaces;
using System;
using System.Linq.Expressions;

namespace RedipalCore
{
    public abstract class RediBase : IRediExtensions
    {
        //public virtual string Redi_DefaultLayer { get; set; } = "";



        public static long? Redi_GetObjectID(string key)
        {
            if (Redipal.IFactory != null)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    return Redipal.IFactory.RediPalInstance.Read.GetIncrementedID(key);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        public static T? Redi_LoadAs<T>(string id) where T : IRediExtensions
        {
            if (Redipal.IFactory != null && Redipal.IFactory.TypeDescriptor.TryGetDescriptor(typeof(T), out var proccessor) && proccessor.KeySpace is not null)
            {
                return Redipal.IFactory.RediPalInstance.Read.Object<T>(id);
            }
            else
            {
                throw new ArgumentException("An Instance of ReiPal must be Initialized before using IRediExtensions");
            }
        }

        public static bool Redi_Write<T, P>(string id, Action<T> action, Expression<Func<T, P>> property) where T : IRediExtensions where P : notnull
        {
            var instance = RediReader.CreateInstance(typeof(T));

            if (instance is RediBase extensions)
            {
                extensions.Redi_WriteName = id;
                if (extensions is T obj)
                {
                    action(obj);
                    return obj.Redi_Write(property);
                }
            }
            return false;
        }

        public bool Redi_Write(string id)
        {
            this.Redi_WriteName = id;
            return this.Redi_Write();
        }

        public void FirePropertyChanged<T>(T obj, Expression<Func<T, IConvertible>> propertySelector)
        {
            Console.WriteLine(propertySelector.Body + "  Changed   " + propertySelector.Compile()(obj));
            //PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        [RediIgnore]
        internal string? Redi_WriteName { get; set; }
    }
}
