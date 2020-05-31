using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SvSoft.DeepEquals
{
    static class Methods
    {
        private static readonly MethodInfo OpenGenericSequenceEqual =
            typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(method => method.Name == nameof(Enumerable.SequenceEqual))
                .Where(method => method.GetParameters() is ParameterInfo[] parameters &&
                    parameters.Count() == 2 &&
                    parameters[0].ParameterType.IsGenericType &&
                    parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    parameters[1].ParameterType.IsGenericType &&
                    parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Single();

        public static MethodInfo SequenceEqual(Type type) => OpenGenericSequenceEqual.MakeGenericMethod(type);

        public static MethodInfo EquatableEquals(Type type) =>
            type.GetMethod(
                nameof(IEquatable<object>.Equals),
                new[] { type });

        public static MethodInfo ObjectEquals =>
            typeof(object).GetMethod(nameof(object.Equals), BindingFlags.Public | BindingFlags.Static);
    }
}
