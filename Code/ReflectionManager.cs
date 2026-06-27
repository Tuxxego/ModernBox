using System;
using System.Reflection;

namespace TuxModLoader.Reflection
{
    public static class ReflectionHelper
    {
        public static object InvokeMethod(object target, string methodName, params object[] parameters)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
            {
                throw new MissingMethodException(target.GetType().Name, methodName);
            }
            return method.Invoke(target, parameters);
        }

        public static T GetFieldValue<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }
            return (T)field.GetValue(target);
        }
        public static T GetStaticFieldValue<T>(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            return (T)field.GetValue(null);
        }

        public static void SetFieldValue(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field == null)
            {
                throw new MissingFieldException(target.GetType().Name, fieldName);
            }
            field.SetValue(target, value);
        }
        public static void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }
            field.SetValue(null, value);
        }

        public static T GetPropertyValue<T>(object target, string propertyName)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (property == null)
            {
                throw new MissingMemberException(target.GetType().Name, propertyName);
            }
            return (T)property.GetValue(target);
        }

        public static void SetPropertyValue(object target, string propertyName, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (property == null)
            {
                throw new MissingMemberException(target.GetType().Name, propertyName);
            }
            property.SetValue(target, value);
        }

        public static T CreateInstance<T>(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new TypeLoadException($"Could not load type: {typeName}");
            }
            return (T)Activator.CreateInstance(type);
        }

        public static object InvokeStaticMethod(string typeName, string methodName, params object[] parameters)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new TypeLoadException($"Could not load type: {typeName}");
            }
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                throw new MissingMethodException(typeName, methodName);
            }
            return method.Invoke(null, parameters);
        }
    }
}