using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace AoC_2020.Benchmarks
{
    [MemoryDiagnoser]
    [NativeMemoryProfiler]
    public class Day_02_Benchmark
    {
        private readonly Day_02 _problem = new();

        [Benchmark]
        public string Part1_Linq() => _problem.Part1_Linq();

        [Benchmark]
        public string Part1_Regex() => _problem.Part1_Regex();

        [Benchmark]
        public string Part2_xor() => _problem.Part2_xor();

        [Benchmark]
        public string Part2_Linq() => _problem.Part2_Linq();
    }
}
