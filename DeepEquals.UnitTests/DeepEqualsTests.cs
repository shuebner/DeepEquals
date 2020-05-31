using DeepEquals.TestSamples;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SvSoft.DeepEquals
{
    public class DeepEqualsTests
    {
        [Fact]
        public void Equals_When_class_has_no_properties_Then_always_returns_true()
        {
            var equals = DeepEquals.FromProperties<Empty>();

            equals(new Empty(), new Empty()).Should().BeTrue();
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        public void Equals_When_class_has_one_primitive_property_Then_returns_operator_equality_of_the_property(int first, int second, bool expectedIsEqual)
        {
            var equals = DeepEquals.FromProperties<SingleProperty<int>>();

            equals(new SingleProperty<int> { Value = first }, new SingleProperty<int> { Value = second }).Should().Be(expectedIsEqual);
        }

        [Theory]
        [InlineData("foo", "foo", true)]
        [InlineData("foo", "bar", false)]
        [InlineData("foo", null, false)]
        [InlineData(null, "foo", false)]
        [InlineData(null, null, true)]
        public void Equals_When_class_has_one_builtin_object_property_Then_returns_object_equality_of_the_property(string first, string second, bool expectedIsEqual)
        {
            var equals = DeepEquals.FromProperties<SingleProperty<string>>();

            equals(new SingleProperty<string> { Value = first }, new SingleProperty<string> { Value = second }).Should().Be(expectedIsEqual);
        }

        [Theory]
        [MemberData(nameof(EquatableSample))]
        public void Equals_When_equatables_are_equal_Then_returns_true(SomeEquatable one, SomeEquatable other, bool expectedIsEqual)
        {
            var equals = DeepEquals.FromProperties<SingleProperty<SomeEquatable>>();

            equals(new SingleProperty<SomeEquatable> { Value = one },
                new SingleProperty<SomeEquatable> { Value = other }).Should().Be(expectedIsEqual);
        }

        [Theory]
        [MemberData(nameof(BuiltInEnumerableSample))]
        public void Equals_When_class_has_one_generic_builtin_IEnumerable_property_Then_returns_sequence_equality(IEnumerable<string> one, IEnumerable<string> other, bool expectedIsEqual)
        {
            var equals = DeepEquals.FromProperties<SingleProperty<IEnumerable<string>>>();

            equals(new SingleProperty<IEnumerable<string>> { Value = one },
                new SingleProperty<IEnumerable<string>> { Value = other }).Should().Be(expectedIsEqual);
        }

        public static IEnumerable<object[]> BuiltInEnumerableSample { get; } = new (IEnumerable<string> One, IEnumerable<string> Other, bool ExpectedIsEqual)[]
        {
            (new[] { "foo", "bar", "baz" }, new[] { "foo", "bar" }, false),
            (new[] { "foo", "bar", "baz" }, null, false),
            (null, new[] { "foo", "bar" }, false),
            (null, null, true),
            (new[] { "foo", "bar", "baz" }, new[] { "foo", "baz", "bar" }, false),
            (new[] { "foo", "bar", "baz" }, new[] { "foo", "bar", "baz" }, true)
        }.Select(tuple => new object[] { tuple.One, tuple.Other, tuple.ExpectedIsEqual });

        public static IEnumerable<object[]> EquatableSample { get; } = new (SomeEquatable One, SomeEquatable Other, bool ExpectedIsEqual)[]
        {
            (new SomeEquatable { Value = "42" }, new SomeEquatable { Value = "42" }, true),
            (new SomeEquatable { Value = "42" }, new SomeEquatable { Value = "23" }, false),
            (new SomeEquatable { Value = "42" }, null, false),
            (null, new SomeEquatable { Value = "23" }, false),
            (null, null, true)
        }.Select(tuple => new object[] { tuple.One, tuple.Other, tuple.ExpectedIsEqual });

        public class SomeEquatable : IEquatable<SomeEquatable>
        {
            public string Value { get; set; }

            public bool Equals(SomeEquatable other) =>
                other != null &&
                other.Value == Value;
        }
    }
}
