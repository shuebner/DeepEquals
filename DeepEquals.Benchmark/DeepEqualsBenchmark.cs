using BenchmarkDotNet.Attributes;
using System;
using System.Threading;

namespace SvSoft.DeepEquals
{
    [DisassemblyDiagnoser(maxDepth: 5)]
    public class DeepEqualsBenchmark
    {
        private SinglePrimitiveHandWritten singleHandWritten;
        private SinglePrimitiveGenerated singleGenerated;

        private SinglePrimitiveHandWritten otherSingleHandWritten;
        private SinglePrimitiveGenerated otherSingleGenerated;

        private MultipleSimplePropertiesHandWritten multipleHandWritten;
        private MultipleSimplePropertiesGenerated multipleGenerated;

        private MultipleSimplePropertiesHandWritten otherMultipleHandWritten;
        private MultipleSimplePropertiesGenerated otherMultipleGenerated;


        [GlobalSetup]
        public void GlobalSetup()
        {
            singleHandWritten = new SinglePrimitiveHandWritten { Value = 42 };
            singleGenerated = new SinglePrimitiveGenerated { Value = 42 };

            otherSingleHandWritten = new SinglePrimitiveHandWritten { Value = 42 };
            otherSingleGenerated = new SinglePrimitiveGenerated { Value = 42 };

            multipleHandWritten = new MultipleSimplePropertiesHandWritten { Value1 = 42, Value2 = "foo bar baz", Value3 = true, Value4 = false };
            multipleGenerated = new MultipleSimplePropertiesGenerated { Value1 = 42, Value2 = "foo bar baz", Value3 = true, Value4 = false };

            otherMultipleHandWritten = new MultipleSimplePropertiesHandWritten { Value1 = 42, Value2 = "foo bar baz", Value3 = true, Value4 = false };
            otherMultipleGenerated = new MultipleSimplePropertiesGenerated { Value1 = 42, Value2 = "foo bar baz", Value3 = true, Value4 = false };
        }

        [Benchmark]
        public void HandWrittenEqualsSingle()
        {
            singleHandWritten.Equals(otherSingleHandWritten);
        }

        [Benchmark]
        public void GeneratedEqualsSingle()
        {
            singleGenerated.Equals(otherSingleGenerated);
        }

        [Benchmark]
        public void HandWrittenEqualsMultiple()
        {
            multipleHandWritten.Equals(otherMultipleHandWritten);
        }

        [Benchmark]
        public void GeneratedEqualsMultiple()
        {
            multipleGenerated.Equals(otherMultipleGenerated);
        }

        private class SinglePrimitiveGenerated
        {
            private static readonly Func<SinglePrimitiveGenerated, SinglePrimitiveGenerated, bool> DeepEqualsFunc = DeepEquals.FromProperties<SinglePrimitiveGenerated>();

            public int Value { get; set; }

            public override bool Equals(object obj)
            {
                return obj is SinglePrimitiveGenerated generated &&
                    DeepEqualsFunc(this, generated);
            }
        }

        private class SinglePrimitiveHandWritten
        {
            public int Value { get; set; }

            public override bool Equals(object obj)
            {
                return obj is SinglePrimitiveHandWritten written &&
                    Value == written.Value;
            }
        }
        private class MultipleSimplePropertiesGenerated
        {
            private static readonly Func<MultipleSimplePropertiesGenerated, MultipleSimplePropertiesGenerated, bool> DeepEqualsFunc = DeepEquals.FromProperties<MultipleSimplePropertiesGenerated>();

            public int Value1 { get; set; }

            public string Value2 { get; set; }

            public bool Value3 { get; set; }

            public bool? Value4 { get; set; }

            public override bool Equals(object obj)
            {
                return obj is MultipleSimplePropertiesGenerated generated &&
                       DeepEqualsFunc(this, generated);
            }
        }

        private class MultipleSimplePropertiesHandWritten
        {
            public int Value1 { get; set; }

            public string Value2 { get; set; }

            public bool Value3 { get; set; }

            public bool? Value4 { get; set; }

            public override bool Equals(object obj)
            {
                return obj is MultipleSimplePropertiesHandWritten written &&
                       Value1 == written.Value1 &&
                       Value2 == written.Value2 &&
                       Value3 == written.Value3 &&
                       Value4 == written.Value4;
            }
        }
    }
}
