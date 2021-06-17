using RedipalCore.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RedipalCore.Interfaces
{
    public interface ITypeDiscriptor
    {
        public static ConcurrentDictionary<Type, RediTypeProccessor>? Descriptors { get; }

        public bool TryGetDescriptor(Type type, out RediTypeProccessor typeProccessor, bool asClone = true);

        public RediTypeProccessor? TryGetDescriptor(Type type);

        public List<string> GetKeys(Type type, string name, string keySpace = "");
    }
}
