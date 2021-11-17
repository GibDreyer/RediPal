using RedipalCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;

namespace RedipalCore.Objects
{
    public class RediTypeProccessor : IRediTypeProccessor, ICloneable
    {
        public RediTypeProccessor()
        {

        }

        protected RediTypeProccessor(RediTypeProccessor another)
        {
            var current = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var other in another.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var value = other.GetValue(another);
                if (value is not null)
                {
                    var otherProp = current.FirstOrDefault(x => x.Name == other.Name);
                    if (otherProp is not null && otherProp.CanWrite)
                    {
                        if (value is IList list)
                        {
                            var array = new object[list.Count];
                            list.CopyTo(array, 0);

                            var newValue = otherProp.GetValue(this);
                            if (newValue is null)
                            {
                                newValue = Activator.CreateInstance(otherProp.PropertyType);
                            }

                            if (newValue is IList newList)
                            {
                                foreach (var item in array)
                                {
                                    newList.Add(item);
                                }
                                otherProp.SetValue(this, newList);
                            }
                        }
                        else
                        {
                            otherProp.SetValue(this, value);
                        }
                    }
                }
            };
        }

        internal string? defaultSet = null;
        internal string? keySpace = null;
        internal TimeSpan? expiration = null;
        internal bool? ignore = null;
        internal bool? asJson = null;




        public string? DefaultSet { get; set; }
        public string? KeySpace { get; set; }
        public TimeSpan? Expiration { get; set; }
        public bool Ignore { get; set; }
        public bool AsJson { get; set; }

        internal double? Score { get; set; } = null;
        internal string WriteName { get; set; } = string.Empty;
        internal string Name { get; set; } = string.Empty;
        internal PropertyInfo? WriteNameProperty { get; set; }
        internal PropertyInfo? ScoreProperty { get; set; }
        internal List<string>? AppendToKey { get; set; }
        internal Type? PropertyType { get; set; }
        internal List<RediConditional>? Conditionals { get; set; }
        internal List<Delegate>? Modifiers { get; set; }
        internal List<(RediType, Delegate)>? ParameterModifiers { get; set; }

        internal List<Type>? SubTypes { get; set; }
        internal List<RediType>? Properties { get; set; }

        internal List<string>? RemoveFromSets { get; set; }
        internal List<string>? AppendToSets { get; set; }
        internal List<string>? Append_ToSearchSets { get; set; }
        internal List<string>? Append_PostID { get; set; }
        internal List<string>? Append_PreID { get; set; }

        internal void RunConditionals(object obj)
        {
            if (Conditionals is not null)
            {
                foreach (var conditional in Conditionals)
                {
                    if (conditional.Condition != null && conditional.Actions is not null)
                    {
                        if (conditional.Condition.DynamicInvoke(obj) is bool met && met)
                        {
                            foreach (var action in conditional.Actions)
                            {
                                action.DynamicInvoke(this);
                            }
                        }
                    }
                }
            }
            if (Modifiers is not null)
            {
                foreach (var modifier in Modifiers)
                {
                    if (modifier != null)
                    {
                        try
                        {
                            modifier.DynamicInvoke(obj, this);
                        }
                        catch { }
                    }
                }
            }

            if (ParameterModifiers is not null)
            {
                foreach (var modifiers in ParameterModifiers)
                {
                    modifiers.Item2.DynamicInvoke(obj, modifiers.Item1);
                }
            }
        }

        internal void RestoreDefaults()
        {
            if (RemoveFromSets is not null)
                RemoveFromSets.Clear();
            if (AppendToSets is not null)
                AppendToSets.Clear();
            if (Append_PostID is not null)
                Append_PostID.Clear();

            if (Append_PreID is not null)
                Append_PreID.Clear();
            if (Append_ToSearchSets is not null)
                Append_ToSearchSets.Clear();

            if (defaultSet is not null)
                DefaultSet = defaultSet;
            else
                defaultSet = DefaultSet;

            if (keySpace is not null)
                KeySpace = keySpace;
            else
                keySpace = KeySpace;

            if (expiration is not null)
                Expiration = expiration;
            else
                expiration = Expiration;

            if (ignore.HasValue)
                Ignore = ignore.Value;
            else
                ignore = Ignore;

            if (asJson.HasValue)
                AsJson = asJson.Value;
            else
                asJson = AsJson;
        }

        public void AppendToSet(string set, bool isSearchable = false)
        {
            if (isSearchable)
            {
                if (Append_ToSearchSets == null)
                {
                    Append_ToSearchSets = new List<string> { set };
                }
                else
                {
                    if (!Append_ToSearchSets.Contains(set))
                    {
                        Append_ToSearchSets.Add(set);
                    }
                }
            }
            else
            {
                if (AppendToSets == null)
                {
                    AppendToSets = new List<string> { set };
                }
                else
                {
                    if (!AppendToSets.Contains(set))
                    {
                        AppendToSets.Add(set);
                    }
                }
            }
        }
        public void RemoveFromSet(string set)
        {
            if (RemoveFromSets == null)
            {
                RemoveFromSets = new List<string> { set };
            }
            else
            {
                if (!RemoveFromSets.Contains(set))
                {
                    RemoveFromSets.Add(set);
                }
            }
        }

        public void AppendPostID(string id, bool addColon = true)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (addColon)
                    id = ":" + id;

                if (Append_PostID == null)
                {
                    Append_PostID = new List<string> { id };
                }
                else
                {
                    if (!Append_PostID.Contains(id))
                    {
                        Append_PostID.Add(id);
                    }
                }
            }
        }

        public void AppendPreID(string id, bool addColon = true)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (addColon)
                    id += ":";

                if (Append_PreID == null)
                {
                    Append_PreID = new List<string> { id };
                }
                else
                {
                    if (!Append_PreID.Contains(id))
                    {
                        Append_PreID.Add(id);
                    }
                }
            }
        }


        public void IncrementKey(string key)
        {
            //TODO
        }
        public void DecrementKey(string key)
        {
            //TODO
        }
        public void DeleteKey(string key)
        {
            //TODO
        }

        public object Clone()
        {
            return new RediTypeProccessor(this);
        }
    }

    public class RediType : IRediType
    {
        internal string? defaultSet = null;
        internal string? keySpace = null;
        internal TimeSpan? expiration = null;
        internal bool? ignore = null;
        internal bool? asJson = null;
        internal List<(PropertyInfo, Delegate)>? ParameterModifiers { get; set; }

        public Type? PropertyType { get; set; }
        public PropertyInfo? PropertyInfo { get; set; }

        public bool HasFieldOveride => !string.IsNullOrEmpty(Name);
        public string Name { get; set; } = "";

        public bool Ignore { get; set; }
        public bool CanSet { get; set; }
        public bool AsJson { get; set; }
        public bool IsList { get; set; }
        public bool IsPrimitive { get; set; }
        public TimeSpan? Expiration { get; set; }

        public ImageFormat? ImageFormat { get; set; } = null;
        public long? CompressionLevel { get; set; } = null;

        internal List<string>? RemoveFromSets { get; set; }
        internal List<string>? AppendToSets { get; set; }
        internal List<string>? Append_ToSearchSets { get; set; }

        public void IncrementKey(string key)
        {
            //TODO
        }
        public void DecrementKey(string key)
        {
            //TODO
        }
        public void DeleteKey(string key)
        {
            //TODO
        }

        public void AppendToSet(string set, bool isSearchable = false)
        {
            if (isSearchable)
            {
                if (Append_ToSearchSets == null)
                {
                    Append_ToSearchSets = new List<string> { set };
                }
                else
                {
                    if (!Append_ToSearchSets.Contains(set))
                    {
                        Append_ToSearchSets.Add(set);
                    }
                }
            }
            else
            {
                if (AppendToSets == null)
                {
                    AppendToSets = new List<string> { set };
                }
                else
                {
                    if (!AppendToSets.Contains(set))
                    {
                        AppendToSets.Add(set);
                    }
                }
            }
        }
        public void RemoveFromSet(string set)
        {
            if (RemoveFromSets == null)
            {
                RemoveFromSets = new List<string> { set };
            }
            else
            {
                if (!RemoveFromSets.Contains(set))
                {
                    RemoveFromSets.Add(set);
                }
            }
        }

        internal void RestoreDefaults()
        {
            if (RemoveFromSets is not null)
                RemoveFromSets.Clear();
            if (AppendToSets is not null)
                AppendToSets.Clear();

            if (expiration is not null)
                Expiration = expiration;
            else
                expiration = Expiration;

            if (ignore.HasValue)
                Ignore = ignore.Value;
            else
                ignore = Ignore;

            if (asJson.HasValue)
                AsJson = asJson.Value;
            else
                asJson = AsJson;
        }
    }


    public class RediSearch
    {
        public RediSearch(string key)
        {
            Key = key;
        }

        public PropertyInfo? ScoreProperty { get; set; }
        public double? Score { get; set; } = null;
        public string Key { get; set; }
        public RediHoldType HoldType { get; set; }
    }

    public enum RediHoldType
    {
        SortedSet,
        Set,
        List
    }
}
