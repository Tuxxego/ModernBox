using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace TuxModLoader.Reflection
{
    public static class ReflectionHelper
    {
        private const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static readonly ConcurrentDictionary<(Type, string), MethodInfo> MethodCache = new ConcurrentDictionary<(Type, string), MethodInfo>();
        private static readonly ConcurrentDictionary<(Type, string), FieldInfo> FieldCache = new ConcurrentDictionary<(Type, string), FieldInfo>();
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> PropertyCache = new ConcurrentDictionary<(Type, string), PropertyInfo>();
        private static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();

        private static MethodInfo GetCachedMethod(Type type, string methodName)
        {
            return MethodCache.GetOrAdd((type, methodName), k => k.Item1.GetMethod(k.Item2, AllFlags));
        }

        private static FieldInfo GetCachedField(Type type, string fieldName)
        {
            return FieldCache.GetOrAdd((type, fieldName), k => k.Item1.GetField(k.Item2, AllFlags));
        }

        private static PropertyInfo GetCachedProperty(Type type, string propertyName)
        {
            return PropertyCache.GetOrAdd((type, propertyName), k => k.Item1.GetProperty(k.Item2, AllFlags));
        }

        private static Type GetCachedType(string typeName)
        {
            return TypeCache.GetOrAdd(typeName, Type.GetType);
        }

        public static object InvokeMethod(object target, string methodName, params object[] parameters)
        {
            Type type = target.GetType();
            MethodInfo method = GetCachedMethod(type, methodName);
            if (method == null)
            {
                throw new MissingMethodException(type.Name, methodName);
            }
            return method.Invoke(target, parameters);
        }

        public static T GetFieldValue<T>(object target, string fieldName)
        {
            Type type = target.GetType();
            FieldInfo field = GetCachedField(type, fieldName);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            return (T)field.GetValue(target);
        }

        public static T GetStaticFieldValue<T>(Type type, string fieldName)
        {
            FieldInfo field = GetCachedField(type, fieldName);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            return (T)field.GetValue(null);
        }

        public static void SetFieldValue(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            FieldInfo field = GetCachedField(type, fieldName);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            field.SetValue(target, value);
        }

        public static void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            FieldInfo field = GetCachedField(type, fieldName);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            field.SetValue(null, value);
        }

        public static T GetPropertyValue<T>(object target, string propertyName)
        {
            Type type = target.GetType();
            PropertyInfo property = GetCachedProperty(type, propertyName);
            if (property == null)
            {
                throw new MissingMemberException(type.Name, propertyName);
            }
            return (T)property.GetValue(target);
        }

        public static void SetPropertyValue(object target, string propertyName, object value)
        {
            Type type = target.GetType();
            PropertyInfo property = GetCachedProperty(type, propertyName);
            if (property == null)
            {
                throw new MissingMemberException(type.Name, propertyName);
            }
            property.SetValue(target, value);
        }

        public static T CreateInstance<T>(string typeName)
        {
            Type type = GetCachedType(typeName);
            if (type == null)
            {
                throw new TypeLoadException($"Could not load type: {typeName}");
            }
            return (T)Activator.CreateInstance(type);
        }

        public static object InvokeStaticMethod(string typeName, string methodName, params object[] parameters)
        {
            Type type = GetCachedType(typeName);
            if (type == null)
            {
                throw new TypeLoadException($"Could not load type: {typeName}");
            }
            MethodInfo method = GetCachedMethod(type, methodName);
            if (method == null)
            {
                throw new MissingMethodException(typeName, methodName);
            }
            return method.Invoke(null, parameters);
        }
    }
}
