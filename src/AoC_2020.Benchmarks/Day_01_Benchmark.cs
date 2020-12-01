using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace AoC_2020.Benchmarks
{
    [MemoryDiagnoser]
    [NativeMemoryProfiler]
    public class Day_01_Benchmark
    {
        private readonly Day_01 _problem = new Day_01();

        [Benchmark]
        public string DictionaryApproach() => _problem.DictionaryApproach();

        [Benchmark]
        public string CombinationsApproach() => _problem.CombinationsApproach(3);

        [Benchmark]
        public string NestedLoopsApproach() => _problem.NestedLoopsApproach();
    }
}
