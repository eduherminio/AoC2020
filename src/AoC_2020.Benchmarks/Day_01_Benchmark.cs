using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace AoC_2020.Benchmarks
{
    [MemoryDiagnoser]
    [NativeMemoryProfiler]
    public class Day_01_Benchmark : BaseDayBenchmark
    {
        private readonly Day_01 _problem = new Day_01();

        [Benchmark]
        public string Part2_Dictionary() => _problem.Part2_Dictionary();

        [Benchmark]
        public string Part2_Combinations() => _problem.Part2_Combinations(3);

        [Benchmark]
        public string Part2_NestedLoops() => _problem.Part2_NestedLoops();

        [Benchmark]
        public string Part2_Linq() => _problem.Part2_Linq();
    }
}
