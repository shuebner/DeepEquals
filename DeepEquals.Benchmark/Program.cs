using BenchmarkDotNet.Running;
using SvSoft.DeepEquals;
using System;

namespace DeepEquals.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<DeepEqualsBenchmark>();
        }
    }
}
