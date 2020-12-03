using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_02_Benchmark : BaseDayBenchmark
    {
        private readonly Day_02 _problem = new();

        [Benchmark]
        public string Part2_xor() => _problem.Part2_xor();

        [Benchmark]
        public string Part2_Linq() => _problem.Part2_Linq();
    }
}
