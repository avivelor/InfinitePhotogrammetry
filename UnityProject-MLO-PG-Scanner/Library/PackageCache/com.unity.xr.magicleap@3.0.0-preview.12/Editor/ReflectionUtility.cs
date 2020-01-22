using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEditor.XR.MagicLeap
{
    internal static class ReflectionUtility
    {
        const BindingFlags kDefaultMethodBindingFlags =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static;
        public static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes());
        }

        public static IEnumerable<T> GetAllAttributesOfType<T>() where T : Attribute
        {
            return GetAllTypes()
                .SelectMany(t => Attribute.GetCustomAttributes(t))
                .Where(a => a is T)
                .Select(a => a as T);
        }

        public static IEnumerable<KeyValuePair<MethodInfo, List<T>>> GetMethodsWithAttributes<T>(BindingFlags flags = kDefaultMethodBindingFlags) where T : Attribute
        {
            foreach (var m in GetAllTypes().SelectMany(t => t.GetMethods(flags)))
            {
                List<T> attribs = null;
                try
                {
                    attribs = Attribute.GetCustomAttributes(m).Select(a => a as T).Where(a => a != null).ToList();
                }
                catch (BadImageFormatException)
                {
                    // ignore
                }
                if (attribs != null && attribs.Count > 0)
                    yield return new KeyValuePair<MethodInfo, List<T>>(m, attribs);
            }
        }

        public static IEnumerable<KeyValuePair<Type, List<T>>> GetTypesWithAttributes<T>() where T : Attribute
        {
            foreach (var t in GetAllTypes())
            {
                var attribs = Attribute.GetCustomAttributes(t).Select(a => a as T).Where(a => a != null).ToList();
                if (attribs.Count > 0)
                    yield return new KeyValuePair<Type, List<T>>(t, attribs);
            }
        }

        public static bool AreSignaturesMatching(MethodInfo left, MethodInfo right)
        {
            if (left.IsStatic != right.IsStatic)
                return false;
            if (left.ReturnType != right.ReturnType)
                return false;

            ParameterInfo[] leftParams = left.GetParameters();
            ParameterInfo[] rightParams = right.GetParameters();
            if (leftParams.Length != rightParams.Length)
                return false;
            for (int i = 0; i < leftParams.Length; i++)
            {
                if (leftParams[i].ParameterType != rightParams[i].ParameterType)
                    return false;
            }

            return true;
        }
    }
}