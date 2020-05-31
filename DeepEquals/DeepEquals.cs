using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SvSoft.DeepEquals
{
    public static class DeepEquals
    {
        private delegate MemberExpression GetMemberAccessor(Expression param, string name);
        
        private static readonly Type[] ValueTypeEquivalents = { typeof(string) };

        public static Func<T, T, bool> FromFields<T>() =>
            ForMembers<T, FieldInfo>(typeof(T).GetFields(), fieldInfo => fieldInfo.FieldType, Expression.Field);

        public static Func<T, T, bool> FromProperties<T>() =>
            ForMembers<T, PropertyInfo>(typeof(T).GetProperties(), propertyInfo => propertyInfo.PropertyType, Expression.Property);

        private static Func<T, T, bool> ForMembers<T, TMember>(
            IEnumerable<TMember> memberInfos,
            Func<TMember, Type> getMemberType,
            GetMemberAccessor getMemberAccessor)
            where TMember : MemberInfo
        {
            ParameterExpression oneParam = Expression.Parameter(typeof(T), "one");
            ParameterExpression otherParam = Expression.Parameter(typeof(T), "other");

            if (!memberInfos.Any())
            {
                return Expression.Lambda<Func<T, T, bool>>(
                    Expression.Constant(true),
                    oneParam,
                    otherParam)
                    .Compile();
            }

            var equalityExpressions = memberInfos.Select(memberInfo =>
                EqualityExpr(oneParam, otherParam, memberInfo.Name, getMemberType(memberInfo), getMemberAccessor));
            var andExpression = equalityExpressions.Aggregate(Expression.AndAlso);

            Expression<Func<T, T, bool>> equalsExpression = Expression.Lambda<Func<T, T, bool>>(
                andExpression,
                oneParam,
                otherParam);

            return equalsExpression.Compile();
        }

        private static Expression EqualityExpr(Expression oneParam, Expression otherParam, string memberName, Type memberType,
            GetMemberAccessor getMemberAccessor)
        {
            if (memberType.IsValueType || ValueTypeEquivalents.Contains(memberType))
            {
                // one == other
                return EqualOperatorExpr(oneParam, otherParam, memberName, getMemberAccessor);
            }

            if (memberType.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEquatable<>) &&
                i.GenericTypeArguments.Single() == memberType))
            {
                // one == null ? other == null : one.Equals(other)
                return
                    Expression.Condition(
                        Expression.Equal(
                            getMemberAccessor(oneParam, memberName),
                            Expression.Constant(null)),
                        Expression.Equal(
                            getMemberAccessor(otherParam, memberName),
                            Expression.Constant(null)),
                        TypedEqualsExpr(oneParam, otherParam, memberName, memberType, getMemberAccessor));
            }

            if (new[] { memberType }.Concat(memberType.GetInterfaces()).FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) is Type firstEnumerableType)
            {
                // (one == null && other == null) || (one != null && other != null && Enumerable.SequenceEquals(one, other))
                return
                    Expression.OrElse(
                        Expression.AndAlso(
                            Expression.Equal(
                                    getMemberAccessor(oneParam, memberName),
                                    Expression.Constant(null)),
                            Expression.Equal(
                                getMemberAccessor(otherParam, memberName),
                                Expression.Constant(null))),
                        Expression.AndAlso(
                            Expression.AndAlso(
                                Expression.NotEqual(
                                    getMemberAccessor(oneParam, memberName),
                                    Expression.Constant(null)),
                                Expression.NotEqual(
                                    getMemberAccessor(otherParam, memberName),
                                    Expression.Constant(null))),
                            SequenceEqualsExpr(oneParam, otherParam, memberName, firstEnumerableType.GetGenericArguments().Single(), getMemberAccessor)));
            }

            return ObjectEqualsExpression(oneParam, otherParam, memberName, getMemberAccessor);
        }

        private static BinaryExpression EqualOperatorExpr(
            Expression oneParam, Expression otherParam, string fieldName, GetMemberAccessor getMemberAccessor) =>
            Expression.Equal(
                getMemberAccessor(oneParam, fieldName),
                getMemberAccessor(otherParam, fieldName));

        private static MethodCallExpression TypedEqualsExpr(
            Expression oneParam, Expression otherParam, string memberName, Type memberType, GetMemberAccessor getMemberAccessor) =>
            Expression.Call(
                getMemberAccessor(oneParam, memberName),
                Methods.EquatableEquals(memberType),
                getMemberAccessor(otherParam, memberName));

        private static MethodCallExpression ObjectEqualsExpression
            (Expression oneParam, Expression otherParam, string memberName, GetMemberAccessor getMemberAccessor) =>
            Expression.Call(
                Methods.ObjectEquals,
                getMemberAccessor(oneParam, memberName),
                getMemberAccessor(otherParam, memberName));

        private static MethodCallExpression SequenceEqualsExpr(
            Expression oneParam, Expression otherParam, string memberName, Type elementType, GetMemberAccessor getMemberAccessor)
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);

            return Expression.Call(
                Methods.SequenceEqual(elementType),
                Expression.Convert(
                    getMemberAccessor(oneParam, memberName), enumerableType),
                Expression.Convert(
                    getMemberAccessor(otherParam, memberName), enumerableType));
        }
    }
}
