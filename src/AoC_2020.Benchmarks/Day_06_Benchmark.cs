using BenchmarkDotNet.Attributes;

namespace AoC_2020.Benchmarks
{
    public class Day_06_Benchmark : BaseDayBenchmark
    {
        private readonly Day_06 _problem = new();

        [Benchmark]
        public string Part1_Binary() => _problem.Part1_Binary();

        [Benchmark]
        public string Part1_Linq() => _problem.Part1_Linq();

        [Benchmark]
        public string Part2_Binary() => _problem.Part2_Binary();

        [Benchmark]
        public string Part2_IntersectAll() => _problem.Part2_IntersectAll();

        [Benchmark]
        public string Part2_Linq() => _problem.Part2_Linq();

        [Benchmark]
        public string Part2_Loop() => _problem.Part2_Loop();
    }
}
