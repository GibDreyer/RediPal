﻿using RedipalCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RedipalCore.Objects
{
    public class RediTypeDefaults<T>
    {
        public void AppendToSet(string set)
        {
            if (Redi_AppendToSets == null)
            {
                Redi_AppendToSets = new List<string> { set };
            }
            else
            {
                Redi_AppendToSets.Add(set);
            }
        }
        public void RemoveFromSet(string set)
        {
            if (Redi_RemoveFromSets == null)
            {
                Redi_RemoveFromSets = new List<string> { set };
            }
            else
            {
                Redi_RemoveFromSets.Add(set);
            }
        }

        public void SetWriteNameProperty<P>(Expression<Func<T, P>> propertyName) where P : IConvertible
        {
            var varCheck = propertyName.Body.ToString();
            if (varCheck != null && varCheck.Split(".").Length > 2)
            {
                throw new ArgumentException("The given value must be on the parent object");
            }
            else
            {
                if (propertyName.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
                {
                    Redi_WriteName = propertyInfo;
                }
            }
        }
        public void SetSearchScoreProperty(Expression<Func<T, DateTime>> propertyName)
        {
            var varCheck = propertyName.Body.ToString();
            if (varCheck != null && varCheck.Split(".").Length > 2)
            {
                throw new ArgumentException("The given value must be on the parent object");
            }
            else
            {
                if (propertyName.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
                {
                    Redi_SearchScore = propertyInfo;
                }
            }
        }


        //public void IncrementKey(string key, Expression<Func<T, int>> propertyName)
        //{
        //    var varCheck = propertyName.Body.ToString();
        //    if (varCheck != null && varCheck.Split(".").Length > 2)
        //    {
        //        throw new ArgumentException("The given value must be on the parent object");
        //    }
        //    else
        //    {
        //        if (propertyName.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
        //        {
        //            if (IncrementKeysByProperty is null)
        //            {
        //                IncrementKeysByProperty = new List<(string, PropertyInfo)>();
        //            }
        //            IncrementKeysByProperty.Add((key, propertyInfo));
        //        }
        //    }
        //}
        //public void IncrementKey(string key, int amount = 1)
        //{
        //    if (IncrementKeysByAmount is null)
        //    {
        //        IncrementKeysByAmount = new List<(string, int)>();
        //    }
        //    IncrementKeysByAmount.Add((key, amount));
        //}

        //public void DecrementKey(string key, Expression<Func<T, int>> propertyName)
        //{
        //    var varCheck = propertyName.Body.ToString();
        //    if (varCheck != null && varCheck.Split(".").Length > 2)
        //    {
        //        throw new ArgumentException("The given value must be on the parent object");
        //    }
        //    else
        //    {
        //        if (propertyName.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
        //        {
        //            if (DecrementKeysByProperty is null)
        //            {
        //                DecrementKeysByProperty = new List<(string, PropertyInfo)>();
        //            }
        //            DecrementKeysByProperty.Add((key, propertyInfo));
        //        }
        //    }
        //}
        //public void DecrementKey(string key, int amount = 1)
        //{
        //    if (DecrementKeysByAmount is null)
        //    {
        //        DecrementKeysByAmount = new List<(string, int)>();
        //    }
        //    DecrementKeysByAmount.Add((key, amount));
        //}


        public void AddConditional(Func<T, bool> condition, Action<IRediTypeProccessor> action, params Action<IRediTypeProccessor>[] actions)
        {
            var acts = new Action<IRediTypeProccessor>[actions.Length + 1];
            acts[0] = action;

            actions.CopyTo(acts, 1);


            if (Redi_Conditionals == null)
                Redi_Conditionals = new List<RediConditional>();

            Redi_Conditionals.Add(new RediConditional
            {
                Condition = condition,
                Actions = acts
            });
        }
        public void AddModifier(Action<T, IRediTypeProccessor> action)
        {
            if (Redi_Modifier == null)
                Redi_Modifier = new List<Delegate>();

            Redi_Modifier.Add(action);
        }

        public void AddParameterModifier<P>(Expression<Func<T, P>> property, Action<T, IRediType> action)
        {
            if (property.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                if (Redi_ParameterModifier == null)
                    Redi_ParameterModifier = new List<(PropertyInfo, Delegate)>();

                Redi_ParameterModifier.Add((propertyInfo, action));
            }
        }


        public string? KeySpace { get; set; } = null;
        public string? SearchSet { get; set; } = null;
        public string? DefaultID { get; set; } = null;
        public string? DefaultSet { get; set; } = null;
        public TimeSpan? Expiration { get; set; } = null;
        public bool? Ignore { get; set; } = null;
        public bool? WriteAsJson { get; set; } = null;


        internal List<(string, PropertyInfo)>? IncrementKeysByProperty { get; set; }
        internal List<(string, int)>? IncrementKeysByAmount { get; set; }
        internal List<(string, PropertyInfo)>? DecrementKeysByProperty { get; set; }
        internal List<(string, int)>? DecrementKeysByAmount { get; set; }

        internal List<RediConditional>? Redi_Conditionals { get; set; }
        internal List<(PropertyInfo, Delegate)>? Redi_ParameterModifier { get; set; }
        internal List<Delegate>? Redi_Modifier { get; set; }
        internal string? Redi_RediWriteName { get; set; }
        internal List<string>? Redi_RemoveFromSets { get; set; }
        internal List<string>? Redi_AppendToSets { get; set; }
        internal PropertyInfo? Redi_SearchScore { get; set; }
        internal PropertyInfo? Redi_WriteName { get; set; }
        internal List<PropertyInfo>? Redi_PropertiesToIgnore { get; set; }
    }
}
