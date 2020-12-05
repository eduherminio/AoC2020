using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_05_Benchmark : BaseDayBenchmark
    {
        private readonly Day_05 _problem = new();

        [Benchmark]
        public string Part2_Linq() => _problem.Part2_Linq();

        [Benchmark]
        public string Part2_Loop() => _problem.Part2_Loop();
    }
}
