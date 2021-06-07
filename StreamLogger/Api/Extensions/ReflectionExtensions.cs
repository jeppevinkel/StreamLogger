﻿using System;
using System.Linq;
using System.Reflection;

namespace StreamLogger.Api.Extensions
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a static method.
        /// </summary>
        /// <param name="type">The method type.</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="param">The method parameters.</param>
        public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;

            type.GetMethod(methodName, flags)?.Invoke(null, param);
        }

        /// <summary>
        /// Copy all properties from the source class to the target one.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="source">The source object to copy properties from.</param>
        public static void CopyProperties(this object target, object source)
        {
            Type type = target.GetType();

            if (type != source.GetType())
                throw new Exception("Target and source type mismatch!");

            foreach (var sourceProperty in type.GetProperties())
                type.GetProperty(sourceProperty.Name)?.SetValue(target, sourceProperty.GetValue(source, null), null);
        }
        
        public static bool TrySetProperty<TValue>(this object obj, string propertyName, TValue value)
        {
            var property = obj.GetType()
                .GetProperties()
                .Where(p => p.CanWrite && p.PropertyType == typeof(TValue))
                .FirstOrDefault(p => p.Name == propertyName);

            if (property == null)
            {
                return false;
            }

            property.SetValue(obj, value);
            return true;
        }

        public static bool TryGetPropertyValue<TProperty>(this object obj, string propertyName, out TProperty value)
        {
            var property = obj.GetType()
                .GetProperties()
                .Where(p => p.CanRead && p.PropertyType == typeof(TProperty))
                .FirstOrDefault(p => p.Name == propertyName);

            if (property == null)
            {
                value = default(TProperty);
                return false;
            }

            value = (TProperty) property.GetValue(obj);
            return true;
        }
    }
}