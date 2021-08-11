using RedipalCore;
using RedipalCore.Attributes;
using RedipalCore.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RediPal.Messages
{
    [RediExpire(seconds: 45)]
    [RediKeySpace("status-message")]
    public abstract class RediMessageBase : RediBase
    {
        protected RediMessageBase(string templateName)
        {
            TemplateName = "template:" + templateName;
        }

        internal protected RediMessageBase(string templateName, string[] values)
        {
            TemplateName = "template:" + templateName;
            Params = values;
        }

        internal string TemplateName { get; private set; }

        [RediWriteAsJson]
        internal string[] Params { get; set; } = Array.Empty<string>();

        internal void Compile()
        {
            if (Params.Length == 0)
            {
                var p = new List<(ushort index, string id)>();
                var type = GetType();
                foreach (var item in type.GetProperties())
                {
                    if (Attribute.IsDefined(item, typeof(RediMessageIndex), true))
                    {
                        object? v = item.GetValue(this);
                        if (v != null)
                        {
                            string? value = v.ToString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                Attribute? attribute = Attribute.GetCustomAttribute(item, typeof(RediMessageIndex), true);
                                if (attribute is RediMessageIndex a)
                                {
                                    p.Add((a.Index, value));
                                }
                            }
                        }
                    }
                }
                Params = p.Count > 0 ? p.OrderBy(x => x.index).Select(x => x.id).ToArray() : Array.Empty<string>();
            }
        }
    }

    public class RediMessage : RediMessageBase
    {
        public RediMessage(string templateName, params string[] values) : base(templateName, values) { }

        [RediIgnore]
        public new string[] Parameters { get => Params; set => Params = value; }
    }
}
